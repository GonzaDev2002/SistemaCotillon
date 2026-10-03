using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Ventas;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// Venta de productos que no se venden por unidad. Lo que se prueba es que la
/// fracción sobreviva entera: el importe de 350 g y el stock que queda después.
/// </summary>
public class VentaPorPesoTests : IDisposable
{
    private const int CaramelosId = 10;
    private const int CintaId = 11;

    private readonly EscenarioVentas _escenario = new();

    public VentaPorPesoTests()
    {
        using var conexion = _escenario.Abrir();

        conexion.Execute(
            """
            -- $4.500 el kilo, 2,5 kg en el frasco
            INSERT INTO Productos
                (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos,
                 Stock, StockMinimo, UnidadMedida)
            VALUES (10, 1, 'Caramelos surtidos', 450000, 200000, 2.5, 0.5, 'KILO');

            -- $800 el metro, 50 m en el rollo
            INSERT INTO Productos
                (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos,
                 Stock, StockMinimo, UnidadMedida)
            VALUES (11, 1, 'Cinta de raso', 80000, 30000, 50, 5, 'METRO');
            """);
    }

    public void Dispose() => _escenario.Dispose();

    [Fact]
    public void Vender_350_gramos_cobra_la_fraccion_del_precio_por_kilo()
    {
        _escenario.AbrirCaja();

        // 0,350 kg x $4.500 = $1.575
        var venta = _escenario.Vender((CaramelosId, 0.350m));

        Assert.Equal(1575.00m, venta.TotalVenta);
        Assert.Equal(0.350m, venta.Lineas.Single().Cantidad);
    }

    [Fact]
    public void El_stock_queda_con_la_fraccion_descontada()
    {
        _escenario.AbrirCaja();

        _escenario.Vender((CaramelosId, 0.350m));

        // 2,5 kg - 0,350 kg = 2,150 kg. Si el stock fuera entero quedaría en 2.
        Assert.Equal(2.15, _escenario.StockDe(CaramelosId), precision: 6);
    }

    [Fact]
    public void Varias_ventas_chicas_no_acumulan_error_de_redondeo()
    {
        _escenario.AbrirCaja();

        // Diez pesadas de 100 g tienen que dejar el frasco en 1,5 kg exacto.
        for (var i = 0; i < 10; i++)
            _escenario.Vender((CaramelosId, 0.100m));

        Assert.Equal(1.5, _escenario.StockDe(CaramelosId), precision: 6);
    }

    [Fact]
    public void No_se_puede_vender_mas_peso_del_que_hay()
    {
        _escenario.AbrirCaja();

        var error = Assert.Throws<Cotillon.LocalApi.Service.Repositories.VentaInvalidaException>(
            () => _escenario.Vender((CaramelosId, 3m)));

        // El mensaje tiene que hablar en kilos, no en números sueltos.
        Assert.Contains("3 kg", error.Message);
        Assert.Contains("2,5 kg", error.Message);
    }

    [Fact]
    public void Se_puede_vender_por_metro_con_decimales()
    {
        _escenario.AbrirCaja();

        // 1,5 m x $800 = $1.200
        var venta = _escenario.Vender((CintaId, 1.5m));

        Assert.Equal(1200.00m, venta.TotalVenta);
        Assert.Equal(48.5, _escenario.StockDe(CintaId), precision: 6);
    }

    [Fact]
    public void Anular_devuelve_la_fraccion_exacta_al_stock()
    {
        _escenario.AbrirCaja();

        var venta = _escenario.Vender((CaramelosId, 0.350m));

        _escenario.Anular(venta.VentaId);

        Assert.Equal(2.5, _escenario.StockDe(CaramelosId), precision: 6);
    }
}

/// <summary>
/// Conversión entre lo que se tipea y lo que se guarda. El vendedor escribe los
/// gramos que marca la balanza; la venta se registra en kilos.
/// </summary>
public class UnidadesMedidaTests
{
    [Fact]
    public void Un_codigo_desconocido_cae_en_unidad_y_no_rompe_la_venta()
    {
        // Una base migrada puede traer textos viejos en UnidadMedida.
        Assert.Equal(UnidadesMedida.Unidad, UnidadesMedida.De("CAJA").Codigo);
        Assert.Equal(UnidadesMedida.Unidad, UnidadesMedida.De(null).Codigo);
        Assert.False(UnidadesMedida.EsFraccionada("cualquier cosa"));
    }

    [Fact]
    public void El_codigo_se_reconoce_sin_importar_mayusculas()
    {
        Assert.Equal(UnidadesMedida.Kilo, UnidadesMedida.De("kilo").Codigo);
        Assert.True(UnidadesMedida.EsFraccionada("Kilo"));
    }

    [Fact]
    public void Mil_gramos_es_un_kilo()
    {
        var kilo = UnidadesMedida.De(UnidadesMedida.Kilo);

        Assert.Equal(1000, kilo.PorUnidad);
        Assert.Equal(0.350m, 350m / kilo.PorUnidad);
    }

    [Fact]
    public void Solo_la_unidad_suelta_no_admite_fracciones()
    {
        Assert.False(UnidadesMedida.EsFraccionada(UnidadesMedida.Unidad));
        Assert.True(UnidadesMedida.EsFraccionada(UnidadesMedida.Kilo));
        Assert.True(UnidadesMedida.EsFraccionada(UnidadesMedida.Metro));
    }

    [Fact]
    public void El_formato_lleva_la_unidad_al_lado_del_numero()
    {
        Assert.Equal("0,35 kg", UnidadesMedida.De(UnidadesMedida.Kilo).Formatear(0.35m));
        Assert.Equal("3 u.", UnidadesMedida.De(UnidadesMedida.Unidad).Formatear(3m));
        Assert.Equal("1,5 m", UnidadesMedida.De(UnidadesMedida.Metro).Formatear(1.5m));
    }
}
