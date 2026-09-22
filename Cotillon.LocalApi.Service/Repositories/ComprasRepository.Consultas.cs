using Cotillon.Data;
using Cotillon.Shared.DTOs.Compras;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public partial class ComprasRepository
{
    public CompraDto? ObtenerPorId(int compraId)
    {
        using var conexion = _factory.Crear();

        var cabecera = conexion.QuerySingleOrDefault<FilaCompra>(
            """
            SELECT
                c.CompraId, c.ProveedorId, c.UsuarioId, c.FechaCompra, c.NumeroComprobante,
                c.SubtotalCentavos, c.DescuentoTotalCentavos, c.TotalCompraCentavos,
                c.Estado, c.Observaciones,
                p.NombreProveedor                AS Proveedor,
                COALESCE(u.NombreCompleto, '')   AS Usuario
            FROM Compras c
            JOIN Proveedores p ON c.ProveedorId = p.ProveedorId
            LEFT JOIN Usuarios u ON c.UsuarioId = u.UsuarioId
            WHERE c.CompraId = @Id;
            """,
            new { Id = compraId });

        if (cabecera is null)
            return null;

        var lineas = conexion.Query<FilaDetalleCompra>(
            """
            SELECT DetalleCompraId, ProductoId, NombreProducto, Cantidad,
                   CostoUnitarioCentavos, SubtotalLineaCentavos
            FROM DetalleCompras
            WHERE CompraId = @Id
            ORDER BY DetalleCompraId;
            """,
            new { Id = compraId });

        return new CompraDto
        {
            CompraId = cabecera.CompraId,
            ProveedorId = cabecera.ProveedorId,
            Proveedor = cabecera.Proveedor,
            UsuarioId = cabecera.UsuarioId,
            Usuario = cabecera.Usuario,
            FechaCompra = cabecera.FechaCompra,
            NumeroComprobante = cabecera.NumeroComprobante,
            Subtotal = Money.FromCentavos(cabecera.SubtotalCentavos),
            DescuentoTotal = Money.FromCentavos(cabecera.DescuentoTotalCentavos),
            TotalCompra = Money.FromCentavos(cabecera.TotalCompraCentavos),
            Estado = cabecera.Estado,
            Observaciones = cabecera.Observaciones,
            Lineas = lineas.Select(l => new LineaCompraDto
            {
                DetalleCompraId = l.DetalleCompraId,
                ProductoId = l.ProductoId,
                NombreProducto = l.NombreProducto,
                Cantidad = (decimal)l.Cantidad,
                CostoUnitario = Money.FromCentavos(l.CostoUnitarioCentavos),
                SubtotalLinea = Money.FromCentavos(l.SubtotalLineaCentavos)
            }).ToList()
        };
    }

    public List<CompraResumenDto> Listar(DateTime desde, DateTime hasta, int? proveedorId)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaResumenCompra>(
            $"""
            SELECT
                c.CompraId, c.FechaCompra, c.NumeroComprobante, c.TotalCompraCentavos, c.Estado,
                p.NombreProveedor AS Proveedor,
                (SELECT COUNT(*) FROM DetalleCompras d WHERE d.CompraId = c.CompraId) AS CantidadItems
            FROM Compras c
            JOIN Proveedores p ON c.ProveedorId = p.ProveedorId
            WHERE c.FechaCompra BETWEEN @Desde AND @Hasta
            {(proveedorId.HasValue ? "AND c.ProveedorId = @ProveedorId" : string.Empty)}
            ORDER BY c.FechaCompra DESC;
            """,
            new { Desde = Fechas.Inicio(desde), Hasta = Fechas.Fin(hasta), ProveedorId = proveedorId });

        return filas.Select(f => new CompraResumenDto
        {
            CompraId = f.CompraId,
            FechaCompra = f.FechaCompra,
            Proveedor = f.Proveedor,
            NumeroComprobante = f.NumeroComprobante,
            CantidadItems = f.CantidadItems,
            TotalCompra = Money.FromCentavos(f.TotalCompraCentavos),
            Estado = f.Estado
        }).ToList();
    }

    private sealed class FilaCompra
    {
        public int CompraId { get; init; }
        public int ProveedorId { get; init; }
        public string Proveedor { get; init; } = string.Empty;
        public int UsuarioId { get; init; }
        public string Usuario { get; init; } = string.Empty;
        public DateTime FechaCompra { get; init; }
        public string? NumeroComprobante { get; init; }
        public long SubtotalCentavos { get; init; }
        public long DescuentoTotalCentavos { get; init; }
        public long TotalCompraCentavos { get; init; }
        public string Estado { get; init; } = string.Empty;
        public string? Observaciones { get; init; }
    }

    private sealed class FilaDetalleCompra
    {
        public int DetalleCompraId { get; init; }
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public double Cantidad { get; init; }
        public long CostoUnitarioCentavos { get; init; }
        public long SubtotalLineaCentavos { get; init; }
    }

    private sealed class FilaResumenCompra
    {
        public int CompraId { get; init; }
        public DateTime FechaCompra { get; init; }
        public string Proveedor { get; init; } = string.Empty;
        public string? NumeroComprobante { get; init; }
        public int CantidadItems { get; init; }
        public long TotalCompraCentavos { get; init; }
        public string Estado { get; init; } = string.Empty;
    }
}
