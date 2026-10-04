-- Lo que un cotillón necesita y un punto de venta genérico no tiene.
--
-- Son cinco cosas que se apoyan en lo mismo (el producto y la venta) y por eso
-- van juntas en un solo script: el bulto, la temporada, los combos, la cuenta
-- corriente de clientes y los encargues con seña.

-- ---------------------------------------------------------------- Productos --

-- Los globos vienen en bolsa de 50 y se vende la bolsa y también el globo
-- suelto. El stock se lleva SIEMPRE en unidades: el bulto es nada más una forma
-- de cargar y de cobrar, y al vender uno se descuentan sus unidades. Llevar el
-- stock en dos medidas a la vez es lo que termina en inventarios que no cierran.
ALTER TABLE Productos ADD COLUMN UnidadesPorBulto   REAL    NOT NULL DEFAULT 1;
ALTER TABLE Productos ADD COLUMN PrecioBultoCentavos INTEGER NOT NULL DEFAULT 0;

-- Halloween, Navidad, Día del Niño, carnaval. Vacío es todo el año.
ALTER TABLE Productos ADD COLUMN Temporada TEXT;

-- Un combo no tiene stock propio: se arma en el momento con lo que hay. Su
-- disponible sale de sus componentes y al venderlo se descuentan ellos.
ALTER TABLE Productos ADD COLUMN EsCombo INTEGER NOT NULL DEFAULT 0 CHECK (EsCombo IN (0, 1));

CREATE INDEX IX_Productos_Temporada ON Productos (Temporada) WHERE Temporada IS NOT NULL;

-- ------------------------------------------------------------------ Combos --

CREATE TABLE ComboComponentes (
    ComboProductoId      INTEGER NOT NULL REFERENCES Productos (ProductoId) ON DELETE CASCADE,
    ComponenteProductoId INTEGER NOT NULL REFERENCES Productos (ProductoId) ON DELETE RESTRICT,
    Cantidad             REAL    NOT NULL CHECK (Cantidad > 0),
    PRIMARY KEY (ComboProductoId, ComponenteProductoId),
    -- Un combo que se contenga a sí mismo descontaría stock infinito.
    CHECK (ComboProductoId <> ComponenteProductoId)
);

CREATE INDEX IX_ComboComponentes_Componente ON ComboComponentes (ComponenteProductoId);

-- ------------------------------------------------------ Cuenta corriente --

-- El saldo del cliente no se guarda: se suma de acá. Un total guardado y un
-- detalle que no lo explica es el peor lugar donde puede estar un error de plata.
--
-- Importe positivo es deuda (una venta a cuenta), negativo es pago.
CREATE TABLE MovimientosCuentaCorriente (
    MovimientoCtaCteId INTEGER PRIMARY KEY AUTOINCREMENT,
    ClienteId          INTEGER NOT NULL REFERENCES Clientes (ClienteId) ON DELETE RESTRICT,
    UsuarioId          INTEGER NOT NULL REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT,
    Fecha              TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    Tipo               TEXT    NOT NULL CHECK (Tipo IN ('VENTA', 'PAGO', 'AJUSTE', 'ANULACION_VENTA')),
    ImporteCentavos    INTEGER NOT NULL CHECK (ImporteCentavos <> 0),
    VentaId            INTEGER REFERENCES Ventas (VentaId) ON DELETE RESTRICT,
    MedioPagoId        INTEGER REFERENCES MediosPago (MedioPagoId) ON DELETE RESTRICT,
    Observaciones      TEXT
);

CREATE INDEX IX_MovCtaCte_Cliente ON MovimientosCuentaCorriente (ClienteId, Fecha);
CREATE INDEX IX_MovCtaCte_Venta   ON MovimientosCuentaCorriente (VentaId) WHERE VentaId IS NOT NULL;

-- --------------------------------------------------------------- Encargues --

-- "Me preparás 60 bolsitas para el 15, te dejo seña." El stock NO se toca al
-- tomar el pedido: la mercadería sigue en el salón y se puede vender. Se
-- descuenta al entregar, que es cuando el encargue se convierte en venta.
CREATE TABLE Encargues (
    EncargueId      INTEGER PRIMARY KEY AUTOINCREMENT,
    ClienteId       INTEGER REFERENCES Clientes (ClienteId) ON DELETE RESTRICT,
    UsuarioId       INTEGER NOT NULL REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT,
    NombreContacto  TEXT    NOT NULL,
    Telefono        TEXT,
    FechaPedido     TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    FechaEntrega    TEXT    NOT NULL,
    TotalCentavos   INTEGER NOT NULL DEFAULT 0 CHECK (TotalCentavos >= 0),
    SenaCentavos    INTEGER NOT NULL DEFAULT 0 CHECK (SenaCentavos >= 0),
    Estado          TEXT    NOT NULL DEFAULT 'PENDIENTE'
                            CHECK (Estado IN ('PENDIENTE', 'ENTREGADO', 'CANCELADO')),
    VentaId         INTEGER REFERENCES Ventas (VentaId) ON DELETE RESTRICT,
    Observaciones   TEXT,
    -- La seña no puede superar lo que se encargó.
    CHECK (SenaCentavos <= TotalCentavos)
);

CREATE INDEX IX_Encargues_Entrega ON Encargues (FechaEntrega, Estado);

CREATE TABLE DetalleEncargues (
    DetalleEncargueId      INTEGER PRIMARY KEY AUTOINCREMENT,
    EncargueId             INTEGER NOT NULL REFERENCES Encargues (EncargueId) ON DELETE CASCADE,
    ProductoId             INTEGER NOT NULL REFERENCES Productos (ProductoId) ON DELETE RESTRICT,
    NombreProducto         TEXT    NOT NULL,
    Cantidad               REAL    NOT NULL CHECK (Cantidad > 0),
    PrecioUnitarioCentavos INTEGER NOT NULL CHECK (PrecioUnitarioCentavos >= 0)
);

CREATE INDEX IX_DetalleEncargues_Encargue ON DetalleEncargues (EncargueId);
