using Cotillon.Desktop.WinForms.Helpers;
using Cotillon.Shared.DTOs.Clientes;
using System.Net.Http.Json;

namespace Cotillon.Desktop.WinForms.Services;

public class ClientesApiService
{
    private readonly HttpClient _http;

    public ClientesApiService()
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl)
        };
    }

    public async Task<List<ClienteDto>> ListarAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<ClienteDto>>("api/clientes")
                   ?? new List<ClienteDto>();
        }
        catch
        {
            return new List<ClienteDto>();
        }
    }

    public async Task<bool> CrearAsync(ClienteDto cliente)
    {
        var response = await _http.PostAsJsonAsync("api/clientes", cliente);

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            MessageBox.Show("Error al guardar cliente:\n" + error);
            return false;
        }

        return true;
    }

    public async Task<bool> ActualizarAsync(ClienteDto cliente)
    {
        var response = await _http.PutAsJsonAsync($"api/clientes/{cliente.ClienteId}", cliente);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> EliminarAsync(int clienteId)
    {
        var response = await _http.DeleteAsync($"api/clientes/{clienteId}");
        return response.IsSuccessStatusCode;
    }
}