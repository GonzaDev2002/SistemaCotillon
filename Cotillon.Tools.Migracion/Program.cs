using System.Data;
using System.Data.OleDb;
using System.Globalization;
using Cotillon.Data;
using Dapper;

namespace Cotillon.Tools.Migracion;

internal static class Program
{
    private const string FormatoFecha = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Tablas transaccionales que este migrador no traslada. Si tienen datos la
    /// migración se aborta en lugar de perderlos silenciosamente.
    /// </summary>
    private static readonly string[] TablasTransaccionales =
    [
        "Ventas", "DetalleVentas", "PagosVenta", "SesionesCaja",
        "MovimientosCaja", "Compras", "DetalleCompras", "StockMovimientos"
    ];

    private static int Main(string[] args)
    {
        try
        {
            var opciones = Opciones.Parsear(args);

            if (opciones is null)
            {
                MostrarAyuda();
                return 1;
            }

            return opciones.Verificar
                ? VerificarBase(opciones.RutaSqlite)
                : Ejecutar(opciones);
        }
        catch (Exception ex)
        {
            Error($"Falló la migración: {ex.Message}");
            return 1;
        }
    }

    private static int Ejecutar(Opciones opciones)
    {
        if (!File.Exists(opciones.RutaAccess))
        {
            Error($"No se encontró la base Access en '{opciones.RutaAccess}'.");
            return 1;
        }

        if (File.Exists(opciones.RutaSqlite))
        {
            if (!opciones.Forzar)
            {
                Error($"Ya existe '{opciones.RutaSqlite}'. Usá --forzar para reemplazarla.");
                return 1;
            }

            var respaldo = $"{opciones.RutaSqlite}.{DateTime.Now:yyyyMMddHHmmss}.bak";
            File.Move(opciones.RutaSqlite, respaldo);
            Aviso($"Base existente movida a '{Path.GetFileName(respaldo)}'.");
        }

        Titulo("Migración Access -> SQLite");
        Console.WriteLine($"  Origen:  {opciones.RutaAccess}");
        Console.WriteLine($"  Destino: {opciones.RutaSqlite}");
        Console.WriteLine();

        using var access = AbrirAccess(opciones.RutaAccess);

        if (!Validar(access, opciones))
            return 1;

        var factory = new SqliteConnectionFactory(opciones.RutaSqlite);
        new DatabaseInitializer(factory).AplicarEsquema();

        using var destino = factory.Crear();
        using var transaccion = destino.BeginTransaction();

        var resumen = new List<(string Tabla, int Filas)>
        {
            ("Usuarios",      MigrarUsuarios(access, destino, transaccion)),
            ("Secciones",     MigrarSecciones(access, destino, transaccion)),
            ("Categorias",    MigrarCategorias(access, destino, transaccion)),
            ("Productos",     MigrarProductos(access, destino, transaccion)),
            ("Clientes",      MigrarClientes(access, destino, transaccion)),
            ("Proveedores",   MigrarProveedores(access, destino, transaccion)),
            ("MediosPago",    MigrarMediosPago(access, destino, transaccion)),
            ("Configuracion", MigrarConfiguracion(access, destino, transaccion))
        };

        transaccion.Commit();

        // Completa lo que la base de origen no tenía (medios de pago, claves de
        // configuración, usuario admin) sin pisar lo migrado.
        new DatabaseInitializer(factory).Sembrar();

        Console.WriteLine();
        Titulo("Resultado");

        foreach (var (tabla, filas) in resumen)
            Console.WriteLine($"  {tabla,-16} {filas,6} fila(s)");

        Console.WriteLine();
        Exito($"Migración completada en '{opciones.RutaSqlite}'.");
        Aviso("Los PIN se migraron hasheados con PBKDF2; los valores originales ya no son recuperables.");

        return 0;
    }

    // ----------------------------------------------------------- Verificación --

