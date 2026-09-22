using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Caja;
using Cotillon.Shared.Responses;
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

    [HttpGet("actual")]
    public IActionResult Actual() => Ejecutar(() => Ok(_service.ObtenerSesionAbierta()));

    [HttpPost("abrir")]
    public IActionResult Abrir([FromBody] AbrirCajaDto datos) =>
        Ejecutar(() => Responder(_service.Abrir(datos)));

    [HttpPost("cerrar")]
    public IActionResult Cerrar([FromBody] CerrarCajaDto datos) =>
        Ejecutar(() => Responder(_service.Cerrar(datos)));

    [HttpGet("arqueo")]
    public IActionResult ArqueoActual() =>
        Ejecutar(() => Responder(_service.ObtenerArqueoActual()));

    [HttpGet("arqueo/{sesionCajaId}")]
    public IActionResult Arqueo(int sesionCajaId) =>
        Ejecutar(() => Responder(_service.ObtenerArqueo(sesionCajaId)));

    [HttpPost("movimientos")]
    public IActionResult RegistrarMovimiento([FromBody] RegistrarMovimientoCajaDto movimiento) =>
        Ejecutar(() => Responder(_service.RegistrarMovimiento(movimiento)));

    [HttpGet("movimientos/{sesionCajaId}")]
    public IActionResult Movimientos(int sesionCajaId) =>
        Ejecutar(() => Responder(_service.ObtenerMovimientos(sesionCajaId)));

    [HttpGet("historial")]
    public IActionResult Historial([FromQuery] DateTime desde, [FromQuery] DateTime hasta) =>
        Ejecutar(() => Responder(_service.ObtenerHistorial(desde, hasta)));

    private IActionResult Responder<T>(ApiResponse<T> resultado) =>
        resultado.Success ? Ok(resultado) : BadRequest(resultado);

    private IActionResult Ejecutar(Func<IActionResult> accion)
    {
        try
        {
            return accion();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en caja: {ex.Message}"));
        }
    }
}
