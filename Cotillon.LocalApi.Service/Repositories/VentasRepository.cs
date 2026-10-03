using System.Data;
using Cotillon.Data;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Ventas;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

/// <summary>
/// Error de negocio detectado dentro de la transacción de venta. Se usa para
/// abortar y devolver un mensaje entendible en lugar de un error de base.
/// </summary>
public class VentaInvalidaException : Exception
{
    public VentaInvalidaException(string mensaje) : base(mensaje) { }
}

public partial class VentasRepository
{
    private readonly IDbConnectionFactory _factory;

    public VentasRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Registra la venta completa en una sola transacción: cabecera, detalle, pagos,
    /// descuento de stock con su trazabilidad, impacto en caja y auditoría. Si algo
    /// falla no queda nada a medias.
    /// </summary>
    public int Crear(CrearVentaDto venta, bool permitirVentaSinStockGlobal)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var sesion = CajaRepository.ObtenerSesionAbierta(conexion, tx)
            ?? throw new VentaInvalidaException("No hay una caja abierta. Abrí la caja antes de vender.");

        var lineas = ResolverLineas(conexion, tx, venta, permitirVentaSinStockGlobal);

        var subtotalCentavos = lineas.Sum(l => l.BrutoCentavos);
        var descuentoLineasCentavos = lineas.Sum(l => l.DescuentoCentavos);
        var descuentoGeneralCentavos = Money.ToCentavos(venta.DescuentoTotal);
        var descuentoTotalCentavos = descuentoLineasCentavos + descuentoGeneralCentavos;

        if (descuentoTotalCentavos > subtotalCentavos)
            throw new VentaInvalidaException("El descuento no puede superar el total de la venta.");

        var totalCentavos = subtotalCentavos - descuentoTotalCentavos;

        var pagos = ResolverPagos(conexion, tx, venta, totalCentavos);

        var ventaId = conexion.ExecuteScalar<int>(
            """
            INSERT INTO Ventas
                (ClienteId, UsuarioId, SesionCajaId, TipoComprobante,
                 SubtotalCentavos, DescuentoTotalCentavos, TotalVentaCentavos, Estado, Observaciones)
            VALUES
                (@ClienteId, @UsuarioId, @SesionId, @TipoComprobante,
                 @Subtotal, @Descuento, @Total, 'COMPLETADA', @Observaciones);
            SELECT last_insert_rowid();
            """,
            new
            {
                venta.ClienteId,
                venta.UsuarioId,
                SesionId = sesion.SesionCajaId,
                venta.TipoComprobante,
                Subtotal = subtotalCentavos,
                Descuento = descuentoTotalCentavos,
                Total = totalCentavos,
                venta.Observaciones
            }, tx);

        foreach (var linea in lineas)
        {
            conexion.Execute(
                """
                INSERT INTO DetalleVentas
                    (VentaId, ProductoId, NombreProducto, Cantidad,
                     PrecioUnitarioCentavos, CostoUnitarioCentavos, DescuentoLineaCentavos, SubtotalLineaCentavos)
                VALUES
                    (@VentaId, @ProductoId, @Nombre, @Cantidad,
                     @Precio, @Costo, @Descuento, @Subtotal);
                """,
                new
                {
                    VentaId = ventaId,
                    linea.ProductoId,
                    Nombre = linea.NombreProducto,
                    linea.Cantidad,
                    Precio = linea.PrecioCentavos,
                    Costo = linea.CostoCentavos,
                    Descuento = linea.DescuentoCentavos,
                    Subtotal = linea.BrutoCentavos - linea.DescuentoCentavos
                }, tx);

            DescontarStock(conexion, tx, venta.UsuarioId, ventaId, linea);
        }

        foreach (var pago in pagos)
        {
            conexion.Execute(
                """
                INSERT INTO PagosVenta (VentaId, MedioPagoId, ImporteCentavos, VueltoCentavos, Referencia)
                VALUES (@VentaId, @MedioPagoId, @Importe, @Vuelto, @Referencia);
                """,
                new
                {
                    VentaId = ventaId,
                    pago.MedioPagoId,
                    Importe = pago.ImporteCentavos,
                    Vuelto = pago.VueltoCentavos,
                    pago.Referencia
                }, tx);

            if (!pago.AfectaEfectivo)
                continue;

            // Deja rastro del efectivo que entró al cajón. El arqueo no suma los
            // movimientos de tipo VENTA (los calcula desde PagosVenta), así que
            // este registro es trazabilidad y no doble conteo.
            conexion.Execute(
                """
                INSERT INTO MovimientosCaja
                    (SesionCajaId, UsuarioId, TipoMovimiento, MedioPagoId, ImporteCentavos, Motivo, OrigenTipo, OrigenId)
                VALUES
                    (@SesionId, @UsuarioId, 'VENTA', @MedioPagoId, @Importe, @Motivo, 'VENTA', @VentaId);
                """,
                new
                {
                    SesionId = sesion.SesionCajaId,
                    venta.UsuarioId,
                    pago.MedioPagoId,
                    Importe = pago.ImporteCentavos - pago.VueltoCentavos,
                    Motivo = $"Venta #{ventaId}",
                    VentaId = ventaId
                }, tx);
        }

