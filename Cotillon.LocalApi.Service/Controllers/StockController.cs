using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Stock;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly StockService _service;

    public StockController(StockService service)
    {
        _service = service;
    }

    [HttpPost("ajustar")]
    public IActionResult Ajustar([FromBody] AjustarStockDto ajuste) =>
        Ejecutar(() => Responder(_service.Ajustar(ajuste)));

    [HttpGet("movimientos")]
    public IActionResult Movimientos(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int? productoId = null) =>
        Ejecutar(() => Responder(_service.ObtenerMovimientos(productoId, desde, hasta)));

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
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en stock: {ex.Message}"));
        }
    }
}
