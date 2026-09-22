using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Compras;
using Cotillon.Shared.DTOs.Stock;
using Cotillon.Shared.DTOs.Terceros;
using Cotillon.Shared.DTOs.Ventas;

namespace Cotillon.Tests;

public class ComprasYStockTests
{
    private static CrearCompraDto NuevaCompra(int proveedorId, params CrearLineaCompraDto[] lineas) => new()
    {
        UsuarioId = EscenarioVentas.UsuarioId,
        ProveedorId = proveedorId,
        Lineas = [.. lineas]
    };

    private static CrearLineaCompraDto LineaCompra(int productoId, decimal cantidad, decimal costo) => new()
    {
        ProductoId = productoId,
        Cantidad = cantidad,
        CostoUnitario = costo
    };

    private static int CrearProveedor(EscenarioVentas escenario) =>
        escenario.Proveedores.Insertar(
            new ProveedorDto { NombreProveedor = "Distribuidora Test" }, EscenarioVentas.UsuarioId);

    [Fact]
    public void Una_compra_recibida_ingresa_stock_y_actualiza_costo()
    {
        using var escenario = new EscenarioVentas();
        var proveedorId = CrearProveedor(escenario);

        var compra = NuevaCompra(proveedorId, LineaCompra(EscenarioVentas.ProductoGlobo, 20, 55m));
        var compraId = escenario.Compras.Crear(compra);
        var guardada = escenario.Compras.ObtenerPorId(compraId)!;

        Assert.Equal("RECIBIDA", guardada.Estado);
        Assert.Equal(1100m, guardada.TotalCompra);
        Assert.Equal(70, escenario.StockDe(EscenarioVentas.ProductoGlobo));
        Assert.Equal(55m, escenario.CostoDe(EscenarioVentas.ProductoGlobo));
    }

    [Fact]
    public void Una_compra_pendiente_no_toca_el_stock_hasta_recibirla()
    {
        using var escenario = new EscenarioVentas();
        var proveedorId = CrearProveedor(escenario);

        var compra = NuevaCompra(proveedorId, LineaCompra(EscenarioVentas.ProductoGlobo, 20, 55m));
        compra.RecibirAhora = false;

        var compraId = escenario.Compras.Crear(compra);

        Assert.Equal("PENDIENTE", escenario.Compras.ObtenerPorId(compraId)!.Estado);
        Assert.Equal(50, escenario.StockDe(EscenarioVentas.ProductoGlobo));

        escenario.Compras.Recibir(compraId, new RecibirCompraDto { UsuarioId = EscenarioVentas.UsuarioId });

        Assert.Equal("RECIBIDA", escenario.Compras.ObtenerPorId(compraId)!.Estado);
        Assert.Equal(70, escenario.StockDe(EscenarioVentas.ProductoGlobo));
    }

    [Fact]
    public void No_se_puede_recibir_dos_veces_la_misma_compra()
    {
        using var escenario = new EscenarioVentas();
        var proveedorId = CrearProveedor(escenario);

        var compraId = escenario.Compras.Crear(
            NuevaCompra(proveedorId, LineaCompra(EscenarioVentas.ProductoGlobo, 10, 50m)));

        var ex = Assert.Throws<CompraInvalidaException>(() =>
            escenario.Compras.Recibir(compraId, new RecibirCompraDto { UsuarioId = EscenarioVentas.UsuarioId }));

        Assert.Contains("ya fue recibida", ex.Message);
    }

    [Fact]
    public void Anular_una_compra_recibida_devuelve_el_stock()
    {
        using var escenario = new EscenarioVentas();
        var proveedorId = CrearProveedor(escenario);

        var compraId = escenario.Compras.Crear(
            NuevaCompra(proveedorId, LineaCompra(EscenarioVentas.ProductoGlobo, 20, 55m)));

        Assert.Equal(70, escenario.StockDe(EscenarioVentas.ProductoGlobo));

        escenario.Compras.Anular(compraId, EscenarioVentas.UsuarioId, "Mercadería fallada");

        Assert.Equal(50, escenario.StockDe(EscenarioVentas.ProductoGlobo));
        Assert.Equal("ANULADA", escenario.Compras.ObtenerPorId(compraId)!.Estado);
    }

