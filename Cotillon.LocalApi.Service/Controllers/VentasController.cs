using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Ventas;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly VentasService _service;

    public VentasController(VentasService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Crear([FromBody] CrearVentaDto venta) =>
        Ejecutar(() =>
        {
            var resultado = _service.Crear(venta);

            return resultado.Success
                ? CreatedAtAction(nameof(Obtener), new { ventaId = resultado.Data!.VentaId }, resultado)
                : BadRequest(resultado);
        });

    [HttpGet("{ventaId}")]
    public IActionResult Obtener(int ventaId) =>
        Ejecutar(() => Responder(_service.ObtenerPorId(ventaId)));

    [HttpGet]
    public IActionResult Listar(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] bool incluirAnuladas = false) =>
        Ejecutar(() => Responder(_service.Listar(desde, hasta, incluirAnuladas)));

    [HttpPost("{ventaId}/anular")]
    public IActionResult Anular(int ventaId, [FromBody] AnularVentaDto datos) =>
        Ejecutar(() => Responder(_service.Anular(ventaId, datos)));

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
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en ventas: {ex.Message}"));
        }
    }
}
