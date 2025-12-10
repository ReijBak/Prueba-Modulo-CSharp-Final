namespace TalentoPlus.Core.Entities;

/// <summary>
/// Represents a department within the organization.
/// </summary>
public class Departamento
{
    public int DepartamentoId { get; set; }
    public string NombreDepartamento { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
