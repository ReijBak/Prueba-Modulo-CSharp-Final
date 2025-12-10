using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TalentoPlus.Core.Interfaces;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Services;

/// <summary>
/// OpenAI ChatGPT service for natural language to SQL conversion.
/// </summary>
public class OpenAIService : IAIService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly TalentoPlusDbContext _context;
    private readonly ILogger<OpenAIService> _logger;

    private const string OpenAIApiUrl = "https://api.openai.com/v1/chat/completions";

    public OpenAIService(
        HttpClient httpClient,
        TalentoPlusDbContext context,
        ILogger<OpenAIService> logger)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? 
            throw new InvalidOperationException("OPENAI_API_KEY environment variable is not set");
    }

    public async Task<string> QueryDatabaseAsync(string question)
    {
        try
        {
            // Build the prompt with database schema
            var prompt = BuildPrompt(question);
            
            // Call OpenAI API
            var sqlQuery = await CallOpenAIApiAsync(prompt);
            
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return JsonSerializer.Serialize(new { success = false, message = "No se pudo generar una consulta SQL válida." });
            }

            // Validate that it's a SELECT query only (security)
            if (!IsSelectQuery(sqlQuery))
            {
                return JsonSerializer.Serialize(new { success = false, message = "Solo se permiten consultas de tipo SELECT." });
            }

            // Execute the SQL query
            var result = await ExecuteSqlQueryAsync(sqlQuery);
            
            return JsonSerializer.Serialize(new { 
                success = true, 
                query = sqlQuery, 
                result = result,
                message = $"Consulta ejecutada exitosamente. {result.Count} resultado(s) encontrado(s)."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Question}", question);
            return JsonSerializer.Serialize(new { success = false, message = $"Error al procesar la consulta: {ex.Message}" });
        }
    }

    private string BuildPrompt(string question)
    {
        var schemaDescription = @"
La base de datos PostgreSQL tiene las siguientes tablas:

1. empleado (documento BIGINT PK, nombres VARCHAR, apellidos VARCHAR, fecha_nacimiento TIMESTAMP, direccion VARCHAR, telefono VARCHAR, email VARCHAR, salario DECIMAL, fecha_ingreso TIMESTAMP, perfil_profesional TEXT, password_hash VARCHAR, estado_id INT FK, nivel_educativo_id INT FK, departamento_id INT FK, cargo_id INT FK)

2. estado (estado_id SERIAL PK, nombre_estado VARCHAR) - Valores: Activo, Inactivo, Vacaciones, Licencia

3. departamento (departamento_id SERIAL PK, nombre_departamento VARCHAR) - Valores: Recursos Humanos, Tecnología, Marketing, Operaciones, Logística, Contabilidad, Ventas

4. cargo (cargo_id SERIAL PK, nombre_cargo VARCHAR) - Valores: Ingeniero, Soporte Técnico, Analista, Coordinador, Desarrollador, Auxiliar, Administrador

5. nivel_educativo (nivel_educativo_id SERIAL PK, nombre_nivel VARCHAR) - Valores: Bachiller, Técnico, Tecnólogo, Profesional, Especialización, Maestría, Doctorado

Relaciones:
- empleado.estado_id -> estado.estado_id
- empleado.departamento_id -> departamento.departamento_id
- empleado.cargo_id -> cargo.cargo_id
- empleado.nivel_educativo_id -> nivel_educativo.nivel_educativo_id
";

        return $@"Eres un experto en SQL para PostgreSQL. Tu tarea es convertir preguntas en lenguaje natural a consultas SQL.

{schemaDescription}

REGLAS IMPORTANTES:
1. SOLO genera consultas SELECT (nunca INSERT, UPDATE, DELETE, DROP, etc.)
2. Usa JOINs cuando necesites información de tablas relacionadas
3. Los nombres de las tablas y columnas están en snake_case
4. Responde SOLO con la consulta SQL, sin explicaciones ni markdown
5. No uses comillas dobles para los nombres de columnas
6. Si la pregunta no se puede responder con los datos disponibles, responde: SELECT 'Consulta no válida' as error

Pregunta del usuario: {question}

Consulta SQL:";
    }

    private async Task<string> CallOpenAIApiAsync(string prompt)
    {
        var requestBody = new
        {
            model = "gpt-3.5-turbo",
            messages = new[]
            {
                new { role = "system", content = "Eres un asistente que convierte preguntas en lenguaje natural a consultas SQL para PostgreSQL. Solo respondes con la consulta SQL, sin explicaciones." },
                new { role = "user", content = prompt }
            },
            max_tokens = 500,
            temperature = 0.1
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.PostAsync(OpenAIApiUrl, content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("OpenAI API error: {Response}", responseContent);
            throw new Exception($"OpenAI API error: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;
        
        var choices = root.GetProperty("choices");
        if (choices.GetArrayLength() > 0)
        {
            var message = choices[0].GetProperty("message");
            var text = message.GetProperty("content").GetString() ?? "";
            
            // Clean up the response - remove markdown code blocks if present
            text = text.Trim();
            if (text.StartsWith("```sql"))
                text = text.Substring(6);
            if (text.StartsWith("```"))
                text = text.Substring(3);
            if (text.EndsWith("```"))
                text = text.Substring(0, text.Length - 3);
            
            return text.Trim();
        }

        return string.Empty;
    }

    private bool IsSelectQuery(string sql)
    {
        var trimmedSql = sql.Trim().ToUpperInvariant();
        
        // Only allow SELECT statements
        if (!trimmedSql.StartsWith("SELECT"))
            return false;
            
        // Block dangerous keywords
        var dangerousKeywords = new[] { "INSERT", "UPDATE", "DELETE", "DROP", "TRUNCATE", "ALTER", "CREATE", "EXEC", "EXECUTE", "--", ";" };
        
        // Allow semicolon only at the end
        var sqlWithoutTrailingSemicolon = trimmedSql.TrimEnd(';');
        
        foreach (var keyword in dangerousKeywords)
        {
            if (keyword == ";" && sqlWithoutTrailingSemicolon.Contains(keyword))
                return false;
            if (keyword != ";" && trimmedSql.Contains(keyword))
                return false;
        }
        
        return true;
    }

    private async Task<List<Dictionary<string, object>>> ExecuteSqlQueryAsync(string sql)
    {
        var results = new List<Dictionary<string, object>>();
        
        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;
        
        await _context.Database.OpenConnectionAsync();
        
        try
        {
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.GetValue(i);
                    row[reader.GetName(i)] = value == DBNull.Value ? null! : value;
                }
                results.Add(row);
            }
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
        
        return results;
    }
}

