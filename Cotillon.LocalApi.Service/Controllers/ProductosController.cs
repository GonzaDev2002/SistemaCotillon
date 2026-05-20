using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly ProductosService _service;

    public ProductosController(ProductosService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Post([FromBody] ProductoDto producto)
    {
        try
        {
            var result = _service.Crear(producto);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al guardar producto: {ex.Message}"));
        }
    }

    [HttpGet("todos")]
    public IActionResult GetTodos([FromQuery] bool incluirInactivos = false)
    {
        try
        {
            var result = _service.ObtenerTodos(incluirInactivos);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al obtener productos: {ex.Message}"));
        }
    }

    [HttpDelete("{productoId}")]
    public IActionResult Delete(int productoId)
    {
        try
        {
            var result = _service.Eliminar(productoId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al eliminar producto: {ex.Message}"));
        }
    }
}