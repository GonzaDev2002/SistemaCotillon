using Cotillon.Data;
using Cotillon.Shared.DTOs.Terceros;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class ClientesRepository
{
    private readonly IDbConnectionFactory _factory;

    public ClientesRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<ClienteDto> Buscar(string? texto, bool incluirInactivos)
    {
        using var conexion = _factory.Crear();

        var filtros = new List<string>();

        if (!incluirInactivos)
            filtros.Add("Activo = 1");

        if (!string.IsNullOrWhiteSpace(texto))
            filtros.Add("(NombreCompleto LIKE @Texto OR Documento LIKE @Texto OR Telefono LIKE @Texto)");

        var where = filtros.Count > 0 ? $"WHERE {string.Join(" AND ", filtros)}" : string.Empty;

        return conexion.Query<ClienteDto>(
            $"""
            SELECT ClienteId, NombreCompleto, Documento, Telefono, Email, Direccion,
                   Observaciones, Activo, FechaAlta
            FROM Clientes
            {where}
            ORDER BY NombreCompleto;
            """,
            new { Texto = $"%{texto?.Trim()}%" }).ToList();
    }

    public ClienteDto? ObtenerPorId(int clienteId)
    {
        using var conexion = _factory.Crear();

        return conexion.QuerySingleOrDefault<ClienteDto>(
            """
            SELECT ClienteId, NombreCompleto, Documento, Telefono, Email, Direccion,
                   Observaciones, Activo, FechaAlta
            FROM Clientes
            WHERE ClienteId = @Id;
            """,
            new { Id = clienteId });
    }

    public int Insertar(ClienteDto cliente, int usuarioId)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var id = conexion.ExecuteScalar<int>(
            """
            INSERT INTO Clientes
                (NombreCompleto, Documento, Telefono, Email, Direccion, Observaciones, Activo)
            VALUES
                (@NombreCompleto, @Documento, @Telefono, @Email, @Direccion, @Observaciones, @Activo);
            SELECT last_insert_rowid();
            """,
            Parametros(cliente), tx);

        Auditoria.Registrar(conexion, tx, usuarioId, "Clientes", "ALTA", id, cliente.NombreCompleto);

        tx.Commit();

        return id;
    }

    public bool Actualizar(ClienteDto cliente, int usuarioId)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var filas = conexion.Execute(
            """
            UPDATE Clientes SET
                NombreCompleto = @NombreCompleto,
                Documento = @Documento,
                Telefono = @Telefono,
                Email = @Email,
                Direccion = @Direccion,
                Observaciones = @Observaciones,
                Activo = @Activo,
                FechaActualizacion = datetime('now', 'localtime')
            WHERE ClienteId = @ClienteId;
            """,
            Parametros(cliente), tx);

        if (filas > 0)
            Auditoria.Registrar(conexion, tx, usuarioId, "Clientes", "MODIFICACION", cliente.ClienteId,
                cliente.NombreCompleto);

        tx.Commit();

        return filas > 0;
    }

    public bool Desactivar(int clienteId, int usuarioId)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var filas = conexion.Execute(
            """
            UPDATE Clientes
            SET Activo = 0, FechaActualizacion = datetime('now', 'localtime')
            WHERE ClienteId = @Id;
            """,
            new { Id = clienteId }, tx);

        if (filas > 0)
            Auditoria.Registrar(conexion, tx, usuarioId, "Clientes", "BAJA", clienteId);

        tx.Commit();

        return filas > 0;
    }

    public bool ExisteDocumento(string documento, int excluirId = 0)
    {
        using var conexion = _factory.Crear();

        return conexion.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM Clientes WHERE Documento = @Documento AND ClienteId <> @ExcluirId;",
            new { Documento = documento, ExcluirId = excluirId }) > 0;
    }

    private static object Parametros(ClienteDto cliente) => new
    {
        cliente.ClienteId,
        NombreCompleto = cliente.NombreCompleto.Trim(),
        Documento = Opcional(cliente.Documento),
        Telefono = Opcional(cliente.Telefono),
        Email = Opcional(cliente.Email),
        Direccion = Opcional(cliente.Direccion),
        Observaciones = Opcional(cliente.Observaciones),
        Activo = cliente.Activo ? 1 : 0
    };

    private static string? Opcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
