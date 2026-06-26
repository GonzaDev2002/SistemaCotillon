using Cotillon.Shared.DTOs.Estadisticas;
using System.Net.Http.Json;

namespace Cotillon.Desktop.WinForms.Services;

public class EstadisticasApiService
{
    private readonly HttpClient _httpClient;

    public EstadisticasApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:44355/")
        };
    }

    public async Task<List<EstadisticaVentaDiaDto>> VentasPorDiaAsync(DateTime desde, DateTime hasta)
    {
        string url = $"api/estadisticas/ventas-por-dia?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

        return await _httpClient.GetFromJsonAsync<List<EstadisticaVentaDiaDto>>(url)
               ?? new List<EstadisticaVentaDiaDto>();
    }

    public async Task<List<EstadisticaProductoVendidoDto>> ProductosMasVendidosAsync(DateTime desde, DateTime hasta)
    {
        string url = $"api/estadisticas/productos-mas-vendidos?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

        return await _httpClient.GetFromJsonAsync<List<EstadisticaProductoVendidoDto>>(url)
               ?? new List<EstadisticaProductoVendidoDto>();
    }

    public async Task<List<EstadisticaMedioPagoDto>> VentasPorMedioPagoAsync(DateTime desde, DateTime hasta)
    {
        string url = $"api/estadisticas/ventas-por-medio-pago?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}";

        return await _httpClient.GetFromJsonAsync<List<EstadisticaMedioPagoDto>>(url)
               ?? new List<EstadisticaMedioPagoDto>();
    }

    public async Task<List<EstadisticaBajoStockDto>> ProductosBajoStockAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<EstadisticaBajoStockDto>>("api/estadisticas/productos-bajo-stock")
               ?? new List<EstadisticaBajoStockDto>();
    }
}