-- Esquema inicial de Cotillon (SQLite).
--
-- Convención de importes: todas las columnas monetarias se almacenan como INTEGER
-- en centavos y llevan el sufijo "Centavos". Evita el error de redondeo de punto
-- flotante y permite SUM() exacto en los reportes. La conversión a decimal vive
-- en Cotillon.Data.Money.
--
-- Convención de fechas: TEXT en formato ISO-8601 hora local ('YYYY-MM-DD HH:MM:SS').
-- Se usa hora local y no UTC porque los cortes de caja y los reportes diarios
-- operan sobre el día comercial del local.

PRAGMA foreign_keys = ON;

-- ---------------------------------------------------------------- Seguridad --

CREATE TABLE Usuarios (
    UsuarioId           INTEGER PRIMARY KEY AUTOINCREMENT,
    NombreUsuario       TEXT    NOT NULL COLLATE NOCASE UNIQUE,
    NombreCompleto      TEXT    NOT NULL,
    PinHash             TEXT    NOT NULL,
    PinSalt             TEXT    NOT NULL,
    Rol                 TEXT    NOT NULL CHECK (Rol IN ('Admin', 'Supervisor', 'Cajero')),
    Activo              INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1)),
    FechaAlta           TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    FechaActualizacion  TEXT,
    UltimoAcceso        TEXT
);

-- ------------------------------------------------------------- Catalogación --

CREATE TABLE Secciones (
    SeccionId           INTEGER PRIMARY KEY AUTOINCREMENT,
    NombreSeccion       TEXT    NOT NULL COLLATE NOCASE UNIQUE,
    Activo              INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1))
);

CREATE TABLE Categorias (
    CategoriaId         INTEGER PRIMARY KEY AUTOINCREMENT,
    SeccionId           INTEGER NOT NULL REFERENCES Secciones (SeccionId) ON DELETE RESTRICT,
    NombreCategoria     TEXT    NOT NULL COLLATE NOCASE,
    Activo              INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1)),
    UNIQUE (SeccionId, NombreCategoria)
);

CREATE INDEX IX_Categorias_Seccion ON Categorias (SeccionId);

CREATE TABLE Productos (
    ProductoId          INTEGER PRIMARY KEY AUTOINCREMENT,
    CategoriaId         INTEGER NOT NULL REFERENCES Categorias (CategoriaId) ON DELETE RESTRICT,
    CodigoProducto      TEXT    COLLATE NOCASE,
    NombreProducto      TEXT    NOT NULL COLLATE NOCASE,
    Descripcion         TEXT,
    PrecioCentavos      INTEGER NOT NULL DEFAULT 0 CHECK (PrecioCentavos >= 0),
    CostoCentavos       INTEGER NOT NULL DEFAULT 0 CHECK (CostoCentavos >= 0),
    Stock               REAL    NOT NULL DEFAULT 0,
    StockMinimo         REAL    NOT NULL DEFAULT 0 CHECK (StockMinimo >= 0),
    CodigoBarras        TEXT,
    Marca               TEXT,
    UnidadMedida        TEXT    NOT NULL DEFAULT 'UNIDAD',
    ImagenUrl           TEXT,
    PermiteVentaSinStock INTEGER NOT NULL DEFAULT 0 CHECK (PermiteVentaSinStock IN (0, 1)),
    Activo              INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1)),
    FechaAlta           TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    FechaActualizacion  TEXT
);

CREATE INDEX IX_Productos_Categoria ON Productos (CategoriaId);
CREATE INDEX IX_Productos_Nombre    ON Productos (NombreProducto);
CREATE UNIQUE INDEX UX_Productos_CodigoBarras
    ON Productos (CodigoBarras) WHERE CodigoBarras IS NOT NULL AND CodigoBarras <> '';
CREATE UNIQUE INDEX UX_Productos_CodigoProducto
    ON Productos (CodigoProducto) WHERE CodigoProducto IS NOT NULL AND CodigoProducto <> '';

-- ------------------------------------------------------------------ Terceros --

CREATE TABLE Clientes (
    ClienteId           INTEGER PRIMARY KEY AUTOINCREMENT,
    NombreCompleto      TEXT    NOT NULL COLLATE NOCASE,
    Documento           TEXT,
    Telefono            TEXT,
    Email               TEXT,
    Direccion           TEXT,
    Observaciones       TEXT,
    Activo              INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1)),
    FechaAlta           TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    FechaActualizacion  TEXT
);

