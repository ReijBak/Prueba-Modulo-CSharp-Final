namespace TalentoPlus.Core.DTOs;

/// <summary>
/// DTO for admin login request.
/// </summary>
public class AdminLoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for admin registration request.
/// </summary>
public class AdminRegisterDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}

/// <summary>
/// DTO for employee login request.
/// </summary>
public class EmployeeLoginDto
{
    public long Documento { get; set; }
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// DTO for authentication response.
/// </summary>
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string? Message { get; set; }
    public string? UserType { get; set; }
    public string? FullName { get; set; }
    public long? Documento { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

