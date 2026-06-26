using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs.Usuarios;
using System.Net.Http.Json;

namespace Cotillon.Desktop.WinForms.Services;

public class UsuariosApiService
{
    private readonly HttpClient _http;

    public UsuariosApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<List<UsuarioDto>> ListarAsync(bool incluirInactivos = true)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<UsuarioDto>>(
                $"api/usuarios?incluirInactivos={incluirInactivos.ToString().ToLower()}")
                ?? new List<UsuarioDto>();
        }
        catch
        {
            return new List<UsuarioDto>();
        }
    }

    public async Task<bool> CrearAsync(UsuarioDto usuario)
    {
        var response = await _http.PostAsJsonAsync("api/usuarios", usuario);

        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync());
            return false;
        }

        return true;
    }

    public async Task<bool> ActualizarAsync(UsuarioDto usuario)
    {
        var response = await _http.PutAsJsonAsync($"api/usuarios/{usuario.UsuarioId}", usuario);

        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync());
            return false;
        }

        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int usuarioId, bool activo)
    {
        var response = await _http.PutAsync($"api/usuarios/{usuarioId}/estado?activo={activo.ToString().ToLower()}", null);

        if (!response.IsSuccessStatusCode)
        {
            MessageBox.Show(await response.Content.ReadAsStringAsync());
            return false;
        }

        return true;
    }
}