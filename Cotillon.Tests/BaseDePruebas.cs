using System.Data;
using Cotillon.Data;

namespace Cotillon.Tests;

/// <summary>
/// Crea una base SQLite temporal con el esquema aplicado y la borra al terminar.
/// La base vive en una carpeta propia, igual que en producción (ProgramData\Cotillon):
/// así lo que se guarda al lado de la base, como los adjuntos, queda aislado entre
/// pruebas y se limpia junto con ella.
/// </summary>
public sealed class BaseDePruebas : IDisposable
{
    private readonly string _carpeta;

    public BaseDePruebas(bool sembrar = true)
    {
        _carpeta = Path.Combine(Path.GetTempPath(), $"cotillon-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_carpeta);

        Factory = new SqliteConnectionFactory(Path.Combine(_carpeta, "cotillon.db"));

        var inicializador = new DatabaseInitializer(Factory);
        inicializador.AplicarEsquema();

        if (sembrar)
            inicializador.Sembrar();
    }

    public IDbConnectionFactory Factory { get; }

    public IDbConnection Abrir() => Factory.Crear();

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();

        if (Directory.Exists(_carpeta))
            Directory.Delete(_carpeta, recursive: true);
    }
}
