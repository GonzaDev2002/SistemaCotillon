using Cotillon.LocalApi.Service.Repositories;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// La bolsita sorpresa. Lo que se cuida es que al venderla salga de la góndola
/// exactamente lo que la arma, ni más ni menos, y que al anularla vuelva lo
/// mismo que salió.
/// </summary>
public class CombosTests : IDisposable
{
    private const int Caramelo = 20;
    private const int Gorro = 21;
    private const int Bolsita = 22;

    private readonly EscenarioVentas _escenario = new();
    private readonly CombosRepository _combos;

    public CombosTests()
    {
        _combos = new CombosRepository(_escenario.Factory);

        using var conexion = _escenario.Abrir();

        conexion.Execute(
            """
            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo)
            VALUES (20, 1, 'Caramelo', 10000, 4000, 100, 10);

            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo)
            VALUES (21, 1, 'Gorrito', 50000, 20000, 30, 5);

            -- La bolsita: no tiene stock propio, se arma con lo de arriba.
            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo, EsCombo)
            VALUES (22, 1, 'Bolsita sorpresa', 150000, 0, 0, 0, 1);
            """);

        _combos.Guardar(Bolsita,
        [
            new ComponenteComboDto { ComponenteProductoId = Caramelo, Cantidad = 5 },
            new ComponenteComboDto { ComponenteProductoId = Gorro, Cantidad = 1 }
        ], EscenarioVentas.UsuarioId);
    }

    public void Dispose() => _escenario.Dispose();

    [Fact]
    public void Vender_un_combo_descuenta_sus_componentes_y_no_el_combo()
    {
        _escenario.AbrirCaja();

        _escenario.Vender((Bolsita, 2));

        // 2 bolsitas = 10 caramelos y 2 gorritos.
        Assert.Equal(90, _escenario.StockDe(Caramelo), precision: 6);
        Assert.Equal(28, _escenario.StockDe(Gorro), precision: 6);

        // El combo no tiene stock propio: sigue en cero, no en -2.
        Assert.Equal(0, _escenario.StockDe(Bolsita), precision: 6);
    }

    [Fact]
    public void El_combo_se_cobra_a_su_propio_precio_y_no_a_la_suma_de_las_partes()
    {
        _escenario.AbrirCaja();

        // $1.500 la bolsita, aunque adentro lleve $1.000 de caramelos y $500 de gorro.
        var venta = _escenario.Vender((Bolsita, 1));

        Assert.Equal(1500.00m, venta.TotalVenta);
    }

    [Fact]
    public void No_se_vende_un_combo_si_falta_uno_solo_de_sus_componentes()
    {
        _escenario.AbrirCaja();

        using (var conexion = _escenario.Abrir())
            conexion.Execute("UPDATE Productos SET Stock = 1 WHERE ProductoId = 21;");

        var error = Assert.Throws<VentaInvalidaException>(() => _escenario.Vender((Bolsita, 5)));

        // El mensaje tiene que decir qué falta, no "stock insuficiente" a secas.
        Assert.Contains("Bolsita sorpresa", error.Message);
        Assert.Contains("Gorrito", error.Message);
    }

    [Fact]
    public void Anular_devuelve_los_componentes_y_no_el_combo()
    {
        _escenario.AbrirCaja();

        var venta = _escenario.Vender((Bolsita, 3));

        _escenario.Anular(venta.VentaId);

        Assert.Equal(100, _escenario.StockDe(Caramelo), precision: 6);
        Assert.Equal(30, _escenario.StockDe(Gorro), precision: 6);
        Assert.Equal(0, _escenario.StockDe(Bolsita), precision: 6);
    }

    [Fact]
    public void Anular_devuelve_lo_que_salio_aunque_la_receta_haya_cambiado_despues()
    {
        _escenario.AbrirCaja();

        var venta = _escenario.Vender((Bolsita, 2));

        // Se cambia la receta DESPUÉS de vender: ahora lleva 10 caramelos.
        _combos.Guardar(Bolsita,
        [
            new ComponenteComboDto { ComponenteProductoId = Caramelo, Cantidad = 10 }
        ], EscenarioVentas.UsuarioId);

        _escenario.Anular(venta.VentaId);

        // Se devuelve lo que la venta realmente sacó (10 caramelos, 2 gorros),
        // no lo que diría la receta de hoy (20 caramelos y ningún gorro).
        Assert.Equal(100, _escenario.StockDe(Caramelo), precision: 6);
        Assert.Equal(30, _escenario.StockDe(Gorro), precision: 6);
    }

