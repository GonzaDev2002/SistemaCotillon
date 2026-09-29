using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Usuarios;
using Cotillon.Shared.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private static readonly string[] Roles = ["Admin", "Supervisor", "Cajero"];

    private readonly UsuariosRepository _repository;

    public UsuariosController(UsuariosRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IActionResult Listar([FromQuery] bool incluirInactivos = false) =>
        Ejecutar(() => Ok(ApiResponse<List<UsuarioDto>>.Ok(
            _repository.ObtenerTodos(incluirInactivos), "Usuarios obtenidos correctamente.")));

    [HttpPost]
    public IActionResult Crear([FromBody] CrearUsuarioDto usuario, [FromQuery] int usuarioEjecutor)
    {
        return Ejecutar(() =>
        {
            if (string.IsNullOrWhiteSpace(usuario.NombreUsuario))
                return BadRequest(ApiResponse<string>.Fail("El nombre de usuario es obligatorio."));

            if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
                return BadRequest(ApiResponse<string>.Fail("El nombre completo es obligatorio."));

            if (string.IsNullOrWhiteSpace(usuario.Pin) || usuario.Pin.Trim().Length < 4)
                return BadRequest(ApiResponse<string>.Fail("El PIN debe tener al menos 4 dígitos."));

            if (!Roles.Contains(usuario.Rol))
                return BadRequest(ApiResponse<string>.Fail("El rol debe ser Admin, Supervisor o Cajero."));

            var id = _repository.Crear(usuario, usuarioEjecutor);

            return Ok(ApiResponse<UsuarioDto>.Ok(_repository.ObtenerPorId(id), "Usuario creado con éxito."));
        });
    }

    [HttpPut]
    public IActionResult Actualizar([FromBody] UsuarioDto usuario, [FromQuery] int usuarioEjecutor)
    {
        return Ejecutar(() =>
        {
            if (usuario.UsuarioId <= 0)
                return BadRequest(ApiResponse<string>.Fail("El usuario es inválido."));

            if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
                return BadRequest(ApiResponse<string>.Fail("El nombre completo es obligatorio."));

            if (!Roles.Contains(usuario.Rol))
                return BadRequest(ApiResponse<string>.Fail("El rol debe ser Admin, Supervisor o Cajero."));

            if (!_repository.Actualizar(usuario, usuarioEjecutor))
                return BadRequest(ApiResponse<string>.Fail("No se pudo actualizar el usuario."));

            return Ok(ApiResponse<UsuarioDto>.Ok(
                _repository.ObtenerPorId(usuario.UsuarioId), "Usuario actualizado con éxito."));
        });
    }

    [HttpPost("cambiar-pin")]
    public IActionResult CambiarPin([FromBody] CambiarPinDto datos, [FromQuery] bool esAdmin = false)
    {
        return Ejecutar(() =>
        {
            if (string.IsNullOrWhiteSpace(datos.PinNuevo) || datos.PinNuevo.Trim().Length < 4)
                return BadRequest(ApiResponse<string>.Fail("El PIN nuevo debe tener al menos 4 dígitos."));

            // Un administrador puede resetear el PIN de otro usuario sin conocer el anterior.
            _repository.CambiarPin(datos, exigirPinActual: !esAdmin);

            return Ok(ApiResponse<string>.Ok("OK", "PIN actualizado con éxito."));
        });
    }

    private IActionResult Ejecutar(Func<IActionResult> accion)
    {
        try
        {
            return accion();
        }
        catch (UsuarioInvalidoException ex)
        {
            return BadRequest(ApiResponse<string>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return StatusCode(500, ApiResponse<string>.Fail($"Error interno en usuarios: {ex.Message}"));
        }
    }
}
