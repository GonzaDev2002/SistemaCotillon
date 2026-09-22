using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Terceros;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly ClientesService _service;

    public ClientesController(ClientesService service)
    {
        _service = service;
    }

    [HttpGet]
    public IActionResult Buscar([FromQuery] string? texto, [FromQuery] bool incluirInactivos = false) =>
        Ejecutar(() => Ok(_service.Buscar(texto, incluirInactivos)));

    [HttpGet("{clienteId}")]
    public IActionResult Obtener(int clienteId) =>
        Ejecutar(() => Responder(_service.ObtenerPorId(clienteId)));

    [HttpPost]
    public IActionResult Crear([FromBody] ClienteDto cliente, [FromQuery] int usuarioId) =>
        Ejecutar(() => Responder(_service.Crear(cliente, usuarioId)));

    [HttpPut]
    public IActionResult Actualizar([FromBody] ClienteDto cliente, [FromQuery] int usuarioId) =>
        Ejecutar(() => Responder(_service.Actualizar(cliente, usuarioId)));

    [HttpDelete("{clienteId}")]
    public IActionResult Desactivar(int clienteId, [FromQuery] int usuarioId) =>
        Ejecutar(() => Responder(_service.Desactivar(clienteId, usuarioId)));

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
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en clientes: {ex.Message}"));
        }
    }
}
