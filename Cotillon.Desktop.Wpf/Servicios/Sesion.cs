using Cotillon.Shared.DTOs;
using Cotillon.Shared.DTOs.Permisos;

namespace Cotillon.Desktop.Wpf.Servicios;

/// <summary>
/// Usuario logueado y lo que puede hacer en cada módulo.
///
/// Los permisos los define el administrador usuario por usuario y llegan con el
/// login; el rol quedó sólo como punto de partida al dar de alta a alguien.
///
/// Esto es un control operativo, no una barrera de seguridad: la API local no
/// pide credenciales en cada llamada, así que sirve para que cada uno vea su
/// trabajo y no toque de más, no para frenar a alguien que vaya directo a la API.
/// </summary>
public static class Sesion
{
    public static LoginResponseDto? Usuario { get; private set; }

    public static int UsuarioId => Usuario?.UsuarioId ?? 0;

    public static string NombreCompleto => Usuario?.NombreCompleto ?? string.Empty;

    public static string Rol => Usuario?.Rol ?? string.Empty;

    public static bool EsAdmin => Rol == "Admin";

    /// <summary>Nivel del usuario en un módulo. Lo que no está dado es Ninguno.</summary>
    public static NivelAcceso Nivel(string modulo) =>
        Usuario?.Permisos.GetValueOrDefault(modulo, NivelAcceso.Ninguno) ?? NivelAcceso.Ninguno;

    /// <summary>El módulo aparece en el menú.</summary>
    public static bool PuedeVer(string modulo) => Nivel(modulo) >= NivelAcceso.Ver;

    /// <summary>Puede cargar y guardar: el trabajo de todos los días.</summary>
    public static bool PuedeEditar(string modulo) => Nivel(modulo) >= NivelAcceso.Editar;

    /// <summary>Puede las acciones delicadas: anular, cerrar caja, dar de baja, borrar.</summary>
    public static bool PuedeTodo(string modulo) => Nivel(modulo) >= NivelAcceso.Total;

    /// <summary>Al menos un módulo visible: si no, el usuario entra a una ventana vacía.</summary>
    public static bool TieneAlgunModulo =>
        Usuario?.Permisos.Values.Any(n => n > NivelAcceso.Ninguno) ?? false;

    public static void Iniciar(LoginResponseDto usuario) => Usuario = usuario;

    public static void Cerrar() => Usuario = null;
}
