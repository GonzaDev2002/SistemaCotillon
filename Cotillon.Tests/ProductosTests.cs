using Cotillon.LocalApi.Service.Repositories;
using Cotillon.LocalApi.Service.Services;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// Búsqueda por código: es lo que dispara el lector de código de barras en el alta
/// de productos, y de ahí sale el formulario completo.
/// </summary>
public class ProductosTests : IDisposable
{
    private readonly BaseDePruebas _db = new();
    private readonly ProductosService _service;

    public ProductosTests()
    {
        _service = new ProductosService(new ProductosRepository(_db.Factory));

        using var conexion = _db.Abrir();

        conexion.Execute(
            """
            INSERT INTO Secciones (SeccionId, NombreSeccion, Activo) VALUES (1, 'Globos', 1);
            INSERT INTO Categorias (CategoriaId, SeccionId, NombreCategoria, Activo) VALUES (1, 1, 'Látex', 1);

            INSERT INTO Productos
                (ProductoId, CategoriaId, NombreProducto, CodigoBarras, CodigoProducto,
                 PrecioCentavos, CostoCentavos, Stock, StockMinimo, Marca, Activo)
            VALUES
                (1, 1, 'Globo rojo nº 9', '7791234567890', 'GLB-ROJ-9', 45050, 20000, 120, 20, 'Sempertex', 1);

            INSERT INTO Productos
                (ProductoId, CategoriaId, NombreProducto, CodigoBarras, PrecioCentavos, Stock, Activo)
            VALUES
                (2, 1, 'Globo descontinuado', '7790000000001', 10000, 0, 0);
            """);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void Escanear_un_codigo_de_barras_trae_el_producto_con_todos_sus_campos()
    {
        var respuesta = _service.ObtenerPorCodigo("7791234567890");

        Assert.True(respuesta.Success, respuesta.Message);
        Assert.NotNull(respuesta.Data);

        var producto = respuesta.Data;

        // Todo lo que la pantalla necesita para llenar el formulario de una pasada.
        Assert.Equal(1, producto.ProductoId);
        Assert.Equal(1, producto.CategoriaId);
        Assert.Equal("Globo rojo nº 9", producto.NombreProducto);
        Assert.Equal("GLB-ROJ-9", producto.CodigoProducto);
        Assert.Equal(450.50m, producto.Precio);
        Assert.Equal(200.00m, producto.Costo);
        Assert.Equal(120, producto.Stock);
        Assert.Equal(20, producto.StockMinimo);
        Assert.Equal("Sempertex", producto.Marca);
        Assert.True(producto.Activo);
    }

    [Fact]
    public void Tambien_encuentra_por_el_codigo_interno_del_negocio()
    {
        // Los artículos sueltos de cotillón no traen código de barras y se les pone
        // uno propio; el lector lee ese igual si se lo imprime en una etiqueta.
        var respuesta = _service.ObtenerPorCodigo("GLB-ROJ-9");

        Assert.Equal(1, respuesta.Data!.ProductoId);
    }

    [Fact]
    public void Un_codigo_que_no_existe_no_es_un_error_sino_un_alta_nueva()
    {
        var respuesta = _service.ObtenerPorCodigo("0000000000000");

        // Exitosa y sin datos: así la pantalla sabe que tiene que ofrecer el alta
        // en vez de mostrar un cartel de error.
        Assert.True(respuesta.Success);
        Assert.Null(respuesta.Data);
    }

    [Fact]
    public void Encuentra_tambien_los_productos_dados_de_baja()
    {
        // Si no los trajera, escanear una etiqueta vieja intentaría crear un
        // duplicado que el índice único del código de barras rechazaría igual,
        // sin que el usuario entienda por qué.
        var respuesta = _service.ObtenerPorCodigo("7790000000001");

        Assert.NotNull(respuesta.Data);
        Assert.False(respuesta.Data.Activo);
    }

    [Fact]
    public void Un_codigo_vacio_se_rechaza()
    {
        Assert.False(_service.ObtenerPorCodigo("   ").Success);
        Assert.False(_service.ObtenerPorCodigo(null).Success);
    }

    [Fact]
    public void Reactivar_vuelve_a_poner_el_producto_en_venta()
    {
        Assert.True(_service.Reactivar(2).Success);

        Assert.True(_service.ObtenerPorCodigo("7790000000001").Data!.Activo);
        Assert.Contains(_service.ObtenerTodos(false).Data!, p => p.ProductoId == 2);
    }

    [Fact]
    public void Dar_de_baja_lo_saca_del_listado_de_ventas_pero_no_lo_borra()
    {
        Assert.True(_service.Eliminar(1).Success);

        Assert.DoesNotContain(_service.ObtenerTodos(false).Data!, p => p.ProductoId == 1);
        Assert.Contains(_service.ObtenerTodos(true).Data!, p => p.ProductoId == 1);
    }
}
