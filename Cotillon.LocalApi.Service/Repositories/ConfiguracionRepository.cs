using Cotillon.Data;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class ConfiguracionRepository
{
    private readonly IDbConnectionFactory _factory;

    public ConfiguracionRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public Dictionary<string, string> ObtenerTodo()
    {
        using var conexion = _factory.Crear();

        return conexion.Query<FilaConfiguracion>(
                "SELECT Clave, Valor FROM Configuracion ORDER BY Clave;")
            .ToDictionary(x => x.Clave, x => x.Valor ?? string.Empty, StringComparer.OrdinalIgnoreCase);
    }

    public string? Obtener(string clave)
    {
        using var conexion = _factory.Crear();

        return conexion.ExecuteScalar<string?>(
            "SELECT Valor FROM Configuracion WHERE Clave = @Clave;", new { Clave = clave });
    }

    public bool ObtenerBooleano(string clave, bool porDefecto = false) =>
        bool.TryParse(Obtener(clave), out var valor) ? valor : porDefecto;

    public void Guardar(string clave, string? valor, int usuarioId)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        conexion.Execute(
            """
            INSERT INTO Configuracion (Clave, Valor, FechaActualizacion)
            VALUES (@Clave, @Valor, datetime('now', 'localtime'))
            ON CONFLICT (Clave) DO UPDATE SET
                Valor = excluded.Valor,
                FechaActualizacion = excluded.FechaActualizacion;
            """,
            new { Clave = clave, Valor = valor }, tx);

        Auditoria.Registrar(conexion, tx, usuarioId, "Configuracion", "MODIFICACION", null,
            $"{clave} = {valor}");

        tx.Commit();
    }

    private sealed class FilaConfiguracion
    {
        public string Clave { get; init; } = string.Empty;
        public string? Valor { get; init; }
    }
}
