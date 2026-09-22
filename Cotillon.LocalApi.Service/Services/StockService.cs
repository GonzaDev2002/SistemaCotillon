using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Stock;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class StockService
{
    private static readonly string[] TiposAjuste = ["AJUSTE", "MERMA", "DEVOLUCION"];

    private readonly StockRepository _repository;

    public StockService(StockRepository repository)
    {
        _repository = repository;
    }

    public ApiResponse<string> Ajustar(AjustarStockDto ajuste)
    {
        if (ajuste.UsuarioId <= 0)
            return ApiResponse<string>.Fail("El usuario es obligatorio.");

        if (ajuste.ProductoId <= 0)
            return ApiResponse<string>.Fail("El producto es obligatorio.");

        if (ajuste.Cantidad == 0)
            return ApiResponse<string>.Fail("La cantidad no puede ser cero.");

        if (string.IsNullOrWhiteSpace(ajuste.Motivo))
            return ApiResponse<string>.Fail("El motivo del ajuste es obligatorio.");

        ajuste.TipoMovimiento = string.IsNullOrWhiteSpace(ajuste.TipoMovimiento)
            ? "AJUSTE"
            : ajuste.TipoMovimiento.Trim().ToUpperInvariant();

        if (!TiposAjuste.Contains(ajuste.TipoMovimiento))
            return ApiResponse<string>.Fail("El tipo de ajuste debe ser AJUSTE, MERMA o DEVOLUCION.");

        // Una merma siempre descuenta: evita que se cargue en positivo por error.
        if (ajuste.TipoMovimiento == "MERMA" && ajuste.Cantidad > 0)
            ajuste.Cantidad = -ajuste.Cantidad;

        ajuste.Motivo = ajuste.Motivo.Trim();

        try
        {
            _repository.Ajustar(ajuste);

            return ApiResponse<string>.Ok("OK", "Stock ajustado con éxito.");
        }
        catch (StockInvalidoException ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }
    }

    public ApiResponse<List<MovimientoStockDto>> ObtenerMovimientos(int? productoId, DateTime desde, DateTime hasta)
    {
        if (desde.Date > hasta.Date)
            return ApiResponse<List<MovimientoStockDto>>.Fail("La fecha desde no puede ser mayor a la fecha hasta.");

        return ApiResponse<List<MovimientoStockDto>>.Ok(
            _repository.ObtenerMovimientos(productoId, desde, hasta), "Movimientos obtenidos correctamente.");
    }
}
