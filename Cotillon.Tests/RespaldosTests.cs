using System.IO.Compression;
using Cotillon.Data;
using Cotillon.Shared.DTOs.Respaldos;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Cotillon.Tests;

/// <summary>
/// Los respaldos. Lo que se prueba acá no es que el archivo exista: es que la
/// copia SIRVA. Un respaldo que nunca se abrió no es un respaldo, y el día que
/// se necesita ya es tarde para descubrirlo.
/// </summary>
public class RespaldosTests : IDisposable
{
    private readonly EscenarioVentas _escenario = new();
    private readonly Respaldos _respaldos;

    public RespaldosTests()
    {
        _respaldos = new Respaldos(_escenario.Factory);
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        _escenario.Dispose();
    }

    private static ConfiguracionRespaldoDto Config(string? nube = null, int copias = 30) => new()
    {
        Activo = true,
        CadaDias = 1,
        Copias = copias,
        CarpetaNube = nube ?? string.Empty
    };

    /// <summary>Extrae la base del zip a una carpeta temporal y devuelve su ruta.</summary>
    private static string ExtraerBase(string zip)
    {
        var carpeta = Path.Combine(Path.GetTempPath(), $"restaurar-{Guid.NewGuid():N}");
        Directory.CreateDirectory(carpeta);

        ZipFile.ExtractToDirectory(zip, carpeta);

        return Path.Combine(carpeta, "cotillon.db");
    }

