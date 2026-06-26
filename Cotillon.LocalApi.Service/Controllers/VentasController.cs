using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Ventas;
using Microsoft.AspNetCore.Mvc;

namespace Cotillon.LocalApi.Service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VentasController : ControllerBase
{
    private readonly VentasService _service;

    public VentasController(VentasService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var ventas = await _service.ListarVentasAsync();
        return Ok(ventas);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearVentaRequestDto request)
    {
        if (request.Detalles == null || request.Detalles.Count == 0)
            return BadRequest("La venta debe tener al menos un producto.");

        var ventaId = await _service.CrearVentaAsync(request);

        return Ok(new
        {
            success = true,
            message = "Venta registrada correctamente.",
            ventaId
        });
    }
}