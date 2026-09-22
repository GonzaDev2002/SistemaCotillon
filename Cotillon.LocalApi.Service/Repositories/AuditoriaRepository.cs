using Cotillon.Data;
using Cotillon.Shared.DTOs.Auditoria;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class AuditoriaRepository
{
    private readonly IDbConnectionFactory _factory;

    public AuditoriaRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<EventoAuditoriaDto> Buscar(
        DateTime desde, DateTime hasta, int? usuarioId, string? tabla, string? accion, int limite)
    {
        using var conexion = _factory.Crear();

        var filtros = new List<string> { "a.FechaEvento BETWEEN @Desde AND @Hasta" };

        if (usuarioId.HasValue)
            filtros.Add("a.UsuarioId = @UsuarioId");

        if (!string.IsNullOrWhiteSpace(tabla))
            filtros.Add("a.TablaAfectada = @Tabla");

        if (!string.IsNullOrWhiteSpace(accion))
            filtros.Add("a.Accion = @Accion");

        return conexion.Query<EventoAuditoriaDto>(
            $"""
            SELECT
                a.AuditoriaId, a.UsuarioId, a.FechaEvento, a.TablaAfectada,
                a.Accion, a.RegistroId, a.Detalle, a.Terminal,
                COALESCE(u.NombreCompleto, '(usuario eliminado)') AS Usuario
            FROM Auditoria a
            LEFT JOIN Usuarios u ON a.UsuarioId = u.UsuarioId
            WHERE {string.Join(" AND ", filtros)}
            ORDER BY a.FechaEvento DESC, a.AuditoriaId DESC
            LIMIT @Limite;
            """,
            new
            {
                Desde = Fechas.Inicio(desde),
                Hasta = Fechas.Fin(hasta),
                UsuarioId = usuarioId,
                Tabla = tabla,
                Accion = accion,
                Limite = limite
            }).ToList();
    }
}
