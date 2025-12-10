using TalentoPlus.Core.DTOs;

namespace TalentoPlus.Core.Interfaces;

/// <summary>
/// Interface for Excel import service operations.
/// </summary>
public interface IExcelImportService
{
    /// <summary>
    /// Imports employees from an Excel file stream.
    /// </summary>
    /// <param name="stream">Excel file stream</param>
    /// <returns>Import result with details</returns>
    Task<ExcelImportResultDto> ImportEmployeesAsync(Stream stream);
}