CREATE INDEX IX_Clientes_Nombre ON Clientes (NombreCompleto);
CREATE UNIQUE INDEX UX_Clientes_Documento
    ON Clientes (Documento) WHERE Documento IS NOT NULL AND Documento <> '';

CREATE TABLE Proveedores (
    ProveedorId         INTEGER PRIMARY KEY AUTOINCREMENT,
    NombreProveedor     TEXT    NOT NULL COLLATE NOCASE,
    Cuit                TEXT,
    Telefono            TEXT,
    Email               TEXT,
    Direccion           TEXT,
    Observaciones       TEXT,
    Activo              INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1)),
    FechaAlta           TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    FechaActualizacion  TEXT
);

CREATE INDEX IX_Proveedores_Nombre ON Proveedores (NombreProveedor);

-- ---------------------------------------------------------------------- Caja --

CREATE TABLE MediosPago (
    MedioPagoId         INTEGER PRIMARY KEY AUTOINCREMENT,
    NombreMedioPago     TEXT    NOT NULL COLLATE NOCASE UNIQUE,
    -- Indica si el importe entra físicamente al cajón: define qué se cuenta en el arqueo.
    AfectaEfectivo      INTEGER NOT NULL DEFAULT 0 CHECK (AfectaEfectivo IN (0, 1)),
    OrdenVisual         INTEGER NOT NULL DEFAULT 0,
    Activo              INTEGER NOT NULL DEFAULT 1 CHECK (Activo IN (0, 1)),
    FechaAlta           TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    FechaActualizacion  TEXT
);

CREATE TABLE SesionesCaja (
    SesionCajaId            INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioAperturaId       INTEGER NOT NULL REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT,
    UsuarioCierreId         INTEGER REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT,
    FechaApertura           TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    FechaCierre             TEXT,
    MontoInicialCentavos    INTEGER NOT NULL DEFAULT 0 CHECK (MontoInicialCentavos >= 0),
    MontoEsperadoCentavos   INTEGER,
    MontoContadoCentavos    INTEGER,
    DiferenciaCentavos      INTEGER,
    Estado                  TEXT    NOT NULL DEFAULT 'ABIERTA' CHECK (Estado IN ('ABIERTA', 'CERRADA')),
    Observaciones           TEXT
);

CREATE INDEX IX_SesionesCaja_Estado  ON SesionesCaja (Estado);
CREATE INDEX IX_SesionesCaja_Fecha   ON SesionesCaja (FechaApertura);

-- Sólo puede haber una sesión de caja abierta a la vez: al ser único el valor
-- 'ABIERTA' dentro del índice parcial, la segunda apertura falla en la base.
CREATE UNIQUE INDEX UX_SesionesCaja_UnicaAbierta
    ON SesionesCaja (Estado) WHERE Estado = 'ABIERTA';

CREATE TABLE MovimientosCaja (
    MovimientoCajaId    INTEGER PRIMARY KEY AUTOINCREMENT,
    SesionCajaId        INTEGER NOT NULL REFERENCES SesionesCaja (SesionCajaId) ON DELETE RESTRICT,
    UsuarioId           INTEGER NOT NULL REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT,
    FechaMovimiento     TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    TipoMovimiento      TEXT    NOT NULL CHECK (TipoMovimiento IN
                            ('APERTURA', 'VENTA', 'ANULACION_VENTA', 'INGRESO', 'EGRESO', 'RETIRO', 'CIERRE')),
    MedioPagoId         INTEGER REFERENCES MediosPago (MedioPagoId) ON DELETE RESTRICT,
    -- Positivo suma al saldo de caja, negativo resta.
    ImporteCentavos     INTEGER NOT NULL,
    Motivo              TEXT    NOT NULL,
    OrigenTipo          TEXT    CHECK (OrigenTipo IN ('VENTA', 'COMPRA', 'MANUAL')),
    OrigenId            INTEGER,
    Observaciones       TEXT
);

CREATE INDEX IX_MovimientosCaja_Sesion ON MovimientosCaja (SesionCajaId);
CREATE INDEX IX_MovimientosCaja_Fecha  ON MovimientosCaja (FechaMovimiento);

-- ------------------------------------------------------------------- Ventas --

