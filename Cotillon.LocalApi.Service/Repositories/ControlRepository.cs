using System.Data;
using Cotillon.Data;
using Cotillon.Shared.DTOs.Ventas;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

/// <summary>
/// Los controles sobre quién maneja la plata.
///
/// Hay que ser honesto con el alcance: ningún sistema puede impedir que alguien
/// le cobre al cliente y no registre la venta. Si la mercadería sale por la
/// puerta sin pasar por el lector, no hay software que lo vea. Lo que sí se
/// puede hacer es que cada forma de hacer desaparecer una venta deje rastro, y
/// que ese rastro se pueda mirar junto: un faltante suelto es un error de
/// vuelto, el mismo faltante todos los días es otra cosa.
///
/// Lo que de verdad cierra el círculo es el inventario. Si falta mercadería y no
/// hay venta que la explique, ahí está la diferencia.
/// </summary>
public class ControlRepository
{
    private readonly IDbConnectionFactory _factory;

    public ControlRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Anota una venta que se armó y no se cobró. El motivo es obligatorio a
    /// propósito: obliga a escribir algo en vez de hacer desaparecer la venta en
    /// silencio con una tecla.
    /// </summary>
    public void RegistrarCancelacion(CancelarVentaEnCursoDto datos)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var sesion = CajaRepository.ObtenerSesionAbierta(conexion, tx);

        Auditoria.Registrar(conexion, tx, datos.UsuarioId, "Ventas", "CANCELACION", sesion?.SesionCajaId,
            $"Venta cancelada antes de cobrar por {datos.Total:C2} " +
            $"({datos.CantidadItems} ítem(s)). Motivo: {datos.Motivo}. {datos.Detalle}");

