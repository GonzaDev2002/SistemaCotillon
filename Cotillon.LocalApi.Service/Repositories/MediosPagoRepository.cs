using Cotillon.Data;
using Cotillon.Shared.DTOs.Usuarios;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class MediosPagoRepository
{
    private readonly IDbConnectionFactory _factory;

    public MediosPagoRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public List<MedioPagoDto> ObtenerTodos(bool incluirInactivos)
    {
        using var conexion = _factory.Crear();

        return conexion.Query<MedioPagoDto>(
            $"""
            SELECT MedioPagoId, NombreMedioPago, AfectaEfectivo, OrdenVisual, Activo
            FROM MediosPago
            {(incluirInactivos ? string.Empty : "WHERE Activo = 1")}
            ORDER BY OrdenVisual, NombreMedioPago;
            """).ToList();
    }
}
