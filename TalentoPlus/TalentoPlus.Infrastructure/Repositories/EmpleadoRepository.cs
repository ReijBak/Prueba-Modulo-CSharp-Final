using Microsoft.EntityFrameworkCore;
using TalentoPlus.Core.Entities;
using TalentoPlus.Infrastructure.Data;

namespace TalentoPlus.Infrastructure.Repositories;

/// <summary>
/// Specialized repository for Employee operations.
/// </summary>
public class EmpleadoRepository : Repository<Empleado>
{
    public EmpleadoRepository(TalentoPlusDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Gets an employee with all related entities.
    /// </summary>
    public async Task<Empleado?> GetWithDetailsAsync(long documento)
    {
        return await _dbSet
            .Include(e => e.Estado)
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .Include(e => e.NivelEducativo)
            .FirstOrDefaultAsync(e => e.Documento == documento);
    }

    /// <summary>
    /// Gets all employees with related entities.
    /// </summary>
    public async Task<IEnumerable<Empleado>> GetAllWithDetailsAsync()
    {
        return await _dbSet
            .Include(e => e.Estado)
            .Include(e => e.Departamento)
            .Include(e => e.Cargo)
            .Include(e => e.NivelEducativo)
            .ToListAsync();
    }

    /// <summary>
    /// Gets an employee by email.
    /// </summary>
    public async Task<Empleado?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(e => e.Email == email);
    }
}

