using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Terceros;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProveedoresController : ControllerBase
{
    private readonly ProveedoresService _service;

    public ProveedoresController(ProveedoresService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Buscar([FromQuery] string? texto, [FromQuery] bool incluirInactivos = false) =>
        Ejecutar(() => Ok(_service.Buscar(texto, incluirInactivos)));

    [HttpGet("{proveedorId}")]
    public IActionResult Obtener(int proveedorId) =>
        Ejecutar(() => Responder(_service.ObtenerPorId(proveedorId)));

    [HttpPost]
    public IActionResult Crear([FromBody] ProveedorDto proveedor, [FromQuery] int usuarioId) =>
        Ejecutar(() => Responder(_service.Crear(proveedor, usuarioId)));

    [HttpPut]
    public IActionResult Actualizar([FromBody] ProveedorDto proveedor, [FromQuery] int usuarioId) =>
        Ejecutar(() => Responder(_service.Actualizar(proveedor, usuarioId)));

    [HttpDelete("{proveedorId}")]
    public IActionResult Desactivar(int proveedorId, [FromQuery] int usuarioId) =>
        Ejecutar(() => Responder(_service.Desactivar(proveedorId, usuarioId)));

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
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en proveedores: {ex.Message}"));
        }
    }
}
