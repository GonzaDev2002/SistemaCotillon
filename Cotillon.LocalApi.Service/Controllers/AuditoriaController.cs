using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditoriaController : ControllerBase
{
    private const int LimiteMaximo = 1000;

    private readonly AuditoriaRepository _repository;

    public AuditoriaController(AuditoriaRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Buscar(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int? usuarioId = null,
        [FromQuery] string? tabla = null,
        [FromQuery] string? accion = null,
        [FromQuery] int limite = 200)
    {
        try
        {
            if (desde.Date > hasta.Date)
                return BadRequest(ApiResponse<string>.Fail("La fecha desde no puede ser mayor a la fecha hasta."));

            var eventos = _repository.Buscar(
                desde, hasta, usuarioId, tabla, accion, Math.Clamp(limite, 1, LimiteMaximo));

            return Ok(ApiResponse<object>.Ok(eventos, "Eventos obtenidos correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en auditoría: {ex.Message}"));
        }
    }
}
