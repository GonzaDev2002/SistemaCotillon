using Cotillon.Data;
using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Adjuntos;
using Cotillon.Shared.Responses;

namespace Cotillon.LocalApi.Service.Services;

/// <summary>
/// Guarda y recupera los archivos adjuntos. El contenido va a disco y los datos
/// del archivo a la base; esta clase es la única que conoce la carpeta real.
/// </summary>
public class AdjuntosService
{
    private readonly AdjuntosRepository _repository;
    private readonly string _carpeta;

    public AdjuntosService(AdjuntosRepository repository, IDbConnectionFactory factory)
    {
        _repository = repository;

        // Los adjuntos viven al lado de la base: así "copiar la carpeta Cotillon"
        // es un backup completo, y mover la base de equipo se lleva los archivos.
        _carpeta = Path.Combine(
            Path.GetDirectoryName(factory.RutaBase) ?? AppContext.BaseDirectory, "Adjuntos");
    }

    public string Carpeta => _carpeta;

    public ApiResponse<List<AdjuntoDto>> Listar(string entidadTipo, int entidadId)
    {
        var tipo = Normalizar(entidadTipo);

        if (tipo is null)
            return ApiResponse<List<AdjuntoDto>>.Fail("El tipo de registro no es válido.");

        return ApiResponse<List<AdjuntoDto>>.Ok(
            _repository.ObtenerPorEntidad(tipo, entidadId), "Adjuntos obtenidos correctamente.");
    }

    public ApiResponse<AdjuntoDto> Guardar(
        string entidadTipo, int entidadId, string nombreOriginal,
        Stream contenido, long tamano, string? descripcion, int usuarioId)
    {
        var tipo = Normalizar(entidadTipo);

        if (tipo is null)
            return ApiResponse<AdjuntoDto>.Fail("El tipo de registro no es válido.");

        if (entidadId <= 0 || !_repository.ExisteEntidad(tipo, entidadId))
            return ApiResponse<AdjuntoDto>.Fail("No existe el registro al que querés adjuntar el archivo.");

        if (tamano <= 0)
            return ApiResponse<AdjuntoDto>.Fail("El archivo está vacío.");

        if (tamano > Adjunto.TamanoMaximoBytes)
            return ApiResponse<AdjuntoDto>.Fail(
                $"El archivo supera el máximo de {Adjunto.TamanoMaximoBytes / 1024 / 1024} MB.");

        // Sólo el nombre: si llega una ruta completa o con "..", se descarta todo
        // menos el último tramo antes de mirar la extensión.
        var nombre = Path.GetFileName(nombreOriginal ?? string.Empty).Trim();

        if (nombre.Length == 0)
            return ApiResponse<AdjuntoDto>.Fail("El archivo no tiene nombre.");

        var extension = Path.GetExtension(nombre);

        if (!Adjunto.TiposPermitidos.TryGetValue(extension, out var tipoContenido))
            return ApiResponse<AdjuntoDto>.Fail(
                "Sólo se pueden adjuntar archivos PDF o imágenes (PNG, JPG, WEBP, BMP, TIFF).");

        Directory.CreateDirectory(_carpeta);

        // Nombre generado por el servidor: el cliente no decide dónde se escribe.
        var nombreEnDisco =
            $"{tipo.ToLowerInvariant()}-{entidadId}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";

        var destino = Path.Combine(_carpeta, nombreEnDisco);

        try
        {
            using (var archivo = File.Create(destino))
                contenido.CopyTo(archivo);

            if (!FirmaCoincide(destino, extension))
            {
                File.Delete(destino);

                return ApiResponse<AdjuntoDto>.Fail(
                    $"El contenido no corresponde a un {extension.TrimStart('.').ToUpperInvariant()}. " +
                    "Verificá que no se le haya cambiado la extensión al archivo.");
            }

            var adjunto = new AdjuntoDto
            {
                EntidadTipo = tipo,
                EntidadId = entidadId,
                NombreArchivo = nombre,
                TipoContenido = tipoContenido,
                TamanoBytes = new FileInfo(destino).Length,
                Descripcion = descripcion,
                FechaAlta = DateTime.Now
            };

            adjunto.AdjuntoId = _repository.Insertar(adjunto, nombreEnDisco, usuarioId);

            return ApiResponse<AdjuntoDto>.Ok(adjunto, "Archivo adjuntado con éxito.");
        }
        catch
        {
            // Si la fila no llegó a grabarse, el archivo quedaría huérfano en disco:
            // sin registro nadie lo ve, pero ocupa lugar para siempre.
            if (File.Exists(destino))
                File.Delete(destino);

            throw;
        }
    }

