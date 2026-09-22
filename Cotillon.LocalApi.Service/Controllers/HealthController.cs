using Cotillon.Data;
using Cotillon.Shared.Responses;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IDbConnectionFactory _factory;

    public HealthController(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    [HttpGet("ping")]
    public IActionResult Ping() =>
        Ok(ApiResponse<string>.Ok("pong", "API funcionando correctamente"));

    [HttpGet("db")]
    public IActionResult CheckDatabase()
    {
        try
        {
            using var conexion = _factory.Crear();

            var version = conexion.ExecuteScalar<long>("PRAGMA user_version;");
            var integridad = conexion.ExecuteScalar<string>("PRAGMA integrity_check;");

            if (integridad != "ok")
                return StatusCode(500, ApiResponse<string>.Fail($"La base tiene problemas de integridad: {integridad}"));

            return Ok(ApiResponse<string>.Ok(
                $"Esquema v{version}", $"Base SQLite conectada correctamente en {_factory.RutaBase}"));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error al conectar con la base: {ex.Message}"));
        }
    }
}
