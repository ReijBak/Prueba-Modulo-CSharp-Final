namespace TalentoPlus.Core.Interfaces;

/// <summary>
/// Interface for PDF generation service operations.
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Generates a resume/CV PDF for an employee.
    /// </summary>
    /// <param name="documento">Employee document ID</param>
    /// <returns>PDF as byte array</returns>
    Task<byte[]> GenerateEmployeeResumeAsync(long documento);
}

