using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ProductosService _productosService;

    public DashboardController(ProductosService productosService)
    {
        _productosService = productosService;
    }

    [HttpGet("resumen")]
    public IActionResult GetResumen()
    {
        try
        {
            var result = _productosService.ObtenerResumenDashboard();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al obtener resumen: {ex.Message}"));
        }
    }

    [HttpGet("bajo-stock")]
    public IActionResult GetBajoStock()
    {
        try
        {
            var result = _productosService.ObtenerProductosBajoStock();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al obtener productos bajo stock: {ex.Message}"));
        }
    }

    [HttpGet("ultimos-productos")]
    public IActionResult GetUltimosProductos()
    {
        try
        {
            var result = _productosService.ObtenerUltimosProductos();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al obtener últimos productos: {ex.Message}"));
        }
    }
}