    [Fact]
    public void Dos_combos_en_la_misma_venta_que_comparten_componente_descuentan_los_dos()
    {
        using (var conexion = _escenario.Abrir())
            conexion.Execute(
                """
                INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo, EsCombo)
                VALUES (23, 1, 'Bolsita chica', 80000, 0, 0, 0, 1);
                """);

        _combos.Guardar(23,
        [
            new ComponenteComboDto { ComponenteProductoId = Caramelo, Cantidad = 3 }
        ], EscenarioVentas.UsuarioId);

        _escenario.AbrirCaja();

        // Una grande (5 caramelos) y dos chicas (6) = 11 caramelos.
        _escenario.Vender((Bolsita, 1), (23, 2));

        Assert.Equal(89, _escenario.StockDe(Caramelo), precision: 6);
    }

    [Fact]
    public void Un_combo_sin_receta_no_se_puede_vender()
    {
        using (var conexion = _escenario.Abrir())
            conexion.Execute(
                """
                INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo, EsCombo)
                VALUES (24, 1, 'Combo vacío', 50000, 0, 0, 0, 1);
                """);

        _escenario.AbrirCaja();

        var error = Assert.Throws<VentaInvalidaException>(() => _escenario.Vender((24, 1)));

        Assert.Contains("no tiene cargado qué lleva adentro", error.Message);
    }

    [Fact]
    public void Un_combo_no_puede_contenerse_a_si_mismo()
    {
        var error = Assert.Throws<ComboInvalidoException>(() => _combos.Guardar(Bolsita,
            [new ComponenteComboDto { ComponenteProductoId = Bolsita, Cantidad = 1 }],
            EscenarioVentas.UsuarioId));

        Assert.Contains("sí mismo", error.Message);
    }

    [Fact]
    public void Un_combo_no_puede_llevar_otro_combo_adentro()
    {
        using (var conexion = _escenario.Abrir())
            conexion.Execute(
                """
                INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, CostoCentavos, Stock, StockMinimo, EsCombo)
                VALUES (25, 1, 'Combo grande', 300000, 0, 0, 0, 1);
                """);

        var error = Assert.Throws<ComboInvalidoException>(() => _combos.Guardar(25,
            [new ComponenteComboDto { ComponenteProductoId = Bolsita, Cantidad = 1 }],
            EscenarioVentas.UsuarioId));

        Assert.Contains("Bolsita sorpresa", error.Message);
    }

    [Fact]
    public void El_mismo_componente_repetido_se_suma_en_vez_de_fallar()
    {
        _combos.Guardar(Bolsita,
        [
            new ComponenteComboDto { ComponenteProductoId = Caramelo, Cantidad = 2 },
            new ComponenteComboDto { ComponenteProductoId = Caramelo, Cantidad = 3 }
        ], EscenarioVentas.UsuarioId);

        var componente = Assert.Single(_combos.Componentes(Bolsita));

        Assert.Equal(5m, componente.Cantidad);
    }

    [Fact]
    public void Guardar_la_receta_reemplaza_la_anterior_entera()
    {
        _combos.Guardar(Bolsita,
            [new ComponenteComboDto { ComponenteProductoId = Gorro, Cantidad = 2 }],
            EscenarioVentas.UsuarioId);

        var componente = Assert.Single(_combos.Componentes(Bolsita));

        Assert.Equal(Gorro, componente.ComponenteProductoId);
    }

    [Fact]
    public void Un_combo_sin_componentes_no_se_guarda()
    {
        var error = Assert.Throws<ComboInvalidoException>(
            () => _combos.Guardar(Bolsita, [], EscenarioVentas.UsuarioId));

        Assert.Contains("al menos un producto", error.Message);
    }
}
