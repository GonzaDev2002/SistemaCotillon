using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Caja;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CajaController : ControllerBase
{
    private readonly CajaService _service;

    public CajaController(CajaService service)
    {
        _service = service;
    }

    [HttpGet("abierta/{usuarioId}")]
    public async Task<IActionResult> ObtenerCajaAbierta(int usuarioId)
    {
        var caja = await _service.ObtenerCajaAbiertaAsync(usuarioId);

        if (caja == null)
            return NotFound("No hay caja abierta.");

        return Ok(caja);
    }

    [HttpPost("abrir")]
    public async Task<IActionResult> AbrirCaja([FromBody] AbrirCajaRequestDto request)
    {
        var sesionCajaId = await _service.AbrirCajaAsync(request);

        return Ok(new
        {
            success = true,
            message = "Caja abierta correctamente.",
            sesionCajaId
        });
    }
    [HttpPost("cerrar")]
    public async Task<IActionResult> CerrarCaja([FromBody] CerrarCajaRequestDto request)
    {
        if (request.SesionCajaId <= 0)
            return BadRequest("La sesión de caja no es válida.");

        await _service.CerrarCajaAsync(request);

        return Ok(new
        {
            success = true,
            message = "Caja cerrada correctamente."
        });
    }
}