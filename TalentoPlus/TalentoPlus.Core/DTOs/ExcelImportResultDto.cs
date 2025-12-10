namespace TalentoPlus.Core.DTOs;

/// <summary>
/// DTO for Excel import result.
/// </summary>
public class ExcelImportResultDto
{
    public bool Success { get; set; }
    public int TotalRows { get; set; }
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ErrorCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public string? Message { get; set; }
}

