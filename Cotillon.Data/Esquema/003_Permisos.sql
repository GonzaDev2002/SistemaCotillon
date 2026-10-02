-- Permisos por usuario y por módulo.
--
-- El rol sigue existiendo como punto de partida (de ahí sale la plantilla con la
-- que arranca un usuario nuevo), pero lo que manda en la pantalla es esta tabla:
-- cada negocio reparte el trabajo distinto y el dueño necesita decidir qué ve y
-- qué puede hacer cada empleado sin que haya que tocar el código.
--
-- Nivel es una escala, no una lista: cada nivel incluye al anterior.
--   NINGUNO  el módulo no aparece en el menú
--   VER      entra y consulta
--   EDITAR   el trabajo diario: cargar, vender, guardar
--   TOTAL    además las acciones delicadas: anular, cerrar caja, dar de baja, borrar
--
-- Las filas no se cargan acá sino en el sembrado, que corre después de este
-- script: en una instalación nueva el usuario administrador todavía no existe
-- cuando se aplica el esquema, así que un INSERT en este punto no alcanzaría a
-- nadie y el primer login quedaría sin un solo módulo.

CREATE TABLE PermisosUsuario (
    UsuarioId   INTEGER NOT NULL REFERENCES Usuarios (UsuarioId) ON DELETE CASCADE,
    Modulo      TEXT    NOT NULL,
    Nivel       TEXT    NOT NULL CHECK (Nivel IN ('NINGUNO', 'VER', 'EDITAR', 'TOTAL')),
    PRIMARY KEY (UsuarioId, Modulo)
);

CREATE INDEX IX_PermisosUsuario_Usuario ON PermisosUsuario (UsuarioId);
