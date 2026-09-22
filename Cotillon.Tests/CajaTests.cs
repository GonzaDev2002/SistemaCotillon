using Cotillon.Shared.DTOs.Caja;
using Cotillon.Shared.DTOs.Ventas;

namespace Cotillon.Tests;

public class CajaTests
{
    [Fact]
    public void Abrir_caja_deja_la_sesion_abierta_con_su_movimiento()
    {
        using var escenario = new EscenarioVentas();

        var sesionId = escenario.AbrirCaja(1500m);
        var sesion = escenario.Caja.ObtenerSesionAbierta();

        Assert.NotNull(sesion);
        Assert.Equal(sesionId, sesion.SesionCajaId);
        Assert.Equal("ABIERTA", sesion.Estado);
        Assert.Equal(1500m, sesion.MontoInicial);
        Assert.Equal(1, escenario.ContarFilas("MovimientosCaja"));
    }

    [Fact]
    public void El_efectivo_esperado_arranca_en_el_monto_inicial()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja(1000m);

        var arqueo = escenario.Caja.ObtenerArqueo(escenario.Caja.ObtenerSesionAbierta()!);

        Assert.Equal(1000m, arqueo.EfectivoEsperado);
        Assert.Equal(0m, arqueo.TotalVentas);
        Assert.Equal(0, arqueo.CantidadVentas);
    }

    [Fact]
    public void Las_ventas_en_efectivo_suman_al_esperado_descontando_el_vuelto()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja(1000m);

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 3));
        venta.Pagos = [escenario.PagoEfectivo(500m)];
        escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);

        var arqueo = escenario.Caja.ObtenerArqueo(escenario.Caja.ObtenerSesionAbierta()!);

        // Entraron 500 y se devolvieron 200 de vuelto: en el cajón quedan 300.
        Assert.Equal(1300m, arqueo.EfectivoEsperado);
        Assert.Equal(300m, arqueo.TotalVentas);
        Assert.Equal(1, arqueo.CantidadVentas);
    }

    [Fact]
    public void Las_ventas_con_debito_no_suman_al_efectivo_esperado()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja(1000m);

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 3));
        venta.Pagos = [escenario.PagoDebito(300m)];
        escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);

        var arqueo = escenario.Caja.ObtenerArqueo(escenario.Caja.ObtenerSesionAbierta()!);

        Assert.Equal(1000m, arqueo.EfectivoEsperado);
        Assert.Equal(300m, arqueo.TotalVentas);

        var debito = Assert.Single(arqueo.TotalesPorMedioPago);
        Assert.Equal("DEBITO", debito.MedioPago);
        Assert.False(debito.AfectaEfectivo);
        Assert.Equal(300m, debito.Total);
    }

    [Fact]
    public void Un_pago_mixto_separa_el_efectivo_del_resto()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja(1000m);

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 5));
        venta.Pagos = [escenario.PagoDebito(300m), escenario.PagoEfectivo(200m)];
        escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);

        var arqueo = escenario.Caja.ObtenerArqueo(escenario.Caja.ObtenerSesionAbierta()!);

        Assert.Equal(1200m, arqueo.EfectivoEsperado);
        Assert.Equal(500m, arqueo.TotalVentas);
        Assert.Equal(2, arqueo.TotalesPorMedioPago.Count);
    }

    [Fact]
    public void Los_ingresos_y_egresos_manuales_mueven_el_esperado()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja(1000m);

        escenario.Caja.RegistrarMovimiento(escenario.Caja.ObtenerSesionAbierta()!.SesionCajaId,
            new RegistrarMovimientoCajaDto
            {
                UsuarioId = EscenarioVentas.UsuarioId,
                TipoMovimiento = "INGRESO",
                Importe = 500m,
                Motivo = "Aporte de socio"
            });

        escenario.Caja.RegistrarMovimiento(escenario.Caja.ObtenerSesionAbierta()!.SesionCajaId,
            new RegistrarMovimientoCajaDto
            {
                UsuarioId = EscenarioVentas.UsuarioId,
                TipoMovimiento = "EGRESO",
                Importe = 200m,
                Motivo = "Pago de flete"
            });

        var arqueo = escenario.Caja.ObtenerArqueo(escenario.Caja.ObtenerSesionAbierta()!);

        Assert.Equal(1300m, arqueo.EfectivoEsperado);
        Assert.Equal(500m, arqueo.TotalIngresos);
        Assert.Equal(200m, arqueo.TotalEgresos);
    }

    [Fact]
    public void Anular_una_venta_de_la_caja_abierta_baja_el_efectivo_esperado()
    {
        using var escenario = new EscenarioVentas();
        escenario.AbrirCaja(1000m);

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 3));
        venta.Pagos = [escenario.PagoEfectivo(300m)];
        var ventaId = escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);

        Assert.Equal(1300m, escenario.Caja.ObtenerArqueo(escenario.Caja.ObtenerSesionAbierta()!).EfectivoEsperado);

        escenario.Ventas.Anular(ventaId, new AnularVentaDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            Motivo = "Devolución"
        });

        var arqueo = escenario.Caja.ObtenerArqueo(escenario.Caja.ObtenerSesionAbierta()!);

        Assert.Equal(1000m, arqueo.EfectivoEsperado);
        Assert.Equal(0, arqueo.CantidadVentas);
    }

    [Fact]
    public void Cerrar_la_caja_registra_la_diferencia_del_arqueo()
    {
        using var escenario = new EscenarioVentas();
        var sesionId = escenario.AbrirCaja(1000m);

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 3));
        venta.Pagos = [escenario.PagoEfectivo(300m)];
        escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);

        // Se cuentan 1290 cuando deberían ser 1300: faltan 10.
        escenario.Caja.Cerrar(sesionId, EscenarioVentas.UsuarioId, 1290m, 1300m, "Cierre de prueba");

        var sesion = escenario.Caja.ObtenerPorId(sesionId)!;

        Assert.Equal("CERRADA", sesion.Estado);
        Assert.Equal(1300m, sesion.MontoEsperado);
        Assert.Equal(1290m, sesion.MontoContado);
        Assert.Equal(-10m, sesion.Diferencia);
        Assert.Null(escenario.Caja.ObtenerSesionAbierta());
    }

    [Fact]
    public void Despues_de_cerrar_se_puede_abrir_otra_caja()
    {
        using var escenario = new EscenarioVentas();
        var primera = escenario.AbrirCaja(1000m);

        escenario.Caja.Cerrar(primera, EscenarioVentas.UsuarioId, 1000m, 1000m, null);

        var segunda = escenario.AbrirCaja(500m);

        Assert.NotEqual(primera, segunda);
        Assert.Equal(500m, escenario.Caja.ObtenerSesionAbierta()!.MontoInicial);
    }

    [Fact]
    public void El_historial_de_movimientos_queda_completo()
    {
        using var escenario = new EscenarioVentas();
        var sesionId = escenario.AbrirCaja(1000m);

        var venta = escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 1));
        venta.Pagos = [escenario.PagoEfectivo(100m)];
        escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false);

        var movimientos = escenario.Caja.ObtenerMovimientos(sesionId);

        Assert.Equal(2, movimientos.Count);
        Assert.Equal("APERTURA", movimientos[0].TipoMovimiento);
        Assert.Equal("VENTA", movimientos[1].TipoMovimiento);
        Assert.Equal(100m, movimientos[1].Importe);
    }
}
