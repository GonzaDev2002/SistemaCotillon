using System.Data;
using Cotillon.Data;
using Cotillon.Shared.DTOs.Stock;
using Dapper;

namespace Cotillon.LocalApi.Service.Repositories;

public class StockInvalidoException : Exception
{
    public StockInvalidoException(string mensaje) : base(mensaje) { }
}

public class StockRepository
{
    private readonly IDbConnectionFactory _factory;

    public StockRepository(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Aplica un movimiento de stock dejando registrado el antes y el después.
    /// Es el único camino por el que el stock cambia fuera de ventas y compras,
    /// para que el inventario siempre tenga explicación.
    /// </summary>
    public void Ajustar(AjustarStockDto ajuste)
    {
        using var conexion = _factory.Crear();
        using var tx = conexion.IniciarEscritura();

        var producto = conexion.QuerySingleOrDefault<FilaProducto>(
            "SELECT ProductoId, NombreProducto, Stock FROM Productos WHERE ProductoId = @Id;",
            new { Id = ajuste.ProductoId }, tx)
            ?? throw new StockInvalidoException("El producto no existe.");

        var cantidad = (double)ajuste.Cantidad;
        var stockNuevo = producto.Stock + cantidad;

        if (stockNuevo < 0)
            throw new StockInvalidoException(
                $"El ajuste dejaría el stock de '{producto.NombreProducto}' en {stockNuevo}. Disponible: {producto.Stock}.");

        AplicarMovimiento(conexion, tx, new Movimiento
        {
            ProductoId = producto.ProductoId,
            UsuarioId = ajuste.UsuarioId,
            Tipo = ajuste.TipoMovimiento,
            Cantidad = cantidad,
            StockAnterior = producto.Stock,
            StockNuevo = stockNuevo,
            OrigenTipo = "MANUAL",
            Observaciones = ajuste.Motivo
        });

        Auditoria.Registrar(conexion, tx, ajuste.UsuarioId, "Productos", "MODIFICACION", producto.ProductoId,
            $"{ajuste.TipoMovimiento} de {ajuste.Cantidad} en '{producto.NombreProducto}': {ajuste.Motivo}");

        tx.Commit();
    }

    internal static void AplicarMovimiento(IDbConnection conexion, IDbTransaction tx, Movimiento movimiento)
    {
        conexion.Execute(
            """
            UPDATE Productos
            SET Stock = @Stock, FechaActualizacion = datetime('now', 'localtime')
            WHERE ProductoId = @ProductoId;
            """,
            new { Stock = movimiento.StockNuevo, movimiento.ProductoId }, tx);

        conexion.Execute(
            """
            INSERT INTO StockMovimientos
                (ProductoId, UsuarioId, TipoMovimiento, Cantidad, StockAnterior, StockNuevo,
                 OrigenTipo, OrigenId, Observaciones)
            VALUES
                (@ProductoId, @UsuarioId, @Tipo, @Cantidad, @StockAnterior, @StockNuevo,
                 @OrigenTipo, @OrigenId, @Observaciones);
            """,
            movimiento, tx);
    }

    public List<MovimientoStockDto> ObtenerMovimientos(int? productoId, DateTime desde, DateTime hasta)
    {
        using var conexion = _factory.Crear();

        var filas = conexion.Query<FilaMovimiento>(
            $"""
            SELECT
                sm.StockMovimientoId, sm.ProductoId, sm.FechaMovimiento, sm.TipoMovimiento,
                sm.Cantidad, sm.StockAnterior, sm.StockNuevo, sm.OrigenTipo, sm.OrigenId, sm.Observaciones,
                p.NombreProducto,
                u.NombreCompleto AS Usuario
            FROM StockMovimientos sm
            JOIN Productos p ON sm.ProductoId = p.ProductoId
            JOIN Usuarios  u ON sm.UsuarioId  = u.UsuarioId
            WHERE sm.FechaMovimiento BETWEEN @Desde AND @Hasta
            {(productoId.HasValue ? "AND sm.ProductoId = @ProductoId" : string.Empty)}
            ORDER BY sm.FechaMovimiento DESC, sm.StockMovimientoId DESC;
            """,
            new { Desde = Fechas.Inicio(desde), Hasta = Fechas.Fin(hasta), ProductoId = productoId });

        return filas.Select(f => new MovimientoStockDto
        {
            StockMovimientoId = f.StockMovimientoId,
            ProductoId = f.ProductoId,
            NombreProducto = f.NombreProducto,
            Usuario = f.Usuario,
            FechaMovimiento = f.FechaMovimiento,
            TipoMovimiento = f.TipoMovimiento,
            Cantidad = (decimal)f.Cantidad,
            StockAnterior = (decimal)f.StockAnterior,
            StockNuevo = (decimal)f.StockNuevo,
            OrigenTipo = f.OrigenTipo,
            OrigenId = f.OrigenId,
            Observaciones = f.Observaciones
        }).ToList();
    }

    internal sealed class Movimiento
    {
        public int ProductoId { get; init; }
        public int UsuarioId { get; init; }
        public string Tipo { get; init; } = string.Empty;
        public double Cantidad { get; init; }
        public double StockAnterior { get; init; }
        public double StockNuevo { get; init; }
        public string? OrigenTipo { get; init; }
        public int? OrigenId { get; init; }
        public string? Observaciones { get; init; }
    }

    private sealed class FilaProducto
    {
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public double Stock { get; init; }
    }

    private sealed class FilaMovimiento
    {
        public int StockMovimientoId { get; init; }
        public int ProductoId { get; init; }
        public string NombreProducto { get; init; } = string.Empty;
        public string Usuario { get; init; } = string.Empty;
        public DateTime FechaMovimiento { get; init; }
        public string TipoMovimiento { get; init; } = string.Empty;
        public double Cantidad { get; init; }
        public double StockAnterior { get; init; }
        public double StockNuevo { get; init; }
        public string? OrigenTipo { get; init; }
        public int? OrigenId { get; init; }
        public string? Observaciones { get; init; }
    }
}
