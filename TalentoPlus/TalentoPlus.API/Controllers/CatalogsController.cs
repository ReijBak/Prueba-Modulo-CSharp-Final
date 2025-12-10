using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Core.DTOs;
using TalentoPlus.Core.Entities;
using TalentoPlus.Core.Interfaces;

namespace TalentoPlus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CatalogsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public CatalogsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // ==================== ESTADOS ====================
    
    [HttpGet("estados")]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetEstados()
    {
        var items = await _unitOfWork.Estados.GetAllAsync();
        return Ok(items.Select(e => new CatalogItemDto { Id = e.EstadoId, Nombre = e.NombreEstado }));
    }

    [HttpGet("estados/{id}")]
    public async Task<ActionResult<CatalogItemDto>> GetEstado(int id)
    {
        var item = await _unitOfWork.Estados.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Estado no encontrado." });
        return Ok(new CatalogItemDto { Id = item.EstadoId, Nombre = item.NombreEstado });
    }

    [HttpPost("estados")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogItemDto>> CreateEstado([FromBody] CreateCatalogItemDto dto)
    {
        var item = new Estado { NombreEstado = dto.Nombre };
        await _unitOfWork.Estados.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEstado), new { id = item.EstadoId }, 
            new CatalogItemDto { Id = item.EstadoId, Nombre = item.NombreEstado });
    }

    [HttpPut("estados/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogItemDto>> UpdateEstado(int id, [FromBody] CreateCatalogItemDto dto)
    {
        var item = await _unitOfWork.Estados.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Estado no encontrado." });
        item.NombreEstado = dto.Nombre;
        await _unitOfWork.Estados.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new CatalogItemDto { Id = item.EstadoId, Nombre = item.NombreEstado });
    }

    [HttpDelete("estados/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteEstado(int id)
    {
        var item = await _unitOfWork.Estados.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Estado no encontrado." });
        await _unitOfWork.Estados.DeleteAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    // ==================== DEPARTAMENTOS ====================
    
    [HttpGet("departamentos")]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetDepartamentos()
    {
        var items = await _unitOfWork.Departamentos.GetAllAsync();
        return Ok(items.Select(d => new CatalogItemDto { Id = d.DepartamentoId, Nombre = d.NombreDepartamento }));
    }

    [HttpGet("departamentos/{id}")]
    public async Task<ActionResult<CatalogItemDto>> GetDepartamento(int id)
    {
        var item = await _unitOfWork.Departamentos.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Departamento no encontrado." });
        return Ok(new CatalogItemDto { Id = item.DepartamentoId, Nombre = item.NombreDepartamento });
    }

    [HttpPost("departamentos")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogItemDto>> CreateDepartamento([FromBody] CreateCatalogItemDto dto)
    {
        var item = new Departamento { NombreDepartamento = dto.Nombre };
        await _unitOfWork.Departamentos.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDepartamento), new { id = item.DepartamentoId }, 
            new CatalogItemDto { Id = item.DepartamentoId, Nombre = item.NombreDepartamento });
    }

    [HttpPut("departamentos/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogItemDto>> UpdateDepartamento(int id, [FromBody] CreateCatalogItemDto dto)
    {
        var item = await _unitOfWork.Departamentos.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Departamento no encontrado." });
        item.NombreDepartamento = dto.Nombre;
        await _unitOfWork.Departamentos.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new CatalogItemDto { Id = item.DepartamentoId, Nombre = item.NombreDepartamento });
    }

    [HttpDelete("departamentos/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteDepartamento(int id)
    {
        var item = await _unitOfWork.Departamentos.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Departamento no encontrado." });
        await _unitOfWork.Departamentos.DeleteAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    // ==================== CARGOS ====================
    
    [HttpGet("cargos")]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetCargos()
    {
        var items = await _unitOfWork.Cargos.GetAllAsync();
        return Ok(items.Select(c => new CatalogItemDto { Id = c.CargoId, Nombre = c.NombreCargo }));
    }

    [HttpGet("cargos/{id}")]
    public async Task<ActionResult<CatalogItemDto>> GetCargo(int id)
    {
        var item = await _unitOfWork.Cargos.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Cargo no encontrado." });
        return Ok(new CatalogItemDto { Id = item.CargoId, Nombre = item.NombreCargo });
    }

    [HttpPost("cargos")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogItemDto>> CreateCargo([FromBody] CreateCatalogItemDto dto)
    {
        var item = new Cargo { NombreCargo = dto.Nombre };
        await _unitOfWork.Cargos.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetCargo), new { id = item.CargoId }, 
            new CatalogItemDto { Id = item.CargoId, Nombre = item.NombreCargo });
    }

    [HttpPut("cargos/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogItemDto>> UpdateCargo(int id, [FromBody] CreateCatalogItemDto dto)
    {
        var item = await _unitOfWork.Cargos.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Cargo no encontrado." });
        item.NombreCargo = dto.Nombre;
        await _unitOfWork.Cargos.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new CatalogItemDto { Id = item.CargoId, Nombre = item.NombreCargo });
    }

    [HttpDelete("cargos/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteCargo(int id)
    {
        var item = await _unitOfWork.Cargos.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Cargo no encontrado." });
        await _unitOfWork.Cargos.DeleteAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }

    // ==================== NIVELES EDUCATIVOS ====================
    
    [HttpGet("niveles-educativos")]
    public async Task<ActionResult<IEnumerable<CatalogItemDto>>> GetNivelesEducativos()
    {
        var items = await _unitOfWork.NivelesEducativos.GetAllAsync();
        return Ok(items.Select(n => new CatalogItemDto { Id = n.NivelEducativoId, Nombre = n.NombreNivel }));
    }

    [HttpGet("niveles-educativos/{id}")]
    public async Task<ActionResult<CatalogItemDto>> GetNivelEducativo(int id)
    {
        var item = await _unitOfWork.NivelesEducativos.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Nivel educativo no encontrado." });
        return Ok(new CatalogItemDto { Id = item.NivelEducativoId, Nombre = item.NombreNivel });
    }

    [HttpPost("niveles-educativos")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogItemDto>> CreateNivelEducativo([FromBody] CreateCatalogItemDto dto)
    {
        var item = new NivelEducativo { NombreNivel = dto.Nombre };
        await _unitOfWork.NivelesEducativos.AddAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return CreatedAtAction(nameof(GetNivelEducativo), new { id = item.NivelEducativoId }, 
            new CatalogItemDto { Id = item.NivelEducativoId, Nombre = item.NombreNivel });
    }

    [HttpPut("niveles-educativos/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CatalogItemDto>> UpdateNivelEducativo(int id, [FromBody] CreateCatalogItemDto dto)
    {
        var item = await _unitOfWork.NivelesEducativos.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Nivel educativo no encontrado." });
        item.NombreNivel = dto.Nombre;
        await _unitOfWork.NivelesEducativos.UpdateAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return Ok(new CatalogItemDto { Id = item.NivelEducativoId, Nombre = item.NombreNivel });
    }

    [HttpDelete("niveles-educativos/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteNivelEducativo(int id)
    {
        var item = await _unitOfWork.NivelesEducativos.GetByIdAsync(id);
        if (item == null) return NotFound(new { message = "Nivel educativo no encontrado." });
        await _unitOfWork.NivelesEducativos.DeleteAsync(item);
        await _unitOfWork.SaveChangesAsync();
        return NoContent();
    }
}

