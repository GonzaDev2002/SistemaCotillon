using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs.Compras;
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

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var compras = await _service.ListarAsync();
        return Ok(compras);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearCompraRequestDto request)
    {
        int compraId = await _service.CrearCompraAsync(request);

        return Ok(new
        {
            success = true,
            message = "Compra registrada correctamente.",
            compraId
        });
    }
}
