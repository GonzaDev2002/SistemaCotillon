using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Cotillon.Shared.Responses;

namespace Cotillon.Desktop.Wpf.Servicios;

/// <summary>
/// Cliente HTTP compartido contra la API local. Traduce cualquier falla de red o
/// de deserialización a un ApiResponse fallido, para que las pantallas tengan un
/// único camino de error que mostrar.
/// </summary>
public class ApiCliente
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    public ApiCliente(string baseUrl)
    {
        var manejador = new HttpClientHandler
        {
            // La API local usa un certificado de desarrollo autofirmado; el tráfico
            // no sale de la red del comercio.
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };

        _http = new HttpClient(manejador)
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public string BaseUrl => _http.BaseAddress!.ToString();

    public async Task<ApiResponse<T>> ObtenerAsync<T>(string url)
    {
        try
        {
            var respuesta = await _http.GetAsync(url);

            return await LeerAsync<T>(respuesta);
        }
        catch (Exception ex)
        {
            return Falla<T>(ex);
        }
    }

    public async Task<ApiResponse<T>> PublicarAsync<T>(string url, object? cuerpo = null)
    {
        try
        {
            var respuesta = await _http.PostAsJsonAsync(url, cuerpo ?? new { });

            return await LeerAsync<T>(respuesta);
        }
        catch (Exception ex)
        {
            return Falla<T>(ex);
        }
    }

    public async Task<ApiResponse<T>> ActualizarAsync<T>(string url, object cuerpo)
    {
        try
        {
            var respuesta = await _http.PutAsJsonAsync(url, cuerpo);

            return await LeerAsync<T>(respuesta);
        }
        catch (Exception ex)
        {
            return Falla<T>(ex);
        }
    }

    public async Task<ApiResponse<T>> EliminarAsync<T>(string url)
    {
        try
        {
            var respuesta = await _http.DeleteAsync(url);

            return await LeerAsync<T>(respuesta);
        }
        catch (Exception ex)
        {
            return Falla<T>(ex);
        }
    }

    /// <summary>
    /// Sube un archivo del disco local como multipart. Se transmite por streaming
    /// para no cargar en memoria una factura escaneada entera.
    /// </summary>
    public async Task<ApiResponse<T>> SubirArchivoAsync<T>(string url, string rutaArchivo, string? descripcion)
    {
        try
        {
            using var contenido = new MultipartFormDataContent();
            await using var archivo = File.OpenRead(rutaArchivo);

            contenido.Add(new StreamContent(archivo), "archivo", Path.GetFileName(rutaArchivo));

            if (!string.IsNullOrWhiteSpace(descripcion))
                contenido.Add(new StringContent(descripcion), "descripcion");

            var respuesta = await _http.PostAsync(url, contenido);

            return await LeerAsync<T>(respuesta);
        }
        catch (Exception ex)
        {
            return Falla<T>(ex);
        }
    }

    /// <summary>
    /// Descarga un archivo al destino indicado. Devuelve el mensaje de error de la
    /// API si la respuesta no fue un archivo.
    /// </summary>
    public async Task<ApiResponse<string>> DescargarArchivoAsync(string url, string rutaDestino)
    {
        try
        {
            using var respuesta = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

            if (!respuesta.IsSuccessStatusCode)
            {
                // Cuando falla, la API responde el envoltorio ApiResponse de siempre.
                var error = await LeerAsync<string>(respuesta);

                return error.Success ? ApiResponse<string>.Fail("No se pudo descargar el archivo.") : error;
            }

            await using (var origen = await respuesta.Content.ReadAsStreamAsync())
            await using (var destino = File.Create(rutaDestino))
                await origen.CopyToAsync(destino);

            return ApiResponse<string>.Ok(rutaDestino, "Archivo descargado.");
        }
        catch (Exception ex)
        {
            return Falla<string>(ex);
        }
    }

    public async Task<bool> EstaDisponibleAsync()
    {
        try
        {
            using var temporal = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var respuesta = await _http.GetAsync("api/Health/ping", temporal.Token);

            return respuesta.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<ApiResponse<T>> LeerAsync<T>(HttpResponseMessage respuesta)
    {
        var contenido = await respuesta.Content.ReadAsStringAsync();

        if (string.IsNullOrWhiteSpace(contenido))
        {
            return respuesta.IsSuccessStatusCode
                ? ApiResponse<T>.Ok(default, "Operación completada.")
                : ApiResponse<T>.Fail($"La API respondió {(int)respuesta.StatusCode} sin detalle.");
        }

        try
        {
            var resultado = JsonSerializer.Deserialize<ApiResponse<T>>(contenido, Opciones);

            return resultado ?? ApiResponse<T>.Fail("No se pudo interpretar la respuesta de la API.");
        }
        catch (JsonException)
        {
            return ApiResponse<T>.Fail($"Respuesta inesperada de la API: {Recortar(contenido)}");
        }
    }

    private static ApiResponse<T> Falla<T>(Exception ex) =>
        ApiResponse<T>.Fail(ex is HttpRequestException or TaskCanceledException
            ? "No se pudo conectar con el servidor. Verificá que el servicio esté corriendo."
            : ex.Message);

    private static string Recortar(string texto) =>
        texto.Length <= 200 ? texto : texto[..200] + "...";
}
