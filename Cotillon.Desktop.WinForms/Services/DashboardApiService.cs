using System.Net.Http.Json;
using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;

namespace Cotillon.Desktop.WinForms.Services;

public class DashboardApiService
{
    private readonly HttpClient _http;

    public DashboardApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<DashboardResumenDto?> ObtenerResumenAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<DashboardResumenDto>>("api/Dashboard/resumen");
            return result?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<ProductoDashboardDto>> ObtenerBajoStockAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<List<ProductoDashboardDto>>>("api/Dashboard/bajo-stock");
            return result?.Data ?? new List<ProductoDashboardDto>();
        }
        catch
        {
            return new List<ProductoDashboardDto>();
        }
    }

    public async Task<List<ProductoDashboardDto>> ObtenerUltimosProductosAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<List<ProductoDashboardDto>>>("api/Dashboard/ultimos-productos");
            return result?.Data ?? new List<ProductoDashboardDto>();
        }
        catch
        {
            return new List<ProductoDashboardDto>();
        }
    }
}