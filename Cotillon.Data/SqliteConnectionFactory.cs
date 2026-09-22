using System.Data;
using Microsoft.Data.Sqlite;

namespace Cotillon.Data;

public interface IDbConnectionFactory
{
    IDbConnection Crear();
    string RutaBase { get; }
}

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(string rutaBase)
    {
        if (string.IsNullOrWhiteSpace(rutaBase))
            throw new ArgumentException("La ruta de la base de datos no puede estar vacía.", nameof(rutaBase));

        RutaBase = Path.GetFullPath(rutaBase);

        _connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = RutaBase,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared,
            Pooling = true,
            ForeignKeys = true
        }.ToString();
    }

    public string RutaBase { get; }

    public IDbConnection Crear()
    {
        var conexion = new SqliteConnection(_connectionString);
        conexion.Open();

        // WAL permite que las lecturas no bloqueen a la escritura, y el busy timeout
        // evita errores "database is locked" cuando varias terminales operan a la vez.
        using var pragma = conexion.CreateCommand();
        pragma.CommandText = "PRAGMA journal_mode = WAL; PRAGMA busy_timeout = 5000; PRAGMA synchronous = NORMAL;";
        pragma.ExecuteNonQuery();

        return conexion;
    }
}
