using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class CategoriasRepository
{
    private readonly AccessDb _accessDb;

    public CategoriasRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public List<CategoriaDto> ObtenerTodas()
    {
        var lista = new List<CategoriaDto>();

        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
SELECT [CategoriaId], [SeccionId], [NombreCategoria], [Activo]
FROM [Categorias]
ORDER BY [NombreCategoria]";

        using var command = new OleDbCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader != null && reader.Read())
        {
            lista.Add(new CategoriaDto
            {
                CategoriaId = Convert.ToInt32(reader["CategoriaId"]),
                SeccionId = Convert.ToInt32(reader["SeccionId"]),
                NombreCategoria = reader["NombreCategoria"]?.ToString() ?? string.Empty,
                Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"])
            });
        }

        return lista;
    }

    public List<CategoriaDto> ObtenerPorSeccion(int seccionId)
    {
        var lista = new List<CategoriaDto>();

        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
SELECT [CategoriaId], [SeccionId], [NombreCategoria], [Activo]
FROM [Categorias]
WHERE [SeccionId] = ?
ORDER BY [NombreCategoria]";

        using var command = new OleDbCommand(sql, connection);
        command.Parameters.AddWithValue("?", seccionId);

        using var reader = command.ExecuteReader();

        while (reader != null && reader.Read())
        {
            lista.Add(new CategoriaDto
            {
                CategoriaId = Convert.ToInt32(reader["CategoriaId"]),
                SeccionId = Convert.ToInt32(reader["SeccionId"]),
                NombreCategoria = reader["NombreCategoria"]?.ToString() ?? string.Empty,
                Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"])
            });
        }

        return lista;
    }

    public bool Insertar(CategoriaDto categoria)
    {
        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
INSERT INTO [Categorias] ([SeccionId], [NombreCategoria], [Activo])
VALUES (?, ?, ?)";

        using var command = new OleDbCommand(sql, connection);
        command.Parameters.AddWithValue("?", categoria.SeccionId);
        command.Parameters.AddWithValue("?", categoria.NombreCategoria);
        command.Parameters.AddWithValue("?", categoria.Activo);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Actualizar(CategoriaDto categoria)
    {
        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
UPDATE [Categorias]
SET [SeccionId] = ?, [NombreCategoria] = ?, [Activo] = ?
WHERE [CategoriaId] = ?";

        using var command = new OleDbCommand(sql, connection);
        command.Parameters.AddWithValue("?", categoria.SeccionId);
        command.Parameters.AddWithValue("?", categoria.NombreCategoria);
        command.Parameters.AddWithValue("?", categoria.Activo);
        command.Parameters.AddWithValue("?", categoria.CategoriaId);

        return command.ExecuteNonQuery() > 0;
    }
}