    private static int VerificarBase(string rutaSqlite)
    {
        if (!File.Exists(rutaSqlite))
        {
            Error($"No se encontró la base SQLite en '{rutaSqlite}'.");
            return 1;
        }

        Titulo($"Verificación de '{rutaSqlite}'");

        using var conexion = new SqliteConnectionFactory(rutaSqlite).Crear();

        var integridad = conexion.ExecuteScalar<string>("PRAGMA integrity_check;");
        var version = conexion.ExecuteScalar<long>("PRAGMA user_version;");
        var fksRotas = conexion.Query("PRAGMA foreign_key_check;").Count();

        Console.WriteLine($"  Versión de esquema : {version}");
        Console.WriteLine($"  Integridad         : {integridad}");
        Console.WriteLine($"  Claves foráneas    : {(fksRotas == 0 ? "sin violaciones" : $"{fksRotas} violación(es)")}");
        Console.WriteLine();

        Console.WriteLine("  Filas por tabla:");

        var tablas = conexion.Query<string>(
            "SELECT name FROM sqlite_master WHERE type = 'table' AND name NOT LIKE 'sqlite_%' ORDER BY name;");

        foreach (var tabla in tablas)
            Console.WriteLine($"    {tabla,-18} {ContarFilas(conexion, tabla),6}");

        Console.WriteLine();
        Console.WriteLine("  Muestra de productos (importes reconstruidos desde centavos):");

        foreach (var fila in conexion.Query(
            """
            SELECT NombreProducto AS Nombre, PrecioCentavos AS Precio, Stock AS Stock
            FROM Productos ORDER BY ProductoId LIMIT 5;
            """))
        {
            var precio = Money.FromCentavos((long)fila.Precio);
            Console.WriteLine($"    {Texto(fila.Nombre),-32} {precio,12:C2}  stock {fila.Stock}");
        }

        var ok = integridad == "ok" && fksRotas == 0;

        Console.WriteLine();

        if (ok)
            Exito("La base está íntegra.");
        else
            Error("La base tiene problemas de integridad.");

        return ok ? 0 : 1;
    }

    // ----------------------------------------------------------- Validaciones --

    private static bool Validar(IDbConnection access, Opciones opciones)
    {
        var problemas = new List<string>();

        foreach (var tabla in TablasTransaccionales)
        {
            var filas = ContarFilas(access, tabla);

            if (filas > 0)
                problemas.Add($"La tabla '{tabla}' tiene {filas} fila(s) y este migrador no las traslada.");
        }

        foreach (var fila in access.Query(
            """
            SELECT SeccionId AS SeccionId, NombreCategoria AS Nombre, COUNT(*) AS Veces
            FROM Categorias
            GROUP BY SeccionId, NombreCategoria
            HAVING COUNT(*) > 1
            """))
        {
            problemas.Add(
                $"La categoría '{Texto(fila.Nombre)}' está repetida {fila.Veces} veces en la sección {fila.SeccionId}.");
        }

        foreach (var fila in access.Query(
            """
            SELECT NombreSeccion AS Nombre, COUNT(*) AS Veces
            FROM Secciones
            GROUP BY NombreSeccion
            HAVING COUNT(*) > 1
            """))
        {
            problemas.Add($"La sección '{Texto(fila.Nombre)}' está repetida {fila.Veces} veces.");
        }

        foreach (var fila in access.Query(
            """
            SELECT NombreUsuario AS Nombre, COUNT(*) AS Veces
            FROM Usuarios
            GROUP BY NombreUsuario
            HAVING COUNT(*) > 1
            """))
        {
            problemas.Add($"El usuario '{Texto(fila.Nombre)}' está repetido {fila.Veces} veces.");
        }

        if (problemas.Count == 0)
            return true;

        Error("La base de origen tiene problemas que hay que resolver antes de migrar:");

        foreach (var problema in problemas)
            Console.WriteLine($"    - {problema}");

        if (opciones.Forzar)
        {
            Console.WriteLine();
            Aviso("--forzar no omite estas validaciones: corregí los datos en Access y volvé a intentar.");
        }

        return false;
    }

    private static int ContarFilas(IDbConnection conexion, string tabla) =>
        conexion.ExecuteScalar<int>($"SELECT COUNT(*) FROM [{tabla}]");

    // ------------------------------------------------------------ Migraciones --

    private static int MigrarUsuarios(IDbConnection origen, IDbConnection destino, IDbTransaction tx)
    {
        var filas = origen.Query(
            "SELECT UsuarioId, NombreUsuario, NombreCompleto, Pin, Rol, Activo, FechaAlta, FechaActualizacion FROM Usuarios");

        var migrados = 0;

        foreach (var fila in filas)
        {
            string pinOriginal = Texto(fila.Pin) ?? "0000";
            var (hash, salt) = PinHasher.Hashear(pinOriginal);

            destino.Execute(
                """
                INSERT INTO Usuarios
                    (UsuarioId, NombreUsuario, NombreCompleto, PinHash, PinSalt, Rol, Activo, FechaAlta, FechaActualizacion)
                VALUES
                    (@Id, @Usuario, @Nombre, @Hash, @Salt, @Rol, @Activo, @Alta, @Actualizacion);
                """,
                new
                {
                    Id = (int)fila.UsuarioId,
                    Usuario = Texto(fila.NombreUsuario),
                    Nombre = Texto(fila.NombreCompleto) ?? Texto(fila.NombreUsuario),
                    Hash = hash,
                    Salt = salt,
                    Rol = NormalizarRol(Texto(fila.Rol)),
                    Activo = Booleano(fila.Activo),
                    Alta = Fecha(fila.FechaAlta) ?? Ahora(),
                    Actualizacion = Fecha(fila.FechaActualizacion)
                }, tx);

            migrados++;
        }

        return migrados;
    }

