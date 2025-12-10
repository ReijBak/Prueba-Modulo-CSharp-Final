namespace TalentoPlus.Core.Entities;

/// <summary>
/// Represents a job position within the organization.
/// </summary>
public class Cargo
{
    public int CargoId { get; set; }
    public string NombreCargo { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
