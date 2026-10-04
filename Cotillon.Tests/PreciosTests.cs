using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// Aumento de precios en masa. Un porcentaje mal aplicado a una sección entera
/// no se nota hasta que alguien compra, así que lo que se prueba es que la
/// vista previa diga exactamente lo que después se guarda.
/// </summary>
public class PreciosTests : IDisposable
{
    private readonly EscenarioVentas _escenario = new();
    private readonly PreciosRepository _precios;

    public PreciosTests()
    {
        _precios = new PreciosRepository(_escenario.Factory);

        using var conexion = _escenario.Abrir();

        conexion.Execute(
            """
            INSERT INTO Secciones (SeccionId, NombreSeccion, Activo) VALUES (2, 'Golosinas', 1);
            INSERT INTO Categorias (CategoriaId, SeccionId, NombreCategoria, Activo) VALUES (2, 2, 'Caramelos', 1);

            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo, Marca)
            VALUES (30, 2, 'Chupetín', 50000, 20000, 100, 10, 'Arcor');
            """);
    }

    public void Dispose() => _escenario.Dispose();

    private AumentoPreciosDto Aumento(decimal porcentaje) => new()
    {
        UsuarioId = EscenarioVentas.UsuarioId,
        Porcentaje = porcentaje
    };

    [Fact]
    public void Un_aumento_del_doce_por_ciento_sube_precio_y_costo()
    {
        var aumento = Aumento(12);

        var cambios = _precios.Simular(aumento);
        var globo = cambios.Single(c => c.ProductoId == EscenarioVentas.ProductoGlobo);

        // $100 + 12 % = $112. Costo $40 + 12 % = $44,80.
        Assert.Equal(112.00m, globo.PrecioNuevo);
        Assert.Equal(44.80m, globo.CostoNuevo);
    }

    [Fact]
    public void Lo_que_se_guarda_es_exactamente_lo_que_mostro_la_vista_previa()
    {
        var aumento = Aumento(12);
        aumento.RedondearA = 50;

        var previstos = _precios.Simular(aumento).ToDictionary(c => c.ProductoId, c => c.PrecioNuevo);

        _precios.Aplicar(aumento);

        foreach (var (productoId, precio) in previstos)
            Assert.Equal(precio, _escenario.PrecioDe(productoId));
    }

    [Fact]
    public void Se_puede_dejar_el_costo_sin_tocar()
    {
        var aumento = Aumento(12);
        aumento.AjustarCosto = false;

        var globo = _precios.Simular(aumento).Single(c => c.ProductoId == EscenarioVentas.ProductoGlobo);

        Assert.Equal(112.00m, globo.PrecioNuevo);
        Assert.Equal(40.00m, globo.CostoNuevo);
    }

    [Fact]
    public void El_redondeo_deja_los_precios_en_numeros_de_mostrador()
    {
        var aumento = Aumento(12);
        aumento.RedondearA = 50;

        // $112 redondeado a los $50 queda en $100.
        var globo = _precios.Simular(aumento).Single(c => c.ProductoId == EscenarioVentas.ProductoGlobo);

        Assert.Equal(100.00m, globo.PrecioNuevo);
    }

    [Fact]
    public void El_redondeo_nunca_deja_un_precio_en_cero()
    {
        using (var conexion = _escenario.Abrir())
            conexion.Execute(
                """
                INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo)
                VALUES (31, 1, 'Caramelo suelto', 1000, 500, 100, 1);
                """);

        var aumento = Aumento(10);
        aumento.RedondearA = 100;

        var barato = _precios.Simular(aumento).Single(c => c.ProductoId == 31);

        // $11 redondeado a los $100 daría $0: se deja en $100, no en regalado.
        Assert.Equal(100.00m, barato.PrecioNuevo);
    }

    [Fact]
    public void El_filtro_por_seccion_no_toca_lo_de_afuera()
    {
        var aumento = Aumento(50);
        aumento.SeccionId = 2;

        _precios.Aplicar(aumento);

        Assert.Equal(750.00m, _escenario.PrecioDe(30));
        Assert.Equal(100.00m, _escenario.PrecioDe(EscenarioVentas.ProductoGlobo));
    }

    [Fact]
    public void El_filtro_por_marca_tambien_limita()
    {
        var aumento = Aumento(20);
        aumento.Marca = "Arcor";

        Assert.Equal(30, Assert.Single(_precios.Simular(aumento)).ProductoId);
    }

    [Fact]
    public void Se_avisa_cuando_el_precio_queda_por_debajo_del_costo()
    {
        var aumento = Aumento(-80);
        aumento.AjustarCosto = false;

        var globo = _precios.Simular(aumento).Single(c => c.ProductoId == EscenarioVentas.ProductoGlobo);

        // $100 - 80 % = $20, con costo de $40.
        Assert.True(globo.QuedaEnPerdida);
    }

    [Fact]
    public void Una_baja_tambien_se_puede_aplicar()
    {
        _precios.Aplicar(Aumento(-10));

        Assert.Equal(90.00m, _escenario.PrecioDe(EscenarioVentas.ProductoGlobo));
    }

    [Fact]
    public void Un_porcentaje_absurdo_se_frena()
    {
        // 1200 % casi siempre es un cero de más, y deshacerlo es volver a cargar
        // la lista entera a mano.
        var error = Assert.Throws<AumentoInvalidoException>(() => _precios.Simular(Aumento(1200)));

        Assert.Contains("error de tipeo", error.Message);
    }

    [Fact]
    public void Un_porcentaje_en_cero_no_hace_nada()
    {
        Assert.Throws<AumentoInvalidoException>(() => _precios.Simular(Aumento(0)));
    }

    [Fact]
    public void Una_baja_del_cien_por_ciento_se_frena()
    {
        Assert.Throws<AumentoInvalidoException>(() => _precios.Simular(Aumento(-100)));
    }

    [Fact]
    public void Un_filtro_que_no_alcanza_a_nadie_no_se_aplica()
    {
        var aumento = Aumento(10);
        aumento.Marca = "Marca que no existe";

        Assert.Throws<AumentoInvalidoException>(() => _precios.Aplicar(aumento));
    }

    [Fact]
    public void Los_dados_de_baja_no_se_tocan()
    {
        using (var conexion = _escenario.Abrir())
            conexion.Execute("UPDATE Productos SET Activo = 0 WHERE ProductoId = 30;");

        Assert.DoesNotContain(_precios.Simular(Aumento(10)), c => c.ProductoId == 30);
    }
}
