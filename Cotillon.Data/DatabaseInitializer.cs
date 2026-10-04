using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using Cotillon.Shared.DTOs.Permisos;
using Dapper;

namespace Cotillon.Data;

/// <summary>
/// Crea la base si no existe y aplica los scripts de esquema pendientes.
/// La versión aplicada se guarda en PRAGMA user_version, de modo que actualizar
/// una instalación ya vendida consiste sólo en agregar un script nuevo.
/// </summary>
public class DatabaseInitializer
{
    private readonly IDbConnectionFactory _factory;

    public DatabaseInitializer(IDbConnectionFactory factory)
    {
        _factory = factory;
    }

    public void Inicializar()
    {
        AplicarEsquema();
        Sembrar();
    }

    public void AplicarEsquema()
    {
        var directorio = Path.GetDirectoryName(_factory.RutaBase);

        if (!string.IsNullOrWhiteSpace(directorio))
            Directory.CreateDirectory(directorio);

        using var conexion = _factory.Crear();

        var versionActual = conexion.ExecuteScalar<long>("PRAGMA user_version;");

        foreach (var script in ObtenerScripts().Where(s => s.Version > versionActual))
        {
            using var transaccion = conexion.BeginTransaction();

            conexion.Execute(script.Sql, transaction: transaccion);
            // user_version no admite parámetros; el valor viene del nombre del recurso embebido.
            conexion.Execute($"PRAGMA user_version = {script.Version};", transaction: transaccion);

            transaccion.Commit();
        }
    }

    /// <summary>
    /// Carga los datos mínimos para operar. Inserta fila por fila sólo lo que falta,
    /// de modo que una instalación ya en uso reciba las claves y medios de pago que
    /// agreguen las versiones nuevas sin pisar lo que el comercio haya configurado.
    /// </summary>
    public void Sembrar()
    {
        using var conexion = _factory.Crear();

        SembrarDatosIniciales(conexion);
    }

