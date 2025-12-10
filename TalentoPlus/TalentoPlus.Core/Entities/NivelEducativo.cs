namespace TalentoPlus.Core.Entities;

/// <summary>
/// Represents an educational level (e.g., Bachelor's, Master's).
/// </summary>
public class NivelEducativo
{
    public int NivelEducativoId { get; set; }
    public string NombreNivel { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
