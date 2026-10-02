using System.Text;
using Cotillon.LocalApi.Service.Repositories;
using Cotillon.LocalApi.Service.Services;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// Los adjuntos son el único punto donde entra un archivo que eligió el usuario y
/// que después se abre en la terminal, así que lo que se prueba acá es sobre todo
/// qué se rechaza y que nada quede a medio guardar.
/// </summary>
public class AdjuntosTests : IDisposable
{
    private const int UsuarioId = 1;
    private const int ClienteId = 1;

    private readonly BaseDePruebas _db = new();
    private readonly AdjuntosService _service;

    public AdjuntosTests()
    {
        _service = new AdjuntosService(new AdjuntosRepository(_db.Factory), _db.Factory);

        using var conexion = _db.Abrir();

        conexion.Execute(
            """
            INSERT INTO Clientes (ClienteId, NombreCompleto) VALUES (1, 'Fiesta Infantil SRL');
            """);
    }

    // La carpeta de adjuntos queda dentro de la de la base, así que la borra _db.
    public void Dispose() => _db.Dispose();

    // Un PDF mínimo válido: lo que se mira es la firma %PDF de los primeros bytes.
    private static Stream Pdf(int relleno = 64) =>
        new MemoryStream(Encoding.ASCII.GetBytes("%PDF-1.7\n" + new string('x', relleno)));

    private static Stream Png() =>
        new MemoryStream([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0, 1, 2, 3]);

    [Fact]
    public void Guardar_deja_el_archivo_en_disco_y_la_fila_en_la_base()
    {
        using var contenido = Pdf();

        var respuesta = _service.Guardar(
            "CLIENTE", ClienteId, "factura-0001.pdf", contenido, contenido.Length, "Factura A", UsuarioId);

        Assert.True(respuesta.Success, respuesta.Message);
        Assert.NotNull(respuesta.Data);
        Assert.Equal("application/pdf", respuesta.Data.TipoContenido);
        Assert.Equal("factura-0001.pdf", respuesta.Data.NombreArchivo);

        var listado = _service.Listar("CLIENTE", ClienteId);

        Assert.Single(listado.Data!);
        Assert.Single(Directory.GetFiles(_service.Carpeta));
    }

    [Fact]
    public void El_tipo_de_entidad_se_normaliza_y_los_desconocidos_se_rechazan()
    {
        using var contenido = Pdf();

        // En minúsculas tiene que funcionar igual.
        Assert.True(_service.Guardar(
            "cliente", ClienteId, "ok.pdf", contenido, contenido.Length, null, UsuarioId).Success);

        using var otro = Pdf();

        Assert.False(_service.Guardar(
            "USUARIOS", 1, "x.pdf", otro, otro.Length, null, UsuarioId).Success);
    }

    [Fact]
    public void No_se_puede_adjuntar_a_un_registro_que_no_existe()
    {
        using var contenido = Pdf();

        var respuesta = _service.Guardar(
            "CLIENTE", 9999, "factura.pdf", contenido, contenido.Length, null, UsuarioId);

        Assert.False(respuesta.Success);
        Assert.Contains("No existe el registro", respuesta.Message);
        Assert.False(Directory.Exists(_service.Carpeta) && Directory.GetFiles(_service.Carpeta).Length > 0);
    }

    [Fact]
    public void Se_rechazan_las_extensiones_que_no_estan_en_la_lista_blanca()
    {
        using var contenido = Pdf();

        var respuesta = _service.Guardar(
            "CLIENTE", ClienteId, "factura.exe", contenido, contenido.Length, null, UsuarioId);

        Assert.False(respuesta.Success);
        Assert.Contains("PDF o imágenes", respuesta.Message);
    }

    [Fact]
    public void Un_archivo_con_la_extension_cambiada_se_rechaza_y_no_queda_en_disco()
    {
        // Esto es un ejecutable renombrado a .pdf: la extensión pasa la lista blanca
        // pero la firma no coincide. Importa porque la terminal después lo abre.
        using var falso = new MemoryStream(Encoding.ASCII.GetBytes("MZ\x90\0 ejecutable disfrazado"));

        var respuesta = _service.Guardar(
            "CLIENTE", ClienteId, "factura.pdf", falso, falso.Length, null, UsuarioId);

        Assert.False(respuesta.Success);
        Assert.Contains("no corresponde a un PDF", respuesta.Message);
        Assert.Empty(Directory.GetFiles(_service.Carpeta));
        Assert.Empty(_service.Listar("CLIENTE", ClienteId).Data!);
    }