        tx.Commit();
    }

    /// <summary>
    /// Resumen por usuario de todo lo que hace desaparecer plata: faltantes de
    /// caja, ventas canceladas antes de cobrar y ventas anuladas después.
    /// </summary>
    public List<ControlPorUsuarioDto> PorUsuario(DateTime desde, DateTime hasta)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaControl>(
            """
            SELECT
                u.UsuarioId,
                u.NombreCompleto AS Usuario,

                -- Cierres de caja: se cuenta a quien cerró, no a quien abrió.
                (SELECT COUNT(*) FROM SesionesCaja s
                  WHERE s.UsuarioCierreId = u.UsuarioId AND s.Estado = 'CERRADA'
                    AND s.FechaCierre BETWEEN @Desde AND @Hasta)                       AS CajasCerradas,

                (SELECT COUNT(*) FROM SesionesCaja s
                  WHERE s.UsuarioCierreId = u.UsuarioId AND s.Estado = 'CERRADA'
                    AND s.DiferenciaCentavos < 0
                    AND s.FechaCierre BETWEEN @Desde AND @Hasta)                       AS CajasConFaltante,

                COALESCE((SELECT SUM(-s.DiferenciaCentavos) FROM SesionesCaja s
                  WHERE s.UsuarioCierreId = u.UsuarioId AND s.Estado = 'CERRADA'
                    AND s.DiferenciaCentavos < 0
                    AND s.FechaCierre BETWEEN @Desde AND @Hasta), 0)                   AS FaltanteCentavos,

                COALESCE((SELECT SUM(s.DiferenciaCentavos) FROM SesionesCaja s
                  WHERE s.UsuarioCierreId = u.UsuarioId AND s.Estado = 'CERRADA'
                    AND s.DiferenciaCentavos > 0
                    AND s.FechaCierre BETWEEN @Desde AND @Hasta), 0)                   AS SobranteCentavos,

                COALESCE((SELECT MIN(s.DiferenciaCentavos) FROM SesionesCaja s
                  WHERE s.UsuarioCierreId = u.UsuarioId AND s.Estado = 'CERRADA'
                    AND s.FechaCierre BETWEEN @Desde AND @Hasta), 0)                   AS PeorFaltanteCentavos,

                -- Canceladas antes de cobrar: viven en Auditoría porque no llegan
                -- a ser una venta.
                (SELECT COUNT(*) FROM Auditoria a
                  WHERE a.UsuarioId = u.UsuarioId AND a.TablaAfectada = 'Ventas'
                    AND a.Accion = 'CANCELACION'
                    AND a.FechaEvento BETWEEN @Desde AND @Hasta)                         AS VentasCanceladas,

                (SELECT COUNT(*) FROM Ventas v
                  WHERE v.UsuarioAnulacionId = u.UsuarioId AND v.Estado = 'ANULADA'
                    AND v.FechaAnulacion BETWEEN @Desde AND @Hasta)                     AS VentasAnuladas,

                COALESCE((SELECT SUM(v.TotalVentaCentavos) FROM Ventas v
                  WHERE v.UsuarioAnulacionId = u.UsuarioId AND v.Estado = 'ANULADA'
                    AND v.FechaAnulacion BETWEEN @Desde AND @Hasta), 0)                 AS AnuladoCentavos,

                COALESCE((SELECT SUM(v.TotalVentaCentavos) FROM Ventas v
                  WHERE v.UsuarioId = u.UsuarioId AND v.Estado = 'COMPLETADA'
                    AND v.FechaVenta BETWEEN @Desde AND @Hasta), 0)                     AS VendidoCentavos

            FROM Usuarios u
            ORDER BY u.NombreCompleto;
            """,
            new { Desde = Fechas.Inicio(desde), Hasta = Fechas.Fin(hasta) }).ToList();

        // El importe de lo cancelado sale del texto de Auditoría, así que se suma
        // acá en vez de en SQL: no vale la pena una tabla nueva para un dato que
        // sólo se mira en este reporte.
        var cancelados = ImportesCancelados(conexion, desde, hasta);

        return filas
            .Select(f => new ControlPorUsuarioDto
            {
                UsuarioId = f.UsuarioId,
                Usuario = f.Usuario,
                CajasCerradas = f.CajasCerradas,
                CajasConFaltante = f.CajasConFaltante,
                FaltanteAcumulado = Money.FromCentavos(f.FaltanteCentavos),
                SobranteAcumulado = Money.FromCentavos(f.SobranteCentavos),
                PeorFaltante = Money.FromCentavos(Math.Min(f.PeorFaltanteCentavos, 0)),
                VentasCanceladas = f.VentasCanceladas,
                ImporteCancelado = cancelados.GetValueOrDefault(f.UsuarioId),
                VentasAnuladas = f.VentasAnuladas,
                ImporteAnulado = Money.FromCentavos(f.AnuladoCentavos),
                TotalVendido = Money.FromCentavos(f.VendidoCentavos)
            })
            // Sólo quien movió plata en el período: la lista tiene que ser corta
            // para que se mire.
            .Where(c => c.CajasCerradas > 0 || c.VentasCanceladas > 0
                     || c.VentasAnuladas > 0 || c.TotalVendido > 0)
            .ToList();
    }

    /// <summary>
    /// Suma lo cancelado por usuario leyendo el importe del detalle de Auditoría.
    /// </summary>
    private static Dictionary<int, decimal> ImportesCancelados(
        IDbConnection conexion, DateTime desde, DateTime hasta)
    {
        var filas = conexion.Query<FilaCancelacion>(
            """
            SELECT COALESCE(UsuarioId, 0) AS UsuarioId, Detalle
            FROM Auditoria
            WHERE TablaAfectada = 'Ventas' AND Accion = 'CANCELACION'
              AND FechaEvento BETWEEN @Desde AND @Hasta;
            """,
            new { Desde = Fechas.Inicio(desde), Hasta = Fechas.Fin(hasta) });

        var total = new Dictionary<int, decimal>();

        foreach (var fila in filas)
            total[fila.UsuarioId] = total.GetValueOrDefault(fila.UsuarioId) + Importe(fila.Detalle);

        return total;
    }

    /// <summary>
    /// Saca el importe de un detalle como "Venta cancelada antes de cobrar por
    /// $1.500,00 (3 ítem(s))". Si no se puede leer devuelve cero: el reporte
    /// siempre tiene que salir, aunque un renglón viejo tenga otro formato.
    /// </summary>
    private static decimal Importe(string? detalle)
    {
        if (detalle is null)
            return 0m;

        var match = System.Text.RegularExpressions.Regex.Match(
            detalle, @"por\s+\$?\s*([\d.,]+)");

        if (!match.Success)
            return 0m;

        var texto = match.Groups[1].Value;

        texto = texto.Contains(',')
            ? texto.Replace(".", string.Empty).Replace(',', '.')
            : texto.Replace(".", string.Empty);

        return decimal.TryParse(texto, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var valor)
            ? valor
            : 0m;
    }

    private sealed class FilaControl
    {
        public int UsuarioId { get; init; }
        public string Usuario { get; init; } = string.Empty;
        public int CajasCerradas { get; init; }
        public int CajasConFaltante { get; init; }
        public long FaltanteCentavos { get; init; }
        public long SobranteCentavos { get; init; }
        public long PeorFaltanteCentavos { get; init; }
        public int VentasCanceladas { get; init; }
        public int VentasAnuladas { get; init; }
        public long AnuladoCentavos { get; init; }
        public long VendidoCentavos { get; init; }
    }

    private sealed class FilaCancelacion
    {
        public int UsuarioId { get; init; }
        public string? Detalle { get; init; }
    }
}
