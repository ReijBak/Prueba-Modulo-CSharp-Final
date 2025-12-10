using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TalentoPlus.Core.DTOs;
using TalentoPlus.Core.Interfaces;
using TalentoPlus.Infrastructure.Data;
using TalentoPlus.Infrastructure.Repositories;

namespace TalentoPlus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    /// <summary>
    /// Registers a new administrator.
    /// </summary>
    [HttpPost("admin/register")]
    public async Task<ActionResult<AuthResponseDto>> RegisterAdmin([FromBody] AdminRegisterDto dto)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = "El correo electrónico ya está registrado."
            });
        }

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new AuthResponseDto
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            });
        }

        await _userManager.AddToRoleAsync(user, "Admin");

        var token = GenerateJwtToken(user.Id, user.Email!, user.FullName, "Admin");

        return Ok(new AuthResponseDto
        {
            Success = true,
            Token = token,
            UserType = "Admin",
            FullName = user.FullName,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Message = "Registro exitoso."
        });
    }

    /// <summary>
    /// Authenticates an administrator.
    /// </summary>
    [HttpPost("admin/login")]
    public async Task<ActionResult<AuthResponseDto>> AdminLogin([FromBody] AdminLoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Credenciales inválidas."
            });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Credenciales inválidas."
            });
        }

        var token = GenerateJwtToken(user.Id, user.Email!, user.FullName, "Admin");

        return Ok(new AuthResponseDto
        {
            Success = true,
            Token = token,
            UserType = "Admin",
            FullName = user.FullName,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Message = "Inicio de sesión exitoso."
        });
    }

    /// <summary>
    /// Authenticates an employee using document and password.
    /// </summary>
    [HttpPost("employee-login")]
    public async Task<ActionResult<AuthResponseDto>> EmployeeLogin([FromBody] EmployeeLoginDto dto)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(dto.Documento);
        
        if (empleado == null)
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Credenciales inválidas."
            });
        }

        // Verify password hash
        if (string.IsNullOrEmpty(empleado.PasswordHash))
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "El empleado no tiene una contraseña configurada."
            });
        }

        var passwordHasher = new PasswordHasher<object>();
        var verificationResult = passwordHasher.VerifyHashedPassword(null!, empleado.PasswordHash, dto.Password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new AuthResponseDto
            {
                Success = false,
                Message = "Credenciales inválidas."
            });
        }

        var fullName = $"{empleado.Nombres} {empleado.Apellidos}";
        var token = GenerateJwtToken(empleado.Documento.ToString(), empleado.Email ?? "", fullName, "Employee");

        return Ok(new AuthResponseDto
        {
            Success = true,
            Token = token,
            UserType = "Employee",
            FullName = fullName,
            Documento = empleado.Documento,
            ExpiresAt = DateTime.UtcNow.AddHours(24),
            Message = "Inicio de sesión exitoso."
        });
    }

    private string GenerateJwtToken(string userId, string email, string fullName, string role)
    {
        var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
            ?? _configuration["Jwt:Secret"] 
            ?? throw new InvalidOperationException("JWT_SECRET is not configured");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, fullName),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["Jwt:Issuer"] ?? "TalentoPlus",
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["Jwt:Audience"] ?? "TalentoPlus",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

