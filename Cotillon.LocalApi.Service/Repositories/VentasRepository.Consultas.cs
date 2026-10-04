using Cotillon.Data;
using Cotillon.Shared.DTOs.Ventas;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public partial class VentasRepository
{
    public VentaDto? ObtenerPorId(int ventaId)
    {
        using var conexion = _factory.Crear();

        var cabecera = conexion.QuerySingleOrDefault<FilaVenta>(
            """
            SELECT
                v.VentaId, v.ClienteId, v.UsuarioId, v.SesionCajaId, v.FechaVenta,
                v.TipoComprobante, v.NumeroComprobante,
                v.SubtotalCentavos, v.DescuentoTotalCentavos, v.TotalVentaCentavos,
                v.Estado, v.Observaciones,
                COALESCE(cl.NombreCompleto, 'Consumidor final') AS Cliente,
                COALESCE(u.NombreCompleto, '')                  AS Usuario
            FROM Ventas v
            LEFT JOIN Clientes cl ON v.ClienteId = cl.ClienteId
            LEFT JOIN Usuarios u  ON v.UsuarioId = u.UsuarioId
            WHERE v.VentaId = @Id;
            """,
            new { Id = ventaId });

        if (cabecera is null)
            return null;

        var lineas = conexion.Query<FilaDetalle>(
            """
            SELECT d.DetalleVentaId, d.ProductoId, d.NombreProducto, d.Cantidad,
                   d.PrecioUnitarioCentavos, d.DescuentoLineaCentavos, d.SubtotalLineaCentavos,
                   COALESCE(p.UnidadMedida, 'UNIDAD') AS UnidadMedida
            FROM DetalleVentas d
            LEFT JOIN Productos p ON d.ProductoId = p.ProductoId
            WHERE d.VentaId = @Id
            ORDER BY d.DetalleVentaId;
            """,
            new { Id = ventaId });

        var pagos = conexion.Query<FilaPago>(
            """
            SELECT pv.PagoVentaId, pv.MedioPagoId, mp.NombreMedioPago AS MedioPago,
                   pv.ImporteCentavos, pv.VueltoCentavos, pv.Referencia
            FROM PagosVenta pv
            JOIN MediosPago mp ON pv.MedioPagoId = mp.MedioPagoId
            WHERE pv.VentaId = @Id
            ORDER BY pv.PagoVentaId;
            """,
            new { Id = ventaId }).ToList();

        return new VentaDto
        {
            VentaId = cabecera.VentaId,
            ClienteId = cabecera.ClienteId,
            Cliente = cabecera.Cliente,
            UsuarioId = cabecera.UsuarioId,
            Usuario = cabecera.Usuario,
            SesionCajaId = cabecera.SesionCajaId,
            FechaVenta = cabecera.FechaVenta,
            TipoComprobante = cabecera.TipoComprobante,
            NumeroComprobante = cabecera.NumeroComprobante,
            Subtotal = Money.FromCentavos(cabecera.SubtotalCentavos),
            DescuentoTotal = Money.FromCentavos(cabecera.DescuentoTotalCentavos),
            TotalVenta = Money.FromCentavos(cabecera.TotalVentaCentavos),
            Vuelto = Money.FromCentavos(pagos.Sum(p => p.VueltoCentavos)),
            Estado = cabecera.Estado,
            Observaciones = cabecera.Observaciones,
            Lineas = lineas.Select(l => new LineaVentaDto
            {
                DetalleVentaId = l.DetalleVentaId,
                ProductoId = l.ProductoId,
                NombreProducto = l.NombreProducto,
                Cantidad = (decimal)l.Cantidad,
                PrecioUnitario = Money.FromCentavos(l.PrecioUnitarioCentavos),
                DescuentoLinea = Money.FromCentavos(l.DescuentoLineaCentavos),
                SubtotalLinea = Money.FromCentavos(l.SubtotalLineaCentavos),
                UnidadMedida = l.UnidadMedida
            }).ToList(),
            Pagos = pagos.Select(p => new PagoVentaDto
            {
                PagoVentaId = p.PagoVentaId,
                MedioPagoId = p.MedioPagoId,
                MedioPago = p.MedioPago,
                Importe = Money.FromCentavos(p.ImporteCentavos),
                Vuelto = Money.FromCentavos(p.VueltoCentavos),
                Referencia = p.Referencia
            }).ToList()
        };
    }

    public List<VentaResumenDto> Listar(DateTime desde, DateTime hasta, bool incluirAnuladas)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaResumen>(
            $"""
            SELECT
                v.VentaId, v.FechaVenta, v.TotalVentaCentavos, v.Estado,
                COALESCE(cl.NombreCompleto, 'Consumidor final') AS Cliente,
                COALESCE(u.NombreCompleto, '')                  AS Usuario,
                (SELECT COUNT(*) FROM DetalleVentas d WHERE d.VentaId = v.VentaId) AS CantidadItems
            FROM Ventas v
            LEFT JOIN Clientes cl ON v.ClienteId = cl.ClienteId
            LEFT JOIN Usuarios u  ON v.UsuarioId = u.UsuarioId
            WHERE v.FechaVenta BETWEEN @Desde AND @Hasta
            {(incluirAnuladas ? string.Empty : "AND v.Estado = 'COMPLETADA'")}
            ORDER BY v.FechaVenta DESC;
            """,
            new { Desde = Fechas.Inicio(desde), Hasta = Fechas.Fin(hasta) });

        return filas.Select(f => new VentaResumenDto
        {
            VentaId = f.VentaId,
            FechaVenta = f.FechaVenta,
            Cliente = f.Cliente,
            Usuario = f.Usuario,
            CantidadItems = f.CantidadItems,
            TotalVenta = Money.FromCentavos(f.TotalVentaCentavos),
            Estado = f.Estado
        }).ToList();
    }

    private sealed class FilaVenta
    {
        public int VentaId { get; init; }
        public int? ClienteId { get; init; }
        public string Cliente { get; init; } = string.Empty;
        public int UsuarioId { get; init; }
        public string Usuario { get; init; } = string.Empty;
        public int SesionCajaId { get; init; }
        public DateTime FechaVenta { get; init; }
        public string TipoComprobante { get; init; } = string.Empty;
        public string? NumeroComprobante { get; init; }
        public long SubtotalCentavos { get; init; }
        public long DescuentoTotalCentavos { get; init; }
        public long TotalVentaCentavos { get; init; }
        public string Estado { get; init; } = string.Empty;
        public string? Observaciones { get; init; }
    }

    private sealed class FilaDetalle
    {
        public int DetalleVentaId { get; init; }
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public double Cantidad { get; init; }
        public long PrecioUnitarioCentavos { get; init; }
        public long DescuentoLineaCentavos { get; init; }
        public long SubtotalLineaCentavos { get; init; }
        public string UnidadMedida { get; init; } = "UNIDAD";
    }

    private sealed class FilaPago
    {
        public int PagoVentaId { get; init; }
        public int MedioPagoId { get; init; }
        public string MedioPago { get; init; } = string.Empty;
        public long ImporteCentavos { get; init; }
        public long VueltoCentavos { get; init; }
        public string? Referencia { get; init; }
    }

    private sealed class FilaResumen
    {
        public int VentaId { get; init; }
        public DateTime FechaVenta { get; init; }
        public string Cliente { get; init; } = string.Empty;
        public string Usuario { get; init; } = string.Empty;
        public int CantidadItems { get; init; }
        public long TotalVentaCentavos { get; init; }
        public string Estado { get; init; } = string.Empty;
    }
}
