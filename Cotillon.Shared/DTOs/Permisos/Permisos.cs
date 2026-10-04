namespace Cotillon.Shared.DTOs.Permisos;

/// <summary>
/// Qué puede hacer un usuario dentro de un módulo. Son niveles en escala: cada uno
/// incluye al anterior. Se eligió una escala y no una grilla de permisos sueltos
/// (ver / crear / editar / borrar por módulo) porque quien configura esto es el
/// dueño del negocio, y trece módulos por cuatro casilleros es una pantalla que
/// nadie termina de completar bien.
/// </summary>
public enum NivelAcceso
{
    /// <summary>No ve el módulo: no aparece en el menú.</summary>
    Ninguno = 0,

    /// <summary>Entra y consulta, pero no puede modificar nada.</summary>
    Ver = 1,

    /// <summary>El trabajo de todos los días: cargar, vender, guardar.</summary>
    Editar = 2,

    /// <summary>Además, las acciones delicadas: anular, cerrar caja, dar de baja, borrar.</summary>
    Total = 3
}

/// <summary>
/// Un módulo del sistema. El código es el que se guarda en la base; el resto es
/// para que la pantalla de permisos se explique sola.
/// </summary>
public sealed record Modulo(string Codigo, string Nombre, string QueEsEditar, string QueEsTotal);

public static class Modulos
{
    public const string Inicio = "INICIO";
    public const string Ventas = "VENTAS";
    public const string Caja = "CAJA";
    public const string HistorialVentas = "HISTORIAL_VENTAS";
    public const string Productos = "PRODUCTOS";
    public const string Catalogo = "CATALOGO";
    public const string Stock = "STOCK";
    public const string Compras = "COMPRAS";
    public const string Clientes = "CLIENTES";
    public const string CuentaCorriente = "CUENTA_CORRIENTE";
    public const string Encargues = "ENCARGUES";
    public const string Proveedores = "PROVEEDORES";
    public const string Reportes = "REPORTES";
    public const string Usuarios = "USUARIOS";
    public const string Auditoria = "AUDITORIA";
    public const string Configuracion = "CONFIGURACION";

    /// <summary>
    /// Todos los módulos, en el orden en que aparecen en el menú lateral.
    /// </summary>
    public static readonly IReadOnlyList<Modulo> Todos =
    [
        new(Inicio, "Inicio",
            "—",
            "Ve el resumen del día: total vendido, ticket promedio y bajo stock."),

        new(Ventas, "Ventas",
            "Cobrar ventas.",
            "Además, aplicar descuentos sobre el total."),

        new(Caja, "Caja",
            "Abrir la caja y registrar ingresos y egresos.",
            "Además, cerrar la caja y confirmar el arqueo."),

        new(HistorialVentas, "Historial de ventas",
            "—",
            "Anular ventas ya registradas."),

        new(Productos, "Productos",
            "Dar de alta y modificar productos y precios.",
            "Además, dar de baja y volver a poner en venta."),

        new(Catalogo, "Secciones y categorías",
            "Crear y renombrar secciones y categorías.",
            "Además, borrarlas."),

        new(Stock, "Movimientos de stock",
            "Registrar ajustes, mermas y devoluciones.",
            "—"),

        new(Compras, "Compras",
            "Registrar compras y recibir mercadería.",
            "Además, anular compras."),

        new(Clientes, "Clientes",
            "Dar de alta y modificar clientes, y adjuntar archivos.",
            "Además, darlos de baja y quitar archivos."),

        new(CuentaCorriente, "Cuenta corriente",
            "Registrar los pagos de los clientes que compran a cuenta.",
            "Además, ajustar saldos a mano."),

        new(Encargues, "Encargues",
            "Tomar pedidos con seña y entregarlos.",
            "Además, cancelarlos."),

        new(Proveedores, "Proveedores",
            "Dar de alta y modificar proveedores, y adjuntar archivos.",
            "Además, darlos de baja y quitar archivos."),

        new(Reportes, "Reportes",
            "—",
            "—"),

        new(Usuarios, "Usuarios",
            "Dar de alta y modificar usuarios.",
            "Además, cambiar permisos y restablecer PIN."),

        new(Auditoria, "Auditoría",
            "—",
            "—"),

        new(Configuracion, "Configuración",
            "Cambiar los datos del negocio y las preferencias.",
            "—")
    ];

    public static readonly string[] Codigos = [.. Todos.Select(m => m.Codigo)];

    public static string Nombre(string codigo) =>
        Todos.FirstOrDefault(m => m.Codigo == codigo)?.Nombre ?? codigo;

    public static bool Existe(string? codigo) =>
        codigo is not null && Codigos.Contains(codigo, StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// Plantillas por rol: con qué permisos arranca un usuario nuevo. El rol sigue
/// siendo el punto de partida, pero después el administrador ajusta usuario por
/// usuario, que es lo que cambia de un negocio a otro.
/// </summary>
public static class PlantillasDeRol
{
    public static Dictionary<string, NivelAcceso> Para(string rol) => rol switch
    {
        "Admin" => Modulos.Codigos.ToDictionary(c => c, _ => NivelAcceso.Total),

        "Supervisor" => Construir(
            total:
            [
                Modulos.Inicio, Modulos.Ventas, Modulos.Caja, Modulos.HistorialVentas,
                Modulos.Productos, Modulos.Catalogo, Modulos.Stock, Modulos.Compras,
                Modulos.Clientes, Modulos.CuentaCorriente, Modulos.Encargues,
                Modulos.Proveedores, Modulos.Reportes
            ],
            ver: [Modulos.Auditoria]),

        // El cajero vende y maneja la caja del día. No ve costos, compras,
        // proveedores, reportes ni administración.
        _ => Construir(
            editar: [Modulos.Ventas, Modulos.Caja, Modulos.Clientes, Modulos.Encargues],
            ver: [Modulos.Inicio, Modulos.HistorialVentas, Modulos.Productos, Modulos.CuentaCorriente])
    };

    private static Dictionary<string, NivelAcceso> Construir(
        string[]? total = null, string[]? editar = null, string[]? ver = null)
    {
        var permisos = Modulos.Codigos.ToDictionary(c => c, _ => NivelAcceso.Ninguno);

        foreach (var codigo in ver ?? []) permisos[codigo] = NivelAcceso.Ver;
        foreach (var codigo in editar ?? []) permisos[codigo] = NivelAcceso.Editar;
        foreach (var codigo in total ?? []) permisos[codigo] = NivelAcceso.Total;

        return permisos;
    }
}

public class PermisoDto
{
    public string Modulo { get; set; } = string.Empty;
    public NivelAcceso Nivel { get; set; }
}
