using Cotillon.LocalApi.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly ReportesService _service;

    public ReportesController(ReportesService service)
    {
        _service = service;
    }

    [HttpGet("productos")]
    public async Task<IActionResult> Productos()
    {
        var resultado = await _service.ListarProductosAsync();
        return Ok(resultado);
    }

    [HttpGet("productos-bajo-stock")]
    public async Task<IActionResult> ProductosBajoStock()
    {
        var resultado = await _service.ListarProductosBajoStockAsync();
        return Ok(resultado);
    }

    [HttpGet("ventas")]
    public async Task<IActionResult> VentasPorRango([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var resultado = await _service.ListarVentasPorRangoAsync(desde, hasta);
        return Ok(resultado);
    }
}