    [Fact]
    public void No_se_puede_anular_una_compra_cuyo_stock_ya_se_vendio()
    {
        using var escenario = new EscenarioVentas();
        var proveedorId = CrearProveedor(escenario);
        escenario.AbrirCaja();

        // Se compran 20 (stock 70) y después se venden 65: quedan 5, menos que las
        // 20 que habría que devolver al anular.
        var compraId = escenario.Compras.Crear(
            NuevaCompra(proveedorId, LineaCompra(EscenarioVentas.ProductoGlobo, 20, 55m)));

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 65));
        venta.Pagos = [escenario.PagoEfectivo(6500m)];
        escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);

        var ex = Assert.Throws<CompraInvalidaException>(
            () => escenario.Compras.Anular(compraId, EscenarioVentas.UsuarioId, "Error"));

        Assert.Contains("ya se vendió", ex.Message);
        Assert.Equal(5, escenario.StockDe(EscenarioVentas.ProductoGlobo));
    }

    [Fact]
    public void Un_ajuste_positivo_suma_stock_y_deja_rastro()
    {
        using var escenario = new EscenarioVentas();

        escenario.Stock.Ajustar(new AjustarStockDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            ProductoId = EscenarioVentas.ProductoGlobo,
            TipoMovimiento = "AJUSTE",
            Cantidad = 10,
            Motivo = "Recuento de inventario"
        });

        Assert.Equal(60, escenario.StockDe(EscenarioVentas.ProductoGlobo));

        var movimientos = escenario.Stock.ObtenerMovimientos(
            EscenarioVentas.ProductoGlobo, DateTime.Today, DateTime.Today);

        var movimiento = Assert.Single(movimientos);
        Assert.Equal("AJUSTE", movimiento.TipoMovimiento);
        Assert.Equal(50m, movimiento.StockAnterior);
        Assert.Equal(60m, movimiento.StockNuevo);
    }

    [Fact]
    public void Un_ajuste_no_puede_dejar_el_stock_negativo()
    {
        using var escenario = new EscenarioVentas();

        var ex = Assert.Throws<StockInvalidoException>(() => escenario.Stock.Ajustar(new AjustarStockDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            ProductoId = EscenarioVentas.ProductoGlobo,
            TipoMovimiento = "AJUSTE",
            Cantidad = -999,
            Motivo = "Error"
        }));

        Assert.Contains("dejaría el stock", ex.Message);
        Assert.Equal(50, escenario.StockDe(EscenarioVentas.ProductoGlobo));
    }

    [Fact]
    public void No_se_puede_comprar_a_un_proveedor_inexistente()
    {
        using var escenario = new EscenarioVentas();

        var ex = Assert.Throws<CompraInvalidaException>(() =>
            escenario.Compras.Crear(NuevaCompra(999, LineaCompra(EscenarioVentas.ProductoGlobo, 1, 10m))));

        Assert.Contains("proveedor no existe", ex.Message);
    }

    [Fact]
    public void Si_una_linea_de_compra_falla_no_se_registra_nada()
    {
        using var escenario = new EscenarioVentas();
        var proveedorId = CrearProveedor(escenario);

        var compra = NuevaCompra(proveedorId,
            LineaCompra(EscenarioVentas.ProductoGlobo, 10, 50m),
            LineaCompra(999, 5, 30m));

        Assert.Throws<CompraInvalidaException>(() => escenario.Compras.Crear(compra));

        Assert.Equal(50, escenario.StockDe(EscenarioVentas.ProductoGlobo));
        Assert.Equal(0, escenario.ContarFilas("Compras"));
        Assert.Equal(0, escenario.ContarFilas("DetalleCompras"));
    }

    [Fact]
    public void Una_merma_cargada_en_positivo_igual_descuenta()
    {
        using var escenario = new EscenarioVentas();

        var servicio = new Cotillon.LocalApi.Service.Services.StockService(escenario.Stock);

        var resultado = servicio.Ajustar(new AjustarStockDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            ProductoId = EscenarioVentas.ProductoGlobo,
            TipoMovimiento = "MERMA",
            Cantidad = 5,
            Motivo = "Producto roto"
        });

        Assert.True(resultado.Success);
        Assert.Equal(45, escenario.StockDe(EscenarioVentas.ProductoGlobo));
    }
}
