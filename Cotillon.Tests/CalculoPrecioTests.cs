using Cotillon.Shared.DTOs;

namespace Cotillon.Tests;

/// <summary>
/// Las cuentas de la calculadora de precios. Un margen mal despejado se
/// arrastra a toda la lista de precios, así que cada camino se prueba contra su
/// inverso.
/// </summary>
public class CalculoPrecioTests
{
    [Fact]
    public void Costo_mas_margen_da_el_precio()
    {
        // Lo compré a $100 y le gano el 50 %: lo vendo a $150.
        Assert.Equal(150m, CalculoPrecio.PrecioDesdeMargen(100m, 50m));
    }

    [Fact]
    public void El_margen_se_calcula_sobre_el_costo_y_no_sobre_la_venta()
    {
        // $100 de costo y $150 de precio es 50 % sobre el costo. Sobre la venta
        // daría 33 %, y confundir las dos definiciones es el error clásico.
        Assert.Equal(50m, CalculoPrecio.MargenDesdePrecio(100m, 150m));
    }

    [Fact]
    public void El_precio_y_el_margen_son_caminos_inversos()
    {
        var precio = CalculoPrecio.PrecioDesdeMargen(1234.56m, 73m);

        Assert.Equal(73m, Math.Round(CalculoPrecio.MargenDesdePrecio(1234.56m, precio), 6));
    }

    [Fact]
    public void Se_puede_despejar_el_costo_que_admite_un_precio()
    {
        // "Lo quiero vender a $1.500 ganando 50 %: me lo tienen que dejar a $1.000."
        Assert.Equal(1000m, CalculoPrecio.CostoDesdeMargen(1500m, 50m));
    }

    [Fact]
    public void Sin_costo_no_se_inventa_un_margen()
    {
        // Dividir por cero daría infinito y un margen absurdo en pantalla.
        Assert.Equal(0m, CalculoPrecio.MargenDesdePrecio(0m, 500m));
    }

    [Fact]
    public void El_costo_unitario_sale_de_dividir_el_bulto()
    {
        // La bolsa de 50 globos salió $12.000: $240 cada uno.
        Assert.Equal(240m, CalculoPrecio.CostoUnitario(12000m, 50m));
    }

    [Fact]
    public void Un_bulto_de_cero_unidades_no_rompe_la_cuenta()
    {
        Assert.Equal(0m, CalculoPrecio.CostoUnitario(12000m, 0m));
    }

    [Fact]
    public void La_variacion_dice_cuanto_aumento_el_proveedor()
    {
        // Venía a $1.000 y ahora lo facturan a $1.250: aumentó 25 %.
        Assert.Equal(25m, CalculoPrecio.Variacion(1000m, 1250m));
    }

    [Fact]
    public void Una_baja_da_variacion_negativa()
    {
        Assert.Equal(-20m, CalculoPrecio.Variacion(1000m, 800m));
    }

    [Fact]
    public void El_redondeo_deja_precios_de_mostrador()
    {
        Assert.Equal(1500m, CalculoPrecio.Redondear(1487m, 100));
        Assert.Equal(1450m, CalculoPrecio.Redondear(1437m, 50));
        Assert.Equal(1487m, CalculoPrecio.Redondear(1487m, 0));
    }

    [Fact]
    public void El_redondeo_nunca_regala_el_producto()
    {
        // $30 redondeado a los $100 daría $0.
        Assert.Equal(100m, CalculoPrecio.Redondear(30m, 100));
    }

    [Fact]
    public void El_iva_se_agrega_y_se_saca_sin_perder_el_importe()
    {
        var conIva = CalculoPrecio.ConIva(1000m, 21m);

        Assert.Equal(1210m, conIva);
        Assert.Equal(1000m, Math.Round(CalculoPrecio.SinIva(conIva, 21m), 6));
    }
}
