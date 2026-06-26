using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Clientes;
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
    public async Task<IActionResult> Listar()
    {
        var clientes = await _service.ListarAsync();
        return Ok(clientes);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ClienteDto cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.NombreCompleto))
            return BadRequest("El nombre del cliente es obligatorio.");

        if (string.IsNullOrWhiteSpace(cliente.Documento))
            return BadRequest("El documento del cliente es obligatorio.");

        try
        {
            await _service.CrearAsync(cliente);

            return Ok(new
            {
                success = true,
                message = "Cliente registrado correctamente."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ClienteDto cliente)
    {
        if (string.IsNullOrWhiteSpace(cliente.NombreCompleto))
            return BadRequest("El nombre del cliente es obligatorio.");

        if (string.IsNullOrWhiteSpace(cliente.Documento))
            return BadRequest("El documento del cliente es obligatorio.");

        try
        {
            cliente.ClienteId = id;
            await _service.ActualizarAsync(cliente);

            return Ok(new
            {
                success = true,
                message = "Cliente actualizado correctamente."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _service.EliminarAsync(id);

        return Ok(new
        {
            success = true,
            message = "Cliente eliminado correctamente."
        });
    }
}