using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs.Compras;
using System.Net.Http.Json;

namespace Cotillon.Desktop.WinForms.Services;

public class ComprasApiService
{
    private readonly HttpClient _http;

    public ComprasApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<List<CompraListadoDto>> ListarAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<CompraListadoDto>>("api/compras") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> CrearAsync(CrearCompraRequestDto request)
    {
        var response = await _http.PostAsJsonAsync("api/compras", request);
        return response.IsSuccessStatusCode;
    }
}
