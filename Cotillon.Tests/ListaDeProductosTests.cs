using Cotillon.Shared.DTOs;

namespace Cotillon.Tests;

/// <summary>
/// Lectura de la lista de productos pegada desde Excel. Lo que se cuida es que
/// un precio mal interpretado no entre al sistema sin que nadie lo note: una
/// lista de doscientos productos no se revisa a ojo renglón por renglón.
/// </summary>
public class ListaDeProductosTests
{
    [Fact]
    public void Lee_las_columnas_separadas_por_tabulador()
    {
        var filas = ListaDeProductos.Leer("Globo rojo n 9\t1500\t600\t50\t7791234567890");

        var fila = Assert.Single(filas);

        Assert.True(fila.EsValida);
        Assert.Equal("Globo rojo n 9", fila.Nombre);
        Assert.Equal(1500m, fila.Precio);
        Assert.Equal(600m, fila.Costo);
        Assert.Equal(50m, fila.Stock);
        Assert.Equal("7791234567890", fila.Codigo);
    }

    [Fact]
    public void Alcanza_con_el_nombre_y_el_precio()
    {
        var fila = Assert.Single(ListaDeProductos.Leer("Vincha de luces\t2500"));

        Assert.True(fila.EsValida);
        Assert.Equal(2500m, fila.Precio);
        Assert.Equal(0m, fila.Costo);
        Assert.Equal(0m, fila.Stock);
        Assert.Equal(string.Empty, fila.Codigo);
    }

    [Fact]
    public void Mil_quinientos_con_punto_son_mil_quinientos_pesos()
    {
        // El error más caro posible: leer "1.500" como uno con cinco y cargar un
        // producto a $1,50. Acá el punto es separador de miles.
        var fila = Assert.Single(ListaDeProductos.Leer("Piñata\t1.500"));

        Assert.Equal(1500m, fila.Precio);
    }

    [Fact]
    public void La_coma_es_el_separador_decimal()
    {
        Assert.Equal(1500.50m, Assert.Single(ListaDeProductos.Leer("Cinta\t1.500,50")).Precio);
        Assert.Equal(1500.50m, Assert.Single(ListaDeProductos.Leer("Cinta\t1500,50")).Precio);
    }

    [Fact]
    public void El_signo_pesos_y_los_espacios_no_molestan()
    {
        Assert.Equal(1500m, Assert.Single(ListaDeProductos.Leer("Globo\t $ 1.500 ")).Precio);
    }

    [Fact]
    public void El_punto_y_coma_tambien_separa_columnas()
    {
        var fila = Assert.Single(ListaDeProductos.Leer("Globo;1500;600;50"));

        Assert.True(fila.EsValida);
        Assert.Equal(1500m, fila.Precio);
        Assert.Equal(600m, fila.Costo);
    }

    [Fact]
    public void El_encabezado_de_la_planilla_se_saltea()
    {
        var filas = ListaDeProductos.Leer("Nombre\tPrecio\tCosto\nGlobo\t1500\t600");

        Assert.Single(filas);
        Assert.Equal("Globo", filas[0].Nombre);
    }

    [Fact]
    public void Los_renglones_en_blanco_se_ignoran()
    {
        var filas = ListaDeProductos.Leer("Globo\t1500\n\n\nVincha\t2500\n");

        Assert.Equal(2, filas.Count);
    }

    [Fact]
    public void Una_fila_sin_nombre_se_marca_y_no_frena_a_las_demas()
    {
        var filas = ListaDeProductos.Leer("Globo\t1500\n\t900\nVincha\t2500");

        Assert.Equal(3, filas.Count);
        Assert.True(filas[0].EsValida);
        Assert.False(filas[1].EsValida);
        Assert.Contains("nombre", filas[1].Error!);
        Assert.True(filas[2].EsValida);

        // El renglón que se informa es el de la planilla, no el del índice.
        Assert.Equal(2, filas[1].Linea);
    }

    [Fact]
    public void Un_precio_que_no_es_un_numero_se_marca()
    {
        var fila = Assert.Single(ListaDeProductos.Leer("Globo\ta convenir"));

        Assert.False(fila.EsValida);
        Assert.Contains("a convenir", fila.Error!);
    }

    [Fact]
    public void Un_precio_negativo_se_marca()
    {
        var fila = Assert.Single(ListaDeProductos.Leer("Globo\t-500"));

        Assert.False(fila.EsValida);
        Assert.Contains("negativos", fila.Error!);
    }

    [Fact]
    public void Un_codigo_repetido_en_la_lista_se_marca_y_dice_donde_esta_el_otro()
    {
        var filas = ListaDeProductos.Leer(
            "Globo\t1500\t0\t0\t779123\nVincha\t2500\t0\t0\t779123");

        Assert.True(filas[0].EsValida);
        Assert.False(filas[1].EsValida);
        Assert.Contains("renglón 1", filas[1].Error!);
    }

    [Fact]
    public void Varias_filas_sin_codigo_no_se_toman_como_repetidas()
    {
        var filas = ListaDeProductos.Leer("Globo\t1500\nVincha\t2500\nGorro\t900");

        Assert.All(filas, f => Assert.True(f.EsValida));
    }

    [Fact]
    public void Una_lista_vacia_no_devuelve_nada()
    {
        Assert.Empty(ListaDeProductos.Leer(null));
        Assert.Empty(ListaDeProductos.Leer("   \n  \n"));
    }
}