CREATE TABLE Ventas (
    VentaId                 INTEGER PRIMARY KEY AUTOINCREMENT,
    ClienteId               INTEGER REFERENCES Clientes (ClienteId) ON DELETE RESTRICT,
    UsuarioId               INTEGER NOT NULL REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT,
    SesionCajaId            INTEGER NOT NULL REFERENCES SesionesCaja (SesionCajaId) ON DELETE RESTRICT,
    FechaVenta              TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    TipoComprobante         TEXT    NOT NULL DEFAULT 'TICKET'
                                CHECK (TipoComprobante IN ('TICKET', 'REMITO', 'PRESUPUESTO')),
    NumeroComprobante       TEXT,
    SubtotalCentavos        INTEGER NOT NULL DEFAULT 0 CHECK (SubtotalCentavos >= 0),
    DescuentoTotalCentavos  INTEGER NOT NULL DEFAULT 0 CHECK (DescuentoTotalCentavos >= 0),
    TotalVentaCentavos      INTEGER NOT NULL DEFAULT 0 CHECK (TotalVentaCentavos >= 0),
    Estado                  TEXT    NOT NULL DEFAULT 'COMPLETADA'
                                CHECK (Estado IN ('COMPLETADA', 'ANULADA')),
    FechaAnulacion          TEXT,
    UsuarioAnulacionId      INTEGER REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT,
    MotivoAnulacion         TEXT,
    Observaciones           TEXT,
    FechaAlta               TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    FechaActualizacion      TEXT
);

CREATE INDEX IX_Ventas_Fecha   ON Ventas (FechaVenta);
CREATE INDEX IX_Ventas_Sesion  ON Ventas (SesionCajaId);
CREATE INDEX IX_Ventas_Usuario ON Ventas (UsuarioId);
CREATE INDEX IX_Ventas_Cliente ON Ventas (ClienteId);
CREATE INDEX IX_Ventas_Estado  ON Ventas (Estado);
CREATE UNIQUE INDEX UX_Ventas_NumeroComprobante
    ON Ventas (NumeroComprobante) WHERE NumeroComprobante IS NOT NULL AND NumeroComprobante <> '';

CREATE TABLE DetalleVentas (
    DetalleVentaId          INTEGER PRIMARY KEY AUTOINCREMENT,
    VentaId                 INTEGER NOT NULL REFERENCES Ventas (VentaId) ON DELETE CASCADE,
    ProductoId              INTEGER NOT NULL REFERENCES Productos (ProductoId) ON DELETE RESTRICT,
    -- Se copia el nombre al momento de vender: el reporte histórico no debe cambiar
    -- si después renombran el producto.
    NombreProducto          TEXT    NOT NULL,
    Cantidad                REAL    NOT NULL CHECK (Cantidad > 0),
    PrecioUnitarioCentavos  INTEGER NOT NULL CHECK (PrecioUnitarioCentavos >= 0),
    CostoUnitarioCentavos   INTEGER NOT NULL DEFAULT 0 CHECK (CostoUnitarioCentavos >= 0),
    DescuentoLineaCentavos  INTEGER NOT NULL DEFAULT 0 CHECK (DescuentoLineaCentavos >= 0),
    SubtotalLineaCentavos   INTEGER NOT NULL CHECK (SubtotalLineaCentavos >= 0)
);

CREATE INDEX IX_DetalleVentas_Venta    ON DetalleVentas (VentaId);
CREATE INDEX IX_DetalleVentas_Producto ON DetalleVentas (ProductoId);

CREATE TABLE PagosVenta (
    PagoVentaId         INTEGER PRIMARY KEY AUTOINCREMENT,
    VentaId             INTEGER NOT NULL REFERENCES Ventas (VentaId) ON DELETE CASCADE,
    MedioPagoId         INTEGER NOT NULL REFERENCES MediosPago (MedioPagoId) ON DELETE RESTRICT,
    ImporteCentavos     INTEGER NOT NULL CHECK (ImporteCentavos > 0),
    -- Vuelto entregado cuando el medio es efectivo y el cliente paga de más.
    VueltoCentavos      INTEGER NOT NULL DEFAULT 0 CHECK (VueltoCentavos >= 0),
    Referencia          TEXT,
    FechaPago           TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    Observaciones       TEXT
);

CREATE INDEX IX_PagosVenta_Venta ON PagosVenta (VentaId);

-- ------------------------------------------------------------------ Compras --

