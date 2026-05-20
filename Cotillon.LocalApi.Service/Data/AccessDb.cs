using System.Data;
using System.Data.OleDb;

namespace Cotillon.LocalApi.Service.Data;

public class AccessDb
{
    private readonly IConfiguration _configuration;

    public AccessDb(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("AccessConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("La cadena de conexión 'AccessConnection' no está configurada.");

        return new OleDbConnection(connectionString);
    }
}