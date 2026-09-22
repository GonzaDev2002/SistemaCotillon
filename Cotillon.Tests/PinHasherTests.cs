using Cotillon.Data;

namespace Cotillon.Tests;

public class PinHasherTests
{
    [Fact]
    public void Verificar_acepta_el_pin_correcto()
    {
        var (hash, salt) = PinHasher.Hashear("1234");

        Assert.True(PinHasher.Verificar("1234", hash, salt));
    }

    [Fact]
    public void Verificar_rechaza_un_pin_incorrecto()
    {
        var (hash, salt) = PinHasher.Hashear("1234");

        Assert.False(PinHasher.Verificar("4321", hash, salt));
    }

    [Fact]
    public void Hashear_el_mismo_pin_dos_veces_da_hashes_distintos()
    {
        var primero = PinHasher.Hashear("1234");
        var segundo = PinHasher.Hashear("1234");

        Assert.NotEqual(primero.Hash, segundo.Hash);
        Assert.NotEqual(primero.Salt, segundo.Salt);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Hashear_rechaza_pines_vacios(string pin) =>
        Assert.Throws<ArgumentException>(() => PinHasher.Hashear(pin));

    [Theory]
    [InlineData("", "")]
    [InlineData("no-es-base64!", "tampoco")]
    public void Verificar_no_explota_con_datos_corruptos(string hash, string salt) =>
        Assert.False(PinHasher.Verificar("1234", hash, salt));
}
