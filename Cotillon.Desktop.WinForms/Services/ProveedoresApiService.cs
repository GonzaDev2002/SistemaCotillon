using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs.Proveedores;
using System.Net.Http.Json;

namespace Cotillon.Desktop.WinForms.Services;

public class ProveedoresApiService
{
    private readonly HttpClient _http;

    public ProveedoresApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<List<ProveedorDto>> ListarAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ProveedorDto>>("api/proveedores") ?? new();
        }
        catch
        {
            return new();
        }
    }

    public async Task<bool> CrearAsync(ProveedorDto proveedor)
    {
        var response = await _http.PostAsJsonAsync("api/proveedores", proveedor);
        return response.IsSuccessStatusCode;
    }
}
