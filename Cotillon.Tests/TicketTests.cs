using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Caja;
using Cotillon.Shared.DTOs.Ventas;

namespace Cotillon.Tests;

/// <summary>
/// Armado del ticket. Un ticket descuadrado o con un total que no cierra se
/// tiene que ver acá y no en el mostrador con el cliente esperando.
/// </summary>
public class TicketTests
{
    private static readonly DatosTicket Negocio = new(
        "Cotillón La Fiesta", "San Martín 250", "11-5555-5555", "20-12345678-9",
        "¡Gracias por su compra!");

    private static VentaDto Venta() => new()
    {
        VentaId = 42,
        FechaVenta = new DateTime(2026, 10, 3, 17, 30, 0),
        Usuario = "Vendedora",
        Cliente = "Consumidor final",
        Estado = "COMPLETADA",
        Subtotal = 7075.00m,
        TotalVenta = 7075.00m,
        Vuelto = 2925.00m,
        Lineas =
        [
            new LineaVentaDto
            {
                NombreProducto = "Globo rojo n 9", Cantidad = 10, PrecioUnitario = 550.00m,
                SubtotalLinea = 5500.00m, UnidadMedida = UnidadesMedida.Unidad
            },
            new LineaVentaDto
            {
                NombreProducto = "Caramelos surtidos", Cantidad = 0.350m, PrecioUnitario = 4500.00m,
                SubtotalLinea = 1575.00m, UnidadMedida = UnidadesMedida.Kilo
            }
        ],
        Pagos = [new PagoVentaDto { MedioPago = "EFECTIVO", Importe = 10000.00m, Vuelto = 2925.00m }]
    };

    private static string[] Renglones(string ticket) =>
        ticket.Split('\n').Select(l => l.TrimEnd('\r')).ToArray();

    [Fact]
    public void Ningun_renglon_pasa_del_ancho_del_papel()
    {
        // Si un renglón se pasa, la térmica lo corta o lo baja y el ticket sale roto.
        foreach (var ancho in new[] { Ticket.Ancho58mm, Ticket.Ancho80mm })
            Assert.All(Renglones(Ticket.Venta(Venta(), Negocio, ancho)),
                renglon => Assert.True(renglon.Length <= ancho,
                    $"Con papel de {ancho}: «{renglon}» mide {renglon.Length}."));
    }

    [Fact]
    public void El_importe_queda_pegado_al_borde_derecho()
    {
        var total = Renglones(Ticket.Venta(Venta(), Negocio, Ticket.Ancho80mm))
            .Single(l => l.StartsWith("TOTAL"));

        Assert.Equal(Ticket.Ancho80mm, total.Length);
        Assert.EndsWith("7.075,00", total);
    }

    [Fact]
    public void El_peso_sale_con_su_unidad_y_no_como_un_numero_suelto()
    {
        var ticket = Ticket.Venta(Venta(), Negocio, Ticket.Ancho80mm);

        Assert.Contains("0,35 kg x 4.500,00", ticket);
        Assert.Contains("10 u. x 550,00", ticket);
    }

    [Fact]
    public void Lleva_los_datos_del_negocio_y_el_pie_configurado()
    {
        var ticket = Ticket.Venta(Venta(), Negocio, Ticket.Ancho80mm);

        Assert.Contains("COTILLÓN LA FIESTA", ticket);
        Assert.Contains("San Martín 250", ticket);
        Assert.Contains("CUIT 20-12345678-9", ticket);
        Assert.Contains("¡Gracias por su compra!", ticket);
    }

    [Fact]
    public void Lleva_la_venta_la_fecha_y_quien_atendio()
    {
        var ticket = Ticket.Venta(Venta(), Negocio, Ticket.Ancho80mm);

        Assert.Contains("#42", ticket);
        Assert.Contains("03/10/2026 17:30", ticket);
        Assert.Contains("Vendedora", ticket);
    }

    [Fact]
    public void El_vuelto_se_imprime_cuando_lo_hay()
    {
        Assert.Contains("Vuelto", Ticket.Venta(Venta(), Negocio));

        var justa = Venta();
        justa.Vuelto = 0m;
        justa.Pagos = [new PagoVentaDto { MedioPago = "DEBITO", Importe = 7075.00m }];

        Assert.DoesNotContain("Vuelto", Ticket.Venta(justa, Negocio));
    }

