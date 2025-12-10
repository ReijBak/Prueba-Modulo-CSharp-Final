namespace TalentoPlus.Core.DTOs;

/// <summary>
/// DTO for catalog items (Estado, Departamento, Cargo, NivelEducativo).
/// </summary>
public class CatalogItemDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

/// <summary>
/// DTO for creating a catalog item.
/// </summary>
public class CreateCatalogItemDto
{
    public string Nombre { get; set; } = string.Empty;
}

