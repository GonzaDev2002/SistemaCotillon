using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Proveedores;
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
    public async Task<IActionResult> Listar()
    {
        var proveedores = await _service.ListarAsync();
        return Ok(proveedores);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProveedorDto proveedor)
    {
        int proveedorId = await _service.CrearAsync(proveedor);

        return Ok(new
        {
            success = true,
            message = "Proveedor registrado correctamente.",
            proveedorId
        });
    }
}
