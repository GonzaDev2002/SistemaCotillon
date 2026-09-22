using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Compras;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ComprasController : ControllerBase
{
    private readonly ComprasService _service;

    public ComprasController(ComprasService service)
    {
        _service = service;
    }

    [HttpPost]
    public IActionResult Crear([FromBody] CrearCompraDto compra) =>
        Ejecutar(() => Responder(_service.Crear(compra)));

    [HttpGet("{compraId}")]
    public IActionResult Obtener(int compraId) =>
        Ejecutar(() => Responder(_service.ObtenerPorId(compraId)));

    [HttpGet]
    public IActionResult Listar(
        [FromQuery] DateTime desde,
        [FromQuery] DateTime hasta,
        [FromQuery] int? proveedorId = null) =>
        Ejecutar(() => Responder(_service.Listar(desde, hasta, proveedorId)));

    [HttpPost("{compraId}/recibir")]
    public IActionResult Recibir(int compraId, [FromBody] RecibirCompraDto datos) =>
        Ejecutar(() => Responder(_service.Recibir(compraId, datos)));

    [HttpPost("{compraId}/anular")]
    public IActionResult Anular(int compraId, [FromQuery] int usuarioId, [FromQuery] string motivo) =>
        Ejecutar(() => Responder(_service.Anular(compraId, usuarioId, motivo)));

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
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en compras: {ex.Message}"));
        }
    }
}
