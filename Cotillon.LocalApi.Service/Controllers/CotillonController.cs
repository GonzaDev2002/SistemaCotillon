using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Encargues;
using Cotillon.Shared.DTOs.Terceros;
using Cotillon.Shared.DTOs.Ventas;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

/// <summary>
/// Base para los módulos nuevos: traduce las excepciones de negocio en un
/// mensaje que la pantalla puede mostrar tal cual, y cualquier otra cosa en un
/// 500 que no filtra el detalle interno.
/// </summary>
public abstract class CotillonControllerBase : ControllerBase
{
    protected IActionResult Ejecutar<T>(Func<T> accion, string mensaje)
    {
        try
        {
            return Ok(ApiResponse<T>.Ok(accion(), mensaje));
        }
        catch (Exception ex) when (ex is ComboInvalidoException
                                      or CuentaCorrienteInvalidaException
                                      or EncargueInvalidoException
                                      or AumentoInvalidoException
                                      or VentaInvalidaException)
        {
            return BadRequest(ApiResponse<T>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<T>.Fail($"Error interno: {ex.Message}"));
        }
    }
}

[ApiController]
[Route("api/[controller]")]
public class CombosController(CombosRepository repositorio) : CotillonControllerBase
{
    [HttpGet("{productoId:int}")]
    public IActionResult Componentes(int productoId) =>
        Ejecutar(() => repositorio.Componentes(productoId), "Componentes del combo.");

    [HttpPut("{productoId:int}")]
    public IActionResult Guardar(int productoId, [FromQuery] int usuarioId,
        [FromBody] List<ComponenteComboDto> componentes) =>
        Ejecutar(() =>
        {
            repositorio.Guardar(productoId, componentes, usuarioId);
            return true;
        }, "Receta del combo guardada.");
}

[ApiController]
[Route("api/[controller]")]
public class PreciosController(PreciosRepository repositorio) : CotillonControllerBase
{
    [HttpPost("simular")]
    public IActionResult Simular([FromBody] AumentoPreciosDto aumento) =>
        Ejecutar(() => repositorio.Simular(aumento), "Vista previa del ajuste.");

    [HttpPost("aplicar")]
    public IActionResult Aplicar([FromBody] AumentoPreciosDto aumento) =>
        Ejecutar(() => new ResultadoAumentoDto
        {
            Afectados = repositorio.Aplicar(aumento),
            Mensaje = "Precios actualizados."
        }, "Precios actualizados.");
}

[ApiController]
[Route("api/[controller]")]
public class CuentaCorrienteController(CuentaCorrienteRepository repositorio) : CotillonControllerBase
{
    [HttpGet("saldos")]
    public IActionResult Saldos([FromQuery] bool soloConDeuda = true) =>
        Ejecutar(() => repositorio.Saldos(soloConDeuda), "Saldos de clientes.");

    [HttpGet("{clienteId:int}")]
    public IActionResult Obtener(int clienteId) =>
        Ejecutar(() => repositorio.Obtener(clienteId), "Cuenta del cliente.");

    [HttpPost("pago")]
    public IActionResult Pagar([FromBody] RegistrarPagoCuentaDto pago) =>
        Ejecutar(() =>
        {
            repositorio.RegistrarPago(pago);
            return true;
        }, "Pago registrado.");

    [HttpPost("ajuste")]
    public IActionResult Ajustar([FromBody] AjustarCuentaDto ajuste) =>
        Ejecutar(() =>
        {
            repositorio.Ajustar(ajuste);
            return true;
        }, "Ajuste registrado.");
}

[ApiController]
[Route("api/[controller]")]
public class ControlController(ControlRepository repositorio) : CotillonControllerBase
{
    [HttpPost("cancelacion")]
    public IActionResult Cancelacion([FromBody] CancelarVentaEnCursoDto datos) =>
        Ejecutar(() =>
        {
            repositorio.RegistrarCancelacion(datos);
            return true;
        }, "Cancelación registrada.");

    [HttpGet("usuarios")]
    public IActionResult PorUsuario([FromQuery] DateTime desde, [FromQuery] DateTime hasta) =>
        Ejecutar(() => repositorio.PorUsuario(desde, hasta), "Control por usuario.");
}

[ApiController]
[Route("api/[controller]")]
public class EncarguesController(EncarguesRepository repositorio) : CotillonControllerBase
{
    [HttpGet]
    public IActionResult Listar([FromQuery] DateTime? hasta = null, [FromQuery] bool incluirCerrados = false) =>
        Ejecutar(() => repositorio.Listar(hasta, incluirCerrados), "Encargues.");

    [HttpGet("{encargueId:int}")]
    public IActionResult Obtener(int encargueId) =>
        Ejecutar(() => repositorio.Obtener(encargueId), "Encargue.");

    [HttpPost]
    public IActionResult Crear([FromBody] CrearEncargueDto datos) =>
        Ejecutar(() => repositorio.Obtener(repositorio.Crear(datos)), "Encargue registrado.");

    [HttpPost("{encargueId:int}/entregar")]
    public IActionResult Entregar(int encargueId, [FromBody] EntregarEncargueDto datos) =>
        Ejecutar(() => repositorio.Entregar(encargueId, datos), "Encargue entregado.");

    [HttpPost("{encargueId:int}/cancelar")]
    public IActionResult Cancelar(int encargueId, [FromQuery] int usuarioId, [FromQuery] string motivo) =>
        Ejecutar(() =>
        {
            repositorio.Cancelar(encargueId, usuarioId, motivo);
            return true;
        }, "Encargue cancelado.");
}
