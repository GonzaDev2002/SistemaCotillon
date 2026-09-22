using System.Net.Http.Json;
using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs.Reportes;
using Cotillon.Shared.Responses;

namespace Cotillon.Desktop.WinForms.Services;

public class ReportesApiService
{
    private readonly HttpClient _http;

    public ReportesApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<List<ReporteProductoDto>> ListarProductosAsync() =>
        await ObtenerAsync<ReporteProductoDto>("api/Reportes/productos");

    public async Task<List<ReporteProductoDto>> ListarProductosBajoStockAsync() =>
        await ObtenerAsync<ReporteProductoDto>("api/Reportes/productos-bajo-stock");

    public async Task<List<ReporteVentaDto>> ListarVentasPorRangoAsync(DateTime desde, DateTime hasta) =>
        await ObtenerAsync<ReporteVentaDto>(
            $"api/Reportes/ventas?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}");

    private async Task<List<T>> ObtenerAsync<T>(string url)
    {
        var respuesta = await _http.GetFromJsonAsync<ApiResponse<List<T>>>(url);

        return respuesta?.Data ?? [];
    }
}
