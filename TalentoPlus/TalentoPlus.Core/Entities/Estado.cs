namespace TalentoPlus.Core.Entities;

/// <summary>
/// Represents the status of an employee (e.g., Active, Inactive).
/// </summary>
public class Estado
{
    public int EstadoId { get; set; }
    public string NombreEstado { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
