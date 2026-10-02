namespace Cotillon.Shared.DTOs.Adjuntos;

/// <summary>
/// Un archivo adjunto a un cliente, proveedor, venta, compra o producto.
/// </summary>
public class AdjuntoDto
{
    public int AdjuntoId { get; set; }
    public string EntidadTipo { get; set; } = string.Empty;
    public int EntidadId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string TipoContenido { get; set; } = string.Empty;
    public long TamanoBytes { get; set; }
    public string? Descripcion { get; set; }
    public string? Usuario { get; set; }
    public DateTime FechaAlta { get; set; }

    /// <summary>Tamaño listo para mostrar en la grilla, sin formatear en la vista.</summary>
    public string TamanoLegible => TamanoBytes >= 1024 * 1024
        ? $"{TamanoBytes / 1024d / 1024d:0.0} MB"
        : $"{Math.Max(1, TamanoBytes / 1024)} KB";

    public string Extension =>
        (Path.GetExtension(NombreArchivo) ?? string.Empty).TrimStart('.').ToUpperInvariant();
}

/// <summary>
/// Reglas de los adjuntos. Viven en el proyecto compartido para que el cliente
/// pueda filtrar el diálogo de archivos con los mismos valores que valida la API.
/// </summary>
public static class Adjunto
{
    public const long TamanoMaximoBytes = 10 * 1024 * 1024;

    /// <summary>
    /// Lista blanca de extensiones. Es blanca y no negra a propósito: una lista
    /// negra deja pasar todo lo que nadie pensó en prohibir.
    /// </summary>
    public static readonly Dictionary<string, string> TiposPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".webp"] = "image/webp",
        [".bmp"] = "image/bmp",
        [".tif"] = "image/tiff",
        [".tiff"] = "image/tiff"
    };

    public static readonly string[] EntidadesValidas =
        ["CLIENTE", "PROVEEDOR", "VENTA", "COMPRA", "PRODUCTO"];

    public const string FiltroDialogo =
        "Facturas e imágenes|*.pdf;*.png;*.jpg;*.jpeg;*.webp;*.bmp;*.tif;*.tiff|" +
        "PDF|*.pdf|Imágenes|*.png;*.jpg;*.jpeg;*.webp;*.bmp;*.tif;*.tiff";
}