CREATE TABLE Compras (
    CompraId                INTEGER PRIMARY KEY AUTOINCREMENT,
    ProveedorId             INTEGER NOT NULL REFERENCES Proveedores (ProveedorId) ON DELETE RESTRICT,
    UsuarioId               INTEGER NOT NULL REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT,
    FechaCompra             TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    NumeroComprobante       TEXT,
    SubtotalCentavos        INTEGER NOT NULL DEFAULT 0 CHECK (SubtotalCentavos >= 0),
    DescuentoTotalCentavos  INTEGER NOT NULL DEFAULT 0 CHECK (DescuentoTotalCentavos >= 0),
    TotalCompraCentavos     INTEGER NOT NULL DEFAULT 0 CHECK (TotalCompraCentavos >= 0),
    Estado                  TEXT    NOT NULL DEFAULT 'PENDIENTE'
                                CHECK (Estado IN ('PENDIENTE', 'RECIBIDA', 'ANULADA')),
    Observaciones           TEXT,
    FechaAlta               TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    FechaActualizacion      TEXT
);

CREATE INDEX IX_Compras_Proveedor ON Compras (ProveedorId);
CREATE INDEX IX_Compras_Fecha     ON Compras (FechaCompra);

CREATE TABLE DetalleCompras (
    DetalleCompraId         INTEGER PRIMARY KEY AUTOINCREMENT,
    CompraId                INTEGER NOT NULL REFERENCES Compras (CompraId) ON DELETE CASCADE,
    ProductoId              INTEGER NOT NULL REFERENCES Productos (ProductoId) ON DELETE RESTRICT,
    NombreProducto          TEXT    NOT NULL,
    Cantidad                REAL    NOT NULL CHECK (Cantidad > 0),
    CostoUnitarioCentavos   INTEGER NOT NULL CHECK (CostoUnitarioCentavos >= 0),
    SubtotalLineaCentavos   INTEGER NOT NULL CHECK (SubtotalLineaCentavos >= 0)
);

CREATE INDEX IX_DetalleCompras_Compra   ON DetalleCompras (CompraId);
CREATE INDEX IX_DetalleCompras_Producto ON DetalleCompras (ProductoId);

-- -------------------------------------------------------------------- Stock --

CREATE TABLE StockMovimientos (
    StockMovimientoId   INTEGER PRIMARY KEY AUTOINCREMENT,
    ProductoId          INTEGER NOT NULL REFERENCES Productos (ProductoId) ON DELETE RESTRICT,
    UsuarioId           INTEGER NOT NULL REFERENCES Usuarios (UsuarioId) ON DELETE RESTRICT,
    FechaMovimiento     TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    TipoMovimiento      TEXT    NOT NULL CHECK (TipoMovimiento IN
                            ('ALTA', 'VENTA', 'ANULACION_VENTA', 'COMPRA', 'AJUSTE', 'MERMA', 'DEVOLUCION')),
    -- Positivo suma stock, negativo lo descuenta.
    Cantidad            REAL    NOT NULL,
    StockAnterior       REAL    NOT NULL,
    StockNuevo          REAL    NOT NULL,
    OrigenTipo          TEXT    CHECK (OrigenTipo IN ('VENTA', 'COMPRA', 'MANUAL')),
    OrigenId            INTEGER,
    Observaciones       TEXT
);

CREATE INDEX IX_StockMovimientos_Producto ON StockMovimientos (ProductoId);
CREATE INDEX IX_StockMovimientos_Fecha    ON StockMovimientos (FechaMovimiento);

-- ---------------------------------------------------------------- Auditoría --

CREATE TABLE Auditoria (
    AuditoriaId         INTEGER PRIMARY KEY AUTOINCREMENT,
    UsuarioId           INTEGER REFERENCES Usuarios (UsuarioId) ON DELETE SET NULL,
    FechaEvento         TEXT    NOT NULL DEFAULT (datetime('now', 'localtime')),
    TablaAfectada       TEXT    NOT NULL,
    Accion              TEXT    NOT NULL CHECK (Accion IN
                            ('ALTA', 'MODIFICACION', 'BAJA', 'LOGIN', 'LOGIN_FALLIDO', 'LOGOUT',
                             'ANULACION', 'APERTURA_CAJA', 'CIERRE_CAJA')),
    RegistroId          INTEGER,
    Detalle             TEXT,
    Terminal            TEXT
);

CREATE INDEX IX_Auditoria_Fecha   ON Auditoria (FechaEvento);
CREATE INDEX IX_Auditoria_Usuario ON Auditoria (UsuarioId);
CREATE INDEX IX_Auditoria_Tabla   ON Auditoria (TablaAfectada);

-- ------------------------------------------------------------ Configuración --

CREATE TABLE Configuracion (
    Clave               TEXT    PRIMARY KEY COLLATE NOCASE,
    Valor               TEXT,
    Descripcion         TEXT,
    FechaActualizacion  TEXT    NOT NULL DEFAULT (datetime('now', 'localtime'))
);
