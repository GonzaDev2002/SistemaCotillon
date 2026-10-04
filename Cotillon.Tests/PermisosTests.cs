using Cotillon.Data;
using Cotillon.LocalApi.Service.Repositories;
using Cotillon.LocalApi.Service.Services;
using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Permisos;
using Cotillon.Shared.DTOs.Usuarios;
using Dapper;

namespace Cotillon.Tests;

/// <summary>
/// Permisos por usuario y por módulo: qué recibe uno nuevo, qué se puede cambiar
/// y qué no se deja guardar.
/// </summary>
public class PermisosTests : IDisposable
{
    private const int Admin = 1;

    private readonly BaseDePruebas _db = new();
    private readonly PermisosRepository _repository;
    private readonly PermisosService _service;
    private readonly UsuariosRepository _usuarios;
    private readonly AuthService _auth;

    public PermisosTests()
    {
        _repository = new PermisosRepository(_db.Factory);
        _usuarios = new UsuariosRepository(_db.Factory);
        _service = new PermisosService(_repository, _usuarios);
        _auth = new AuthService(new AuthRepository(_db.Factory, _repository));
    }

    public void Dispose() => _db.Dispose();

    private int CrearCajero(string usuario = "vendedora", string pin = "4321")
    {
        var id = _usuarios.Crear(new CrearUsuarioDto
        {
            NombreUsuario = usuario,
            NombreCompleto = "Vendedora del local",
            Pin = pin,
            Rol = "Cajero"
        }, Admin);

        _service.SembrarSegunRol(id, "Cajero", Admin);

        return id;
    }

    [Fact]
    public void La_migracion_le_da_al_admin_existente_acceso_total()
    {
        var permisos = _repository.ObtenerDe(Admin);

        Assert.All(Modulos.Codigos, codigo => Assert.Equal(NivelAcceso.Total, permisos[codigo]));
    }

    [Fact]
    public void Un_modulo_nuevo_le_llega_a_quien_ya_tenia_permisos_cargados()
    {
        // El caso de una actualización: el usuario ya tiene filas de los módulos
        // viejos. Si el sembrado mirara "¿este usuario tiene algún permiso?" en vez
        // de "¿tiene éste?", nadie recibiría el módulo nuevo y el dueño se quedaría
        // sin ver la pantalla recién instalada.
        var cajero = CrearCajero();

        using (var conexion = _db.Abrir())
            conexion.Execute(
                "DELETE FROM PermisosUsuario WHERE Modulo IN ('ENCARGUES', 'CUENTA_CORRIENTE');");

        new DatabaseInitializer(_db.Factory).Sembrar();

        var admin = _repository.ObtenerDe(Admin);
        var vendedora = _repository.ObtenerDe(cajero);

        Assert.Equal(NivelAcceso.Total, admin[Modulos.Encargues]);
        Assert.Equal(NivelAcceso.Total, admin[Modulos.CuentaCorriente]);
        Assert.Equal(NivelAcceso.Editar, vendedora[Modulos.Encargues]);
    }

    [Fact]
    public void Sembrar_de_nuevo_no_pisa_lo_que_el_dueno_configuro()
    {
        var cajero = CrearCajero();

        _service.Guardar(cajero,
            new Dictionary<string, NivelAcceso> { [Modulos.Reportes] = NivelAcceso.Total }, Admin);

        new DatabaseInitializer(_db.Factory).Sembrar();

        // Reportes se lo dio el dueño a mano: el sembrado no puede volver a
        // ponerlo en NINGUNO porque la plantilla del rol diga eso.
        Assert.Equal(NivelAcceso.Total, _repository.ObtenerDe(cajero)[Modulos.Reportes]);
    }

    [Fact]
    public void Un_cajero_nuevo_no_ve_los_modulos_de_administracion()
    {
        var permisos = _repository.ObtenerDe(CrearCajero());

        // Lo que el dueño no quiere que vea un empleado de mostrador.
        Assert.Equal(NivelAcceso.Ninguno, permisos[Modulos.Usuarios]);
        Assert.Equal(NivelAcceso.Ninguno, permisos[Modulos.Configuracion]);
        Assert.Equal(NivelAcceso.Ninguno, permisos[Modulos.Auditoria]);
        Assert.Equal(NivelAcceso.Ninguno, permisos[Modulos.Compras]);
        Assert.Equal(NivelAcceso.Ninguno, permisos[Modulos.Proveedores]);
        Assert.Equal(NivelAcceso.Ninguno, permisos[Modulos.Reportes]);

        // Y lo que sí necesita para trabajar.
        Assert.Equal(NivelAcceso.Editar, permisos[Modulos.Ventas]);
        Assert.Equal(NivelAcceso.Editar, permisos[Modulos.Caja]);
        Assert.Equal(NivelAcceso.Ver, permisos[Modulos.Productos]);
    }

    [Fact]
    public void El_cajero_vende_pero_no_cierra_la_caja_ni_anula()
    {
        var permisos = _repository.ObtenerDe(CrearCajero());

        // Editar alcanza para abrir la caja y mover plata, Total no: cerrar el
        // turno y anular ventas quedan para quien supervisa.
        Assert.True(permisos[Modulos.Caja] >= NivelAcceso.Editar);
        Assert.True(permisos[Modulos.Caja] < NivelAcceso.Total);
        Assert.True(permisos[Modulos.HistorialVentas] < NivelAcceso.Total);
    }

    [Fact]
    public void El_supervisor_maneja_el_deposito_pero_no_toca_usuarios()
    {
        var permisos = PlantillasDeRol.Para("Supervisor");

        Assert.Equal(NivelAcceso.Total, permisos[Modulos.Compras]);
        Assert.Equal(NivelAcceso.Total, permisos[Modulos.Stock]);
        Assert.Equal(NivelAcceso.Ver, permisos[Modulos.Auditoria]);
        Assert.Equal(NivelAcceso.Ninguno, permisos[Modulos.Usuarios]);
        Assert.Equal(NivelAcceso.Ninguno, permisos[Modulos.Configuracion]);
    }

