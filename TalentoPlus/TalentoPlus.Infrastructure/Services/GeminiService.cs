using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TalentoPlus.Core.Interfaces;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Services;

/// <summary>
/// Gemini AI service for natural language to SQL conversion.
/// </summary>
public class GeminiService : IGeminiService
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly TalentoPlusDbContext _context;
    private readonly ILogger<GeminiService> _logger;

    private const string GeminiApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public GeminiService(
        HttpClient httpClient,
        TalentoPlusDbContext context,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
        _apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? 
            throw new InvalidOperationException("GEMINI_API_KEY environment variable is not set");
    }

    public async Task<string> QueryDatabaseAsync(string question)
    {
        try
        {
            // Build the prompt with database schema
            var prompt = BuildPrompt(question);
            
            // Call Gemini API
            var sqlQuery = await CallGeminiApiAsync(prompt);
            
            if (string.IsNullOrEmpty(sqlQuery))
            {
                return JsonSerializer.Serialize(new { error = "No se pudo generar una consulta SQL válida." });
            }

            // Validate that it's a SELECT query only (security)
            if (!IsSelectQuery(sqlQuery))
            {
                return JsonSerializer.Serialize(new { error = "Solo se permiten consultas SELECT." });
            }

            // Execute the query
            var result = await ExecuteQueryAsync(sqlQuery);
            
            return JsonSerializer.Serialize(new 
            { 
                query = sqlQuery, 
                result = result,
                success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Question}", question);
            return JsonSerializer.Serialize(new { error = ex.Message, success = false });
        }
    }

    private string BuildPrompt(string question)
    {
        return $@"
You are a SQL expert. Given the following PostgreSQL database schema, generate ONLY a valid SELECT SQL query to answer the user's question. 
Return ONLY the SQL query, nothing else. No explanations, no markdown, just the raw SQL.

DATABASE SCHEMA:
- Table: estado (estado_id SERIAL PRIMARY KEY, nombre_estado VARCHAR(50) UNIQUE NOT NULL)
- Table: departamento (departamento_id SERIAL PRIMARY KEY, nombre_departamento VARCHAR(100) UNIQUE NOT NULL)
- Table: cargo (cargo_id SERIAL PRIMARY KEY, nombre_cargo VARCHAR(100) UNIQUE NOT NULL)
- Table: nivel_educativo (nivel_educativo_id SERIAL PRIMARY KEY, nombre_nivel VARCHAR(100) UNIQUE NOT NULL)
- Table: empleado (
    documento BIGINT PRIMARY KEY,
    nombres VARCHAR(100) NOT NULL,
    apellidos VARCHAR(100) NOT NULL,
    fecha_nacimiento DATE NOT NULL,
    direccion VARCHAR(255),
    telefono VARCHAR(20),
    email VARCHAR(150) UNIQUE,
    salario NUMERIC(10,2),
    fecha_ingreso DATE NOT NULL,
    perfil_profesional TEXT,
    estado_id INTEGER NOT NULL REFERENCES estado(estado_id),
    nivel_educativo_id INTEGER NOT NULL REFERENCES nivel_educativo(nivel_educativo_id),
    departamento_id INTEGER NOT NULL REFERENCES departamento(departamento_id),
    cargo_id INTEGER NOT NULL REFERENCES cargo(cargo_id)
)

IMPORTANT RULES:
1. Generate ONLY SELECT queries, never INSERT, UPDATE, DELETE, DROP, or any other DML/DDL
2. Always use proper JOINs when referencing multiple tables
3. Use lowercase for table and column names
4. If the question cannot be answered with the schema, return: SELECT 'Pregunta no válida para el esquema' AS error

USER QUESTION: {question}

SQL QUERY:";
    }

    private async Task<string> CallGeminiApiAsync(string prompt)
    {
        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.1,
                maxOutputTokens = 500
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{GeminiApiUrl}?key={_apiKey}", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API error: {Response}", responseContent);
            throw new Exception($"Gemini API error: {response.StatusCode}");
        }

        // Parse the response
        using var doc = JsonDocument.Parse(responseContent);
        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        // Clean up the SQL (remove markdown code blocks if present)
        var sql = text?.Trim()
            .Replace("```sql", "")
            .Replace("```", "")
            .Trim();

        return sql ?? string.Empty;
    }

    private bool IsSelectQuery(string sql)
    {
        var upperSql = sql.ToUpperInvariant().Trim();
        
        // Must start with SELECT
        if (!upperSql.StartsWith("SELECT"))
            return false;

        // Must not contain dangerous keywords
        var dangerousKeywords = new[] 
        { 
            "INSERT", "UPDATE", "DELETE", "DROP", "TRUNCATE", "ALTER", 
            "CREATE", "EXEC", "EXECUTE", "GRANT", "REVOKE", "--", "/*" 
        };

        return !dangerousKeywords.Any(keyword => upperSql.Contains(keyword));
    }

    private async Task<object> ExecuteQueryAsync(string sql)
    {
        var results = new List<Dictionary<string, object?>>();

        using var command = _context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sql;

        await _context.Database.OpenConnectionAsync();

        try
        {
            using var reader = await command.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                }
                results.Add(row);
            }

            return results;
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }
}

