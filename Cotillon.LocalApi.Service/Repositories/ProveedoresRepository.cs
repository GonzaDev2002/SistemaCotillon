using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs.Proveedores;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class ProveedoresRepository
{
    private readonly AccessDb _db;

    public ProveedoresRepository(AccessDb db)
    {
        _db = db;
    }

    public async Task<List<ProveedorDto>> ListarAsync()
    {
        List<ProveedorDto> lista = new();

        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            SELECT ProveedorId, NombreProveedor, Telefono, Email, Direccion, Cuit, Observaciones, Activo
            FROM Proveedores
            WHERE Activo = True
            ORDER BY NombreProveedor";

        using var cmd = new OleDbCommand(sql, conexion);
        using var lector = await cmd.ExecuteReaderAsync();

        while (await lector.ReadAsync())
        {
            lista.Add(new ProveedorDto
            {
                ProveedorId = Convert.ToInt32(lector["ProveedorId"]),
                NombreProveedor = lector["NombreProveedor"]?.ToString() ?? "",
                Telefono = lector["Telefono"]?.ToString() ?? "",
                Email = lector["Email"]?.ToString() ?? "",
                Direccion = lector["Direccion"]?.ToString() ?? "",
                Cuit = lector["Cuit"]?.ToString() ?? "",
                Observaciones = lector["Observaciones"]?.ToString() ?? "",
                Activo = lector["Activo"] != DBNull.Value && Convert.ToBoolean(lector["Activo"])
            });
        }

        return lista;
    }

    public async Task<int> CrearAsync(ProveedorDto proveedor)
    {
        using var conexion = (OleDbConnection)_db.CreateConnection();
        await conexion.OpenAsync();

        string sql = @"
            INSERT INTO Proveedores
            (NombreProveedor, Telefono, Email, Direccion, Cuit, Observaciones, Activo, FechaAlta, FechaActualizacion)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?)";

        using var cmd = new OleDbCommand(sql, conexion);
        cmd.Parameters.Add("@NombreProveedor", OleDbType.VarWChar).Value = proveedor.NombreProveedor;
        cmd.Parameters.Add("@Telefono", OleDbType.VarWChar).Value = proveedor.Telefono ?? "";
        cmd.Parameters.Add("@Email", OleDbType.VarWChar).Value = proveedor.Email ?? "";
        cmd.Parameters.Add("@Direccion", OleDbType.VarWChar).Value = proveedor.Direccion ?? "";
        cmd.Parameters.Add("@Cuit", OleDbType.VarWChar).Value = proveedor.Cuit ?? "";
        cmd.Parameters.Add("@Observaciones", OleDbType.LongVarWChar).Value = proveedor.Observaciones ?? "";
        cmd.Parameters.Add("@Activo", OleDbType.Boolean).Value = true;
        cmd.Parameters.Add("@FechaAlta", OleDbType.Date).Value = DateTime.Now;
        cmd.Parameters.Add("@FechaActualizacion", OleDbType.Date).Value = DateTime.Now;

        await cmd.ExecuteNonQueryAsync();

        using var cmdId = new OleDbCommand("SELECT @@IDENTITY", conexion);
        return Convert.ToInt32(await cmdId.ExecuteScalarAsync());
    }
}