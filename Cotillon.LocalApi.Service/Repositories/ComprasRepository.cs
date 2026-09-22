using System.Data;
using Cotillon.Data;
using Cotillon.Shared.DTOs.Compras;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class CompraInvalidaException : Exception
{
    public CompraInvalidaException(string mensaje) : base(mensaje) { }
}

public partial class ComprasRepository
{
    private readonly IDbConnectionFactory _factory;

    public ComprasRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public int Crear(CrearCompraDto compra)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var proveedorExiste = conexion.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM Proveedores WHERE ProveedorId = @Id AND Activo = 1;",
            new { Id = compra.ProveedorId }, tx) > 0;

        if (!proveedorExiste)
            throw new CompraInvalidaException("El proveedor no existe o está inactivo.");

        var lineas = ResolverLineas(conexion, tx, compra);

        var subtotalCentavos = lineas.Sum(l => l.SubtotalCentavos);
        var descuentoCentavos = Money.ToCentavos(compra.DescuentoTotal);

        if (descuentoCentavos > subtotalCentavos)
            throw new CompraInvalidaException("El descuento no puede superar el total de la compra.");

        var estado = compra.RecibirAhora ? "RECIBIDA" : "PENDIENTE";

        var compraId = conexion.ExecuteScalar<int>(
            """
            INSERT INTO Compras
                (ProveedorId, UsuarioId, NumeroComprobante, SubtotalCentavos,
                 DescuentoTotalCentavos, TotalCompraCentavos, Estado, Observaciones)
            VALUES
                (@ProveedorId, @UsuarioId, @NumeroComprobante, @Subtotal,
                 @Descuento, @Total, @Estado, @Observaciones);
            SELECT last_insert_rowid();
            """,
            new
            {
                compra.ProveedorId,
                compra.UsuarioId,
                NumeroComprobante = string.IsNullOrWhiteSpace(compra.NumeroComprobante)
                    ? null : compra.NumeroComprobante.Trim(),
                Subtotal = subtotalCentavos,
                Descuento = descuentoCentavos,
                Total = subtotalCentavos - descuentoCentavos,
                Estado = estado,
                compra.Observaciones
            }, tx);

        foreach (var linea in lineas)
        {
            conexion.Execute(
                """
                INSERT INTO DetalleCompras
                    (CompraId, ProductoId, NombreProducto, Cantidad, CostoUnitarioCentavos, SubtotalLineaCentavos)
                VALUES
                    (@CompraId, @ProductoId, @Nombre, @Cantidad, @Costo, @Subtotal);
                """,
                new
                {
                    CompraId = compraId,
                    linea.ProductoId,
                    Nombre = linea.NombreProducto,
                    linea.Cantidad,
                    Costo = linea.CostoCentavos,
                    Subtotal = linea.SubtotalCentavos
                }, tx);
        }

        if (compra.RecibirAhora)
            IngresarStock(conexion, tx, compraId, compra.UsuarioId, lineas, compra.ActualizarCostos);

        Auditoria.Registrar(conexion, tx, compra.UsuarioId, "Compras", "ALTA", compraId,
            $"Compra por {Money.FromCentavos(subtotalCentavos - descuentoCentavos):C2} ({estado})");

        tx.Commit();

        return compraId;
    }

    public void Recibir(int compraId, RecibirCompraDto datos)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var estado = conexion.ExecuteScalar<string?>(
            "SELECT Estado FROM Compras WHERE CompraId = @Id;", new { Id = compraId }, tx)
            ?? throw new CompraInvalidaException("No se encontró la compra.");

        if (estado == "RECIBIDA")
            throw new CompraInvalidaException("La compra ya fue recibida.");

        if (estado == "ANULADA")
            throw new CompraInvalidaException("La compra está anulada.");

        var lineas = conexion.Query<LineaResuelta>(
            """
            SELECT ProductoId, NombreProducto, Cantidad, CostoUnitarioCentavos AS CostoCentavos,
                   SubtotalLineaCentavos AS SubtotalCentavos
            FROM DetalleCompras
            WHERE CompraId = @Id;
            """,
            new { Id = compraId }, tx).ToList();

        IngresarStock(conexion, tx, compraId, datos.UsuarioId, lineas, datos.ActualizarCostos);

        conexion.Execute(
            """
            UPDATE Compras
            SET Estado = 'RECIBIDA', FechaActualizacion = datetime('now', 'localtime')
            WHERE CompraId = @Id;
            """,
            new { Id = compraId }, tx);

        Auditoria.Registrar(conexion, tx, datos.UsuarioId, "Compras", "MODIFICACION", compraId,
            "Recepción de mercadería");

        tx.Commit();
    }

    public void Anular(int compraId, int usuarioId, string motivo)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var estado = conexion.ExecuteScalar<string?>(
            "SELECT Estado FROM Compras WHERE CompraId = @Id;", new { Id = compraId }, tx)
            ?? throw new CompraInvalidaException("No se encontró la compra.");

        if (estado == "ANULADA")
            throw new CompraInvalidaException("La compra ya está anulada.");

        // Una compra recibida ya sumó stock: anularla tiene que devolverlo, y no se
        // puede permitir si ese stock ya se vendió.
        if (estado == "RECIBIDA")
        {
            var lineas = conexion.Query<LineaResuelta>(
                """
                SELECT ProductoId, NombreProducto, Cantidad, CostoUnitarioCentavos AS CostoCentavos,
                       SubtotalLineaCentavos AS SubtotalCentavos
                FROM DetalleCompras
                WHERE CompraId = @Id;
                """,
                new { Id = compraId }, tx).ToList();

            foreach (var linea in lineas)
            {
                var stockActual = conexion.ExecuteScalar<double>(
                    "SELECT Stock FROM Productos WHERE ProductoId = @Id;", new { Id = linea.ProductoId }, tx);

                var stockNuevo = stockActual - linea.Cantidad;

                if (stockNuevo < 0)
                    throw new CompraInvalidaException(
                        $"No se puede anular: el stock de '{linea.NombreProducto}' ya se vendió.");

                StockRepository.AplicarMovimiento(conexion, tx, new StockRepository.Movimiento
                {
                    ProductoId = linea.ProductoId,
                    UsuarioId = usuarioId,
                    Tipo = "AJUSTE",
                    Cantidad = -linea.Cantidad,
                    StockAnterior = stockActual,
                    StockNuevo = stockNuevo,
                    OrigenTipo = "COMPRA",
                    OrigenId = compraId,
                    Observaciones = $"Anulación de compra #{compraId}"
                });
            }
        }

        conexion.Execute(
            """
            UPDATE Compras
            SET Estado = 'ANULADA',
                Observaciones = COALESCE(Observaciones || ' | ', '') || @Motivo,
                FechaActualizacion = datetime('now', 'localtime')
            WHERE CompraId = @Id;
            """,
            new { Id = compraId, Motivo = $"Anulada: {motivo}" }, tx);

        Auditoria.Registrar(conexion, tx, usuarioId, "Compras", "ANULACION", compraId, motivo);

        tx.Commit();
    }

    private static void IngresarStock(
        IDbConnection conexion, IDbTransaction tx, int compraId, int usuarioId,
        List<LineaResuelta> lineas, bool actualizarCostos)
    {
        foreach (var linea in lineas)
        {
            var stockAnterior = conexion.ExecuteScalar<double>(
                "SELECT Stock FROM Productos WHERE ProductoId = @Id;", new { Id = linea.ProductoId }, tx);

            StockRepository.AplicarMovimiento(conexion, tx, new StockRepository.Movimiento
            {
                ProductoId = linea.ProductoId,
                UsuarioId = usuarioId,
                Tipo = "COMPRA",
                Cantidad = linea.Cantidad,
                StockAnterior = stockAnterior,
                StockNuevo = stockAnterior + linea.Cantidad,
                OrigenTipo = "COMPRA",
                OrigenId = compraId,
                Observaciones = $"Compra #{compraId}"
            });

            if (actualizarCostos)
            {
                conexion.Execute(
                    """
                    UPDATE Productos
                    SET CostoCentavos = @Costo, FechaActualizacion = datetime('now', 'localtime')
                    WHERE ProductoId = @ProductoId;
                    """,
                    new { Costo = linea.CostoCentavos, linea.ProductoId }, tx);
            }
        }
    }

    private static List<LineaResuelta> ResolverLineas(
        IDbConnection conexion, IDbTransaction tx, CrearCompraDto compra)
    {
        if (compra.Lineas.Count == 0)
            throw new CompraInvalidaException("La compra no tiene ítems.");

        var resueltas = new List<LineaResuelta>();

        foreach (var linea in compra.Lineas)
        {
            if (linea.Cantidad <= 0)
                throw new CompraInvalidaException("La cantidad de cada ítem debe ser mayor a cero.");

            if (linea.CostoUnitario < 0)
                throw new CompraInvalidaException("El costo no puede ser negativo.");

            var producto = conexion.QuerySingleOrDefault<FilaProducto>(
                "SELECT ProductoId, NombreProducto FROM Productos WHERE ProductoId = @Id AND Activo = 1;",
                new { Id = linea.ProductoId }, tx)
                ?? throw new CompraInvalidaException($"El producto {linea.ProductoId} no existe o está dado de baja.");

            var costoCentavos = Money.ToCentavos(linea.CostoUnitario);

            resueltas.Add(new LineaResuelta
            {
                ProductoId = producto.ProductoId,
                NombreProducto = producto.NombreProducto,
                Cantidad = (double)linea.Cantidad,
                CostoCentavos = costoCentavos,
                SubtotalCentavos = Money.ToCentavos(linea.CostoUnitario * linea.Cantidad)
            });
        }

        return resueltas;
    }

    private sealed class LineaResuelta
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public double Cantidad { get; init; }
        public long CostoCentavos { get; init; }
        public long SubtotalCentavos { get; init; }
    }

    private sealed class FilaProducto
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
    }
}
