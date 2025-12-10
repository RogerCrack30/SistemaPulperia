-- TABLAS DE SISTEMA
CREATE TABLE Roles (
    RolId SERIAL PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL
);

CREATE TABLE Usuarios (
    UsuarioId SERIAL PRIMARY KEY,
    NombreUsuario VARCHAR(50) NOT NULL UNIQUE,
    ContrasenaHash VARCHAR(255) NOT NULL,
    RolId INT NOT NULL REFERENCES Roles(RolId),
    Estado BOOLEAN DEFAULT TRUE
);

CREATE TABLE Configuracion (
    Clave VARCHAR(50) PRIMARY KEY,
    Valor VARCHAR(255)
);

-- CATÁLOGOS
CREATE TABLE Categorias (
    CategoriaId SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL
);

CREATE TABLE Productos (
    ProductoId SERIAL PRIMARY KEY,
    Codigo VARCHAR(50),
    Nombre VARCHAR(100) NOT NULL,
    CategoriaId INT REFERENCES Categorias(CategoriaId),
    PrecioCompra DECIMAL(18,2) NOT NULL,
    PrecioVenta DECIMAL(18,2) NOT NULL,
    StockActual DECIMAL(18,2) DEFAULT 0,
    StockMinimo DECIMAL(18,2) DEFAULT 5,
    UnidadMedida VARCHAR(20),
    Estado BOOLEAN DEFAULT TRUE
);

CREATE TABLE Clientes (
    ClienteId SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20),
    Direccion TEXT
);

CREATE TABLE Proveedores (
    ProveedorId SERIAL PRIMARY KEY,
    Nombre VARCHAR(100) NOT NULL,
    Telefono VARCHAR(20),
    Direccion TEXT
);

-- CAJA
CREATE TABLE CajasSesiones (
    SesionId SERIAL PRIMARY KEY,
    UsuarioId INT REFERENCES Usuarios(UsuarioId),
    FechaInicio TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FechaFin TIMESTAMP,
    MontoInicial DECIMAL(18,2),
    MontoFinalEsperado DECIMAL(18,2),
    MontoFinalReal DECIMAL(18,2),
    Diferencia DECIMAL(18,2),
    Estado BOOLEAN DEFAULT TRUE
);

-- MOVIMIENTOS Y VENTAS
CREATE TABLE Ventas (
    VentaId SERIAL PRIMARY KEY,
    UsuarioId INT REFERENCES Usuarios(UsuarioId),
    ClienteId INT REFERENCES Clientes(ClienteId),
    Fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Total DECIMAL(18,2) NOT NULL,
    FormaPago VARCHAR(50) -- Efectivo/Tarjeta
);

CREATE TABLE DetalleVentas (
    DetalleId SERIAL PRIMARY KEY,
    VentaId INT REFERENCES Ventas(VentaId),
    ProductoId INT REFERENCES Productos(ProductoId),
    Cantidad DECIMAL(18,2) NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL
);

CREATE TABLE Compras (
    CompraId SERIAL PRIMARY KEY,
    Fecha TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ProveedorId INT REFERENCES Proveedores(ProveedorId),
    UsuarioId INT REFERENCES Usuarios(UsuarioId),
    Total DECIMAL(18,2) NOT NULL
);

CREATE TABLE DetalleCompras (
    DetalleId SERIAL PRIMARY KEY,
    CompraId INT REFERENCES Compras(CompraId),
    ProductoId INT REFERENCES Productos(ProductoId),
    Cantidad DECIMAL(18,2) NOT NULL,
    PrecioUnitario DECIMAL(18,2) NOT NULL,
    Subtotal DECIMAL(18,2) NOT NULL
);

-- DATOS INICIALES
INSERT INTO Roles (Nombre) VALUES ('Administrador'), ('Cajero');
INSERT INTO Usuarios (NombreUsuario, ContrasenaHash, RolId, Estado) VALUES ('admin', 'admin123', 1, TRUE);
INSERT INTO Configuracion (Clave, Valor) VALUES ('NombreNegocio', 'Mi Pulpería Cloud');
INSERT INTO Categorias (Nombre) VALUES ('General');
