namespace TalentoPlus.Core.DTOs;

/// <summary>
/// DTO for employee data transfer.
/// </summary>
public class EmpleadoDto
{
    public long Documento { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public decimal? Salario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public string? PerfilProfesional { get; set; }
    public int EstadoId { get; set; }
    public string? NombreEstado { get; set; }
    public int NivelEducativoId { get; set; }
    public string? NombreNivelEducativo { get; set; }
    public int DepartamentoId { get; set; }
    public string? NombreDepartamento { get; set; }
    public int CargoId { get; set; }
    public string? NombreCargo { get; set; }
}

/// <summary>
/// DTO for creating/updating an employee.
/// </summary>
public class CreateEmpleadoDto
{
    public long Documento { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public decimal? Salario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public string? PerfilProfesional { get; set; }
    public string? Password { get; set; }
    public int EstadoId { get; set; }
    public int NivelEducativoId { get; set; }
    public int DepartamentoId { get; set; }
    public int CargoId { get; set; }
}

/// <summary>
/// DTO for updating an employee.
/// </summary>
public class UpdateEmpleadoDto
{
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public decimal? Salario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public string? PerfilProfesional { get; set; }
    public int EstadoId { get; set; }
    public int NivelEducativoId { get; set; }
    public int DepartamentoId { get; set; }
    public int CargoId { get; set; }
}