    /// <summary>
    /// Devuelve el archivo para descargarlo. El nombre en disco sale de la base,
    /// nunca de la petición, y se vuelve a combinar con la carpeta de adjuntos.
    /// </summary>
    public ApiResponse<DescargaAdjunto> Obtener(int adjuntoId)
    {
        var fila = _repository.ObtenerPorId(adjuntoId);

        if (fila is null)
            return ApiResponse<DescargaAdjunto>.Fail("No se encontró el archivo.");

        var ruta = Path.Combine(_carpeta, fila.Value.NombreEnDisco);

        if (!File.Exists(ruta))
            return ApiResponse<DescargaAdjunto>.Fail(
                "El registro existe pero el archivo no está en la carpeta de adjuntos.");

        return ApiResponse<DescargaAdjunto>.Ok(
            new DescargaAdjunto(fila.Value.Adjunto, ruta), "Archivo encontrado.");
    }

    public ApiResponse<string> Eliminar(int adjuntoId, int usuarioId)
    {
        var fila = _repository.ObtenerPorId(adjuntoId);

        if (fila is null)
            return ApiResponse<string>.Fail("No se encontró el archivo.");

        var (adjunto, nombreEnDisco) = fila.Value;

        // Primero la fila: si el borrado del archivo falla queda un huérfano en
        // disco, pero nunca un registro que apunte a la nada.
        if (!_repository.Eliminar(adjuntoId, usuarioId,
                $"{adjunto.EntidadTipo} #{adjunto.EntidadId}: {adjunto.NombreArchivo}"))
            return ApiResponse<string>.Fail("No se pudo quitar el archivo.");

        var ruta = Path.Combine(_carpeta, nombreEnDisco);

        if (File.Exists(ruta))
            File.Delete(ruta);

        return ApiResponse<string>.Ok("OK", "Archivo quitado con éxito.");
    }

    private static string? Normalizar(string? entidadTipo)
    {
        var tipo = entidadTipo?.Trim().ToUpperInvariant();

        return tipo is not null && Adjunto.EntidadesValidas.Contains(tipo) ? tipo : null;
    }

    /// <summary>
    /// Compara los primeros bytes con la firma del formato. Un archivo renombrado
    /// a .pdf se abriría después en la terminal con el programa de PDF; cortarlo
    /// acá evita guardar algo que no es lo que dice ser.
    /// </summary>
    private static bool FirmaCoincide(string ruta, string extension)
    {
        var encabezado = new byte[12];
        int leidos;

        using (var archivo = File.OpenRead(ruta))
            leidos = archivo.ReadAtLeast(encabezado, encabezado.Length, throwOnEndOfStream: false);

        if (leidos < 4)
            return false;

        bool Empieza(params byte[] firma) =>
            firma.Length <= leidos && encabezado.Take(firma.Length).SequenceEqual(firma);

        return extension.ToLowerInvariant() switch
        {
            ".pdf" => Empieza(0x25, 0x50, 0x44, 0x46),                      // %PDF
            ".png" => Empieza(0x89, 0x50, 0x4E, 0x47),                      // .PNG
            ".jpg" or ".jpeg" => Empieza(0xFF, 0xD8, 0xFF),                 // JFIF / Exif
            ".bmp" => Empieza(0x42, 0x4D),                                  // BM
            ".webp" => leidos >= 12
                       && Empieza(0x52, 0x49, 0x46, 0x46)                   // RIFF
                       && encabezado.Skip(8).Take(4).SequenceEqual("WEBP"u8.ToArray()),
            ".tif" or ".tiff" => Empieza(0x49, 0x49, 0x2A, 0x00)             // little endian
                                 || Empieza(0x4D, 0x4D, 0x00, 0x2A),         // big endian
            _ => false
        };
    }
}

/// <summary>Archivo listo para enviar: sus datos y la ruta real en el servidor.</summary>
public readonly record struct DescargaAdjunto(AdjuntoDto Adjunto, string Ruta);
