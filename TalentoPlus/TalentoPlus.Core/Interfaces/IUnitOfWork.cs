using TalentoPlus.Core.Entities;

namespace TalentoPlus.Core.Interfaces;

/// <summary>
/// Unit of Work interface for managing transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Empleado> Empleados { get; }
    IRepository<Estado> Estados { get; }
    IRepository<Departamento> Departamentos { get; }
    IRepository<Cargo> Cargos { get; }
    IRepository<NivelEducativo> NivelesEducativos { get; }
    
    Task<int> SaveChangesAsync();
}

