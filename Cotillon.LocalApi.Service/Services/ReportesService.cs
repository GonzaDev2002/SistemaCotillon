using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Reportes;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

public class ReportesService
{
    private readonly ReportesRepository _repository;

    public ReportesService(ReportesRepository repository)
    {
        _repository = repository;
    }

    public ApiResponse<List<ReporteProductoDto>> ListarProductos() =>
        ApiResponse<List<ReporteProductoDto>>.Ok(
            _repository.ListarProductos(), "Reporte de productos generado correctamente.");

    public ApiResponse<List<ReporteProductoDto>> ListarProductosBajoStock() =>
        ApiResponse<List<ReporteProductoDto>>.Ok(
            _repository.ListarProductosBajoStock(), "Reporte de productos bajo stock generado correctamente.");

    public ApiResponse<List<ReporteVentaDto>> ListarVentasPorRango(DateTime desde, DateTime hasta)
    {
        if (desde.Date > hasta.Date)
            return ApiResponse<List<ReporteVentaDto>>.Fail("La fecha desde no puede ser mayor a la fecha hasta.");

        return ApiResponse<List<ReporteVentaDto>>.Ok(
            _repository.ListarVentasPorRango(desde, hasta), "Reporte de ventas generado correctamente.");
    }
}
