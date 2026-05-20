using Cotillon.LocalApi.Service.Data;
using Serilog;
using Cotillon.LocalApi.Service.Repositories;
using Cotillon.LocalApi.Service.Services;

var builder = WebApplication.CreateBuilder(args);

// Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        path: @"C:\Cotillon\Logs\api-log-.txt",
        rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Servicios
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<AccessDb>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SeccionesRepository>();
builder.Services.AddScoped<SeccionesService>();
builder.Services.AddScoped<ProductosRepository>();
builder.Services.AddScoped<ProductosService>();
builder.Services.AddScoped<CategoriasRepository>();
builder.Services.AddScoped<CategoriasService>();



var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();