    private static int MigrarSecciones(IDbConnection origen, IDbConnection destino, IDbTransaction tx)
    {
        var filas = origen.Query("SELECT SeccionId, NombreSeccion, Activo FROM Secciones");
        var migrados = 0;

        foreach (var fila in filas)
        {
            destino.Execute(
                """
                INSERT INTO Secciones (SeccionId, NombreSeccion, Activo)
                VALUES (@Id, @Nombre, @Activo);
                """,
                new
                {
                    Id = (int)fila.SeccionId,
                    Nombre = Texto(fila.NombreSeccion),
                    Activo = Booleano(fila.Activo)
                }, tx);

            migrados++;
        }

        return migrados;
    }

    private static int MigrarCategorias(IDbConnection origen, IDbConnection destino, IDbTransaction tx)
    {
        var filas = origen.Query("SELECT CategoriaId, SeccionId, NombreCategoria, Activo FROM Categorias");
        var migrados = 0;

        foreach (var fila in filas)
        {
            destino.Execute(
                """
                INSERT INTO Categorias (CategoriaId, SeccionId, NombreCategoria, Activo)
                VALUES (@Id, @SeccionId, @Nombre, @Activo);
                """,
                new
                {
                    Id = (int)fila.CategoriaId,
                    SeccionId = (int)fila.SeccionId,
                    Nombre = Texto(fila.NombreCategoria),
                    Activo = Booleano(fila.Activo)
                }, tx);

            migrados++;
        }

        return migrados;
    }

    private static int MigrarProductos(IDbConnection origen, IDbConnection destino, IDbTransaction tx)
    {
        var filas = origen.Query(
            """
            SELECT ProductoId, CategoriaId, CodigoProducto, NombreProducto, Descripcion, Precio, Costo,
                   Stock, StockMinimo, CodigoBarras, Marca, UnidadMedida, ImagenUrl,
                   PermiteVentaSinStock, Activo, FechaAlta, FechaActualizacion
            FROM Productos
            """).ToList();

        // Los códigos vacíos o repetidos se guardan como NULL: el esquema nuevo exige
        // que sean únicos cuando están presentes.
        var codigosProductoVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var codigosBarrasVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var migrados = 0;

        foreach (var fila in filas)
        {
            destino.Execute(
                """
                INSERT INTO Productos
                    (ProductoId, CategoriaId, CodigoProducto, NombreProducto, Descripcion,
                     PrecioCentavos, CostoCentavos, Stock, StockMinimo, CodigoBarras, Marca,
                     UnidadMedida, ImagenUrl, PermiteVentaSinStock, Activo, FechaAlta, FechaActualizacion)
                VALUES
                    (@Id, @CategoriaId, @Codigo, @Nombre, @Descripcion,
                     @Precio, @Costo, @Stock, @StockMinimo, @CodigoBarras, @Marca,
                     @Unidad, @Imagen, @SinStock, @Activo, @Alta, @Actualizacion);
                """,
                new
                {
                    Id = (int)fila.ProductoId,
                    CategoriaId = (int)fila.CategoriaId,
                    Codigo = CodigoUnico(Texto(fila.CodigoProducto), codigosProductoVistos),
                    Nombre = Texto(fila.NombreProducto),
                    Descripcion = Texto(fila.Descripcion),
                    Precio = Centavos(fila.Precio),
                    Costo = Centavos(fila.Costo),
                    Stock = Numero(fila.Stock),
                    StockMinimo = Numero(fila.StockMinimo),
                    CodigoBarras = CodigoUnico(Texto(fila.CodigoBarras), codigosBarrasVistos),
                    Marca = Texto(fila.Marca),
                    Unidad = Texto(fila.UnidadMedida) ?? "UNIDAD",
                    Imagen = Texto(fila.ImagenUrl),
                    SinStock = Booleano(fila.PermiteVentaSinStock),
                    Activo = Booleano(fila.Activo),
                    Alta = Fecha(fila.FechaAlta) ?? Ahora(),
                    Actualizacion = Fecha(fila.FechaActualizacion)
                }, tx);

            migrados++;
        }

        return migrados;
    }

