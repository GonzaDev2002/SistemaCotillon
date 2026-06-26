using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Estadisticas;

namespace Cotillon.LocalApi.Service.Services;

public class EstadisticasService
{
    private readonly EstadisticasRepository _repository;

    public EstadisticasService(EstadisticasRepository repository)
    {
        _repository = repository;
    }

    public Task<List<EstadisticaVentaDiaDto>> VentasPorDiaAsync(DateTime desde, DateTime hasta)
    {
        return _repository.VentasPorDiaAsync(desde, hasta);
    }

    public Task<List<EstadisticaProductoVendidoDto>> ProductosMasVendidosAsync(DateTime desde, DateTime hasta)
    {
        return _repository.ProductosMasVendidosAsync(desde, hasta);
    }

    public Task<List<EstadisticaMedioPagoDto>> VentasPorMedioPagoAsync(DateTime desde, DateTime hasta)
    {
        return _repository.VentasPorMedioPagoAsync(desde, hasta);
    }

    public Task<List<EstadisticaBajoStockDto>> ProductosBajoStockAsync()
    {
        return _repository.ProductosBajoStockAsync();
    }
}