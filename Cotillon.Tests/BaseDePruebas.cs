using System.Data;
using Cotillon.Data;

namespace Cotillon.Tests;

/// <summary>
/// Crea una base SQLite temporal con el esquema aplicado y la borra al terminar.
/// </summary>
public sealed class BaseDePruebas : IDisposable
{
    private readonly string _ruta;

    public BaseDePruebas(bool sembrar = true)
    {
        _ruta = Path.Combine(Path.GetTempPath(), $"cotillon-test-{Guid.NewGuid():N}.db");

        Factory = new SqliteConnectionFactory(_ruta);

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

        foreach (var archivo in new[] { _ruta, $"{_ruta}-wal", $"{_ruta}-shm" })
        {
            if (File.Exists(archivo))
                File.Delete(archivo);
        }
    }
}
