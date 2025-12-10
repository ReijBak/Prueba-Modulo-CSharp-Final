using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TalentoPlus.Core.DTOs;
using TalentoPlus.Core.Interfaces;

namespace TalentoPlus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IAIService aiService, ILogger<DashboardController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    /// <summary>
    /// Queries the database using natural language via AI.
    /// </summary>
    [HttpPost("query")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<DashboardQueryResponseDto>> Query([FromBody] DashboardQueryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Question))
        {
            return BadRequest(new DashboardQueryResponseDto
            {
                Success = false,
                Message = "La pregunta no puede estar vacía."
            });
        }

        try
        {
            var resultJson = await _aiService.QueryDatabaseAsync(dto.Question);
            var result = JsonSerializer.Deserialize<Dictionary<string, object>>(resultJson);

            if (result == null)
            {
                return StatusCode(500, new DashboardQueryResponseDto
                {
                    Success = false,
                    Message = "Error al procesar la respuesta."
                });
            }

            if (result.TryGetValue("error", out var error))
            {
                return BadRequest(new DashboardQueryResponseDto
                {
                    Success = false,
                    Message = error?.ToString()
                });
            }

            return Ok(new DashboardQueryResponseDto
            {
                Success = true,
                Query = result.TryGetValue("query", out var query) ? query?.ToString() : null,
                Result = result.TryGetValue("result", out var queryResult) ? queryResult : null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing dashboard query: {Question}", dto.Question);
            return StatusCode(500, new DashboardQueryResponseDto
            {
                Success = false,
                Message = "Error al procesar la consulta. Por favor, intente nuevamente."
            });
        }
    }
}