    [Fact]
    public void Una_venta_anulada_lo_dice_arriba_de_todo()
    {
        var anulada = Venta();
        anulada.Estado = "ANULADA";

        Assert.Contains("*** VENTA ANULADA ***", Ticket.Venta(anulada, Negocio));
    }

    [Fact]
    public void El_cliente_sale_solo_cuando_no_es_consumidor_final()
    {
        Assert.DoesNotContain("Cliente", Ticket.Venta(Venta(), Negocio));

        var aCliente = Venta();
        aCliente.ClienteId = 7;
        aCliente.Cliente = "Fiesta Infantil SRL";

        Assert.Contains("Fiesta Infantil SRL", Ticket.Venta(aCliente, Negocio));
    }

    [Fact]
    public void Un_nombre_largo_baja_de_renglon_en_vez_de_cortarse()
    {
        var larga = Venta();
        larga.Lineas[0].NombreProducto =
            "Guirnalda metalizada multicolor de cinco metros para cumpleaños";

        var ticket = Ticket.Venta(larga, Negocio, Ticket.Ancho58mm);

        // Entero, aunque repartido en varios renglones.
        Assert.Contains("Guirnalda metalizada", ticket);
        Assert.Contains("cumpleaños", ticket);
        Assert.All(Renglones(ticket), r => Assert.True(r.Length <= Ticket.Ancho58mm));
    }

    [Fact]
    public void El_descuento_se_muestra_aparte_del_subtotal()
    {
        var conDescuento = Venta();
        conDescuento.DescuentoTotal = 500.00m;
        conDescuento.TotalVenta = 6575.00m;

        var ticket = Ticket.Venta(conDescuento, Negocio);

        Assert.Contains("Subtotal", ticket);
        Assert.Contains("-500,00", ticket);
    }

    [Fact]
    public void Sin_configuracion_el_ticket_igual_sale()
    {
        // Una instalación recién hecha no tiene los datos del negocio cargados y
        // la caja no puede quedarse sin poder imprimir por eso.
        var vacio = DatosTicket.Desde(null);

        var ticket = Ticket.Venta(Venta(), vacio, Ticket.Ancho58mm);

        Assert.Contains("COTILLÓN", ticket);
        Assert.Contains("TOTAL", ticket);
        Assert.All(Renglones(ticket), r => Assert.True(r.Length <= Ticket.Ancho58mm));
    }

    [Fact]
    public void El_cierre_de_caja_muestra_el_faltante_con_su_palabra()
    {
        var arqueo = new ArqueoCajaDto
        {
            Sesion = new SesionCajaDto
            {
                SesionCajaId = 3,
                FechaApertura = new DateTime(2026, 10, 3, 9, 0, 0),
                FechaCierre = new DateTime(2026, 10, 3, 20, 0, 0),
                UsuarioApertura = "Vendedora",
                UsuarioCierre = "Administrador",
                MontoInicial = 10000m,
                MontoContado = 48000m,
                Diferencia = -2000m
            },
            TotalVentas = 40000m,
            CantidadVentas = 12,
            EfectivoEsperado = 50000m,
            TotalesPorMedioPago = [new TotalPorMedioPagoDto { MedioPago = "EFECTIVO", Total = 40000m, Cantidad = 12 }]
        };

        var ticket = Ticket.CierreDeCaja(arqueo, Negocio, Ticket.Ancho80mm);

        Assert.Contains("CIERRE DE CAJA", ticket);
        Assert.Contains("FALTA", ticket);
        Assert.Contains("2.000,00", ticket);
        Assert.All(Renglones(ticket), r => Assert.True(r.Length <= Ticket.Ancho80mm));
    }

    [Fact]
    public void Un_cierre_sin_diferencia_no_dice_ni_falta_ni_sobra()
    {
        var arqueo = new ArqueoCajaDto
        {
            Sesion = new SesionCajaDto { MontoContado = 50000m, Diferencia = 0m },
            EfectivoEsperado = 50000m
        };

        var ticket = Ticket.CierreDeCaja(arqueo, Negocio);

        Assert.Contains("Diferencia", ticket);
        Assert.DoesNotContain("FALTA", ticket);
        Assert.DoesNotContain("SOBRA", ticket);
    }
}