    private static IEnumerable<(int Version, string Sql)> ObtenerScripts()
    {
        var ensamblado = Assembly.GetExecutingAssembly();

        return ensamblado.GetManifestResourceNames()
            .Where(nombre => nombre.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .Select(nombre => new
            {
                Nombre = nombre,
                Match = Regex.Match(nombre, @"\.(\d+)_[^.]+\.sql$")
            })
            .Where(x => x.Match.Success)
            .Select(x => (
                Version: int.Parse(x.Match.Groups[1].Value),
                Sql: LeerRecurso(ensamblado, x.Nombre)))
            .OrderBy(x => x.Version)
            .ToList();
    }

    private static string LeerRecurso(Assembly ensamblado, string nombre)
    {
        using var stream = ensamblado.GetManifestResourceStream(nombre)
            ?? throw new InvalidOperationException($"No se pudo leer el script de esquema '{nombre}'.");

        using var lector = new StreamReader(stream);

        return lector.ReadToEnd();
    }

    private static void SembrarDatosIniciales(IDbConnection conexion)
    {
        var medios = new[]
        {
            new { Nombre = "EFECTIVO",       Afecta = 1, Orden = 1 },
            new { Nombre = "DEBITO",         Afecta = 0, Orden = 2 },
            new { Nombre = "CREDITO",        Afecta = 0, Orden = 3 },
            new { Nombre = "TRANSFERENCIA",  Afecta = 0, Orden = 4 },
            new { Nombre = "QR / BILLETERA", Afecta = 0, Orden = 5 },
            // No entra plata al cajón: queda anotado en la cuenta del cliente.
            new { Nombre = "CUENTA CORRIENTE", Afecta = 0, Orden = 6 }
        };

        conexion.Execute(
            """
            INSERT INTO MediosPago (NombreMedioPago, AfectaEfectivo, OrdenVisual, Activo)
            SELECT @Nombre, @Afecta, @Orden, 1
            WHERE NOT EXISTS (SELECT 1 FROM MediosPago WHERE NombreMedioPago = @Nombre);
            """, medios);

        var configuracion = new[]
        {
            new { Clave = "NEGOCIO_NOMBRE",          Valor = "Mi Cotillón", Descripcion = "Nombre que se imprime en tickets y reportes." },
            new { Clave = "NEGOCIO_DIRECCION",       Valor = "",            Descripcion = "Dirección del local." },
            new { Clave = "NEGOCIO_TELEFONO",        Valor = "",            Descripcion = "Teléfono de contacto." },
            new { Clave = "NEGOCIO_CUIT",            Valor = "",            Descripcion = "CUIT del negocio." },
            new { Clave = "TICKET_PIE",              Valor = "¡Gracias por su compra!", Descripcion = "Texto al pie del ticket." },
            new { Clave = "VENTA_PERMITE_SIN_STOCK", Valor = "false",       Descripcion = "Permite vender productos sin stock disponible." },
            new { Clave = "BALANZA_MASCARA",         Valor = "",            Descripcion = "Formato de las etiquetas con peso que imprime la balanza. Vacío: no se usan." },
            new { Clave = "TICKET_ANCHO",            Valor = "42",          Descripcion = "Caracteres por renglón del ticket: 32 para papel de 58 mm, 42 para 80 mm." },
            new { Clave = "TICKET_AUTOMATICO",       Valor = "false",       Descripcion = "Imprime el ticket al cobrar, sin preguntar." }
        };

        conexion.Execute(
            """
            INSERT OR IGNORE INTO Configuracion (Clave, Valor, Descripcion)
            VALUES (@Clave, @Valor, @Descripcion);
            """, configuracion);

        if (conexion.ExecuteScalar<long>("SELECT COUNT(*) FROM Usuarios;") == 0)
        {
            var (hash, salt) = PinHasher.Hashear("1234");

            conexion.Execute(
                """
                INSERT INTO Usuarios (NombreUsuario, NombreCompleto, PinHash, PinSalt, Rol, Activo)
                VALUES ('admin', 'Administrador', @Hash, @Salt, 'Admin', 1);
                """, new { Hash = hash, Salt = salt });
        }

        SembrarPermisosFaltantes(conexion);
    }

    /// <summary>
    /// Le da los permisos de su rol a todo usuario que no tenga ninguno.
    ///
    /// Cubre los dos casos de una sola vez: la instalación nueva, donde el usuario
    /// administrador se crea recién acá, y la que se actualiza desde una versión
    /// sin permisos, donde ya hay usuarios cargados. Toca sólo a los que no tienen
    /// ni una fila, así que lo que el dueño haya configurado nunca se pisa.
    /// </summary>
    private static void SembrarPermisosFaltantes(IDbConnection conexion)
    {
        var usuarios = conexion.Query<FilaUsuarioRol>("SELECT UsuarioId, Rol FROM Usuarios;").ToList();

        if (usuarios.Count == 0)
            return;

        // Falta POR MÓDULO y no por usuario. Si fuera por usuario, una versión que
        // agrega un módulo nuevo no le daría permiso a nadie: los usuarios ya
        // tienen filas de los módulos viejos, así que ninguno contaría como "sin
        // permisos" y el dueño se quedaría sin ver la pantalla nueva.
        var filas = usuarios
            .SelectMany(usuario => PlantillasDeRol.Para(usuario.Rol)
                .Select(permiso => new
                {
                    usuario.UsuarioId,
                    Modulo = permiso.Key,
                    Nivel = permiso.Value.ToString().ToUpperInvariant()
                }))
            .ToList();

        conexion.Execute(
            """
            INSERT INTO PermisosUsuario (UsuarioId, Modulo, Nivel)
            SELECT @UsuarioId, @Modulo, @Nivel
            WHERE NOT EXISTS (
                SELECT 1 FROM PermisosUsuario p
                WHERE p.UsuarioId = @UsuarioId AND p.Modulo = @Modulo);
            """,
            filas);
    }

    private sealed class FilaUsuarioRol
    {
        public int UsuarioId { get; init; }
        public string Rol { get; init; } = string.Empty;
    }
}
