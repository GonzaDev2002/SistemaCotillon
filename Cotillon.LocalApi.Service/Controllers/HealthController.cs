using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly AccessDb _accessDb;

    public HealthController(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(ApiResponse<string>.Ok("pong", "API funcionando correctamente"));
    }

    [HttpGet("db")]
    public IActionResult CheckDatabase()
    {
        try
        {
            using var connection = _accessDb.CreateConnection();
            connection.Open();

            return Ok(ApiResponse<string>.Ok("Conexión correcta", "Base Access conectada correctamente"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<string>.Fail($"Error al conectar con Access: {ex.Message}"));
        }
    }
}