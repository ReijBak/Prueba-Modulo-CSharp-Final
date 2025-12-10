namespace TalentoPlus.Core.DTOs;

/// <summary>
/// DTO for dashboard AI query request.
/// </summary>
public class DashboardQueryDto
{
    public string Question { get; set; } = string.Empty;
}

/// <summary>
/// DTO for dashboard AI query response.
/// </summary>
public class DashboardQueryResponseDto
{
    public bool Success { get; set; }
    public string? Query { get; set; }
    public object? Result { get; set; }
    public string? Message { get; set; }
}