    [Fact]
    public void La_copia_se_puede_abrir_y_tiene_los_datos()
    {
        _escenario.AbrirCaja();
        var venta = _escenario.Vender((EscenarioVentas.ProductoGlobo, 3));

        var resultado = _respaldos.Crear(Config());

        Assert.True(resultado.Exito, resultado.Mensaje);

        // Lo que importa: la copia abre y la venta está adentro.
        var restaurada = ExtraerBase(resultado.Archivo);

        using var conexion = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = restaurada, Mode = SqliteOpenMode.ReadOnly }
                .ToString());

        conexion.Open();

        Assert.Equal(1, conexion.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM Ventas WHERE VentaId = @Id;", new { Id = venta.VentaId }));

        Assert.Equal(venta.TotalVenta, Money.FromCentavos(conexion.ExecuteScalar<long>(
            "SELECT TotalVentaCentavos FROM Ventas WHERE VentaId = @Id;", new { Id = venta.VentaId })));
    }

    [Fact]
    public void La_copia_pasa_el_control_de_integridad_de_SQLite()
    {
        var resultado = _respaldos.Crear(Config());

        var restaurada = ExtraerBase(resultado.Archivo);

        using var conexion = new SqliteConnection(
            new SqliteConnectionStringBuilder { DataSource = restaurada, Mode = SqliteOpenMode.ReadOnly }
                .ToString());

        conexion.Open();

        Assert.Equal("ok", conexion.ExecuteScalar<string>("PRAGMA integrity_check;"));
    }

    [Fact]
    public void Se_respalda_con_la_base_en_uso_y_sin_frenarla()
    {
        _escenario.AbrirCaja();

        // Una conexión abierta y escribiendo mientras se saca la copia: es el caso
        // real, porque el respaldo corre con el local trabajando.
        using var abierta = _escenario.Abrir();

        abierta.Execute("UPDATE Productos SET Stock = 42 WHERE ProductoId = 1;");

        var resultado = _respaldos.Crear(Config());

        Assert.True(resultado.Exito, resultado.Mensaje);

        // Y la base original sigue andando después del respaldo.
        _escenario.Vender((EscenarioVentas.ProductoGlobo, 1));
    }

    [Fact]
    public void El_zip_lleva_tambien_los_adjuntos()
    {
        var carpetaAdjuntos = Path.Combine(
            Path.GetDirectoryName(_escenario.Factory.RutaBase)!, "Adjuntos");

        Directory.CreateDirectory(carpetaAdjuntos);
        File.WriteAllText(Path.Combine(carpetaAdjuntos, "factura-1.pdf"), "%PDF-falso");

        var resultado = _respaldos.Crear(Config());

        using var zip = ZipFile.OpenRead(resultado.Archivo);

        // La base sin los comprobantes dejaría la mitad del archivo del negocio
        // afuera del respaldo.
        Assert.Contains(zip.Entries, e => e.FullName == "cotillon.db");
        Assert.Contains(zip.Entries, e => e.FullName == "Adjuntos/factura-1.pdf");
    }

    [Fact]
    public void Sin_carpeta_de_adjuntos_el_respaldo_igual_se_hace()
    {
        // Una instalación nueva no tiene todavía ningún adjunto.
        var resultado = _respaldos.Crear(Config());

        Assert.True(resultado.Exito, resultado.Mensaje);
    }

    [Fact]
    public void La_copia_a_la_nube_deja_el_zip_en_la_carpeta_sincronizada()
    {
        var nube = Path.Combine(Path.GetTempPath(), $"nube-{Guid.NewGuid():N}");

        try
        {
            var resultado = _respaldos.Crear(Config(nube));

            Assert.True(resultado.CopiadoALaNube, resultado.Mensaje);

            // Lo que se copia es un zip terminado, nunca la base en uso: la base
            // dentro de una carpeta sincronizada es lo que corrompe SQLite.
            var copiado = Assert.Single(Directory.GetFiles(nube, "*.zip"));

            Assert.Equal(Path.GetFileName(resultado.Archivo), Path.GetFileName(copiado));
        }
        finally
        {
            if (Directory.Exists(nube))
                Directory.Delete(nube, recursive: true);
        }
    }

    [Fact]
    public void Si_la_nube_falla_el_respaldo_local_igual_queda()
    {
        // Una ruta inválida simula el pendrive desenchufado o la carpeta borrada.
        var resultado = _respaldos.Crear(Config("Z:\\carpeta\\que\\no\\existe\\|<>"));

        Assert.True(resultado.Exito, resultado.Mensaje);
        Assert.False(resultado.CopiadoALaNube);
        Assert.Contains("no se pudo copiar a la nube", resultado.Mensaje);
        Assert.True(File.Exists(resultado.Archivo));
    }

    [Fact]
    public void Se_conservan_solo_las_ultimas_copias()
    {
        for (var i = 0; i < 4; i++)
            _respaldos.Crear(Config(copias: 2));

        Assert.Equal(2, _respaldos.Listar().Count);
    }

    [Fact]
    public void Dos_respaldos_en_el_mismo_segundo_no_se_pisan()
    {
        // El nombre lleva la hora al segundo. La primera versión de esto, al
        // encontrar el nombre ocupado, fallaba y en el catch borraba el archivo
        // —que era el respaldo BUENO ya existente—. Quedaba cero copias.
        var primero = _respaldos.Crear(Config());
        var segundo = _respaldos.Crear(Config());

        Assert.True(primero.Exito, primero.Mensaje);
        Assert.True(segundo.Exito, segundo.Mensaje);

        Assert.NotEqual(primero.Archivo, segundo.Archivo);
        Assert.True(File.Exists(primero.Archivo), "El primer respaldo se borró.");
        Assert.True(File.Exists(segundo.Archivo));
        Assert.Equal(2, _respaldos.Listar().Count);
    }

    [Fact]
    public void Un_respaldo_fallido_no_deja_un_zip_a_medias()
    {
        _respaldos.Crear(Config());

        var antes = _respaldos.Listar().Count;

        // La base borrada hace fallar el VACUUM.
        SqliteConnection.ClearAllPools();
        File.Delete(_escenario.Factory.RutaBase);

        var resultado = _respaldos.Crear(Config());

        Assert.False(resultado.Exito);

        // Ni un zip incompleto nuevo, ni uno menos de los que había.
        Assert.Equal(antes, _respaldos.Listar().Count);
        Assert.Empty(Directory.GetFiles(_respaldos.Carpeta, "parcial-*.zip"));
    }

    [Fact]
    public void No_quedan_archivos_temporales_en_la_carpeta()
    {
        _respaldos.Crear(Config());
        _respaldos.Crear(Config());

        // La copia intermedia de la base pesa lo mismo que la base. Si no se
        // borra, cada respaldo deja una huérfana y la carpeta se llena de basura
        // del tamaño del negocio. Pasaba porque el pool de SQLite mantenía el
        // archivo abierto después de verificarlo.
        Assert.Empty(Directory.GetFiles(_respaldos.Carpeta, "base-*.db"));
        Assert.Empty(Directory.GetFiles(_respaldos.Carpeta, "parcial-*.zip"));

        // Sólo los zips de respaldo, nada más.
        Assert.All(Directory.GetFiles(_respaldos.Carpeta),
            archivo => Assert.StartsWith(Respaldos.Prefijo, Path.GetFileName(archivo)));
    }

    [Fact]
    public void Las_copias_se_listan_de_la_mas_nueva_a_la_mas_vieja()
    {
        _respaldos.Crear(Config());
        Thread.Sleep(1100);
        var ultima = _respaldos.Crear(Config());

        var copias = _respaldos.Listar();

        Assert.Equal(Path.GetFileName(ultima.Archivo), copias[0].Archivo);
    }

    [Fact]
    public void Corresponde_respaldar_cuando_no_hay_ninguna_copia()
    {
        Assert.True(_respaldos.Corresponde(Config()));
    }

    [Fact]
    public void No_corresponde_respaldar_dos_veces_el_mismo_dia()
    {
        _respaldos.Crear(Config());

        Assert.False(_respaldos.Corresponde(Config()));
    }

    [Fact]
    public void Apagado_no_corresponde_nunca()
    {
        var configuracion = Config();
        configuracion.Activo = false;

        Assert.False(_respaldos.Corresponde(configuracion));
    }

    [Fact]
    public void El_estado_avisa_cuando_nunca_se_respaldo()
    {
        var estado = _respaldos.Estado(Config());

        Assert.True(estado.HayQueAvisar);
        Assert.Null(estado.DiasSinRespaldo);
        Assert.Contains("Nunca", estado.Aviso);
    }

    [Fact]
    public void El_estado_no_avisa_con_un_respaldo_de_hoy()
    {
        _respaldos.Crear(Config());

        var estado = _respaldos.Estado(Config());

        Assert.False(estado.HayQueAvisar);
        Assert.Equal(0, estado.DiasSinRespaldo);
    }

    [Fact]
    public void El_estado_marca_cuales_copias_estan_en_la_nube()
    {
        var nube = Path.Combine(Path.GetTempPath(), $"nube-{Guid.NewGuid():N}");

        try
        {
            _respaldos.Crear(Config(nube));

            var copia = Assert.Single(_respaldos.Estado(Config(nube)).Copias);

            Assert.True(copia.EnLaNube);
        }
        finally
        {
            if (Directory.Exists(nube))
                Directory.Delete(nube, recursive: true);
        }
    }

    [Fact]
    public void Apagar_los_respaldos_tambien_cambia_el_aviso()
    {
        var configuracion = Config();
        configuracion.Activo = false;

        var estado = _respaldos.Estado(configuracion);

        Assert.False(estado.HayQueAvisar);
        Assert.Contains("apagados", estado.Aviso);
    }
}
