using System.Net.Http.Json;
using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;

namespace Cotillon.Desktop.WinForms.Services;

public class SeccionesApiService
{
    private readonly HttpClient _http;

    public SeccionesApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<List<SeccionDto>> ObtenerTodasAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<ApiResponse<List<SeccionDto>>>("api/Secciones");
            return result?.Data ?? new List<SeccionDto>();
        }
        catch
        {
            return new List<SeccionDto>();
        }
    }

    public async Task<ApiResponse<string>> GuardarAsync(SeccionDto seccion)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/Secciones", seccion);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

            return result ?? ApiResponse<string>.Fail("No se recibió respuesta válida.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"No se pudo conectar con la API: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> ActualizarAsync(SeccionDto seccion)
    {
        try
        {
            var response = await _http.PutAsJsonAsync("api/Secciones", seccion);
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();

            return result ?? ApiResponse<string>.Fail("No se recibió respuesta válida.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.Fail($"No se pudo conectar con la API: {ex.Message}");
        }
    }
}