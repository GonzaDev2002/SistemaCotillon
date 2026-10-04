using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs.Encargues;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// Los encargues con seña. Lo que se cuida es que la mercadería no se esconda
/// antes de tiempo y que la seña no se cobre dos veces.
/// </summary>
public class EncarguesTests : IDisposable
{
    private readonly EscenarioVentas _escenario = new();
    private readonly EncarguesRepository _encargues;

    public EncarguesTests()
    {
        _encargues = new EncarguesRepository(_escenario.Factory, _escenario.Ventas);

        using var conexion = _escenario.Abrir();

        conexion.Execute(
            "INSERT INTO Clientes (ClienteId, NombreCompleto, Activo) VALUES (1, 'Jardín Arcoiris', 1);");
    }

    public void Dispose() => _escenario.Dispose();

    private CrearEncargueDto Pedido(decimal sena = 0, int globos = 10) => new()
    {
        UsuarioId = EscenarioVentas.UsuarioId,
        ClienteId = 1,
        NombreContacto = "Jardín Arcoiris",
        Telefono = "11-4444-4444",
        FechaEntrega = DateTime.Today.AddDays(7),
        Sena = sena,
        Lineas = [new CrearLineaEncargueDto { ProductoId = EscenarioVentas.ProductoGlobo, Cantidad = globos }]
    };

    [Fact]
    public void Tomar_el_encargue_NO_descuenta_stock()
    {
        _escenario.AbrirCaja();

        _encargues.Crear(Pedido());

        // La mercadería sigue en el salón: reservarla sería esconder stock que
        // está ahí y se le puede vender a quien entre por la puerta.
        Assert.Equal(50, _escenario.StockDe(EscenarioVentas.ProductoGlobo), precision: 6);
    }

    [Fact]
    public void El_total_sale_de_los_precios_del_dia_del_pedido()
    {
        _escenario.AbrirCaja();

        var encargue = _encargues.Obtener(_encargues.Crear(Pedido()))!;

        // 10 globos a $100.
        Assert.Equal(1000m, encargue.Total);
        Assert.Equal(1000m, encargue.Saldo);
    }

    [Fact]
    public void La_sena_entra_a_la_caja_el_dia_que_se_toma_el_pedido()
    {
        _escenario.AbrirCaja(montoInicial: 0m);

        _encargues.Crear(Pedido(sena: 300m));

        Assert.Equal(300m, _escenario.Arqueo().EfectivoEsperado);
    }

    [Fact]
    public void La_sena_no_puede_superar_el_total()
    {
        _escenario.AbrirCaja();

        var error = Assert.Throws<EncargueInvalidoException>(() => _encargues.Crear(Pedido(sena: 5000m)));

        Assert.Contains("supera el total", error.Message);
    }

    [Fact]
    public void Entregar_descuenta_el_stock_y_deja_la_venta()
    {
        _escenario.AbrirCaja(montoInicial: 0m);

        var id = _encargues.Crear(Pedido(sena: 300m));

        var ventaId = _encargues.Entregar(id, new EntregarEncargueDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            MedioPagoId = _escenario.MedioEfectivoId,
            Importe = 700m
        });

        Assert.Equal(40, _escenario.StockDe(EscenarioVentas.ProductoGlobo), precision: 6);

        var encargue = _encargues.Obtener(id)!;

        Assert.Equal("ENTREGADO", encargue.Estado);
        Assert.Equal(ventaId, encargue.VentaId);
    }

    [Fact]
    public void La_sena_no_se_cobra_dos_veces()
    {
        _escenario.AbrirCaja(montoInicial: 0m);

        var id = _encargues.Crear(Pedido(sena: 300m));

        _encargues.Entregar(id, new EntregarEncargueDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            MedioPagoId = _escenario.MedioEfectivoId,
            Importe = 700m
        });

        // El encargue valía $1.000: $300 de seña el día del pedido y $700 al
        // entregar. En el cajón tiene que haber $1.000, no $1.300.
        Assert.Equal(1000m, _escenario.Arqueo().EfectivoEsperado);
    }

    [Fact]
    public void No_se_entrega_cobrando_menos_que_el_saldo()
    {
        _escenario.AbrirCaja();

        var id = _encargues.Crear(Pedido(sena: 300m));

        var error = Assert.Throws<EncargueInvalidoException>(() =>
            _encargues.Entregar(id, new EntregarEncargueDto
            {
                UsuarioId = EscenarioVentas.UsuarioId,
                MedioPagoId = _escenario.MedioEfectivoId,
                Importe = 500m
            }));

        Assert.Contains("Falta cobrar", error.Message);
    }

    [Fact]
    public void Un_encargue_entregado_no_se_entrega_de_nuevo()
    {
        _escenario.AbrirCaja();

        var id = _encargues.Crear(Pedido());

        var entrega = new EntregarEncargueDto
        {
            UsuarioId = EscenarioVentas.UsuarioId,
            MedioPagoId = _escenario.MedioEfectivoId,
            Importe = 1000m
        };

        _encargues.Entregar(id, entrega);

        Assert.Throws<EncargueInvalidoException>(() => _encargues.Entregar(id, entrega));
    }

    [Fact]
    public void Cancelar_no_devuelve_la_sena_sola()
    {
        _escenario.AbrirCaja(montoInicial: 0m);

        var id = _encargues.Crear(Pedido(sena: 300m));

        _encargues.Cancelar(id, EscenarioVentas.UsuarioId, "El cliente se arrepintió");

        Assert.Equal("CANCELADO", _encargues.Obtener(id)!.Estado);

        // La plata sigue en el cajón: devolverla es un egreso que alguien tiene
        // que registrar y firmar, no algo que pase solo.
        Assert.Equal(300m, _escenario.Arqueo().EfectivoEsperado);
    }

    [Fact]
    public void No_se_toma_un_encargue_para_una_fecha_que_ya_paso()
    {
        _escenario.AbrirCaja();

        var pedido = Pedido();
        pedido.FechaEntrega = DateTime.Today.AddDays(-1);

        Assert.Throws<EncargueInvalidoException>(() => _encargues.Crear(pedido));
    }

    [Fact]
    public void Un_encargue_sin_productos_no_se_toma()
    {
        var pedido = Pedido();
        pedido.Lineas = [];

        Assert.Throws<EncargueInvalidoException>(() => _encargues.Crear(pedido));
    }

    [Fact]
    public void La_agenda_ordena_por_fecha_de_entrega_y_deja_los_pendientes_arriba()
    {
        _escenario.AbrirCaja();

        var lejano = Pedido();
        lejano.FechaEntrega = DateTime.Today.AddDays(30);
        _encargues.Crear(lejano);

        var cercano = Pedido();
        cercano.FechaEntrega = DateTime.Today.AddDays(2);
        var idCercano = _encargues.Crear(cercano);

        var agenda = _encargues.Listar(hasta: null, incluirCerrados: false);

        Assert.Equal(idCercano, agenda[0].EncargueId);
        Assert.Equal(2, agenda[0].DiasParaEntrega);
    }

    [Fact]
    public void La_agenda_se_puede_cortar_hasta_una_fecha()
    {
        _escenario.AbrirCaja();

        var lejano = Pedido();
        lejano.FechaEntrega = DateTime.Today.AddDays(30);
        _encargues.Crear(lejano);

        _encargues.Crear(Pedido());

        // Lo de esta semana, que es lo que hay que tener listo.
        Assert.Single(_encargues.Listar(DateTime.Today.AddDays(7), incluirCerrados: false));
    }
}
