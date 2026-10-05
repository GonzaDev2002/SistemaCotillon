-- Controles sobre quién maneja la plata.
--
-- Hace falta una acción nueva en Auditoría: CANCELACION, la venta que se armó y
-- no se cobró. Sin ese registro, armar el carrito, cobrarle al cliente y apretar
-- Escape no deja absolutamente nada, y el arqueo cierra perfecto porque esa
-- venta nunca existió para el sistema.
--
-- No puede ir en el mismo balde que ANULACION: anular es dar de baja una venta
-- que sí se registró y sí movió stock y caja. Son dos cosas distintas y el
-- control pierde sentido si se mezclan.
--
-- SQLite no deja modificar un CHECK, así que la tabla se rehace. Es segura de
-- copiar porque sólo se le agregan filas: nada la actualiza ni la borra.

CREATE TABLE Auditoria_nueva (
    AuditoriaId         INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId           INTEGER REFERENCES Usuarios (UsuarioId) ON DELETE SET NULL,
    FechaEvento         TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    TablaAfectada       TEXT    NOT NULL,
    Accion              TEXT    NOT NULL CHECK (Accion IN
                            ('ALTA', 'MODIFICACION', 'BAJA', 'LOGIN', 'LOGIN_FALLIDO', 'LOGOUT',
                             'ANULACION', 'APERTURA_CAJA', 'CIERRE_CAJA', 'CANCELACION')),
    RegistroId          INTEGER,
    Detalle             TEXT,
    Terminal            TEXT
);

INSERT INTO Auditoria_nueva
    (AuditoriaId, UsuarioId, FechaEvento, TablaAfectada, Accion, RegistroId, Detalle, Terminal)
SELECT
    AuditoriaId, UsuarioId, FechaEvento, TablaAfectada, Accion, RegistroId, Detalle, Terminal
FROM Auditoria;

DROP TABLE Auditoria;

ALTER TABLE Auditoria_nueva RENAME TO Auditoria;

-- Los índices se van con la tabla vieja, así que se vuelven a crear. El de
-- usuario y el de acción llevan la fecha: el reporte de control filtra siempre
-- por rango.
CREATE INDEX IX_Auditoria_Fecha   ON Auditoria (FechaEvento);
CREATE INDEX IX_Auditoria_Usuario ON Auditoria (UsuarioId, FechaEvento);
CREATE INDEX IX_Auditoria_Tabla   ON Auditoria (TablaAfectada);
CREATE INDEX IX_Auditoria_Accion  ON Auditoria (Accion, FechaEvento);
