using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Ventas;

namespace Cotillon.Tests;

public class VentasTests
{
    [Fact]
    public void Una_venta_simple_descuenta_stock_y_guarda_todo()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 3));
        venta.Pagos = [escenario.PagoEfectivo(300m)];

        var ventaId = escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);
        var guardada = escenario.Ventas.ObtenerPorId(ventaId);

        Assert.NotNull(guardada);
        Assert.Equal(300m, guardada.TotalVenta);
        Assert.Equal("COMPLETADA", guardada.Estado);
        Assert.Single(guardada.Lineas);
        Assert.Equal(47, escenario.StockDe(EscenarioVentas.ProductoGlobo));
        Assert.Equal(1, escenario.ContarFilas("StockMovimientos"));
    }

    [Fact]
    public void El_precio_lo_pone_el_servidor_y_no_el_cliente()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        // El DTO de línea no acepta precio: aunque un cliente manipulado quisiera
        // imponerlo, el importe sale siempre del precio guardado en la base.
        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoVincha, 2));
        venta.Pagos = [escenario.PagoEfectivo(5001m)];

        var ventaId = escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);
        var guardada = escenario.Ventas.ObtenerPorId(ventaId)!;

        Assert.Equal(2500.50m, guardada.Lineas[0].PrecioUnitario);
        Assert.Equal(5001m, guardada.TotalVenta);
    }

    [Fact]
    public void No_se_puede_vender_sin_caja_abierta()
    {
        using var escenario = new EscenarioVentas();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 1));
        venta.Pagos = [escenario.PagoEfectivo(100m)];

        var ex = Assert.Throws<VentaInvalidaException>(
            () => escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false));

        Assert.Contains("caja abierta", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void No_se_puede_vender_mas_stock_del_disponible()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 999));
        venta.Pagos = [escenario.PagoEfectivo(99900m)];

        var ex = Assert.Throws<VentaInvalidaException>(
            () => escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false));

        Assert.Contains("Stock insuficiente", ex.Message);
    }

    [Fact]
    public void Un_pago_insuficiente_rechaza_la_venta()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 3));
        venta.Pagos = [escenario.PagoEfectivo(100m)];

        var ex = Assert.Throws<VentaInvalidaException>(
            () => escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false));

        Assert.Contains("insuficiente", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void El_excedente_en_efectivo_se_registra_como_vuelto()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 3));
        venta.Pagos = [escenario.PagoEfectivo(500m)];

        var ventaId = escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);
        var guardada = escenario.Ventas.ObtenerPorId(ventaId)!;

        Assert.Equal(300m, guardada.TotalVenta);
        Assert.Equal(200m, guardada.Vuelto);
    }

    [Fact]
    public void No_se_da_vuelto_cuando_el_excedente_no_es_en_efectivo()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 3));
        venta.Pagos = [escenario.PagoDebito(500m)];

        var ex = Assert.Throws<VentaInvalidaException>(
            () => escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false));

        Assert.Contains("vuelto", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Se_puede_pagar_con_varios_medios_a_la_vez()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 5));
        venta.Pagos = [escenario.PagoDebito(300m), escenario.PagoEfectivo(200m)];

        var ventaId = escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);
        var guardada = escenario.Ventas.ObtenerPorId(ventaId)!;

        Assert.Equal(500m, guardada.TotalVenta);
        Assert.Equal(2, guardada.Pagos.Count);
        Assert.Equal(0m, guardada.Vuelto);
    }

    [Fact]
    public void Los_descuentos_por_linea_y_generales_se_suman()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(
            EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 10, descuento: 100m));
        venta.DescuentoTotal = 50m;
        venta.Pagos = [escenario.PagoEfectivo(850m)];

        var ventaId = escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);
        var guardada = escenario.Ventas.ObtenerPorId(ventaId)!;

        Assert.Equal(1000m, guardada.Subtotal);
        Assert.Equal(150m, guardada.DescuentoTotal);
        Assert.Equal(850m, guardada.TotalVenta);
    }

    [Fact]
    public void Un_descuento_mayor_al_total_rechaza_la_venta()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 1));
        venta.DescuentoTotal = 500m;
        venta.Pagos = [escenario.PagoEfectivo(100m)];

        Assert.Throws<VentaInvalidaException>(
            () => escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false));
    }

    [Fact]
    public void Si_una_linea_falla_no_queda_nada_a_medias()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        // La primera línea es válida, la segunda no tiene stock: la venta entera
        // se tiene que descartar sin tocar el stock de la primera.
        var venta = escenario.NuevaVenta(
            EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 2),
            EscenarioVentas.Linea(EscenarioVentas.ProductoVincha, 999));
        venta.Pagos = [escenario.PagoEfectivo(100000m)];

        Assert.Throws<VentaInvalidaException>(
            () => escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false));

        Assert.Equal(50, escenario.StockDe(EscenarioVentas.ProductoGlobo));
        Assert.Equal(0, escenario.ContarFilas("Ventas"));
        Assert.Equal(0, escenario.ContarFilas("DetalleVentas"));
        Assert.Equal(0, escenario.ContarFilas("PagosVenta"));
        Assert.Equal(0, escenario.ContarFilas("StockMovimientos"));
    }

    [Fact]
    public void Anular_una_venta_devuelve_el_stock()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 4));
        venta.Pagos = [escenario.PagoEfectivo(400m)];

        var ventaId = escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);
        Assert.Equal(46, escenario.StockDe(EscenarioVentas.ProductoGlobo));

        escenario.Ventas.Anular(ventaId, new AnularVentaDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            Motivo = "Error de carga"
        });

        var anulada = escenario.Ventas.ObtenerPorId(ventaId)!;

        Assert.Equal("ANULADA", anulada.Estado);
        Assert.Equal(50, escenario.StockDe(EscenarioVentas.ProductoGlobo));
    }

    [Fact]
    public void No_se_puede_anular_dos_veces_la_misma_venta()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 1));
        venta.Pagos = [escenario.PagoEfectivo(100m)];

        var ventaId = escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);
        var datos = new AnularVentaDto { UsuarioId = EscenarioVentas.UsuarioId, Motivo = "Prueba" };

        escenario.Ventas.Anular(ventaId, datos);

        var ex = Assert.Throws<VentaInvalidaException>(() => escenario.Ventas.Anular(ventaId, datos));

        Assert.Contains("ya está anulada", ex.Message);
    }

    [Fact]
    public void Se_puede_vender_sin_stock_si_la_configuracion_lo_permite()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja();

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 60));
        venta.Pagos = [escenario.PagoEfectivo(6000m)];

        var ventaId = escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: true);

        Assert.True(ventaId > 0);
        Assert.Equal(-10, escenario.StockDe(EscenarioVentas.ProductoGlobo));
    }
}