    [Fact]
    public void El_administrador_puede_abrirle_un_modulo_a_un_empleado()
    {
        var cajero = CrearCajero();

        var permisos = _repository.ObtenerDe(cajero);
        permisos[Modulos.Compras] = NivelAcceso.Editar;

        Assert.True(_service.Guardar(cajero, permisos, Admin).Success);

        Assert.Equal(NivelAcceso.Editar, _repository.ObtenerDe(cajero)[Modulos.Compras]);
    }

    [Fact]
    public void Guardar_reemplaza_todo_y_no_deja_mezclado_con_lo_anterior()
    {
        var cajero = CrearCajero();

        var nuevos = Modulos.Codigos.ToDictionary(c => c, _ => NivelAcceso.Ninguno);
        nuevos[Modulos.Ventas] = NivelAcceso.Editar;

        Assert.True(_service.Guardar(cajero, nuevos, Admin).Success);

        var guardados = _repository.ObtenerDe(cajero);

        Assert.Equal(NivelAcceso.Editar, guardados[Modulos.Ventas]);
        // Antes tenía Caja en Editar y Productos en Ver: no deben sobrevivir.
        Assert.Equal(NivelAcceso.Ninguno, guardados[Modulos.Caja]);
        Assert.Equal(NivelAcceso.Ninguno, guardados[Modulos.Productos]);
    }

    [Fact]
    public void No_se_puede_dejar_a_un_usuario_sin_ningun_modulo()
    {
        var cajero = CrearCajero();

        var vacios = Modulos.Codigos.ToDictionary(c => c, _ => NivelAcceso.Ninguno);

        var respuesta = _service.Guardar(cajero, vacios, Admin);

        Assert.False(respuesta.Success);
        Assert.Contains("sin ningún módulo visible", respuesta.Message);

        // Y lo que ya tenía quedó intacto.
        Assert.Equal(NivelAcceso.Editar, _repository.ObtenerDe(cajero)[Modulos.Ventas]);
    }

    [Fact]
    public void Un_modulo_inventado_se_rechaza()
    {
        var cajero = CrearCajero();

        var permisos = _repository.ObtenerDe(cajero);
        permisos["TESORERIA"] = NivelAcceso.Total;

        var respuesta = _service.Guardar(cajero, permisos, Admin);

        Assert.False(respuesta.Success);
        Assert.Contains("TESORERIA", respuesta.Message);
    }

    [Fact]
    public void Un_modulo_que_no_tiene_fila_queda_sin_acceso()
    {
        var cajero = CrearCajero();

        using var conexion = _db.Abrir();
        conexion.Execute(
            "DELETE FROM PermisosUsuario WHERE UsuarioId = @Id AND Modulo = 'VENTAS';",
            new { Id = cajero });

        // Importa al actualizar el sistema: un módulo nuevo no se gana solo.
        Assert.Equal(NivelAcceso.Ninguno, _repository.ObtenerDe(cajero)[Modulos.Ventas]);
    }

    [Fact]
    public void Los_cambios_de_permisos_quedan_en_la_auditoria_con_el_antes_y_el_despues()
    {
        var cajero = CrearCajero();

        var permisos = _repository.ObtenerDe(cajero);
        permisos[Modulos.Reportes] = NivelAcceso.Ver;

        _service.Guardar(cajero, permisos, Admin);

        using var conexion = _db.Abrir();

        var detalle = conexion.QueryFirst<string>(
            """
            SELECT Detalle FROM Auditoria
            WHERE TablaAfectada = 'PermisosUsuario' AND RegistroId = @Id
            ORDER BY AuditoriaId DESC LIMIT 1;
            """,
            new { Id = cajero });

        Assert.Contains("Reportes: Ninguno → Ver", detalle);
    }

    [Fact]
    public void Guardar_sin_cambios_no_ensucia_la_auditoria()
    {
        var cajero = CrearCajero();

        _service.Guardar(cajero, _repository.ObtenerDe(cajero), Admin);

        using var conexion = _db.Abrir();

        Assert.Equal(0, conexion.ExecuteScalar<long>(
            """
            SELECT COUNT(*) FROM Auditoria
            WHERE TablaAfectada = 'PermisosUsuario' AND RegistroId = @Id;
            """,
            new { Id = cajero }));
    }

    [Fact]
    public void El_login_devuelve_los_permisos_para_que_el_menu_se_arme_de_entrada()
    {
        CrearCajero("vendedora", "4321");

        var respuesta = _auth.Login(new LoginRequestDto { Username = "vendedora", Pin = "4321" });

        Assert.True(respuesta.Success, respuesta.Message);
        Assert.Equal(Modulos.Codigos.Length, respuesta.Data!.Permisos.Count);
        Assert.Equal(NivelAcceso.Editar, respuesta.Data.Permisos[Modulos.Ventas]);
        Assert.Equal(NivelAcceso.Ninguno, respuesta.Data.Permisos[Modulos.Usuarios]);
    }

    [Fact]
    public void Los_permisos_se_borran_junto_con_el_usuario()
    {
        var cajero = CrearCajero();

        using var conexion = _db.Abrir();
        conexion.Execute("DELETE FROM Usuarios WHERE UsuarioId = @Id;", new { Id = cajero });

        Assert.Equal(0, conexion.ExecuteScalar<long>(
            "SELECT COUNT(*) FROM PermisosUsuario WHERE UsuarioId = @Id;", new { Id = cajero }));
    }
}