    private static int MigrarClientes(IDbConnection origen, IDbConnection destino, IDbTransaction tx)
    {
        var filas = origen.Query(
            """
            SELECT ClienteId, NombreCompleto, Documento, Telefono, Email, Direccion,
                   Observaciones, Activo, FechaAlta, FechaActualizacion
            FROM Clientes
            """);

        var documentosVistos = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var migrados = 0;

        foreach (var fila in filas)
        {
            destino.Execute(
                """
                INSERT INTO Clientes
                    (ClienteId, NombreCompleto, Documento, Telefono, Email, Direccion,
                     Observaciones, Activo, FechaAlta, FechaActualizacion)
                VALUES
                    (@Id, @Nombre, @Documento, @Telefono, @Email, @Direccion,
                     @Observaciones, @Activo, @Alta, @Actualizacion);
                """,
                new
                {
                    Id = (int)fila.ClienteId,
                    Nombre = Texto(fila.NombreCompleto),
                    Documento = CodigoUnico(Texto(fila.Documento), documentosVistos),
                    Telefono = Texto(fila.Telefono),
                    Email = Texto(fila.Email),
                    Direccion = Texto(fila.Direccion),
                    Observaciones = Texto(fila.Observaciones),
                    Activo = Booleano(fila.Activo),
                    Alta = Fecha(fila.FechaAlta) ?? Ahora(),
                    Actualizacion = Fecha(fila.FechaActualizacion)
                }, tx);

            migrados++;
        }

        return migrados;
    }

    private static int MigrarProveedores(IDbConnection origen, IDbConnection destino, IDbTransaction tx)
    {
        var filas = origen.Query(
            """
            SELECT ProveedorId, NombreProveedor, Cuit, Telefono, Email, Direccion,
                   Observaciones, Activo, FechaAlta, FechaActualizacion
            FROM Proveedores
            """);

        var migrados = 0;

        foreach (var fila in filas)
        {
            destino.Execute(
                """
                INSERT INTO Proveedores
                    (ProveedorId, NombreProveedor, Cuit, Telefono, Email, Direccion,
                     Observaciones, Activo, FechaAlta, FechaActualizacion)
                VALUES
                    (@Id, @Nombre, @Cuit, @Telefono, @Email, @Direccion,
                     @Observaciones, @Activo, @Alta, @Actualizacion);
                """,
                new
                {
                    Id = (int)fila.ProveedorId,
                    Nombre = Texto(fila.NombreProveedor),
                    Cuit = Texto(fila.Cuit),
                    Telefono = Texto(fila.Telefono),
                    Email = Texto(fila.Email),
                    Direccion = Texto(fila.Direccion),
                    Observaciones = Texto(fila.Observaciones),
                    Activo = Booleano(fila.Activo),
                    Alta = Fecha(fila.FechaAlta) ?? Ahora(),
                    Actualizacion = Fecha(fila.FechaActualizacion)
                }, tx);

            migrados++;
        }

        return migrados;
    }

    private static int MigrarMediosPago(IDbConnection origen, IDbConnection destino, IDbTransaction tx)
    {
        var filas = origen.Query(
            "SELECT MedioPagoId, NombreMedioPago, Activo, OrdenVisual, FechaAlta, FechaActualizacion FROM MediosPago");

        var migrados = 0;

        foreach (var fila in filas)
        {
            var nombre = Texto(fila.NombreMedioPago) ?? "SIN NOMBRE";

            destino.Execute(
                """
                INSERT INTO MediosPago
                    (MedioPagoId, NombreMedioPago, AfectaEfectivo, OrdenVisual, Activo, FechaAlta, FechaActualizacion)
                VALUES
                    (@Id, @Nombre, @Afecta, @Orden, @Activo, @Alta, @Actualizacion);
                """,
                new
                {
                    Id = (int)fila.MedioPagoId,
                    Nombre = nombre,
                    Afecta = nombre.Contains("EFECTIVO", StringComparison.OrdinalIgnoreCase) ? 1 : 0,
                    Orden = (int)Numero(fila.OrdenVisual),
                    Activo = Booleano(fila.Activo),
                    Alta = Fecha(fila.FechaAlta) ?? Ahora(),
                    Actualizacion = Fecha(fila.FechaActualizacion)
                }, tx);

            migrados++;
        }

        return migrados;
    }

