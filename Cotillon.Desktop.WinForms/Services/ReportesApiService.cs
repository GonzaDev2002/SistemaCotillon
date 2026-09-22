using Cotillon.Shared.DTOs.Reportes;
using System.Net.Http.Json;

namespace Cotillon.Desktop.WinForms.Services;

public class ReportesApiService
{
    private readonly HttpClient _httpClient;

    public ReportesApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:44355/")
        };
    }

    public async Task<List<ReporteProductoDto>> ListarProductosAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ReporteProductoDto>>("api/reportes/productos")
               ?? new List<ReporteProductoDto>();
    }

    public async Task<List<ReporteProductoDto>> ListarProductosBajoStockAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<ReporteProductoDto>>("api/reportes/productos-bajo-stock")
               ?? new List<ReporteProductoDto>();
    }

    public async Task<List<ReporteVentaDto>> ListarVentasPorRangoAsync(DateTime desde, DateTime hasta)
    {
        string url = $"api/reportes/ventas?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

        return await _httpClient.GetFromJsonAsync<List<ReporteVentaDto>>(url)
               ?? new List<ReporteVentaDto>();
    }
}