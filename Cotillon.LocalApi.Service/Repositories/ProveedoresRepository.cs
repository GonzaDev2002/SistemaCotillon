using Cotillon.Data;
using Cotillon.Shared.DTOs.Terceros;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class ProveedoresRepository
{
    private readonly IDbConnectionFactory _factory;

    public ProveedoresRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<ProveedorDto> Buscar(string? texto, bool incluirInactivos)
    {
        using var conexion = _factory.Crear();

        var filtros = new List<string>();

        if (!incluirInactivos)
            filtros.Add("Activo = 1");

        if (!string.IsNullOrWhiteSpace(texto))
            filtros.Add("(NombreProveedor LIKE @Texto OR Cuit LIKE @Texto OR Telefono LIKE @Texto)");

        var where = filtros.Count > 0 ? $"WHERE {string.Join(" AND ", filtros)}" : string.Empty;

        return conexion.Query<ProveedorDto>(
            $"""
            SELECT ProveedorId, NombreProveedor, Cuit, Telefono, Email, Direccion,
                   Observaciones, Activo, FechaAlta
            FROM Proveedores
            {where}
            ORDER BY NombreProveedor;
            """,
            new { Texto = $"%{texto?.Trim()}%" }).ToList();
    }

    public ProveedorDto? ObtenerPorId(int proveedorId)
    {
        using var conexion = _factory.Crear();

        return conexion.QuerySingleOrDefault<ProveedorDto>(
            """
            SELECT ProveedorId, NombreProveedor, Cuit, Telefono, Email, Direccion,
                   Observaciones, Activo, FechaAlta
            FROM Proveedores
            WHERE ProveedorId = @Id;
            """,
            new { Id = proveedorId });
    }

    public int Insertar(ProveedorDto proveedor, int usuarioId)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var id = conexion.ExecuteScalar<int>(
            """
            INSERT INTO Proveedores
                (NombreProveedor, Cuit, Telefono, Email, Direccion, Observaciones, Activo)
            VALUES
                (@NombreProveedor, @Cuit, @Telefono, @Email, @Direccion, @Observaciones, @Activo);
            SELECT last_insert_rowid();
            """,
            Parametros(proveedor), tx);

        Auditoria.Registrar(conexion, tx, usuarioId, "Proveedores", "ALTA", id, proveedor.NombreProveedor);

        tx.Commit();

        return id;
    }

    public bool Actualizar(ProveedorDto proveedor, int usuarioId)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var filas = conexion.Execute(
            """
            UPDATE Proveedores SET
                NombreProveedor = @NombreProveedor,
                Cuit = @Cuit,
                Telefono = @Telefono,
                Email = @Email,
                Direccion = @Direccion,
                Observaciones = @Observaciones,
                Activo = @Activo,
                FechaActualizacion = datetime('now', 'localtime')
            WHERE ProveedorId = @ProveedorId;
            """,
            Parametros(proveedor), tx);

        if (filas > 0)
            Auditoria.Registrar(conexion, tx, usuarioId, "Proveedores", "MODIFICACION", proveedor.ProveedorId,
                proveedor.NombreProveedor);

        tx.Commit();

        return filas > 0;
    }

    public bool Desactivar(int proveedorId, int usuarioId)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var filas = conexion.Execute(
            """
            UPDATE Proveedores
            SET Activo = 0, FechaActualizacion = datetime('now', 'localtime')
            WHERE ProveedorId = @Id;
            """,
            new { Id = proveedorId }, tx);

        if (filas > 0)
            Auditoria.Registrar(conexion, tx, usuarioId, "Proveedores", "BAJA", proveedorId);

        tx.Commit();

        return filas > 0;
    }

    private static object Parametros(ProveedorDto proveedor) => new
    {
        proveedor.ProveedorId,
        NombreProveedor = proveedor.NombreProveedor.Trim(),
        Cuit = Opcional(proveedor.Cuit),
        Telefono = Opcional(proveedor.Telefono),
        Email = Opcional(proveedor.Email),
        Direccion = Opcional(proveedor.Direccion),
        Observaciones = Opcional(proveedor.Observaciones),
        Activo = proveedor.Activo ? 1 : 0
    };

    private static string? Opcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
}
