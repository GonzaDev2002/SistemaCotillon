using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs.Caja;
using System.Net.Http.Json;

namespace Cotillon.Desktop.WinForms.Services;

public class CajaApiService
{
    private readonly HttpClient _http;

    public CajaApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<int> ObtenerOCrearCajaAsync(int usuarioId)
    {
        var caja = await ObtenerCajaAbiertaAsync(usuarioId);

        if (caja != null)
            return caja.SesionCajaId;

        return await AbrirCajaAsync(usuarioId, 0);
    }

    public async Task<SesionCajaDto?> ObtenerCajaAbiertaAsync(int usuarioId)
    {
        try
        {
            var response = await _http.GetAsync($"api/caja/abierta/{usuarioId}");

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<SesionCajaDto>();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al consultar caja abierta:\n" + ex.Message);
            return null;
        }
    }

    public async Task<int> AbrirCajaAsync(int usuarioId, decimal montoInicial)
    {
        try
        {
            var request = new AbrirCajaRequestDto
            {
                UsuarioId = usuarioId,
                MontoInicial = montoInicial,
                Observaciones = "Apertura desde sistema"
            };

            var response = await _http.PostAsJsonAsync("api/caja/abrir", request);
            string contenido = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error al abrir caja:\n" + contenido);
                return 0;
            }

            var json = await response.Content.ReadFromJsonAsync<AbrirCajaResponse>();

            if (json == null || json.SesionCajaId <= 0)
            {
                MessageBox.Show("La API no devolvió una caja válida:\n" + contenido);
                return 0;
            }

            return json.SesionCajaId;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al abrir caja:\n" + ex.Message);
            return 0;
        }
    }

    public async Task<bool> CerrarCajaAsync(int sesionCajaId, decimal montoContado, string observaciones)
    {
        try
        {
            var request = new CerrarCajaRequestDto
            {
                SesionCajaId = sesionCajaId,
                MontoContado = montoContado,
                Observaciones = observaciones
            };

            var response = await _http.PostAsJsonAsync("api/caja/cerrar", request);

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show("Error al cerrar caja:\n" + await response.Content.ReadAsStringAsync());
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al cerrar caja:\n" + ex.Message);
            return false;
        }
    }

    private class AbrirCajaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public int SesionCajaId { get; set; }
    }
}