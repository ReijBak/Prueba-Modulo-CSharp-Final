using TalentoPlus.Core.Entities;
using TalentoPlus.Core.Interfaces;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation for managing transactions.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly TalentoPlusDbContext _context;
    private IRepository<Empleado>? _empleados;
    private IRepository<Estado>? _estados;
    private IRepository<Departamento>? _departamentos;
    private IRepository<Cargo>? _cargos;
    private IRepository<NivelEducativo>? _nivelesEducativos;

    public UnitOfWork(TalentoPlusDbContext context)
    {
        _context = context;
    }

    public IRepository<Empleado> Empleados => 
        _empleados ??= new EmpleadoRepository(_context);

    public IRepository<Estado> Estados => 
        _estados ??= new Repository<Estado>(_context);

    public IRepository<Departamento> Departamentos => 
        _departamentos ??= new Repository<Departamento>(_context);

    public IRepository<Cargo> Cargos => 
        _cargos ??= new Repository<Cargo>(_context);

    public IRepository<NivelEducativo> NivelesEducativos => 
        _nivelesEducativos ??= new Repository<NivelEducativo>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

