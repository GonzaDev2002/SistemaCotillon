using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Reportes;

namespace Cotillon.LocalApi.Service.Services;

public class ReportesService
{
    private readonly ReportesRepository _repository;

    public ReportesService(ReportesRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ReporteProductoDto>> ListarProductosAsync()
    {
        return _repository.ListarProductosAsync();
    }

    public Task<List<ReporteProductoDto>> ListarProductosBajoStockAsync()
    {
        return _repository.ListarProductosBajoStockAsync();
    }

    public Task<List<ReporteVentaDto>> ListarVentasPorRangoAsync(DateTime desde, DateTime hasta)
    {
        return _repository.ListarVentasPorRangoAsync(desde, hasta);
    }
}