        Auditoria.Registrar(conexion, tx, venta.UsuarioId, "Ventas", "ALTA", ventaId,
            $"Venta por {Money.FromCentavos(totalCentavos):C2} con {lineas.Count} ítem(s)");

        tx.Commit();

        return ventaId;
    }

    public void Anular(int ventaId, AnularVentaDto datos)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var venta = conexion.QuerySingleOrDefault<FilaVentaAnulacion>(
            "SELECT VentaId, SesionCajaId, Estado, TotalVentaCentavos FROM Ventas WHERE VentaId = @Id;",
            new { Id = ventaId }, tx)
            ?? throw new VentaInvalidaException("No se encontró la venta.");

        if (venta.Estado == "ANULADA")
            throw new VentaInvalidaException("La venta ya está anulada.");

        var sesion = CajaRepository.ObtenerSesionAbierta(conexion, tx)
            ?? throw new VentaInvalidaException("No hay una caja abierta. Abrí la caja para poder anular.");

        conexion.Execute(
            """
            UPDATE Ventas SET
                Estado = 'ANULADA',
                FechaAnulacion = datetime('now', 'localtime'),
                UsuarioAnulacionId = @UsuarioId,
                MotivoAnulacion = @Motivo,
                FechaActualizacion = datetime('now', 'localtime')
            WHERE VentaId = @VentaId;
            """,
            new { VentaId = ventaId, datos.UsuarioId, datos.Motivo }, tx);

        var lineas = conexion.Query<FilaDetalleAnulacion>(
            "SELECT ProductoId, NombreProducto, Cantidad FROM DetalleVentas WHERE VentaId = @VentaId;",
            new { VentaId = ventaId }, tx);

        foreach (var linea in lineas)
            RestituirStock(conexion, tx, datos.UsuarioId, ventaId, linea);

        var efectivoDevueltoCentavos = conexion.ExecuteScalar<long>(
            """
            SELECT COALESCE(SUM(pv.ImporteCentavos - pv.VueltoCentavos), 0)
            FROM PagosVenta pv
            JOIN MediosPago mp ON pv.MedioPagoId = mp.MedioPagoId
            WHERE pv.VentaId = @VentaId AND mp.AfectaEfectivo = 1;
            """,
            new { VentaId = ventaId }, tx);

        if (efectivoDevueltoCentavos > 0)
        {
            // Si la venta pertenece a la caja abierta, el arqueo ya deja de contarla
            // al quedar ANULADA. Si era de una caja anterior y ya cerrada, la plata
            // sale del cajón de hoy y tiene que impactar como egreso real.
            var esDeOtraSesion = venta.SesionCajaId != sesion.SesionCajaId;

            conexion.Execute(
                """
                INSERT INTO MovimientosCaja
                    (SesionCajaId, UsuarioId, TipoMovimiento, ImporteCentavos, Motivo, OrigenTipo, OrigenId)
                VALUES
                    (@SesionId, @UsuarioId, @Tipo, @Importe, @Motivo, 'VENTA', @VentaId);
                """,
                new
                {
                    SesionId = sesion.SesionCajaId,
                    datos.UsuarioId,
                    Tipo = esDeOtraSesion ? "EGRESO" : "ANULACION_VENTA",
                    Importe = -efectivoDevueltoCentavos,
                    Motivo = $"Anulación de venta #{ventaId}: {datos.Motivo}",
                    VentaId = ventaId
                }, tx);
        }

        Auditoria.Registrar(conexion, tx, datos.UsuarioId, "Ventas", "ANULACION", ventaId,
            $"Anulación por {Money.FromCentavos(venta.TotalVentaCentavos):C2}. Motivo: {datos.Motivo}");

        tx.Commit();
    }

    // ------------------------------------------------------------- Auxiliares --

    private static List<LineaResuelta> ResolverLineas(
        IDbConnection conexion, IDbTransaction tx, CrearVentaDto venta, bool permitirSinStockGlobal)
    {
        var resueltas = new List<LineaResuelta>();

        foreach (var linea in venta.Lineas)
        {
            if (linea.Cantidad <= 0)
                throw new VentaInvalidaException("La cantidad de cada ítem debe ser mayor a cero.");

            var producto = conexion.QuerySingleOrDefault<FilaProducto>(
                """
                SELECT ProductoId, NombreProducto, PrecioCentavos, CostoCentavos,
                       Stock, UnidadMedida, PermiteVentaSinStock, Activo
                FROM Productos
                WHERE ProductoId = @Id;
                """,
                new { Id = linea.ProductoId }, tx)
                ?? throw new VentaInvalidaException($"El producto {linea.ProductoId} no existe.");

            if (!producto.Activo)
                throw new VentaInvalidaException($"El producto '{producto.NombreProducto}' está dado de baja.");

            var permiteSinStock = producto.PermiteVentaSinStock || permitirSinStockGlobal;

            if (!permiteSinStock && producto.Stock < (double)linea.Cantidad)
            {
                // Con la unidad en el mensaje: "Disponible: 0,3 kg" se entiende,
                // "Disponible: 0,3" al lado de un pedido de 500 no.
                var unidad = UnidadesMedida.De(producto.UnidadMedida);

                throw new VentaInvalidaException(
                    $"Stock insuficiente de '{producto.NombreProducto}'. " +
                    $"Pediste {unidad.Formatear(linea.Cantidad)} y hay {unidad.Formatear((decimal)producto.Stock)}.");
            }

            var brutoCentavos = Money.ToCentavos(Money.FromCentavos(producto.PrecioCentavos) * linea.Cantidad);
            var descuentoCentavos = Money.ToCentavos(linea.DescuentoLinea);

            if (descuentoCentavos < 0)
                throw new VentaInvalidaException("El descuento por ítem no puede ser negativo.");

            if (descuentoCentavos > brutoCentavos)
                throw new VentaInvalidaException(
                    $"El descuento de '{producto.NombreProducto}' supera el importe del ítem.");

            resueltas.Add(new LineaResuelta
            {
                ProductoId = producto.ProductoId,
                NombreProducto = producto.NombreProducto,
                Cantidad = (double)linea.Cantidad,
                StockAnterior = producto.Stock,
                PrecioCentavos = producto.PrecioCentavos,
                CostoCentavos = producto.CostoCentavos,
                BrutoCentavos = brutoCentavos,
                DescuentoCentavos = descuentoCentavos
            });
        }

        if (resueltas.Count == 0)
            throw new VentaInvalidaException("La venta no tiene ítems.");

        return resueltas;
    }

    private static List<PagoResuelto> ResolverPagos(
        IDbConnection conexion, IDbTransaction tx, CrearVentaDto venta, long totalCentavos)
    {
        if (venta.Pagos.Count == 0)
            throw new VentaInvalidaException("La venta no tiene pagos registrados.");

        var pagos = new List<PagoResuelto>();

        foreach (var pago in venta.Pagos)
        {
            if (pago.Importe <= 0)
                throw new VentaInvalidaException("El importe de cada pago debe ser mayor a cero.");

            var medio = conexion.QuerySingleOrDefault<FilaMedioPago>(
                "SELECT MedioPagoId, NombreMedioPago, AfectaEfectivo, Activo FROM MediosPago WHERE MedioPagoId = @Id;",
                new { Id = pago.MedioPagoId }, tx)
                ?? throw new VentaInvalidaException($"El medio de pago {pago.MedioPagoId} no existe.");

            if (!medio.Activo)
                throw new VentaInvalidaException($"El medio de pago '{medio.NombreMedioPago}' está inactivo.");

            pagos.Add(new PagoResuelto
            {
                MedioPagoId = medio.MedioPagoId,
                AfectaEfectivo = medio.AfectaEfectivo,
                ImporteCentavos = Money.ToCentavos(pago.Importe),
                Referencia = pago.Referencia
            });
        }

        var totalPagadoCentavos = pagos.Sum(p => p.ImporteCentavos);

        if (totalPagadoCentavos < totalCentavos)
            throw new VentaInvalidaException(
                $"El pago es insuficiente. Faltan {Money.FromCentavos(totalCentavos - totalPagadoCentavos):C2}.");

        var excedenteCentavos = totalPagadoCentavos - totalCentavos;

        if (excedenteCentavos == 0)
            return pagos;

        // Sólo se puede dar vuelto del efectivo: un excedente pagado con tarjeta o
        // transferencia sería plata que el comercio cobra de más.
        var pagoEnEfectivo = pagos.LastOrDefault(p => p.AfectaEfectivo)
            ?? throw new VentaInvalidaException(
                "El importe pagado supera el total y no hay pago en efectivo del cual dar vuelto.");

        if (excedenteCentavos > pagoEnEfectivo.ImporteCentavos)
            throw new VentaInvalidaException("El vuelto no puede superar el importe entregado en efectivo.");

        pagoEnEfectivo.VueltoCentavos = excedenteCentavos;

        return pagos;
    }

    private static void DescontarStock(
        IDbConnection conexion, IDbTransaction tx, int usuarioId, int ventaId, LineaResuelta linea)
    {
        var stockNuevo = linea.StockAnterior - linea.Cantidad;

        conexion.Execute(
            """
            UPDATE Productos
            SET Stock = @Stock, FechaActualizacion = datetime('now', 'localtime')
            WHERE ProductoId = @ProductoId;
            """,
            new { Stock = stockNuevo, linea.ProductoId }, tx);

        conexion.Execute(
            """
            INSERT INTO StockMovimientos
                (ProductoId, UsuarioId, TipoMovimiento, Cantidad, StockAnterior, StockNuevo, OrigenTipo, OrigenId, Observaciones)
            VALUES
                (@ProductoId, @UsuarioId, 'VENTA', @Cantidad, @Anterior, @Nuevo, 'VENTA', @VentaId, @Observaciones);
            """,
            new
            {
                linea.ProductoId,
                UsuarioId = usuarioId,
                Cantidad = -linea.Cantidad,
                Anterior = linea.StockAnterior,
                Nuevo = stockNuevo,
                VentaId = ventaId,
                Observaciones = $"Venta #{ventaId}"
            }, tx);
    }

    private static void RestituirStock(
        IDbConnection conexion, IDbTransaction tx, int usuarioId, int ventaId, FilaDetalleAnulacion linea)
    {
        var stockAnterior = conexion.ExecuteScalar<double>(
            "SELECT Stock FROM Productos WHERE ProductoId = @Id;", new { Id = linea.ProductoId }, tx);

        var stockNuevo = stockAnterior + linea.Cantidad;

        conexion.Execute(
            """
            UPDATE Productos
            SET Stock = @Stock, FechaActualizacion = datetime('now', 'localtime')
            WHERE ProductoId = @ProductoId;
            """,
            new { Stock = stockNuevo, linea.ProductoId }, tx);

        conexion.Execute(
            """
            INSERT INTO StockMovimientos
                (ProductoId, UsuarioId, TipoMovimiento, Cantidad, StockAnterior, StockNuevo, OrigenTipo, OrigenId, Observaciones)
            VALUES
                (@ProductoId, @UsuarioId, 'ANULACION_VENTA', @Cantidad, @Anterior, @Nuevo, 'VENTA', @VentaId, @Observaciones);
            """,
            new
            {
                linea.ProductoId,
                UsuarioId = usuarioId,
                linea.Cantidad,
                Anterior = stockAnterior,
                Nuevo = stockNuevo,
                VentaId = ventaId,
                Observaciones = $"Anulación de venta #{ventaId}"
            }, tx);
    }

    private sealed class LineaResuelta
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public double Cantidad { get; init; }
        public double StockAnterior { get; init; }
        public long PrecioCentavos { get; init; }
        public long CostoCentavos { get; init; }
        public long BrutoCentavos { get; init; }
        public long DescuentoCentavos { get; init; }
    }

    private sealed class PagoResuelto
    {
        public int MedioPagoId { get; init; }
        public bool AfectaEfectivo { get; init; }
        public long ImporteCentavos { get; init; }
        public long VueltoCentavos { get; set; }
        public string? Referencia { get; init; }
    }

    private sealed class FilaProducto
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public long PrecioCentavos { get; init; }
        public long CostoCentavos { get; init; }
        public double Stock { get; init; }
        public string UnidadMedida { get; init; } = "UNIDAD";
        public bool PermiteVentaSinStock { get; init; }
        public bool Activo { get; init; }
    }

    private sealed class FilaMedioPago
    {
        public int MedioPagoId { get; init; }
        public string NombreMedioPago { get; init; } = string.Empty;
        public bool AfectaEfectivo { get; init; }
        public bool Activo { get; init; }
    }

    private sealed class FilaVentaAnulacion
    {
        public int VentaId { get; init; }
        public int SesionCajaId { get; init; }
        public string Estado { get; init; } = string.Empty;
        public long TotalVentaCentavos { get; init; }
    }

    private sealed class FilaDetalleAnulacion
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public double Cantidad { get; init; }
    }
}
