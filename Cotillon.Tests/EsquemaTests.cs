using Cotillon.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cotillon.Tests;

public class EsquemaTests
{
    [Fact]
    public void AplicarEsquema_crea_todas_las_tablas()
    {
        using var db = new BaseDePruebas();
        using var conexion = db.Abrir();

        var tablas = conexion.Query<string>(
            "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%';").ToHashSet();

        string[] esperadas =
        [
            "Usuarios", "Secciones", "Categorias", "Productos", "Clientes", "Proveedores",
            "MediosPago", "SesionesCaja", "MovimientosCaja", "Ventas", "DetalleVentas",
            "PagosVenta", "Compras", "DetalleCompras", "StockMovimientos", "Auditoria", "Configuracion",
            "Adjuntos", "PermisosUsuario"
        ];

        Assert.All(esperadas, tabla => Assert.Contains(tabla, tablas));
    }

    [Fact]
    public void Inicializar_dos_veces_no_falla_ni_duplica_datos()
    {
        using var db = new BaseDePruebas();

        new DatabaseInitializer(db.Factory).Inicializar();
        new DatabaseInitializer(db.Factory).Inicializar();

        using var conexion = db.Abrir();

        Assert.Equal(1, conexion.ExecuteScalar<long>("SELECT COUNT(*) FROM Usuarios;"));
        Assert.Equal(6, conexion.ExecuteScalar<long>("SELECT COUNT(*) FROM MediosPago;"));
        Assert.Equal(10, conexion.ExecuteScalar<long>("SELECT COUNT(*) FROM Configuracion;"));
    }

    [Fact]
    public void Sembrar_crea_el_usuario_admin_con_pin_hasheado()
    {
        using var db = new BaseDePruebas();
        using var conexion = db.Abrir();

        var usuario = conexion.QuerySingle(
            "SELECT PinHash, PinSalt FROM Usuarios WHERE NombreUsuario = 'admin';");

        Assert.True(PinHasher.Verificar("1234", (string)usuario.PinHash, (string)usuario.PinSalt));
    }

    [Fact]
    public void Las_claves_foraneas_estan_activas()
    {
        using var db = new BaseDePruebas();
        using var conexion = db.Abrir();

        var excepcion = Assert.Throws<SqliteException>(() => conexion.Execute(
            "INSERT INTO Categorias (SeccionId, NombreCategoria, Activo) VALUES (9999, 'Fantasma', 1);"));

        Assert.Contains("FOREIGN KEY", excepcion.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void No_se_pueden_abrir_dos_sesiones_de_caja_a_la_vez()
    {
        using var db = new BaseDePruebas();
        using var conexion = db.Abrir();

        const string abrirCaja =
            """
            INSERT INTO SesionesCaja (UsuarioAperturaId, MontoInicialCentavos, Estado)
            VALUES (1, 0, 'ABIERTA');
            """;

        conexion.Execute(abrirCaja);

        Assert.Throws<SqliteException>(() => conexion.Execute(abrirCaja));
    }

    [Fact]
    public void Se_puede_abrir_una_caja_nueva_despues_de_cerrar_la_anterior()
    {
        using var db = new BaseDePruebas();
        using var conexion = db.Abrir();

        conexion.Execute(
            """
            INSERT INTO SesionesCaja (UsuarioAperturaId, MontoInicialCentavos, Estado)
            VALUES (1, 0, 'ABIERTA');
            """);

        conexion.Execute("UPDATE SesionesCaja SET Estado = 'CERRADA' WHERE SesionCajaId = 1;");

        conexion.Execute(
            """
            INSERT INTO SesionesCaja (UsuarioAperturaId, MontoInicialCentavos, Estado)
            VALUES (1, 0, 'ABIERTA');
            """);

        Assert.Equal(1, conexion.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM SesionesCaja WHERE Estado = 'ABIERTA';"));
    }

    [Fact]
    public void El_rol_de_usuario_esta_restringido_a_los_valores_conocidos()
    {
        using var db = new BaseDePruebas();
        using var conexion = db.Abrir();

        Assert.Throws<SqliteException>(() => conexion.Execute(
            """
            INSERT INTO Usuarios (NombreUsuario, NombreCompleto, PinHash, PinSalt, Rol)
            VALUES ('intruso', 'Intruso', 'x', 'y', 'SuperAdmin');
            """));
    }

    [Fact]
    public void Borrar_una_venta_arrastra_su_detalle_pero_no_los_productos()
    {
        using var db = new BaseDePruebas();
        using var conexion = db.Abrir();

        conexion.Execute(
            """
            INSERT INTO Secciones (SeccionId, NombreSeccion, Activo) VALUES (1, 'General', 1);
            INSERT INTO Categorias (CategoriaId, SeccionId, NombreCategoria, Activo) VALUES (1, 1, 'Varios', 1);
            INSERT INTO Productos (ProductoId, CategoriaId, NombreProducto, PrecioCentavos, Stock)
                VALUES (1, 1, 'Globo', 10000, 50);
            INSERT INTO SesionesCaja (SesionCajaId, UsuarioAperturaId, MontoInicialCentavos, Estado)
                VALUES (1, 1, 0, 'ABIERTA');
            INSERT INTO Ventas (VentaId, UsuarioId, SesionCajaId, SubtotalCentavos, TotalVentaCentavos)
                VALUES (1, 1, 1, 10000, 10000);
            INSERT INTO DetalleVentas (VentaId, ProductoId, NombreProducto, Cantidad, PrecioUnitarioCentavos, SubtotalLineaCentavos)
                VALUES (1, 1, 'Globo', 1, 10000, 10000);
            """);

        conexion.Execute("DELETE FROM Ventas WHERE VentaId = 1;");

        Assert.Equal(0, conexion.ExecuteScalar<long>("SELECT COUNT(*) FROM DetalleVentas;"));
        Assert.Equal(1, conexion.ExecuteScalar<long>("SELECT COUNT(*) FROM Productos;"));
    }
}
