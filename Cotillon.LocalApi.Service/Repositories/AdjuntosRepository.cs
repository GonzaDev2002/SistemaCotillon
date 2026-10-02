using Cotillon.Data;
using Cotillon.Shared.DTOs.Adjuntos;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class AdjuntosRepository
{
    private readonly IDbConnectionFactory _factory;

    public AdjuntosRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<AdjuntoDto> ObtenerPorEntidad(string entidadTipo, int entidadId)
    {
        using var conexion = _factory.Crear();

        return conexion.Query<AdjuntoDto>(
            """
            SELECT a.AdjuntoId, a.EntidadTipo, a.EntidadId, a.NombreArchivo, a.TipoContenido,
                   a.TamanoBytes, a.Descripcion, a.FechaAlta,
                   u.NombreCompleto AS Usuario
            FROM Adjuntos a
            LEFT JOIN Usuarios u ON a.UsuarioId = u.UsuarioId
            WHERE a.EntidadTipo = @EntidadTipo AND a.EntidadId = @EntidadId
            ORDER BY a.FechaAlta DESC, a.AdjuntoId DESC;
            """,
            new { EntidadTipo = entidadTipo, EntidadId = entidadId }).ToList();
    }

    /// <summary>
    /// Devuelve el adjunto junto con su nombre en disco, que no viaja en el DTO:
    /// el cliente nunca necesita conocer la ruta real del archivo.
    /// </summary>
    public (AdjuntoDto Adjunto, string NombreEnDisco)? ObtenerPorId(int adjuntoId)
    {
        using var conexion = _factory.Crear();

        var fila = conexion.QuerySingleOrDefault<FilaAdjunto>(
            """
            SELECT a.AdjuntoId, a.EntidadTipo, a.EntidadId, a.NombreArchivo, a.NombreEnDisco,
                   a.TipoContenido, a.TamanoBytes, a.Descripcion, a.FechaAlta,
                   u.NombreCompleto AS Usuario
            FROM Adjuntos a
            LEFT JOIN Usuarios u ON a.UsuarioId = u.UsuarioId
            WHERE a.AdjuntoId = @AdjuntoId;
            """,
            new { AdjuntoId = adjuntoId });

        if (fila is null)
            return null;

        return (new AdjuntoDto
        {
            AdjuntoId = fila.AdjuntoId,
            EntidadTipo = fila.EntidadTipo,
            EntidadId = fila.EntidadId,
            NombreArchivo = fila.NombreArchivo,
            TipoContenido = fila.TipoContenido,
            TamanoBytes = fila.TamanoBytes,
            Descripcion = fila.Descripcion,
            Usuario = fila.Usuario,
            FechaAlta = fila.FechaAlta
        }, fila.NombreEnDisco);
    }

    public int Insertar(AdjuntoDto adjunto, string nombreEnDisco, int usuarioId)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var id = conexion.ExecuteScalar<int>(
            """
            INSERT INTO Adjuntos
                (EntidadTipo, EntidadId, NombreArchivo, NombreEnDisco, TipoContenido,
                 TamanoBytes, Descripcion, UsuarioId)
            VALUES
                (@EntidadTipo, @EntidadId, @NombreArchivo, @NombreEnDisco, @TipoContenido,
                 @TamanoBytes, @Descripcion, @UsuarioId);
            SELECT last_insert_rowid();
            """,
            new
            {
                adjunto.EntidadTipo,
                adjunto.EntidadId,
                adjunto.NombreArchivo,
                NombreEnDisco = nombreEnDisco,
                adjunto.TipoContenido,
                adjunto.TamanoBytes,
                Descripcion = string.IsNullOrWhiteSpace(adjunto.Descripcion) ? null : adjunto.Descripcion.Trim(),
                UsuarioId = usuarioId > 0 ? usuarioId : (int?)null
            },
            tx);

        Auditoria.Registrar(conexion, tx, usuarioId > 0 ? usuarioId : null, "Adjuntos", "ALTA", id,
            $"{adjunto.EntidadTipo} #{adjunto.EntidadId}: {adjunto.NombreArchivo}");

        tx.Commit();

        return id;
    }

    public bool Eliminar(int adjuntoId, int usuarioId, string detalle)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var filas = conexion.Execute(
            "DELETE FROM Adjuntos WHERE AdjuntoId = @AdjuntoId;",
            new { AdjuntoId = adjuntoId }, tx);

        if (filas == 0)
        {
            tx.Rollback();
            return false;
        }

        Auditoria.Registrar(conexion, tx, usuarioId > 0 ? usuarioId : null, "Adjuntos", "BAJA",
            adjuntoId, detalle);

        tx.Commit();

        return true;
    }

    /// <summary>
    /// Verifica que el registro dueño del adjunto exista. El nombre de tabla y de
    /// columna salen de un switch cerrado, nunca del texto que llega por la API.
    /// </summary>
    public bool ExisteEntidad(string entidadTipo, int entidadId)
    {
        var (tabla, clave) = entidadTipo switch
        {
            "CLIENTE" => ("Clientes", "ClienteId"),
            "PROVEEDOR" => ("Proveedores", "ProveedorId"),
            "VENTA" => ("Ventas", "VentaId"),
            "COMPRA" => ("Compras", "CompraId"),
            "PRODUCTO" => ("Productos", "ProductoId"),
            _ => (string.Empty, string.Empty)
        };

        if (tabla.Length == 0)
            return false;

        using var conexion = _factory.Crear();

        return conexion.ExecuteScalar<long>(
            $"SELECT COUNT(*) FROM {tabla} WHERE {clave} = @Id;", new { Id = entidadId }) > 0;
    }

    private sealed class FilaAdjunto
    {
        public int AdjuntoId { get; init; }
        public string EntidadTipo { get; init; } = string.Empty;
        public int EntidadId { get; init; }
        public string NombreArchivo { get; init; } = string.Empty;
        public string NombreEnDisco { get; init; } = string.Empty;
        public string TipoContenido { get; init; } = string.Empty;
        public long TamanoBytes { get; init; }
        public string? Descripcion { get; init; }
        public string? Usuario { get; init; }
        public DateTime FechaAlta { get; init; }
    }
}
