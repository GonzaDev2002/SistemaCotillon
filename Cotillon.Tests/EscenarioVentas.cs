using System.Data;
using Cotillon.Data;
using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Caja;
using Cotillon.Shared.DTOs.Ventas;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// Base de prueba con catálogo mínimo cargado y atajos para armar ventas.
/// </summary>
public sealed class EscenarioVentas : IDisposable
{
    public const int UsuarioId = 1;
    public const int ProductoGlobo = 1;
    public const int ProductoVincha = 2;

    private readonly BaseDePruebas _db = new();

    public EscenarioVentas()
    {
        Ventas = new VentasRepository(_db.Factory);
        Caja = new CajaRepository(_db.Factory);
        Configuracion = new ConfiguracionRepository(_db.Factory);
        Compras = new ComprasRepository(_db.Factory);
        Stock = new StockRepository(_db.Factory);
        Proveedores = new ProveedoresRepository(_db.Factory);
        Clientes = new ClientesRepository(_db.Factory);

        using var conexion = _db.Abrir();

        conexion.Execute(
            """
            INSERT INTO Secciones (SeccionId, NombreSeccion, Activo) VALUES (1, 'General', 1);
            INSERT INTO Categorias (CategoriaId, SeccionId, NombreCategoria, Activo) VALUES (1, 1, 'Varios', 1);

            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo)
            VALUES (1, 1, 'Globo', 10000, 4000, 50, 5);

            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo)
            VALUES (2, 1, 'Vincha', 250050, 100000, 10, 2);
            """);
    }

    public VentasRepository Ventas { get; }
    public CajaRepository Caja { get; }
    public ConfiguracionRepository Configuracion { get; }
    public ComprasRepository Compras { get; }
    public StockRepository Stock { get; }
    public ProveedoresRepository Proveedores { get; }
    public ClientesRepository Clientes { get; }

    public int MedioEfectivoId => ObtenerMedio("EFECTIVO");
    public int MedioDebitoId => ObtenerMedio("DEBITO");

    public int AbrirCaja(decimal montoInicial = 1000m) =>
        Caja.Abrir(UsuarioId, montoInicial, "Apertura de prueba");

    public IDbConnection Abrir() => _db.Abrir();

    public double StockDe(int productoId)
    {
        using var conexion = _db.Abrir();

        return conexion.ExecuteScalar<double>(
            "SELECT Stock FROM Productos WHERE ProductoId = @Id;", new { Id = productoId });
    }

    /// <summary>
    /// Registra una venta pagada justa en efectivo y devuelve la guardada. El
    /// importe sale de los precios de la base, igual que en la venta real.
    /// </summary>
    public VentaDto Vender(params (int ProductoId, decimal Cantidad)[] lineas)
    {
        var venta = NuevaVenta([.. lineas.Select(l => Linea(l.ProductoId, l.Cantidad))]);

        var total = lineas.Sum(l => PrecioDe(l.ProductoId) * l.Cantidad);

        venta.Pagos = [PagoEfectivo(total)];

        return Ventas.ObtenerPorId(Ventas.Crear(venta, permitirVentaSinStockGlobal: false))!;
    }

    public void Anular(int ventaId) =>
        Ventas.Anular(ventaId, new AnularVentaDto { UsuarioId = UsuarioId, Motivo = "Prueba" });

    public decimal PrecioDe(int productoId)
    {
        using var conexion = _db.Abrir();

        return Money.FromCentavos(conexion.ExecuteScalar<long>(
            "SELECT PrecioCentavos FROM Productos WHERE ProductoId = @Id;", new { Id = productoId }));
    }

    public decimal CostoDe(int productoId)
    {
        using var conexion = _db.Abrir();

        return Money.FromCentavos(conexion.ExecuteScalar<long>(
            "SELECT CostoCentavos FROM Productos WHERE ProductoId = @Id;", new { Id = productoId }));
    }

    public long ContarFilas(string tabla)
    {
        using var conexion = _db.Abrir();

        return conexion.ExecuteScalar<long>($"SELECT COUNT(*) FROM {tabla};");
    }

    public CrearVentaDto NuevaVenta(params CrearLineaVentaDto[] lineas) => new()
    {
        UsuarioId = UsuarioId,
        TipoComprobante = "TICKET",
        Lineas = [.. lineas]
    };

    public static CrearLineaVentaDto Linea(int productoId, decimal cantidad, decimal descuento = 0) => new()
    {
        ProductoId = productoId,
        Cantidad = cantidad,
        DescuentoLinea = descuento
    };

    public CrearPagoVentaDto PagoEfectivo(decimal importe) => new()
    {
        MedioPagoId = MedioEfectivoId,
        Importe = importe
    };

    public CrearPagoVentaDto PagoDebito(decimal importe) => new()
    {
        MedioPagoId = MedioDebitoId,
        Importe = importe
    };

    private int ObtenerMedio(string nombre)
    {
        using var conexion = _db.Abrir();

        return conexion.ExecuteScalar<int>(
            "SELECT MedioPagoId FROM MediosPago WHERE NombreMedioPago = @Nombre;", new { Nombre = nombre });
    }

    public void Dispose() => _db.Dispose();
}
