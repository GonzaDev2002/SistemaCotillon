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
            BaseAddress = new Uri(Helpers.ApiConfig.BaseUrl)
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

            if (result is null)
                return ApiResponse<UsuarioSesion>.Fail("No se obtuvo respuesta del servidor.");

            if (!result.Success)
                return ApiResponse<UsuarioSesion>.Fail(result.Message ?? "Usuario o PIN incorrectos.");

            if (result.Data is null)
                return ApiResponse<UsuarioSesion>.Fail("La respuesta del servidor no contiene datos de usuario.");

            var usuarioSesion = new UsuarioSesion
            {
                UsuarioId = result.Data.UsuarioId,
                NombreUsuario = result.Data.NombreUsuario ?? "",
                NombreCompleto = result.Data.NombreCompleto ?? "",
                Rol = result.Data.Rol ?? ""
            };

            var mensaje = string.IsNullOrWhiteSpace(result.Message)
                ? "Login correcto."
                : result.Message;

            return ApiResponse<UsuarioSesion>.Ok(usuarioSesion, mensaje);
        }
        catch (Exception ex)
        {
            var mensajeError = string.IsNullOrWhiteSpace(ex.Message)
                ? "No se pudo conectar con la API."
                : ex.Message;

            return ApiResponse<UsuarioSesion>.Fail($"No se pudo conectar con la API: {mensajeError}");
        }
    }
}