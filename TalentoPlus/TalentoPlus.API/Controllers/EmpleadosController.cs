using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Core.DTOs;
using TalentoPlus.Core.Entities;
using TalentoPlus.Core.Interfaces;
using TalentoPlus.Infrastructure.Repositories;

namespace TalentoPlus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmpleadosController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExcelImportService _excelImportService;
    private readonly IPdfService _pdfService;
    private readonly IEmailService _emailService;
    private readonly ILogger<EmpleadosController> _logger;

    public EmpleadosController(
        IUnitOfWork unitOfWork,
        IExcelImportService excelImportService,
        IPdfService pdfService,
        IEmailService emailService,
        ILogger<EmpleadosController> logger)
    {
        _unitOfWork = unitOfWork;
        _excelImportService = excelImportService;
        _pdfService = pdfService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all employees (Admin) or current employee data (Employee).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmpleadoDto>>> GetAll()
    {
        var empleadoRepo = _unitOfWork.Empleados as EmpleadoRepository;
        
        // Check if user is Admin
        var isAdmin = User.IsInRole("Admin");
        
        if (isAdmin)
        {
            // Admin can see all employees
            var empleados = await empleadoRepo!.GetAllWithDetailsAsync();
            var dtos = empleados.Select(MapToDto).ToList();
            return Ok(dtos);
        }
        else
        {
            // Employee can only see their own data
            var documentoClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(documentoClaim) || !long.TryParse(documentoClaim, out var documento))
            {
                return Forbid();
            }
            
            var empleado = await empleadoRepo!.GetWithDetailsAsync(documento);
            
            if (empleado == null)
            {
                return Ok(new List<EmpleadoDto>());
            }
            
            return Ok(new List<EmpleadoDto> { MapToDto(empleado) });
        }
    }

    /// <summary>
    /// Gets an employee by document ID.
    /// Admin can view any employee, Employee can only view themselves.
    /// </summary>
    [HttpGet("{documento}")]
    public async Task<ActionResult<EmpleadoDto>> GetById(long documento)
    {
        // Check if user is Admin or if they're requesting their own data
        var isAdmin = User.IsInRole("Admin");
        
        if (!isAdmin)
        {
            var documentoClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            if (string.IsNullOrEmpty(documentoClaim) || !long.TryParse(documentoClaim, out var userDocumento))
            {
                return Forbid();
            }
            
            // Employee can only see their own data
            if (userDocumento != documento)
            {
                return Forbid();
            }
        }
        
        var empleadoRepo = _unitOfWork.Empleados as EmpleadoRepository;
        var empleado = await empleadoRepo!.GetWithDetailsAsync(documento);
        
        if (empleado == null)
            return NotFound(new { message = "Empleado no encontrado." });
        
        return Ok(MapToDto(empleado));
    }

    /// <summary>
    /// Creates a new employee.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmpleadoDto>> Create([FromBody] CreateEmpleadoDto dto)
    {
        // Check if employee already exists
        if (await _unitOfWork.Empleados.ExistsAsync(e => e.Documento == dto.Documento))
        {
            return BadRequest(new { message = "Ya existe un empleado con este documento." });
        }

        // Check if email is already used
        if (!string.IsNullOrEmpty(dto.Email))
        {
            if (await _unitOfWork.Empleados.ExistsAsync(e => e.Email == dto.Email))
            {
                return BadRequest(new { message = "El correo electrónico ya está en uso." });
            }
        }

        var empleado = new Empleado
        {
            Documento = dto.Documento,
            Nombres = dto.Nombres,
            Apellidos = dto.Apellidos,
            FechaNacimiento = dto.FechaNacimiento,
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Salario = dto.Salario,
            FechaIngreso = dto.FechaIngreso,
            PerfilProfesional = dto.PerfilProfesional,
            EstadoId = dto.EstadoId,
            NivelEducativoId = dto.NivelEducativoId,
            DepartamentoId = dto.DepartamentoId,
            CargoId = dto.CargoId
        };

        // Hash password if provided, otherwise use documento as default
        string plainPassword;
        if (!string.IsNullOrEmpty(dto.Password))
        {
            plainPassword = dto.Password;
        }
        else
        {
            plainPassword = dto.Documento.ToString();
        }
        
        var passwordHasher = new PasswordHasher<object>();
        empleado.PasswordHash = passwordHasher.HashPassword(null!, plainPassword);

        await _unitOfWork.Empleados.AddAsync(empleado);
        await _unitOfWork.SaveChangesAsync();

        // Send welcome email with credentials if employee has email
        if (!string.IsNullOrEmpty(empleado.Email))
        {
            try
            {
                var fullName = $"{empleado.Nombres} {empleado.Apellidos}";
                await _emailService.SendWelcomeEmailWithCredentialsAsync(
                    empleado.Email, 
                    fullName, 
                    empleado.Documento, 
                    plainPassword
                );
                _logger.LogInformation("Welcome email with credentials sent to {Email}", empleado.Email);
            }
            catch (Exception ex)
            {
                // Log error but don't fail the request
                _logger.LogError(ex, "Failed to send welcome email to {Email}", empleado.Email);
            }
        }

        // Reload with details
        var empleadoRepo = _unitOfWork.Empleados as EmpleadoRepository;
        var createdEmpleado = await empleadoRepo!.GetWithDetailsAsync(empleado.Documento);

        return CreatedAtAction(nameof(GetById), new { documento = empleado.Documento }, MapToDto(createdEmpleado!));
    }

    /// <summary>
    /// Updates an existing employee.
    /// </summary>
    [HttpPut("{documento}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<EmpleadoDto>> Update(long documento, [FromBody] UpdateEmpleadoDto dto)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(documento);
        
        if (empleado == null)
            return NotFound(new { message = "Empleado no encontrado." });

        // Check if email is already used by another employee
        if (!string.IsNullOrEmpty(dto.Email))
        {
            if (await _unitOfWork.Empleados.ExistsAsync(e => e.Email == dto.Email && e.Documento != documento))
            {
                return BadRequest(new { message = "El correo electrónico ya está en uso." });
            }
        }

        empleado.Nombres = dto.Nombres;
        empleado.Apellidos = dto.Apellidos;
        empleado.FechaNacimiento = dto.FechaNacimiento;
        empleado.Direccion = dto.Direccion;
        empleado.Telefono = dto.Telefono;
        empleado.Email = dto.Email;
        empleado.Salario = dto.Salario;
        empleado.FechaIngreso = dto.FechaIngreso;
        empleado.PerfilProfesional = dto.PerfilProfesional;
        empleado.EstadoId = dto.EstadoId;
        empleado.NivelEducativoId = dto.NivelEducativoId;
        empleado.DepartamentoId = dto.DepartamentoId;
        empleado.CargoId = dto.CargoId;

        await _unitOfWork.Empleados.UpdateAsync(empleado);
        await _unitOfWork.SaveChangesAsync();

        // Reload with details
        var empleadoRepo = _unitOfWork.Empleados as EmpleadoRepository;
        var updatedEmpleado = await empleadoRepo!.GetWithDetailsAsync(documento);

        return Ok(MapToDto(updatedEmpleado!));
    }

    /// <summary>
    /// Deletes an employee.
    /// </summary>
    [HttpDelete("{documento}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> Delete(long documento)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(documento);
        
        if (empleado == null)
            return NotFound(new { message = "Empleado no encontrado." });

        await _unitOfWork.Empleados.DeleteAsync(empleado);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Imports employees from an Excel file.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ExcelImportResultDto>> ImportFromExcel(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new ExcelImportResultDto
            {
                Success = false,
                Message = "Por favor, seleccione un archivo."
            });
        }

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new ExcelImportResultDto
            {
                Success = false,
                Message = "Solo se permiten archivos .xlsx"
            });
        }

        using var stream = file.OpenReadStream();
        var result = await _excelImportService.ImportEmployeesAsync(stream);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    /// <summary>
    /// Generates a PDF resume for an employee.
    /// Admin can generate for any employee, Employee can only generate their own.
    /// </summary>
    [HttpGet("{documento}/resume")]
    public async Task<IActionResult> GenerateResume(long documento)
    {
        try
        {
            // Check if user is Admin or if they're requesting their own resume
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin)
            {
                var documentoClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                
                if (string.IsNullOrEmpty(documentoClaim) || !long.TryParse(documentoClaim, out var userDocumento))
                {
                    return Forbid();
                }
                
                // Employee can only download their own resume
                if (userDocumento != documento)
                {
                    return Forbid();
                }
            }
            
            var pdfBytes = await _pdfService.GenerateEmployeeResumeAsync(documento);
            return File(pdfBytes, "application/pdf", $"HojaDeVida_{documento}.pdf");
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { message = "Empleado no encontrado." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF for employee {Documento}", documento);
            return StatusCode(500, new { message = "Error al generar el PDF." });
        }
    }

    /// <summary>
    /// Regenerates default passwords for all employees without password.
    /// Admin only endpoint.
    /// </summary>
    [HttpPost("regenerate-passwords")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> RegeneratePasswords()
    {
        try
        {
            var empleados = await _unitOfWork.Empleados.GetAllAsync();
            var passwordHasher = new PasswordHasher<object>();
            var count = 0;

            foreach (var empleado in empleados)
            {
                if (string.IsNullOrEmpty(empleado.PasswordHash))
                {
                    // Set default password as the document number
                    empleado.PasswordHash = passwordHasher.HashPassword(null!, empleado.Documento.ToString());
                    await _unitOfWork.Empleados.UpdateAsync(empleado);
                    count++;
                }
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(new { 
                success = true, 
                message = $"Se regeneraron las contraseñas de {count} empleado(s). La contraseña por defecto es el número de documento." 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error regenerating passwords");
            return StatusCode(500, new { success = false, message = "Error al regenerar las contraseñas." });
        }
    }

    private static EmpleadoDto MapToDto(Empleado empleado)
    {
        return new EmpleadoDto
        {
            Documento = empleado.Documento,
            Nombres = empleado.Nombres,
            Apellidos = empleado.Apellidos,
            FechaNacimiento = empleado.FechaNacimiento,
            Direccion = empleado.Direccion,
            Telefono = empleado.Telefono,
            Email = empleado.Email,
            Salario = empleado.Salario,
            FechaIngreso = empleado.FechaIngreso,
            PerfilProfesional = empleado.PerfilProfesional,
            EstadoId = empleado.EstadoId,
            NombreEstado = empleado.Estado?.NombreEstado,
            NivelEducativoId = empleado.NivelEducativoId,
            NombreNivelEducativo = empleado.NivelEducativo?.NombreNivel,
            DepartamentoId = empleado.DepartamentoId,
            NombreDepartamento = empleado.Departamento?.NombreDepartamento,
            CargoId = empleado.CargoId,
            NombreCargo = empleado.Cargo?.NombreCargo
        };
    }
}

