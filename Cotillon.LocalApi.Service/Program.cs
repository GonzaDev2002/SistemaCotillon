using Cotillon.Data;
using Microsoft.Extensions.Hosting.WindowsServices;
using Cotillon.LocalApi.Service.Repositories;
using Cotillon.LocalApi.Service.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Permite que esto mismo corra como servicio de Windows, prendido con la PC y
// sin que nadie tenga que iniciar sesión. Es lo que hace que la caja ande
// después de un reinicio y que los respaldos salgan aunque el local esté
// cerrado. Corriendo a mano desde la consola no cambia nada: UseWindowsService
// no hace nada cuando el proceso no fue lanzado por el administrador de
// servicios.
//
// También corrige el directorio de trabajo: un servicio arranca en System32, y
// sin esto no encontraría su propio appsettings.json.
builder.Host.UseWindowsService(opciones => opciones.ServiceName = "Cotillon");

var rutaBase = ResolverRutaBaseDeDatos(builder.Configuration);
var rutaLogs = ResolverRutaLogs(builder.Configuration);

Log.Logger = new LoggerConfiguration()
    .WriteTo.File(Path.Combine(rutaLogs, "api-log-.txt"), rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(rutaBase));
builder.Services.AddSingleton<DatabaseInitializer>();

builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SeccionesRepository>();
builder.Services.AddScoped<SeccionesService>();
builder.Services.AddScoped<ProductosRepository>();
builder.Services.AddScoped<ProductosService>();
builder.Services.AddScoped<CategoriasRepository>();
builder.Services.AddScoped<CategoriasService>();
builder.Services.AddScoped<ReportesRepository>();
builder.Services.AddScoped<ReportesService>();
builder.Services.AddScoped<ConfiguracionRepository>();
builder.Services.AddScoped<CajaRepository>();
builder.Services.AddScoped<CajaService>();
builder.Services.AddScoped<VentasRepository>();
builder.Services.AddScoped<VentasService>();
builder.Services.AddScoped<ClientesRepository>();
builder.Services.AddScoped<ClientesService>();
builder.Services.AddScoped<ProveedoresRepository>();
builder.Services.AddScoped<ProveedoresService>();
builder.Services.AddScoped<StockRepository>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<ComprasRepository>();
builder.Services.AddScoped<ComprasService>();
builder.Services.AddScoped<PermisosRepository>();
builder.Services.AddScoped<PermisosService>();
builder.Services.AddScoped<AdjuntosRepository>();
builder.Services.AddScoped<AdjuntosService>();
builder.Services.AddScoped<AuditoriaRepository>();
builder.Services.AddScoped<UsuariosRepository>();
builder.Services.AddScoped<MediosPagoRepository>();
builder.Services.AddScoped<CombosRepository>();
builder.Services.AddScoped<PreciosRepository>();
builder.Services.AddScoped<CuentaCorrienteRepository>();
builder.Services.AddScoped<EncarguesRepository>();
builder.Services.AddScoped<ControlRepository>();
builder.Services.AddHostedService<RespaldoAutomatico>();

var app = builder.Build();

// Crear o actualizar el esquema al arrancar: una instalación nueva queda operativa
// sin pasos manuales y una existente recibe los scripts pendientes.
app.Services.GetRequiredService<DatabaseInitializer>().Inicializar();
Log.Information("Base de datos lista en {RutaBase}", rutaBase);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

app.Run();

static string ResolverRutaBaseDeDatos(IConfiguration configuracion)
{
    var configurada = configuracion["BaseDatos:Ruta"];

    if (!string.IsNullOrWhiteSpace(configurada))
        return Path.GetFullPath(configurada);

    // Sin configuración explícita se usa ProgramData, que es escribible por todos
    // los usuarios del equipo y sobrevive a las actualizaciones de la aplicación.
    return Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Cotillon", "cotillon.db");
}

static string ResolverRutaLogs(IConfiguration configuracion)
{
    var configurada = configuracion["Logs:Ruta"];

    var ruta = string.IsNullOrWhiteSpace(configurada)
        ? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Cotillon", "Logs")
        : Path.GetFullPath(configurada);

    Directory.CreateDirectory(ruta);

    return ruta;
}