    private static int MigrarConfiguracion(IDbConnection origen, IDbConnection destino, IDbTransaction tx)
    {
        var filas = origen.Query("SELECT Clave, Valor, Descripcion, FechaActualizacion FROM Configuracion");
        var migrados = 0;

        foreach (var fila in filas)
        {
            var clave = Texto(fila.Clave);

            if (string.IsNullOrWhiteSpace(clave))
                continue;

            destino.Execute(
                """
                INSERT INTO Configuracion (Clave, Valor, Descripcion, FechaActualizacion)
                VALUES (@Clave, @Valor, @Descripcion, @Actualizacion);
                """,
                new
                {
                    Clave = clave,
                    Valor = Texto(fila.Valor),
                    Descripcion = Texto(fila.Descripcion),
                    Actualizacion = Fecha(fila.FechaActualizacion) ?? Ahora()
                }, tx);

            migrados++;
        }

        return migrados;
    }

    // ------------------------------------------------------------ Conversores --

    private static string? Texto(object? valor)
    {
        if (valor is null || valor is DBNull)
            return null;

        var texto = valor.ToString()?.Trim();

        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    private static string? CodigoUnico(string? codigo, HashSet<string> vistos) =>
        codigo is not null && vistos.Add(codigo) ? codigo : null;

    private static int Booleano(object? valor) =>
        valor is not null && valor is not DBNull && Convert.ToBoolean(valor) ? 1 : 0;

    private static double Numero(object? valor) =>
        valor is null || valor is DBNull ? 0 : Convert.ToDouble(valor, CultureInfo.InvariantCulture);

    private static long Centavos(object? valor) =>
        valor is null || valor is DBNull
            ? 0
            : Money.ToCentavos(Convert.ToDecimal(valor, CultureInfo.InvariantCulture));

    private static string? Fecha(object? valor) =>
        valor is null || valor is DBNull
            ? null
            : Convert.ToDateTime(valor, CultureInfo.InvariantCulture).ToString(FormatoFecha, CultureInfo.InvariantCulture);

    private static string Ahora() => DateTime.Now.ToString(FormatoFecha, CultureInfo.InvariantCulture);

    private static string NormalizarRol(string? rol) => rol?.Trim().ToLowerInvariant() switch
    {
        "admin" or "administrador" => "Admin",
        "supervisor" or "encargado" => "Supervisor",
        _ => "Cajero"
    };

    private static IDbConnection AbrirAccess(string ruta)
    {
        var conexion = new OleDbConnection(
            $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Path.GetFullPath(ruta)};Persist Security Info=False;");

        conexion.Open();

        return conexion;
    }

    // ------------------------------------------------------------------ Salida --

    private static void Titulo(string texto)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(texto);
        Console.WriteLine(new string('-', texto.Length));
        Console.ResetColor();
    }

    private static void Exito(string texto) => Escribir(texto, ConsoleColor.Green);

    private static void Aviso(string texto) => Escribir($"Aviso: {texto}", ConsoleColor.Yellow);

    private static void Error(string texto) => Escribir($"Error: {texto}", ConsoleColor.Red);

    private static void Escribir(string texto, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(texto);
        Console.ResetColor();
    }

    private static void MostrarAyuda()
    {
        Console.WriteLine("""
            Migra la base Access de Cotillon a SQLite.

            Uso:
              Cotillon.Tools.Migracion --access <ruta.accdb> --sqlite <ruta.db> [--forzar]

              Cotillon.Tools.Migracion --verificar [--sqlite <ruta.db>]

            Opciones:
              --access     Base Access de origen.   (por defecto: Data/CotillonPop1.accdb)
              --sqlite     Base SQLite a generar.   (por defecto: Data/cotillon.db)
              --forzar     Respalda y reemplaza la base SQLite si ya existe.
              --verificar  Revisa integridad, claves foráneas y contenido de la base SQLite.
            """);
    }

    private sealed record Opciones(string RutaAccess, string RutaSqlite, bool Forzar, bool Verificar)
    {
        public static Opciones? Parsear(string[] args)
        {
            var rutaAccess = Path.Combine("Data", "CotillonPop1.accdb");
            var rutaSqlite = Path.Combine("Data", "cotillon.db");
            var forzar = false;
            var verificar = false;

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i].ToLowerInvariant())
                {
                    case "--access" when i + 1 < args.Length:
                        rutaAccess = args[++i];
                        break;

                    case "--sqlite" when i + 1 < args.Length:
                        rutaSqlite = args[++i];
                        break;

                    case "--forzar":
                        forzar = true;
                        break;

                    case "--verificar":
                        verificar = true;
                        break;

                    case "--ayuda" or "-h" or "--help":
                        return null;

                    default:
                        Error($"Opción desconocida: {args[i]}");
                        return null;
                }
            }

            return new Opciones(rutaAccess, rutaSqlite, forzar, verificar);
        }
    }
}
