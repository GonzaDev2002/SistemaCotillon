using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Adjuntos;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdjuntosController : ControllerBase
{
    private readonly AdjuntosService _service;

    public AdjuntosController(AdjuntosService service)
    {
        _service = service;
    }

    [HttpGet("{entidadTipo}/{entidadId:int}")]
    public IActionResult Listar(string entidadTipo, int entidadId) =>
        Ejecutar(() => Responder(_service.Listar(entidadTipo, entidadId)));

    /// <summary>
    /// Sube un archivo. El límite propio es menor al de la validación del servicio
    /// para cortar la subida antes de escribir 100 MB en disco y recién después
    /// rechazarlos.
    /// </summary>
    [HttpPost("{entidadTipo}/{entidadId:int}")]
    [RequestSizeLimit(12 * 1024 * 1024)]
    public IActionResult Subir(
        string entidadTipo,
        int entidadId,
        [FromForm] IFormFile? archivo,
        [FromForm] string? descripcion,
        [FromQuery] int usuarioId) =>
        Ejecutar(() =>
        {
            if (archivo is null || archivo.Length == 0)
                return BadRequest(ApiResponse<AdjuntoDto>.Fail("No llegó ningún archivo."));

            using var contenido = archivo.OpenReadStream();

            return Responder(_service.Guardar(
                entidadTipo, entidadId, archivo.FileName, contenido, archivo.Length, descripcion, usuarioId));
        });

    [HttpGet("archivo/{adjuntoId:int}")]
    public IActionResult Descargar(int adjuntoId) =>
        Ejecutar(() =>
        {
            var resultado = _service.Obtener(adjuntoId);

            if (!resultado.Success)
                return BadRequest(ApiResponse<string>.Fail(resultado.Message));

            var (adjunto, ruta) = resultado.Data;

            // FileStream en vez de leer todo a memoria: una factura escaneada de
            // varios MB no tiene por qué pasar entera por el servidor.
            // System.IO.File explícito porque ControllerBase ya tiene un File().
            return File(System.IO.File.OpenRead(ruta), adjunto.TipoContenido, adjunto.NombreArchivo);
        });

    [HttpDelete("{adjuntoId:int}")]
    public IActionResult Eliminar(int adjuntoId, [FromQuery] int usuarioId) =>
        Ejecutar(() => Responder(_service.Eliminar(adjuntoId, usuarioId)));

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
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en adjuntos: {ex.Message}"));
        }
    }
}
