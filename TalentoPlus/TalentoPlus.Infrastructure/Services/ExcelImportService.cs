using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using TalentoPlus.Core.DTOs;
using TalentoPlus.Core.Entities;
using TalentoPlus.Core.Interfaces;

namespace TalentoPlus.Infrastructure.Services;

/// <summary>
/// Excel import service using EPPlus.
/// </summary>
public class ExcelImportService : IExcelImportService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ExcelImportService> _logger;
    private readonly PasswordHasher<object> _passwordHasher;

    static ExcelImportService()
    {
        // Set EPPlus license for version 8+ (static constructor runs once)
        ExcelPackage.License.SetNonCommercialPersonal("TalentoPlus");
    }

    public ExcelImportService(IUnitOfWork unitOfWork, ILogger<ExcelImportService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _passwordHasher = new PasswordHasher<object>();
    }

    /// <summary>
    /// Removes diacritics (tildes, accents) from a string.
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public async Task<ExcelImportResultDto> ImportEmployeesAsync(Stream stream)
    {
        var result = new ExcelImportResultDto
        {
            Success = true,
            Errors = new List<string>()
        };

        try
        {
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();

            if (worksheet == null)
            {
                result.Success = false;
                result.Message = "El archivo Excel no contiene hojas de trabajo.";
                return result;
            }

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount < 2)
            {
                result.Success = false;
                result.Message = "El archivo Excel está vacío o no contiene datos.";
                return result;
            }

            result.TotalRows = rowCount - 1; // Exclude header

            // Get existing catalog data for validation
            var estados = (await _unitOfWork.Estados.GetAllAsync()).ToDictionary(e => e.NombreEstado.ToLower(), e => e.EstadoId);
            var departamentos = (await _unitOfWork.Departamentos.GetAllAsync()).ToDictionary(d => d.NombreDepartamento.ToLower(), d => d.DepartamentoId);
            var cargos = (await _unitOfWork.Cargos.GetAllAsync()).ToDictionary(c => c.NombreCargo.ToLower(), c => c.CargoId);
            var nivelesEducativos = (await _unitOfWork.NivelesEducativos.GetAllAsync()).ToDictionary(n => n.NombreNivel.ToLower(), n => n.NivelEducativoId);

            // Excel columns order:
            // A(1): Documento, B(2): Nombres, C(3): Apellidos, D(4): FechaNacimiento, 
            // E(5): Direccion, F(6): Telefono, G(7): Email, H(8): Cargo, 
            // I(9): Salario, J(10): FechaIngreso, K(11): Estado, L(12): NivelEducativo, 
            // M(13): PerfilProfesional, N(14): Departamento

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    // Read cells according to actual Excel format
                    var documento = GetCellValue<long>(worksheet, row, 1);          // A - Documento
                    var nombres = GetCellValue<string>(worksheet, row, 2);           // B - Nombres
                    var apellidos = GetCellValue<string>(worksheet, row, 3);         // C - Apellidos
                    var fechaNacimientoRaw = GetCellValue<DateTime>(worksheet, row, 4); // D - FechaNacimiento
                    var direccion = GetCellValue<string>(worksheet, row, 5);         // E - Direccion
                    var telefono = GetCellValue<string>(worksheet, row, 6);          // F - Telefono
                    var emailRaw = GetCellValue<string>(worksheet, row, 7);          // G - Email
                    var cargoNombre = GetCellValue<string>(worksheet, row, 8)?.ToLower();  // H - Cargo
                    var salario = GetCellValue<decimal?>(worksheet, row, 9);         // I - Salario
                    var fechaIngresoRaw = GetCellValue<DateTime>(worksheet, row, 10);   // J - FechaIngreso
                    var estadoNombre = GetCellValue<string>(worksheet, row, 11)?.ToLower();  // K - Estado
                    var nivelEducativoNombre = GetCellValue<string>(worksheet, row, 12)?.ToLower();  // L - NivelEducativo
                    var perfilProfesional = GetCellValue<string>(worksheet, row, 13); // M - PerfilProfesional
                    var departamentoNombre = GetCellValue<string>(worksheet, row, 14)?.ToLower();  // N - Departamento

                    // Convert dates to UTC for PostgreSQL
                    var fechaNacimiento = fechaNacimientoRaw != default 
                        ? DateTime.SpecifyKind(fechaNacimientoRaw, DateTimeKind.Utc) 
                        : default;
                    var fechaIngreso = fechaIngresoRaw != default 
                        ? DateTime.SpecifyKind(fechaIngresoRaw, DateTimeKind.Utc) 
                        : default;

                    // Remove diacritics (tildes) from email
                    var email = RemoveDiacritics(emailRaw);

                    // Validate required fields
                    var errors = new List<string>();
                    if (documento == 0) errors.Add("Documento es requerido");
                    if (string.IsNullOrEmpty(nombres)) errors.Add("Nombres es requerido");
                    if (string.IsNullOrEmpty(apellidos)) errors.Add("Apellidos es requerido");
                    if (fechaNacimiento == default) errors.Add("Fecha de nacimiento es requerida");
                    if (fechaIngreso == default) errors.Add("Fecha de ingreso es requerida");
                    
                    // Validate catalogs - create if not exists or validate
                    int estadoId = 0;
                    int nivelEducativoId = 0;
                    int departamentoId = 0;
                    int cargoId = 0;

                    if (string.IsNullOrEmpty(estadoNombre))
                    {
                        errors.Add("Estado es requerido");
                    }
                    else if (!estados.TryGetValue(estadoNombre, out estadoId))
                    {
                        errors.Add($"Estado '{estadoNombre}' no existe en el sistema");
                    }

                    if (string.IsNullOrEmpty(nivelEducativoNombre))
                    {
                        errors.Add("Nivel educativo es requerido");
                    }
                    else if (!nivelesEducativos.TryGetValue(nivelEducativoNombre, out nivelEducativoId))
                    {
                        errors.Add($"Nivel educativo '{nivelEducativoNombre}' no existe en el sistema");
                    }

                    if (string.IsNullOrEmpty(departamentoNombre))
                    {
                        errors.Add("Departamento es requerido");
                    }
                    else if (!departamentos.TryGetValue(departamentoNombre, out departamentoId))
                    {
                        errors.Add($"Departamento '{departamentoNombre}' no existe en el sistema");
                    }

                    if (string.IsNullOrEmpty(cargoNombre))
                    {
                        errors.Add("Cargo es requerido");
                    }
                    else if (!cargos.TryGetValue(cargoNombre, out cargoId))
                    {
                        errors.Add($"Cargo '{cargoNombre}' no existe en el sistema");
                    }

                    if (errors.Any())
                    {
                        result.ErrorCount++;
                        result.Errors.Add($"Fila {row}: {string.Join(", ", errors)}");
                        continue;
                    }

                    // Check if employee exists (upsert logic)
                    var existingEmployee = await _unitOfWork.Empleados.GetByIdAsync(documento);
                    
                    if (existingEmployee != null)
                    {
                        // Update existing employee
                        existingEmployee.Nombres = nombres!;
                        existingEmployee.Apellidos = apellidos!;
                        existingEmployee.FechaNacimiento = fechaNacimiento;
                        existingEmployee.Direccion = direccion;
                        existingEmployee.Telefono = telefono;
                        existingEmployee.Email = email;
                        existingEmployee.Salario = salario;
                        existingEmployee.FechaIngreso = fechaIngreso;
                        existingEmployee.PerfilProfesional = perfilProfesional;
                        existingEmployee.EstadoId = estadoId;
                        existingEmployee.NivelEducativoId = nivelEducativoId;
                        existingEmployee.DepartamentoId = departamentoId;
                        existingEmployee.CargoId = cargoId;

                        // Generate password if not exists
                        if (string.IsNullOrEmpty(existingEmployee.PasswordHash))
                        {
                            var defaultPassword = documento.ToString();
                            existingEmployee.PasswordHash = _passwordHasher.HashPassword(null!, defaultPassword);
                        }

                        await _unitOfWork.Empleados.UpdateAsync(existingEmployee);
                        result.UpdatedCount++;
                    }
                    else
                    {
                        // Create new employee with default password (documento)
                        var defaultPassword = documento.ToString();
                        var passwordHash = _passwordHasher.HashPassword(null!, defaultPassword);

                        var newEmployee = new Empleado
                        {
                            Documento = documento,
                            Nombres = nombres!,
                            Apellidos = apellidos!,
                            FechaNacimiento = fechaNacimiento,
                            Direccion = direccion,
                            Telefono = telefono,
                            Email = email,
                            Salario = salario,
                            FechaIngreso = fechaIngreso,
                            PerfilProfesional = perfilProfesional,
                            PasswordHash = passwordHash,
                            EstadoId = estadoId,
                            NivelEducativoId = nivelEducativoId,
                            DepartamentoId = departamentoId,
                            CargoId = cargoId
                        };

                        await _unitOfWork.Empleados.AddAsync(newEmployee);
                        result.InsertedCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.Errors.Add($"Fila {row}: Error inesperado - {ex.Message}");
                    _logger.LogError(ex, "Error processing row {Row}", row);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            result.Message = $"Importación completada. Insertados: {result.InsertedCount}, Actualizados: {result.UpdatedCount}, Errores: {result.ErrorCount}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing Excel file");
            result.Success = false;
            result.Message = $"Error al procesar el archivo: {ex.Message}";
        }

        return result;
    }

    private T? GetCellValue<T>(ExcelWorksheet worksheet, int row, int column)
    {
        var cellValue = worksheet.Cells[row, column].Value;
        
        if (cellValue == null)
            return default;

        try
        {
            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
            {
                if (cellValue is DateTime dt)
                    return (T)(object)dt;
                if (double.TryParse(cellValue.ToString(), out var oaDate))
                    return (T)(object)DateTime.FromOADate(oaDate);
                if (DateTime.TryParse(cellValue.ToString(), out var parsedDate))
                    return (T)(object)parsedDate;
            }

            if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
            {
                if (decimal.TryParse(cellValue.ToString(), out var decimalValue))
                    return (T)(object)decimalValue;
            }

            if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
            {
                if (long.TryParse(cellValue.ToString(), out var longValue))
                    return (T)(object)longValue;
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)cellValue.ToString()!.Trim();
            }

            return (T)Convert.ChangeType(cellValue, typeof(T));
        }
        catch
        {
            return default;
        }
    }
}

