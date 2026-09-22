using Cotillon.Data;

namespace Cotillon.Tests;

public class MoneyTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 100)]
    [InlineData(0.01, 1)]
    [InlineData(1234.56, 123456)]
    [InlineData(50000, 5000000)]
    public void ToCentavos_convierte_importes_exactos(decimal importe, long esperado) =>
        Assert.Equal(esperado, Money.ToCentavos(importe));

    [Theory]
    [InlineData(0.005, 1)]
    [InlineData(0.004, 0)]
    [InlineData(2.675, 268)]
    public void ToCentavos_redondea_medio_hacia_arriba(decimal importe, long esperado) =>
        Assert.Equal(esperado, Money.ToCentavos(importe));

    [Theory]
    [InlineData(0.01)]
    [InlineData(19.99)]
    [InlineData(123456.78)]
    public void Conversion_ida_y_vuelta_conserva_el_importe(decimal importe) =>
        Assert.Equal(importe, Money.FromCentavos(Money.ToCentavos(importe)));

    [Fact]
    public void Sumar_en_centavos_no_arrastra_error_de_punto_flotante()
    {
        // 0.1 + 0.2 en double da 0.30000000000000004; en centavos tiene que dar 0.30 exacto.
        var total = Money.ToCentavos(0.1m) + Money.ToCentavos(0.2m);

        Assert.Equal(0.30m, Money.FromCentavos(total));
    }
}
