-- Archivos adjuntos (facturas, remitos, comprobantes, fotos).
--
-- Una sola tabla sirve a todas las pantallas: el par (EntidadTipo, EntidadId)
-- dice a qué registro pertenece el archivo. Evita repetir la misma estructura
-- cinco veces y permite una única pantalla de adjuntos reutilizable.
--
-- El archivo no se guarda en la base: vive en disco junto a cotillon.db, en
-- ProgramData\Cotillon\Adjuntos. Así la base se mantiene liviana, el backup es
-- copiar la carpeta, y servir un PDF no obliga a leerlo entero en memoria.

CREATE TABLE Adjuntos (
    AdjuntoId           INTEGER PRIMARY KEY AUTOINCREMENT,
    EntidadTipo         TEXT    NOT NULL CHECK (EntidadTipo IN
                            ('CLIENTE', 'PROVEEDOR', 'VENTA', 'COMPRA', 'PRODUCTO')),
    EntidadId           INTEGER NOT NULL,
    -- Nombre que eligió el usuario: sólo se muestra, nunca se usa para abrir el archivo.
    NombreArchivo       TEXT    NOT NULL,
    -- Nombre real en disco, generado por el servidor. Que el cliente no decida la
    -- ruta es lo que evita que un nombre como "..\..\algo.exe" escriba fuera de
    -- la carpeta de adjuntos.
    NombreEnDisco       TEXT    NOT NULL UNIQUE,
    TipoContenido       TEXT    NOT NULL,
    TamanoBytes         INTEGER NOT NULL CHECK (TamanoBytes > 0),
    Descripcion         TEXT,
    UsuarioId           INTEGER REFERENCES Usuarios (UsuarioId) ON DELETE SET NULL,
    FechaAlta           TEXT    NOT NULL DEFAULT (datetime('now', 'localtime'))
);

CREATE INDEX IX_Adjuntos_Entidad ON Adjuntos (EntidadTipo, EntidadId);
CREATE INDEX IX_Adjuntos_Fecha   ON Adjuntos (FechaAlta);
