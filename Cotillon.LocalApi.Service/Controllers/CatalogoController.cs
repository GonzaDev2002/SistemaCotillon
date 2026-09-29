using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Usuarios;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/MediosPago")]
public class MediosPagoController : ControllerBase
{
    private readonly MediosPagoRepository _repository;

    public MediosPagoController(MediosPagoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Listar([FromQuery] bool incluirInactivos = false)
    {
        try
        {
            return Ok(ApiResponse<List<MedioPagoDto>>.Ok(
                _repository.ObtenerTodos(incluirInactivos), "Medios de pago obtenidos correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en medios de pago: {ex.Message}"));
        }
    }
}

[ApiController]
[Route("api/Configuracion")]
public class ConfiguracionController : ControllerBase
{
    private readonly ConfiguracionRepository _repository;

    public ConfiguracionController(ConfiguracionRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Obtener()
    {
        try
        {
            return Ok(ApiResponse<Dictionary<string, string>>.Ok(
                _repository.ObtenerTodo(), "Configuración obtenida correctamente."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en configuración: {ex.Message}"));
        }
    }

    [HttpPut]
    public IActionResult Guardar([FromBody] ConfiguracionItemDto item, [FromQuery] int usuarioId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(item.Clave))
                return BadRequest(ApiResponse<string>.Fail("La clave es obligatoria."));

            _repository.Guardar(item.Clave.Trim(), item.Valor, usuarioId);

            return Ok(ApiResponse<string>.Ok("OK", "Configuración guardada con éxito."));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en configuración: {ex.Message}"));
        }
    }
}
