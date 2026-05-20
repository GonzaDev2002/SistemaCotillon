using Cotillon.LocalApi.Service.Data;
using Cotillon.Shared.DTOs;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Repositories;

public class SeccionesRepository
{
    private readonly AccessDb _accessDb;

    public SeccionesRepository(AccessDb accessDb)
    {
        _accessDb = accessDb;
    }

    public List<SeccionDto> ObtenerTodas()
    {
        var lista = new List<SeccionDto>();

        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
SELECT [SeccionId], [NombreSeccion], [Activo]
FROM [Secciones]
ORDER BY [NombreSeccion]";

        using var command = new OleDbCommand(sql, connection);
        using var reader = command.ExecuteReader();

        while (reader != null && reader.Read())
        {
            lista.Add(new SeccionDto
            {
                SeccionId = Convert.ToInt32(reader["SeccionId"]),
                NombreSeccion = reader["NombreSeccion"]?.ToString() ?? string.Empty,
                Activo = reader["Activo"] != DBNull.Value && Convert.ToBoolean(reader["Activo"])
            });
        }

        return lista;
    }

    public bool Insertar(SeccionDto seccion)
    {
        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
INSERT INTO [Secciones] ([NombreSeccion], [Activo])
VALUES (?, ?)";

        using var command = new OleDbCommand(sql, connection);
        command.Parameters.AddWithValue("?", seccion.NombreSeccion);
        command.Parameters.AddWithValue("?", seccion.Activo);

        return command.ExecuteNonQuery() > 0;
    }

    public bool Actualizar(SeccionDto seccion)
    {
        using var connection = (OleDbConnection)_accessDb.CreateConnection();
        connection.Open();

        const string sql = @"
UPDATE [Secciones]
SET [NombreSeccion] = ?, [Activo] = ?
WHERE [SeccionId] = ?";

        using var command = new OleDbCommand(sql, connection);
        command.Parameters.AddWithValue("?", seccion.NombreSeccion);
        command.Parameters.AddWithValue("?", seccion.Activo);
        command.Parameters.AddWithValue("?", seccion.SeccionId);

        return command.ExecuteNonQuery() > 0;
    }
}