using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Terceros;
using Cotillon.Shared.DTOs.Ventas;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// La cuenta corriente de los clientes que pagan a fin de mes. Es plata, así que
/// lo que se prueba es que el saldo siempre pueda explicarse con sus movimientos
/// y que lo fiado no se cuente como plata en el cajón.
/// </summary>
public class CuentaCorrienteTests : IDisposable
{
    private const int JardinId = 1;

    private readonly EscenarioVentas _escenario = new();
    private readonly CuentaCorrienteRepository _cuenta;

    public CuentaCorrienteTests()
    {
        _cuenta = new CuentaCorrienteRepository(_escenario.Factory);

        using var conexion = _escenario.Abrir();

        conexion.Execute(
            """
            INSERT INTO Clientes (ClienteId, NombreCompleto, Telefono, Activo)
            VALUES (1, 'Jardín Arcoiris', '11-4444-4444', 1);
            """);
    }

    public void Dispose() => _escenario.Dispose();

    /// <summary>Vende en cuenta corriente al jardín y devuelve la venta.</summary>
    private VentaDto VenderFiado(decimal cantidadGlobos)
    {
        var total = _escenario.PrecioDe(EscenarioVentas.ProductoGlobo) * cantidadGlobos;

        var venta = _escenario.NuevaVenta(
            EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, cantidadGlobos));

        venta.ClienteId = JardinId;
        venta.Pagos = [new CrearPagoVentaDto { MedioPagoId = _escenario.MedioCuentaCorrienteId, Importe = total }];

