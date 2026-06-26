using Cotillon.Shared.DTOs.Ventas;
using System.Net.Http.Json;

namespace Cotillon.Desktop.WinForms.Services;

public class VentasApiService
{
    private readonly HttpClient _httpClient;

    public VentasApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://localhost:44355/")
        };
    }

    public async Task<List<VentaListadoDto>> ListarVentasAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<VentaListadoDto>>("api/ventas")
               ?? new List<VentaListadoDto>();
    }

    public async Task<bool> CrearVentaAsync(CrearVentaRequestDto request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/ventas", request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error al registrar venta:\n" + error);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error al conectar con la API:\n" + ex.Message);
            return false;
        }
    }
}