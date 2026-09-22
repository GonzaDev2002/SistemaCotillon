using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.Responses;
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
    public IActionResult Productos()
    {
        try
        {
            return Ok(_service.ListarProductos());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al generar el reporte: {ex.Message}"));
        }
    }

    [HttpGet("productos-bajo-stock")]
    public IActionResult ProductosBajoStock()
    {
        try
        {
            return Ok(_service.ListarProductosBajoStock());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al generar el reporte: {ex.Message}"));
        }
    }

    [HttpGet("ventas")]
    public IActionResult VentasPorRango([FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        try
        {
            var resultado = _service.ListarVentasPorRango(desde, hasta);

            if (!resultado.Success)
                return BadRequest(resultado);

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al generar el reporte: {ex.Message}"));
        }
    }
}
