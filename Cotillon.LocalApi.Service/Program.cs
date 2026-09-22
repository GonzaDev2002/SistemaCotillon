using Cotillon.Data;
using Cotillon.LocalApi.Service.Repositories;
using Cotillon.LocalApi.Service.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

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