        return _escenario.Ventas.ObtenerPorId(
            _escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false))!;
    }

    [Fact]
    public void Vender_en_cuenta_corriente_deja_la_deuda_anotada()
    {
        _escenario.AbrirCaja();

        VenderFiado(10);

        // 10 globos a $100 = $1.000.
        Assert.Equal(1000m, _cuenta.Saldo(JardinId));
    }

    [Fact]
    public void Lo_fiado_no_entra_al_cajon()
    {
        _escenario.AbrirCaja(montoInicial: 5000m);

        VenderFiado(10);

        var arqueo = _escenario.Arqueo();

        // La venta existe, pero en el cajón sigue habiendo sólo el monto inicial:
        // el cliente no dejó un peso.
        Assert.Equal(1000m, arqueo.TotalVentas);
        Assert.Equal(5000m, arqueo.EfectivoEsperado);
    }

    [Fact]
    public void No_se_puede_fiar_sin_elegir_el_cliente()
    {
        _escenario.AbrirCaja();

        var venta = _escenario.NuevaVenta(EscenarioVentas.Linea(EscenarioVentas.ProductoGlobo, 1));
        venta.Pagos = [new CrearPagoVentaDto { MedioPagoId = _escenario.MedioCuentaCorrienteId, Importe = 100m }];

        var error = Assert.Throws<VentaInvalidaException>(
            () => _escenario.Ventas.Crear(venta, permitirVentaSinStockGlobal: false));

        Assert.Contains("elegir el cliente", error.Message);
    }

    [Fact]
    public void El_pago_baja_el_saldo_y_entra_al_cajon()
    {
        _escenario.AbrirCaja(montoInicial: 0m);

        VenderFiado(10);

        _cuenta.RegistrarPago(new RegistrarPagoCuentaDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            ClienteId = JardinId,
            Importe = 600m,
            MedioPagoId = _escenario.MedioEfectivoId
        });

        Assert.Equal(400m, _cuenta.Saldo(JardinId));
        Assert.Equal(600m, _escenario.Arqueo().EfectivoEsperado);
    }

    [Fact]
    public void No_se_puede_pagar_mas_de_lo_que_se_debe()
    {
        _escenario.AbrirCaja();

        VenderFiado(10);

        var error = Assert.Throws<CuentaCorrienteInvalidaException>(() =>
            _cuenta.RegistrarPago(new RegistrarPagoCuentaDto
            {
                UsuarioId = EscenarioVentas.UsuarioId,
                ClienteId = JardinId,
                Importe = 1500m,
                MedioPagoId = _escenario.MedioEfectivoId
            }));

        Assert.Contains("supera la deuda", error.Message);
    }

    [Fact]
    public void No_se_puede_pagar_la_cuenta_corriente_con_cuenta_corriente()
    {
        _escenario.AbrirCaja();

        VenderFiado(10);

        Assert.Throws<CuentaCorrienteInvalidaException>(() =>
            _cuenta.RegistrarPago(new RegistrarPagoCuentaDto
            {
                UsuarioId = EscenarioVentas.UsuarioId,
                ClienteId = JardinId,
                Importe = 500m,
                MedioPagoId = _escenario.MedioCuentaCorrienteId
            }));
    }

    [Fact]
    public void Un_cliente_sin_deuda_no_puede_pagar()
    {
        _escenario.AbrirCaja();

        var error = Assert.Throws<CuentaCorrienteInvalidaException>(() =>
            _cuenta.RegistrarPago(new RegistrarPagoCuentaDto
            {
                UsuarioId = EscenarioVentas.UsuarioId,
                ClienteId = JardinId,
                Importe = 100m,
                MedioPagoId = _escenario.MedioEfectivoId
            }));

        Assert.Contains("no tiene deuda", error.Message);
    }

    [Fact]
    public void Anular_una_venta_fiada_borra_la_deuda()
    {
        _escenario.AbrirCaja();

        var venta = VenderFiado(10);

        _escenario.Anular(venta.VentaId);

        Assert.Equal(0m, _cuenta.Saldo(JardinId));

        // Y queda explicado: dos movimientos, no uno borrado.
        Assert.Equal(2, _cuenta.Obtener(JardinId).Movimientos.Count);
    }

    [Fact]
    public void El_resumen_trae_el_saldo_acumulado_de_cada_renglon()
    {
        _escenario.AbrirCaja(montoInicial: 0m);

        VenderFiado(10);
        VenderFiado(5);

        _cuenta.RegistrarPago(new RegistrarPagoCuentaDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            ClienteId = JardinId,
            Importe = 500m,
            MedioPagoId = _escenario.MedioEfectivoId
        });

        var cuenta = _cuenta.Obtener(JardinId);

        Assert.Equal(1000m, cuenta.Saldo);

        // El más reciente primero, y su acumulado es el saldo final.
        Assert.Equal(1000m, cuenta.Movimientos[0].SaldoAcumulado);
        Assert.Equal("PAGO", cuenta.Movimientos[0].Tipo);
    }

    [Fact]
    public void El_ajuste_sirve_para_cargar_una_deuda_vieja()
    {
        _cuenta.Ajustar(new AjustarCuentaDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            ClienteId = JardinId,
            Importe = 3000m,
            Motivo = "Saldo del cuaderno"
        });

        Assert.Equal(3000m, _cuenta.Saldo(JardinId));
    }

    [Fact]
    public void Un_ajuste_sin_motivo_no_se_registra()
    {
        Assert.Throws<CuentaCorrienteInvalidaException>(() =>
            _cuenta.Ajustar(new AjustarCuentaDto
            {
                UsuarioId = EscenarioVentas.UsuarioId,
                ClienteId = JardinId,
                Importe = 100m,
                Motivo = "  "
            }));
    }

    [Fact]
    public void La_lista_de_deudores_muestra_solo_a_los_que_deben()
    {
        _escenario.AbrirCaja();

        using (var conexion = _escenario.Abrir())
            conexion.Execute(
                "INSERT INTO Clientes (ClienteId, NombreCompleto, Activo) VALUES (2, 'Cliente al día', 1);");

        VenderFiado(10);

        var conDeuda = _cuenta.Saldos(soloConDeuda: true);

        Assert.Equal(JardinId, Assert.Single(conDeuda).ClienteId);
        Assert.Equal(2, _cuenta.Saldos(soloConDeuda: false).Count);
    }
}
