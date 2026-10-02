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

    [HttpGet("buscar")]
    public IActionResult Buscar([FromQuery] string? texto)
    {
        try
        {
            return Ok(_service.Buscar(texto));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al buscar productos: {ex.Message}"));
        }
    }

    /// <summary>
    /// Producto por código exacto: lo usa el lector de código de barras en el alta.
    /// </summary>
    [HttpGet("por-codigo/{codigo}")]
    public IActionResult ObtenerPorCodigo(string codigo)
    {
        try
        {
            return Ok(_service.ObtenerPorCodigo(codigo));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al buscar el código: {ex.Message}"));
        }
    }

    [HttpGet("{productoId:int}")]
    public IActionResult Obtener(int productoId)
    {
        try
        {
            var result = _service.ObtenerPorId(productoId);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al obtener producto: {ex.Message}"));
        }
    }

    [HttpPut]
    public IActionResult Put([FromBody] ProductoDto producto)
    {
        try
        {
            var result = _service.Actualizar(producto);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al actualizar producto: {ex.Message}"));
        }
    }

    [HttpPost("{productoId:int}/reactivar")]
    public IActionResult Reactivar(int productoId)
    {
        try
        {
            var result = _service.Reactivar(productoId);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al reactivar producto: {ex.Message}"));
        }
    }

    [HttpDelete("{productoId:int}")]
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