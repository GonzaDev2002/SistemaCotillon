using System.Data;
using Dapper;

namespace Cotillon.Data;

/// <summary>
/// Registro de auditoría. Se escribe dentro de la misma transacción que la operación
/// auditada para que no pueda quedar un movimiento sin rastro ni un rastro sin
/// movimiento.
/// </summary>
public static class Auditoria
{
    public static void Registrar(
        IDbConnection conexion,
        IDbTransaction? tx,
        int? usuarioId,
        string tabla,
        string accion,
        int? registroId,
        string? detalle = null,
        string? terminal = null)
    {
        conexion.Execute(
            """
            INSERT INTO Auditoria (UsuarioId, TablaAfectada, Accion, RegistroId, Detalle, Terminal)
            VALUES (@UsuarioId, @Tabla, @Accion, @RegistroId, @Detalle, @Terminal);
            """,
            new
            {
                UsuarioId = usuarioId,
                Tabla = tabla,
                Accion = accion,
                RegistroId = registroId,
                Detalle = detalle,
                Terminal = terminal ?? Environment.MachineName
            },
            tx);
    }
}
