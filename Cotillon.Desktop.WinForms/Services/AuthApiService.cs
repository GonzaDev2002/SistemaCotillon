using System.Net.Http.Json;
using Cotillon.Desktop.WinForms.Models;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.Responses;

namespace Cotillon.Desktop.WinForms.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;

    public AuthApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:44355/")
        };
    }

    public async Task<ApiResponse<UsuarioSesion>> LoginAsync(string username, string pin)
    {
        try
        {
            var request = new LoginRequestDto
            {
                Username = username,
                Pin = pin
            };

            var response = await _httpClient.PostAsJsonAsync("api/Auth/login", request);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();

                return ApiResponse<UsuarioSesion>.Fail(
                    errorResponse?.Message ?? "Error al iniciar sesión.");
            }

            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();

            if (result is null || !result.Success || result.Data is null)
                return ApiResponse<UsuarioSesion>.Fail("Respuesta inválida del servidor.");

            var usuarioSesion = new UsuarioSesion
            {
                UsuarioId = result.Data.UsuarioId,
                NombreUsuario = result.Data.NombreUsuario,
                NombreCompleto = result.Data.NombreCompleto,
                Rol = result.Data.Rol
            };

            return ApiResponse<UsuarioSesion>.Ok(usuarioSesion, result.Message);
        }
        catch (Exception ex)
        {
            return ApiResponse<UsuarioSesion>.Fail($"No se pudo conectar con la API: {ex.Message}");
        }
    }
}