using Cotillon.Data;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class SeccionesRepository
{
    private readonly IDbConnectionFactory _factory;

    public SeccionesRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<SeccionDto> ObtenerTodas()
    {
        using var conexion = _factory.Crear();

        return conexion.Query<SeccionDto>(
            """
            SELECT SeccionId, NombreSeccion, Activo
            FROM Secciones
            ORDER BY NombreSeccion;
            """).ToList();
    }

    public bool Insertar(SeccionDto seccion)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            """
            INSERT INTO Secciones (NombreSeccion, Activo)
            VALUES (@NombreSeccion, @Activo);
            """,
            new { seccion.NombreSeccion, Activo = seccion.Activo ? 1 : 0 }) > 0;
    }

    public bool Actualizar(SeccionDto seccion)
    {
        using var conexion = _factory.Crear();

        return conexion.Execute(
            """
            UPDATE Secciones
            SET NombreSeccion = @NombreSeccion, Activo = @Activo
            WHERE SeccionId = @SeccionId;
            """,
            new { seccion.NombreSeccion, Activo = seccion.Activo ? 1 : 0, seccion.SeccionId }) > 0;
    }

    public bool ExisteNombre(string nombre, int excluirId = 0)
    {
        using var conexion = _factory.Crear();

        return conexion.ExecuteScalar<long>(
            """
            SELECT COUNT(*) FROM Secciones
            WHERE NombreSeccion = @Nombre AND SeccionId <> @ExcluirId;
            """,
            new { Nombre = nombre, ExcluirId = excluirId }) > 0;
    }
}
