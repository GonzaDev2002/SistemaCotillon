using System.Data;
using Cotillon.Data;
using Cotillon.Shared.DTOs.Caja;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class CajaRepository
{
    private readonly IDbConnectionFactory _factory;

    public CajaRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public SesionCajaDto? ObtenerSesionAbierta()
    {
        using var conexion = _factory.Crear();

        return ObtenerSesionAbierta(conexion, null);
    }

    public static SesionCajaDto? ObtenerSesionAbierta(IDbConnection conexion, IDbTransaction? tx)
    {
        var fila = conexion.QuerySingleOrDefault<FilaSesion>(
            """
            SELECT
                s.SesionCajaId, s.UsuarioAperturaId, s.UsuarioCierreId,
                s.FechaApertura, s.FechaCierre,
                s.MontoInicialCentavos, s.MontoEsperadoCentavos, s.MontoContadoCentavos, s.DiferenciaCentavos,
                s.Estado, s.Observaciones,
                ua.NombreCompleto AS UsuarioApertura,
                uc.NombreCompleto AS UsuarioCierre
            FROM SesionesCaja s
            JOIN Usuarios ua ON s.UsuarioAperturaId = ua.UsuarioId
            LEFT JOIN Usuarios uc ON s.UsuarioCierreId = uc.UsuarioId
            WHERE s.Estado = 'ABIERTA';
            """, transaction: tx);

        return fila is null ? null : Mapear(fila);
    }

    public SesionCajaDto? ObtenerPorId(int sesionCajaId)
    {
        using var conexion = _factory.Crear();

        var fila = conexion.QuerySingleOrDefault<FilaSesion>(
            """
            SELECT
                s.SesionCajaId, s.UsuarioAperturaId, s.UsuarioCierreId,
                s.FechaApertura, s.FechaCierre,
                s.MontoInicialCentavos, s.MontoEsperadoCentavos, s.MontoContadoCentavos, s.DiferenciaCentavos,
                s.Estado, s.Observaciones,
                ua.NombreCompleto AS UsuarioApertura,
                uc.NombreCompleto AS UsuarioCierre
            FROM SesionesCaja s
            JOIN Usuarios ua ON s.UsuarioAperturaId = ua.UsuarioId
            LEFT JOIN Usuarios uc ON s.UsuarioCierreId = uc.UsuarioId
            WHERE s.SesionCajaId = @Id;
            """, new { Id = sesionCajaId });

        return fila is null ? null : Mapear(fila);
    }

    public List<SesionCajaDto> ObtenerHistorial(DateTime desde, DateTime hasta)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaSesion>(
            """
            SELECT
                s.SesionCajaId, s.UsuarioAperturaId, s.UsuarioCierreId,
                s.FechaApertura, s.FechaCierre,
                s.MontoInicialCentavos, s.MontoEsperadoCentavos, s.MontoContadoCentavos, s.DiferenciaCentavos,
                s.Estado, s.Observaciones,
                ua.NombreCompleto AS UsuarioApertura,
                uc.NombreCompleto AS UsuarioCierre
            FROM SesionesCaja s
            JOIN Usuarios ua ON s.UsuarioAperturaId = ua.UsuarioId
            LEFT JOIN Usuarios uc ON s.UsuarioCierreId = uc.UsuarioId
            WHERE s.FechaApertura BETWEEN @Desde AND @Hasta
            ORDER BY s.FechaApertura DESC;
            """,
            new { Desde = Fechas.Inicio(desde), Hasta = Fechas.Fin(hasta) });

        return filas.Select(Mapear).ToList();
    }

    public int Abrir(int usuarioId, decimal montoInicial, string? observaciones)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.BeginTransaction();

        var sesionId = conexion.ExecuteScalar<int>(
            """
            INSERT INTO SesionesCaja (UsuarioAperturaId, MontoInicialCentavos, Estado, Observaciones)
            VALUES (@UsuarioId, @Monto, 'ABIERTA', @Observaciones);
            SELECT last_insert_rowid();
            """,
            new { UsuarioId = usuarioId, Monto = Money.ToCentavos(montoInicial), Observaciones = observaciones },
            tx);

        conexion.Execute(
            """
            INSERT INTO MovimientosCaja
                (SesionCajaId, UsuarioId, TipoMovimiento, ImporteCentavos, Motivo, OrigenTipo)
            VALUES
                (@SesionId, @UsuarioId, 'APERTURA', @Monto, 'Apertura de caja', 'MANUAL');
            """,
            new { SesionId = sesionId, UsuarioId = usuarioId, Monto = Money.ToCentavos(montoInicial) },
            tx);

        Auditoria.Registrar(conexion, tx, usuarioId, "SesionesCaja", "APERTURA_CAJA", sesionId,
            $"Apertura con monto inicial {montoInicial:C2}");

        tx.Commit();

        return sesionId;
    }

    public void Cerrar(int sesionCajaId, int usuarioId, decimal montoContado, decimal montoEsperado, string? observaciones)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.BeginTransaction();

        var esperado = Money.ToCentavos(montoEsperado);
        var contado = Money.ToCentavos(montoContado);

        conexion.Execute(
            """
            UPDATE SesionesCaja SET
                UsuarioCierreId = @UsuarioId,
                FechaCierre = datetime('now', 'localtime'),
                MontoEsperadoCentavos = @Esperado,
                MontoContadoCentavos = @Contado,
                DiferenciaCentavos = @Diferencia,
                Estado = 'CERRADA',
                Observaciones = COALESCE(@Observaciones, Observaciones)
            WHERE SesionCajaId = @SesionId;
            """,
            new
            {
                SesionId = sesionCajaId,
                UsuarioId = usuarioId,
                Esperado = esperado,
                Contado = contado,
                Diferencia = contado - esperado,
                Observaciones = observaciones
            }, tx);

        conexion.Execute(
            """
            INSERT INTO MovimientosCaja
                (SesionCajaId, UsuarioId, TipoMovimiento, ImporteCentavos, Motivo, OrigenTipo)
            VALUES
                (@SesionId, @UsuarioId, 'CIERRE', 0, 'Cierre de caja', 'MANUAL');
            """,
            new { SesionId = sesionCajaId, UsuarioId = usuarioId }, tx);

        Auditoria.Registrar(conexion, tx, usuarioId, "SesionesCaja", "CIERRE_CAJA", sesionCajaId,
            $"Cierre. Esperado {montoEsperado:C2}, contado {montoContado:C2}, diferencia {montoContado - montoEsperado:C2}");

        tx.Commit();
    }

    public void RegistrarMovimiento(int sesionCajaId, RegistrarMovimientoCajaDto movimiento)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.BeginTransaction();

        // Egresos y retiros restan del saldo, por eso se guardan en negativo.
        var signo = movimiento.TipoMovimiento is "EGRESO" or "RETIRO" ? -1 : 1;

        conexion.Execute(
            """
            INSERT INTO MovimientosCaja
                (SesionCajaId, UsuarioId, TipoMovimiento, ImporteCentavos, Motivo, OrigenTipo, Observaciones)
            VALUES
                (@SesionId, @UsuarioId, @Tipo, @Importe, @Motivo, 'MANUAL', @Observaciones);
            """,
            new
            {
                SesionId = sesionCajaId,
                movimiento.UsuarioId,
                Tipo = movimiento.TipoMovimiento,
                Importe = signo * Money.ToCentavos(movimiento.Importe),
                movimiento.Motivo,
                movimiento.Observaciones
            }, tx);

        Auditoria.Registrar(conexion, tx, movimiento.UsuarioId, "MovimientosCaja", "ALTA", sesionCajaId,
            $"{movimiento.TipoMovimiento} de {movimiento.Importe:C2}: {movimiento.Motivo}");

        tx.Commit();
    }

    public List<MovimientoCajaDto> ObtenerMovimientos(int sesionCajaId)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaMovimiento>(
            """
            SELECT
                m.MovimientoCajaId, m.SesionCajaId, m.FechaMovimiento, m.TipoMovimiento,
                m.ImporteCentavos, m.Motivo, m.Observaciones,
                u.NombreCompleto AS Usuario,
                mp.NombreMedioPago AS MedioPago
            FROM MovimientosCaja m
            JOIN Usuarios u ON m.UsuarioId = u.UsuarioId
            LEFT JOIN MediosPago mp ON m.MedioPagoId = mp.MedioPagoId
            WHERE m.SesionCajaId = @SesionId
            ORDER BY m.FechaMovimiento, m.MovimientoCajaId;
            """,
            new { SesionId = sesionCajaId });

        return filas.Select(f => new MovimientoCajaDto
        {
            MovimientoCajaId = f.MovimientoCajaId,
            SesionCajaId = f.SesionCajaId,
            Usuario = f.Usuario,
            FechaMovimiento = f.FechaMovimiento,
            TipoMovimiento = f.TipoMovimiento,
            MedioPago = f.MedioPago,
            Importe = Money.FromCentavos(f.ImporteCentavos),
            Motivo = f.Motivo,
            Observaciones = f.Observaciones
        }).ToList();
    }

    public ArqueoCajaDto ObtenerArqueo(SesionCajaDto sesion)
    {
        using var conexion = _factory.Crear();

        var ventas = conexion.QuerySingle<FilaTotalVentas>(
            """
            SELECT
                COUNT(*)                              AS Cantidad,
                COALESCE(SUM(TotalVentaCentavos), 0)  AS TotalCentavos
            FROM Ventas
            WHERE SesionCajaId = @SesionId AND Estado = 'COMPLETADA';
            """,
            new { SesionId = sesion.SesionCajaId });

        var porMedio = conexion.Query<FilaMedioPago>(
            """
            SELECT
                mp.NombreMedioPago                        AS MedioPago,
                mp.AfectaEfectivo                         AS AfectaEfectivo,
                COALESCE(SUM(pv.ImporteCentavos - pv.VueltoCentavos), 0) AS TotalCentavos,
                COUNT(*)                                  AS Cantidad
            FROM PagosVenta pv
            JOIN Ventas v      ON pv.VentaId = v.VentaId
            JOIN MediosPago mp ON pv.MedioPagoId = mp.MedioPagoId
            WHERE v.SesionCajaId = @SesionId AND v.Estado = 'COMPLETADA'
            GROUP BY mp.NombreMedioPago, mp.AfectaEfectivo
            ORDER BY mp.NombreMedioPago;
            """,
            new { SesionId = sesion.SesionCajaId }).ToList();

        var movimientos = conexion.QuerySingle<FilaTotalMovimientos>(
            """
            SELECT
                COALESCE(SUM(CASE WHEN TipoMovimiento = 'INGRESO' THEN ImporteCentavos ELSE 0 END), 0) AS IngresosCentavos,
                COALESCE(SUM(CASE WHEN TipoMovimiento IN ('EGRESO', 'RETIRO') THEN -ImporteCentavos ELSE 0 END), 0) AS EgresosCentavos
            FROM MovimientosCaja
            WHERE SesionCajaId = @SesionId;
            """,
            new { SesionId = sesion.SesionCajaId });

        var efectivoDeVentas = porMedio.Where(m => m.AfectaEfectivo).Sum(m => m.TotalCentavos);

        var efectivoEsperado =
            Money.ToCentavos(sesion.MontoInicial)
            + efectivoDeVentas
            + movimientos.IngresosCentavos
            - movimientos.EgresosCentavos;

        return new ArqueoCajaDto
        {
            Sesion = sesion,
            CantidadVentas = ventas.Cantidad,
            TotalVentas = Money.FromCentavos(ventas.TotalCentavos),
            TotalIngresos = Money.FromCentavos(movimientos.IngresosCentavos),
            TotalEgresos = Money.FromCentavos(movimientos.EgresosCentavos),
            EfectivoEsperado = Money.FromCentavos(efectivoEsperado),
            TotalesPorMedioPago = porMedio.Select(m => new TotalPorMedioPagoDto
            {
                MedioPago = m.MedioPago,
                AfectaEfectivo = m.AfectaEfectivo,
                Total = Money.FromCentavos(m.TotalCentavos),
                Cantidad = m.Cantidad
            }).ToList()
        };
    }

    private static SesionCajaDto Mapear(FilaSesion f) => new()
    {
        SesionCajaId = f.SesionCajaId,
        UsuarioAperturaId = f.UsuarioAperturaId,
        UsuarioApertura = f.UsuarioApertura,
        UsuarioCierreId = f.UsuarioCierreId,
        UsuarioCierre = f.UsuarioCierre,
        FechaApertura = f.FechaApertura,
        FechaCierre = f.FechaCierre,
        MontoInicial = Money.FromCentavos(f.MontoInicialCentavos),
        MontoEsperado = f.MontoEsperadoCentavos is null ? null : Money.FromCentavos(f.MontoEsperadoCentavos.Value),
        MontoContado = f.MontoContadoCentavos is null ? null : Money.FromCentavos(f.MontoContadoCentavos.Value),
        Diferencia = f.DiferenciaCentavos is null ? null : Money.FromCentavos(f.DiferenciaCentavos.Value),
        Estado = f.Estado,
        Observaciones = f.Observaciones
    };

    private sealed class FilaSesion
    {
        public int SesionCajaId { get; init; }
        public int UsuarioAperturaId { get; init; }
        public string UsuarioApertura { get; init; } = string.Empty;
        public int? UsuarioCierreId { get; init; }
        public string? UsuarioCierre { get; init; }
        public DateTime FechaApertura { get; init; }
        public DateTime? FechaCierre { get; init; }
        public long MontoInicialCentavos { get; init; }
        public long? MontoEsperadoCentavos { get; init; }
        public long? MontoContadoCentavos { get; init; }
        public long? DiferenciaCentavos { get; init; }
        public string Estado { get; init; } = string.Empty;
        public string? Observaciones { get; init; }
    }

    private sealed class FilaMovimiento
    {
        public int MovimientoCajaId { get; init; }
        public int SesionCajaId { get; init; }
        public string Usuario { get; init; } = string.Empty;
        public DateTime FechaMovimiento { get; init; }
        public string TipoMovimiento { get; init; } = string.Empty;
        public string? MedioPago { get; init; }
        public long ImporteCentavos { get; init; }
        public string Motivo { get; init; } = string.Empty;
        public string? Observaciones { get; init; }
    }

    private sealed class FilaTotalVentas
    {
        public int Cantidad { get; init; }
        public long TotalCentavos { get; init; }
    }

    private sealed class FilaTotalMovimientos
    {
        public long IngresosCentavos { get; init; }
        public long EgresosCentavos { get; init; }
    }

    private sealed class FilaMedioPago
    {
        public string MedioPago { get; init; } = string.Empty;
        public bool AfectaEfectivo { get; init; }
        public long TotalCentavos { get; init; }
        public int Cantidad { get; init; }
    }
}
