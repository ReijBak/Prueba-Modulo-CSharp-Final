using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentoPlus.Core.Interfaces;

namespace TalentoPlus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<TestController> _logger;

    public TestController(IEmailService emailService, ILogger<TestController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Test endpoint to verify email service is working.
    /// </summary>
    [HttpPost("send-test-email")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SendTestEmail([FromBody] TestEmailDto dto)
    {
        try
        {
            _logger.LogInformation("Attempting to send test email to {Email}", dto.Email);
            
            await _emailService.SendWelcomeEmailWithCredentialsAsync(
                dto.Email,
                "Usuario de Prueba",
                12345678,
                "password123"
            );

            return Ok(new { success = true, message = $"Email enviado exitosamente a {dto.Email}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test email to {Email}", dto.Email);
            return StatusCode(500, new { 
                success = false, 
                message = "Error al enviar el email", 
                error = ex.Message,
                innerError = ex.InnerException?.Message 
            });
        }
    }
}

public class TestEmailDto
{
    public string Email { get; set; } = string.Empty;
}

