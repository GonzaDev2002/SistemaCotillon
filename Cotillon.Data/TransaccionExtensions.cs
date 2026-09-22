using System.Data;
using Microsoft.Data.Sqlite;

namespace Cotillon.Data;

public static class TransaccionExtensions
{
    /// <summary>
    /// Abre una transacción de escritura tomando el lock desde el inicio (BEGIN IMMEDIATE).
    /// Con el modo diferido que usa SQLite por defecto, dos operaciones que leen y
    /// después escriben el mismo dato pueden leer ambas el mismo valor y una fallar
    /// al promocionar; tomando el lock antes de leer, la segunda espera su turno
    /// (hasta el busy_timeout) y opera sobre datos ya actualizados.
    /// </summary>
    public static IDbTransaction IniciarEscritura(this IDbConnection conexion)
    {
        if (conexion is SqliteConnection sqlite)
            return sqlite.BeginTransaction(deferred: false);

        return conexion.BeginTransaction();
    }
}
