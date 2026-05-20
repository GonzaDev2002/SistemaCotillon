using System.Net.Http.Json;
using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;

namespace Cotillon.Desktop.WinForms.Services;

public class ProductosApiService
{
    private readonly HttpClient _http;

    public ProductosApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<ApiResponse<string>> GuardarAsync(ProductoDto producto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Productos", producto);
            var rawContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(rawContent))
                return ApiResponse<string>.Fail("La API devolvió una respuesta vacía.");

            var result = System.Text.Json.JsonSerializer.Deserialize<ApiResponse<string>>(rawContent,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return result ?? ApiResponse<string>.Fail("No se pudo interpretar la respuesta de la API.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"No se pudo conectar con la API: {ex.Message}");
        }
    }

    public async Task<List<ProductoListadoDto>> ObtenerTodosAsync(bool incluirInactivos)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<List<ProductoListadoDto>>>($"api/Productos/todos?incluirInactivos={incluirInactivos.ToString().ToLower()}");
            return result?.Data ?? new List<ProductoListadoDto>();
        }
        catch
        {
            return new List<ProductoListadoDto>();
        }
    }

    public async Task<ApiResponse<string>> EliminarAsync(int productoId)
    {
        try
        {
            var response = await _http.DeleteAsync($"api/Productos/{productoId}");
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("No se recibió respuesta válida.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"No se pudo conectar con la API: {ex.Message}");
        }
    }
}