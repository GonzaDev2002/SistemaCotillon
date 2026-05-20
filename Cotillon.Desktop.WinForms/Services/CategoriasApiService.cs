using System.Net.Http.Json;
using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;

namespace Cotillon.Desktop.WinForms.Services;

public class CategoriasApiService
{
    private readonly HttpClient _http;

    public CategoriasApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<List<CategoriaDto>> ObtenerTodasAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<List<CategoriaDto>>>("api/Categorias");
            return result?.Data ?? new List<CategoriaDto>();
        }
        catch
        {
            return new List<CategoriaDto>();
        }
    }

    public async Task<List<CategoriaDto>> GetPorSeccionAsync(int seccionId)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<List<CategoriaDto>>>($"api/Categorias/por-seccion/{seccionId}");
            return result?.Data ?? new List<CategoriaDto>();
        }
        catch
        {
            return new List<CategoriaDto>();
        }
    }

    public async Task<ApiResponse<string>> GuardarAsync(CategoriaDto categoria)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Categorias", categoria);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("No se recibió respuesta válida.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"No se pudo conectar con la API: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> ActualizarAsync(CategoriaDto categoria)
    {
        try
        {
            var response = await _http.PutAsJsonAsync("api/Categorias", categoria);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
            return result ?? ApiResponse<string>.Fail("No se recibió respuesta válida.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"No se pudo conectar con la API: {ex.Message}");
        }
    }
}