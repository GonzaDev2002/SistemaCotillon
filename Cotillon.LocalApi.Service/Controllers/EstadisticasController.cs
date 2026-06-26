using Cotillon.LocalApi.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstadisticasController : ControllerBase
{
    private readonly EstadisticasService _service;

    public EstadisticasController(EstadisticasService service)
    {
        _service = service;
    }

    [HttpGet("ventas-por-dia")]
    public async Task<IActionResult> VentasPorDia([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var resultado = await _service.VentasPorDiaAsync(desde, hasta);
        return Ok(resultado);
    }

    [HttpGet("productos-mas-vendidos")]
    public async Task<IActionResult> ProductosMasVendidos([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var resultado = await _service.ProductosMasVendidosAsync(desde, hasta);
        return Ok(resultado);
    }

    [HttpGet("ventas-por-medio-pago")]
    public async Task<IActionResult> VentasPorMedioPago([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var resultado = await _service.VentasPorMedioPagoAsync(desde, hasta);
        return Ok(resultado);
    }

    [HttpGet("productos-bajo-stock")]
    public async Task<IActionResult> ProductosBajoStock()
    {
        var resultado = await _service.ProductosBajoStockAsync();
        return Ok(resultado);
    }
}