using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuariosService _service;

    public UsuariosController(UsuariosService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] bool incluirInactivos = true)
    {
        var usuarios = await _service.ListarAsync(incluirInactivos);
        return Ok(usuarios);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] UsuarioDto usuario)
    {
        if (string.IsNullOrWhiteSpace(usuario.NombreUsuario))
            return BadRequest("El nombre de usuario es obligatorio.");

        if (string.IsNullOrWhiteSpace(usuario.NombreCompleto))
            return BadRequest("El nombre completo es obligatorio.");

        if (string.IsNullOrWhiteSpace(usuario.Pin))
            return BadRequest("El PIN es obligatorio.");

        try
        {
            await _service.CrearAsync(usuario);
            return Ok(new { success = true, message = "Usuario registrado correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] UsuarioDto usuario)
    {
        usuario.UsuarioId = id;

        try
        {
            await _service.ActualizarAsync(usuario);
            return Ok(new { success = true, message = "Usuario actualizado correctamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool activo)
    {
        await _service.CambiarEstadoAsync(id, activo);
        return Ok(new { success = true, message = "Estado actualizado correctamente." });
    }
}