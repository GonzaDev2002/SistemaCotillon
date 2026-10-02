using Cotillon.LocalApi.Service.Repositories;
using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// Secciones y categorías: los contadores que muestra la pantalla y el borrado,
/// que sólo puede pasar cuando no deja productos ni categorías huérfanas.
/// </summary>
public class CatalogoTests : IDisposable
{
    private readonly BaseDePruebas _db = new();
    private readonly SeccionesService _secciones;
    private readonly CategoriasService _categorias;

    public CatalogoTests()
    {
        _secciones = new SeccionesService(new SeccionesRepository(_db.Factory));
        _categorias = new CategoriasService(new CategoriasRepository(_db.Factory));

        using var conexion = _db.Abrir();

        conexion.Execute(
            """
            INSERT INTO Secciones (SeccionId, NombreSeccion, Activo) VALUES (1, 'Globos', 1);
            INSERT INTO Secciones (SeccionId, NombreSeccion, Activo) VALUES (2, 'Piñatas', 1);
            INSERT INTO Secciones (SeccionId, NombreSeccion, Activo) VALUES (3, 'Vacía', 1);

            INSERT INTO Categorias (CategoriaId, SeccionId, NombreCategoria, Activo) VALUES (1, 1, 'Látex', 1);
            INSERT INTO Categorias (CategoriaId, SeccionId, NombreCategoria, Activo) VALUES (2, 1, 'Metalizados', 1);
            INSERT INTO Categorias (CategoriaId, SeccionId, NombreCategoria, Activo) VALUES (3, 2, 'Temáticas', 1);

            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, Stock, Activo)
                VALUES (1, 1, 'Globo rojo', 10000, 10, 1);
            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, Stock, Activo)
                VALUES (2, 1, 'Globo azul', 10000, 10, 1);
            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, Stock, Activo)
                VALUES (3, 1, 'Globo viejo', 10000, 0, 0);
            """);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public void Las_secciones_informan_cuantas_categorias_y_productos_tienen()
    {
        var lista = _secciones.ObtenerTodas().Data!;

        var globos = lista.Single(s => s.NombreSeccion == "Globos");

        Assert.Equal(2, globos.CantidadCategorias);
        // Dos activos: el dado de baja no se cuenta, porque el número está para
        // saber qué se está vendiendo hoy.
        Assert.Equal(2, globos.CantidadProductos);

        var vacia = lista.Single(s => s.NombreSeccion == "Vacía");

        Assert.Equal(0, vacia.CantidadCategorias);
        Assert.Equal(0, vacia.CantidadProductos);
    }

    [Fact]
    public void Las_categorias_traen_el_nombre_de_su_seccion()
    {
        var latex = _categorias.ObtenerTodas().Data!.Single(c => c.NombreCategoria == "Látex");

        Assert.Equal("Globos", latex.NombreSeccion);
        Assert.Equal(2, latex.CantidadProductos);
    }

    [Fact]
    public void No_se_puede_borrar_una_seccion_con_categorias()
    {
        var respuesta = _secciones.Eliminar(1);

        Assert.False(respuesta.Success);
        Assert.Contains("2 categoría(s)", respuesta.Message);
        Assert.Equal(3, _secciones.ObtenerTodas().Data!.Count);
    }

    [Fact]
    public void Una_seccion_vacia_si_se_puede_borrar()
    {
        Assert.True(_secciones.Eliminar(3).Success);
        Assert.DoesNotContain(_secciones.ObtenerTodas().Data!, s => s.NombreSeccion == "Vacía");
    }

    [Fact]
    public void No_se_puede_borrar_una_categoria_con_productos()
    {
        var respuesta = _categorias.Eliminar(1);

        Assert.False(respuesta.Success);
        // Cuenta los tres, incluido el dado de baja: ese producto sigue apareciendo
        // en el historial de ventas y necesita su categoría para mostrarse.
        Assert.Contains("3 producto(s)", respuesta.Message);
    }

    [Fact]
    public void Una_categoria_sin_productos_si_se_puede_borrar()
    {
        Assert.True(_categorias.Eliminar(2).Success);
        Assert.Single(_categorias.ObtenerPorSeccion(1).Data!);
    }

    [Fact]
    public void Mover_una_categoria_de_seccion_se_lleva_sus_productos()
    {
        var respuesta = _categorias.Actualizar(new CategoriaDto
        {
            CategoriaId = 1,
            SeccionId = 2,
            NombreCategoria = "Látex",
            Activo = true
        });

        Assert.True(respuesta.Success, respuesta.Message);

        var secciones = _secciones.ObtenerTodas().Data!;

        Assert.Equal(0, secciones.Single(s => s.NombreSeccion == "Globos").CantidadProductos);
        Assert.Equal(2, secciones.Single(s => s.NombreSeccion == "Piñatas").CantidadProductos);
    }

    [Fact]
    public void No_se_repiten_nombres_de_categoria_dentro_de_la_misma_seccion()
    {
        var respuesta = _categorias.Crear(new CategoriaDto
        {
            SeccionId = 1,
            NombreCategoria = "látex",
            Activo = true
        });

        Assert.False(respuesta.Success);
        Assert.Contains("Ya existe", respuesta.Message);
    }

    [Fact]
    public void El_mismo_nombre_si_puede_repetirse_en_otra_seccion()
    {
        // "Números" tiene sentido en Globos y en Velas a la vez.
        Assert.True(_categorias.Crear(new CategoriaDto
        {
            SeccionId = 2,
            NombreCategoria = "Látex",
            Activo = true
        }).Success);
    }
}