    [Fact]
    public void Se_rechaza_un_archivo_mas_grande_que_el_maximo()
    {
        using var contenido = Pdf();

        // Se declara un tamaño por encima del límite sin tener que generar 10 MB.
        var respuesta = _service.Guardar(
            "CLIENTE", ClienteId, "escaneo.pdf", contenido, 11 * 1024 * 1024, null, UsuarioId);

        Assert.False(respuesta.Success);
        Assert.Contains("supera el máximo", respuesta.Message);
    }

    [Fact]
    public void Se_rechaza_un_archivo_vacio()
    {
        using var vacio = new MemoryStream();

        Assert.False(_service.Guardar(
            "CLIENTE", ClienteId, "nada.pdf", vacio, 0, null, UsuarioId).Success);
    }

    [Fact]
    public void El_nombre_que_manda_el_cliente_no_decide_donde_se_escribe()
    {
        using var contenido = Pdf();

        var respuesta = _service.Guardar(
            "CLIENTE", ClienteId, @"..\..\Windows\System32\payload.pdf",
            contenido, contenido.Length, null, UsuarioId);

        Assert.True(respuesta.Success, respuesta.Message);

        // Del nombre recibido sólo sobrevive el último tramo, y el archivo real se
        // llama como lo bautizó el servidor: queda dentro de la carpeta de adjuntos.
        Assert.Equal("payload.pdf", respuesta.Data!.NombreArchivo);

        var archivos = Directory.GetFiles(_service.Carpeta);

        Assert.Single(archivos);
        Assert.StartsWith("cliente-1-", Path.GetFileName(archivos[0]));
    }

    [Fact]
    public void Obtener_devuelve_la_ruta_real_del_archivo()
    {
        using var contenido = Png();

        var guardado = _service.Guardar(
            "CLIENTE", ClienteId, "frente-local.png", contenido, contenido.Length, null, UsuarioId);

        var descarga = _service.Obtener(guardado.Data!.AdjuntoId);

        Assert.True(descarga.Success, descarga.Message);
        Assert.True(File.Exists(descarga.Data.Ruta));
        Assert.Equal("image/png", descarga.Data.Adjunto.TipoContenido);
    }

    [Fact]
    public void Eliminar_borra_la_fila_y_el_archivo()
    {
        using var contenido = Pdf();

        var guardado = _service.Guardar(
            "CLIENTE", ClienteId, "remito.pdf", contenido, contenido.Length, null, UsuarioId);

        var respuesta = _service.Eliminar(guardado.Data!.AdjuntoId, UsuarioId);

        Assert.True(respuesta.Success, respuesta.Message);
        Assert.Empty(_service.Listar("CLIENTE", ClienteId).Data!);
        Assert.Empty(Directory.GetFiles(_service.Carpeta));
    }

    [Fact]
    public void El_alta_y_la_baja_quedan_registradas_en_la_auditoria()
    {
        using var contenido = Pdf();

        var guardado = _service.Guardar(
            "CLIENTE", ClienteId, "factura-0002.pdf", contenido, contenido.Length, null, UsuarioId);

        _service.Eliminar(guardado.Data!.AdjuntoId, UsuarioId);

        using var conexion = _db.Abrir();

        var acciones = conexion.Query<string>(
            "SELECT Accion FROM Auditoria WHERE TablaAfectada = 'Adjuntos' ORDER BY AuditoriaId;").ToList();

        Assert.Equal(["ALTA", "BAJA"], acciones);
    }

    [Fact]
    public void Los_adjuntos_de_un_registro_no_se_mezclan_con_los_de_otro()
    {
        using var conexion = _db.Abrir();
        conexion.Execute("INSERT INTO Clientes (ClienteId, NombreCompleto) VALUES (2, 'Otro cliente');");

        using var uno = Pdf();
        using var dos = Pdf();

        _service.Guardar("CLIENTE", 1, "del-uno.pdf", uno, uno.Length, null, UsuarioId);
        _service.Guardar("CLIENTE", 2, "del-dos.pdf", dos, dos.Length, null, UsuarioId);

        Assert.Equal("del-uno.pdf", Assert.Single(_service.Listar("CLIENTE", 1).Data!).NombreArchivo);
        Assert.Equal("del-dos.pdf", Assert.Single(_service.Listar("CLIENTE", 2).Data!).NombreArchivo);
    }
}
