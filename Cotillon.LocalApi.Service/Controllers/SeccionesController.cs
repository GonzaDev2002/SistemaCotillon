using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeccionesController : ControllerBase
{
    private readonly SeccionesService _service;

    public SeccionesController(SeccionesService service)
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
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al obtener secciones: {ex.Message}"));
        }
    }

    [HttpPost]
    public IActionResult Post([FromBody] SeccionDto seccion)
    {
        try
        {
            var result = _service.Crear(seccion);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al guardar sección: {ex.Message}"));
        }
    }

    [HttpPut]
    public IActionResult Put([FromBody] SeccionDto seccion)
    {
        try
        {
            var result = _service.Actualizar(seccion);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno al actualizar sección: {ex.Message}"));
        }
    }
}