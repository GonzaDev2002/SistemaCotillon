using Cotillon.Shared.DTOs;

namespace Cotillon.Desktop.Wpf.Servicios;

/// <summary>
/// Usuario logueado y permisos derivados de su rol.
/// </summary>
public static class Sesion
{
    public static LoginResponseDto? Usuario { get; private set; }

    public static int UsuarioId => Usuario?.UsuarioId ?? 0;

    public static string NombreCompleto => Usuario?.NombreCompleto ?? string.Empty;

    public static string Rol => Usuario?.Rol ?? string.Empty;

    public static bool EsAdmin => Rol == "Admin";

    public static bool EsSupervisor => Rol is "Admin" or "Supervisor";

    /// <summary>Anular ventas, cerrar caja con diferencia y ajustar stock.</summary>
    public static bool PuedeSupervisar => EsSupervisor;

    /// <summary>Alta y baja de usuarios, y cambios de configuración del negocio.</summary>
    public static bool PuedeAdministrar => EsAdmin;

    public static void Iniciar(LoginResponseDto usuario) => Usuario = usuario;

    public static void Cerrar() => Usuario = null;
}
