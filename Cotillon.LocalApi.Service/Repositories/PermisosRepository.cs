using System.Data;
using Cotillon.Data;
using Cotillon.Shared.DTOs.Permisos;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class PermisosRepository
{
    private readonly IDbConnectionFactory _factory;

    public PermisosRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public Dictionary<string, NivelAcceso> ObtenerDe(int usuarioId)
    {
        using var conexion = _factory.Crear();

        return Leer(conexion, usuarioId, null);
    }

    /// <summary>
    /// Reemplaza todos los permisos del usuario de una sola vez. Es un reemplazo y
    /// no una actualización fila por fila para que nunca quede una mezcla de los
    /// permisos viejos con los nuevos si algo falla en el medio.
    /// </summary>
    public void Guardar(
        int usuarioId, Dictionary<string, NivelAcceso> permisos, int usuarioEjecutor, bool auditar = true)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var anteriores = Leer(conexion, usuarioId, tx);

        conexion.Execute(
            "DELETE FROM PermisosUsuario WHERE UsuarioId = @UsuarioId;",
            new { UsuarioId = usuarioId }, tx);

        conexion.Execute(
            """
            INSERT INTO PermisosUsuario (UsuarioId, Modulo, Nivel)
            VALUES (@UsuarioId, @Modulo, @Nivel);
            """,
            permisos.Select(p => new
            {
                UsuarioId = usuarioId,
                Modulo = p.Key,
                Nivel = p.Value.ToString().ToUpperInvariant()
            }).ToList(),
            tx);

        var cambios = permisos
            .Where(p => !anteriores.TryGetValue(p.Key, out var antes) || antes != p.Value)
            .Select(p => $"{Modulos.Nombre(p.Key)}: " +
                         $"{(anteriores.TryGetValue(p.Key, out var antes) ? antes : NivelAcceso.Ninguno)} → {p.Value}")
            .ToList();

        // Sin cambios no se registra nada: el historial sirve si no está lleno de
        // "guardó y dejó todo igual".
        if (auditar && cambios.Count > 0)
            Auditoria.Registrar(conexion, tx, usuarioEjecutor, "PermisosUsuario", "MODIFICACION",
                usuarioId, string.Join(" | ", cambios));

        tx.Commit();
    }

    private static Dictionary<string, NivelAcceso> Leer(
        IDbConnection conexion, int usuarioId, IDbTransaction? tx)
    {
        // Clase y no ValueTuple: Dapper no mapea tuplas por nombre de columna.
        var filas = conexion.Query<FilaPermiso>(
            "SELECT Modulo, Nivel FROM PermisosUsuario WHERE UsuarioId = @UsuarioId;",
            new { UsuarioId = usuarioId }, tx);

        var permisos = new Dictionary<string, NivelAcceso>(StringComparer.OrdinalIgnoreCase);

        foreach (var fila in filas)
        {
            if (Enum.TryParse<NivelAcceso>(fila.Nivel, ignoreCase: true, out var nivel))
                permisos[fila.Modulo] = nivel;
        }

        // Un módulo sin fila es un módulo sin acceso: al agregar módulos nuevos en
        // una versión posterior, nadie los gana sin que el administrador los dé.
        foreach (var codigo in Modulos.Codigos)
            permisos.TryAdd(codigo, NivelAcceso.Ninguno);

        return permisos;
    }

    private sealed class FilaPermiso
    {
        public string Modulo { get; init; } = string.Empty;
        public string Nivel { get; init; } = string.Empty;
    }
}
