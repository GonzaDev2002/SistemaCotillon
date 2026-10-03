using Cotillon.Shared.DTOs;

namespace Cotillon.Tests;

/// <summary>
/// Lectura de las etiquetas que imprime la balanza. Lo que se cuida es que una
/// etiqueta mal leída no cobre un peso equivocado y que un código común no se
/// confunda nunca con una pesada.
/// </summary>
public class EtiquetaBalanzaTests
{
    private const string Mascara = "20CCCCCPPPPPD";

    [Fact]
    public void Lee_el_codigo_y_el_peso_de_una_pesada()
    {
        // 20 | 00123 | 01250 | 7  ->  artículo 00123, 1,250 kg
        var etiqueta = EtiquetaBalanza.Leer("2000123012507", Mascara);

        Assert.NotNull(etiqueta);
        Assert.Equal("00123", etiqueta.Codigo);
        Assert.Equal(1.250m, etiqueta.Peso);
        Assert.Null(etiqueta.Importe);
    }

    [Fact]
    public void Los_gramos_sueltos_no_se_redondean_a_kilos()
    {
        var etiqueta = EtiquetaBalanza.Leer("2000123003507", Mascara);

        Assert.Equal(0.350m, etiqueta!.Peso);
    }

    [Fact]
    public void Una_etiqueta_con_el_importe_adentro_devuelve_pesos()
    {
        // La misma balanza se puede configurar para imprimir el precio.
        var etiqueta = EtiquetaBalanza.Leer("2001231575007", "2CCCCCIIIIIID");

        Assert.Equal(1575.00m, etiqueta!.Importe);
        Assert.Null(etiqueta.Peso);
    }

    [Fact]
    public void Un_codigo_de_barras_comun_no_se_toma_como_pesada()
    {
        // Prefijo distinto: es un producto de góndola, se busca como siempre.
        Assert.Null(EtiquetaBalanza.Leer("7791234567890", Mascara));
    }

    [Fact]
    public void Un_codigo_de_otro_largo_no_se_toma_como_pesada()
    {
        Assert.Null(EtiquetaBalanza.Leer("200012301250", Mascara));
        Assert.Null(EtiquetaBalanza.Leer("20001230125078", Mascara));
    }

    [Fact]
    public void Un_nombre_tipeado_a_mano_no_se_toma_como_pesada()
    {
        Assert.Null(EtiquetaBalanza.Leer("caramelos", Mascara));
        Assert.Null(EtiquetaBalanza.Leer("2000123A1250", Mascara));
    }

    [Fact]
    public void Sin_mascara_configurada_la_funcion_esta_apagada()
    {
        // Es el estado de fábrica: nadie se encuentra con pesadas que no pidió.
        Assert.Null(EtiquetaBalanza.Leer("2000123012507", null));
        Assert.Null(EtiquetaBalanza.Leer("2000123012507", "   "));
    }

    [Fact]
    public void Una_mascara_sin_peso_ni_importe_se_ignora()
    {
        // Describe un código común, no una etiqueta de balanza.
        Assert.Null(EtiquetaBalanza.Leer("2000123012507", "20CCCCCCCCCCD"));
        Assert.Null(EtiquetaBalanza.Leer("2000123012507", "20DDDDDPPPPPD"));
    }

    [Fact]
    public void La_mascara_se_acepta_en_minuscula()
    {
        var etiqueta = EtiquetaBalanza.Leer("2000123012507", "20cccccpppppd");

        Assert.Equal("00123", etiqueta!.Codigo);
        Assert.Equal(1.250m, etiqueta.Peso);
    }

    [Fact]
    public void Funciona_con_el_prefijo_de_un_solo_digito()
    {
        // Otras balanzas usan un prefijo más corto y más dígitos de código.
        var etiqueta = EtiquetaBalanza.Leer("2001234123457", "2CCCCCCPPPPPD");

        Assert.Equal("001234", etiqueta!.Codigo);
        Assert.Equal(12.345m, etiqueta.Peso);
    }
}
