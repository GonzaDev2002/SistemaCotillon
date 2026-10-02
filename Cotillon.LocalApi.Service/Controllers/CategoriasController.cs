using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly CategoriasService _service;

    public CategoriasController(CategoriasService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Get()
    {
        try
        {
            var result = _service.ObtenerTodas();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al obtener categorías: {ex.Message}"));
        }
    }

    [HttpGet("por-seccion/{seccionId}")]
    public IActionResult GetPorSeccion(int seccionId)
    {
        try
        {
            var result = _service.ObtenerPorSeccion(seccionId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al obtener categorías: {ex.Message}"));
        }
    }

    [HttpPost]
    public IActionResult Post([FromBody] CategoriaDto categoria)
    {
        try
        {
            var result = _service.Crear(categoria);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al guardar categoría: {ex.Message}"));
        }
    }

    [HttpPut]
    public IActionResult Put([FromBody] CategoriaDto categoria)
    {
        try
        {
            var result = _service.Actualizar(categoria);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al actualizar categoría: {ex.Message}"));
        }
    }

    [HttpDelete("{categoriaId:int}")]
    public IActionResult Delete(int categoriaId)
    {
        try
        {
            var result = _service.Eliminar(categoriaId);

            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al borrar categoría: {ex.Message}"));
        }
    }
}