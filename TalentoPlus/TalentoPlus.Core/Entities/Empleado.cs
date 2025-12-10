namespace TalentoPlus.Core.Entities;

/// <summary>
/// Represents an employee in the system.
/// </summary>
public class Empleado
{
    // Primary Key
    public long Documento { get; set; }
    
    // Personal Information
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    
    // Work Information
    public decimal? Salario { get; set; }
    public DateTime FechaIngreso { get; set; }
    public string? PerfilProfesional { get; set; }
    
    // Password for employee authentication
    public string? PasswordHash { get; set; }
    
    // Foreign Keys
    public int EstadoId { get; set; }
    public int NivelEducativoId { get; set; }
    public int DepartamentoId { get; set; }
    public int CargoId { get; set; }
    
    // Navigation Properties
    public Estado Estado { get; set; } = null!;
    public NivelEducativo NivelEducativo { get; set; } = null!;
    public Departamento Departamento { get; set; } = null!;
    public Cargo Cargo { get; set; } = null!;
}
