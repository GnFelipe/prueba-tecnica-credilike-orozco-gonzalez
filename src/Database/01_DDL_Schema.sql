-- =============================================================================
-- Credilike SaaS - Script DDL de Base de Datos (SQL Server 2019+)
-- Punto 3.1: Diseño de Esquema Multi-Tenant para US-042
-- =============================================================================

USE master;
GO

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'CredilikeDb')
BEGIN
    CREATE DATABASE CredilikeDb;
END
GO

USE CredilikeDb;
GO

-- -----------------------------------------------------------------------------
-- 1. Tabla: Tenants (Empresas / Clientes SaaS)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.Tenants', 'U') IS NOT NULL DROP TABLE dbo.Tenants;
CREATE TABLE dbo.Tenants (
    Id INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Nit VARCHAR(20) NOT NULL,
    Activo BIT NOT NULL CONSTRAINT DF_Tenants_Activo DEFAULT (1),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Tenants_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy INT NOT NULL,
    CONSTRAINT PK_Tenants PRIMARY KEY CLUSTERED (Id)
);
GO

-- -----------------------------------------------------------------------------
-- 2. Tabla: Usuarios (Asesores, Administradores, Supervisores, Aprobadores)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.Usuarios', 'U') IS NOT NULL DROP TABLE dbo.Usuarios;
CREATE TABLE dbo.Usuarios (
    Id INT IDENTITY(1,1) NOT NULL,
    TenantId INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Email VARCHAR(150) NOT NULL,
    Rol VARCHAR(50) NOT NULL, -- 'Admin', 'Supervisor', 'Aprobador', 'Asesor'
    Activo BIT NOT NULL CONSTRAINT DF_Usuarios_Activo DEFAULT (1),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Usuarios_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy INT NOT NULL,
    CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Usuarios_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id)
);
GO

-- -----------------------------------------------------------------------------
-- 3. Tabla: Liquidaciones (Cabecera del proceso de liquidación por período)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.Liquidaciones', 'U') IS NOT NULL DROP TABLE dbo.Liquidaciones;
CREATE TABLE dbo.Liquidaciones (
    Id INT IDENTITY(1,1) NOT NULL,
    TenantId INT NOT NULL,
    Periodo VARCHAR(7) NOT NULL, -- Formato 'YYYY-MM'
    MontoTotal DECIMAL(18,2) NOT NULL CONSTRAINT DF_Liquidaciones_MontoTotal DEFAULT (0.00),
    TotalAsesores INT NOT NULL CONSTRAINT DF_Liquidaciones_TotalAsesores DEFAULT (0),
    Estado VARCHAR(20) NOT NULL CONSTRAINT DF_Liquidaciones_Estado DEFAULT ('Borrador'), -- 'Borrador', 'EnProceso', 'Aprobada', 'Rechazada'
    Observaciones NVARCHAR(500) NULL,
    AprobadoPor INT NULL,
    FechaAprobacion DATETIME2 NULL,
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Liquidaciones_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy INT NOT NULL,
    CONSTRAINT PK_Liquidaciones PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Liquidaciones_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_Liquidaciones_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES dbo.Usuarios(Id),
    CONSTRAINT FK_Liquidaciones_AprobadoPor FOREIGN KEY (AprobadoPor) REFERENCES dbo.Usuarios(Id)
);
GO

-- -----------------------------------------------------------------------------
-- 4. Tabla: LiquidacionDetalles (Detalle por asesor dentro de una liquidación)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.LiquidacionDetalles', 'U') IS NOT NULL DROP TABLE dbo.LiquidacionDetalles;
CREATE TABLE dbo.LiquidacionDetalles (
    Id INT IDENTITY(1,1) NOT NULL,
    TenantId INT NOT NULL,
    LiquidacionId INT NOT NULL,
    AsesorId INT NOT NULL,
    MontoVentas DECIMAL(18,2) NOT NULL,
    MontoComision DECIMAL(18,2) NOT NULL,
    Estado VARCHAR(20) NOT NULL CONSTRAINT DF_LiquidacionDetalles_Estado DEFAULT ('Pendiente'), -- 'Pendiente', 'ListaParaPago', 'Pagada'
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_LiquidacionDetalles_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy INT NOT NULL,
    CONSTRAINT PK_LiquidacionDetalles PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_LiquidacionDetalles_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_LiquidacionDetalles_Liquidaciones FOREIGN KEY (LiquidacionId) REFERENCES dbo.Liquidaciones(Id),
    CONSTRAINT FK_LiquidacionDetalles_Asesores FOREIGN KEY (AsesorId) REFERENCES dbo.Usuarios(Id)
);
GO

-- -----------------------------------------------------------------------------
-- 5. Tabla: Comisiones (Registro individual de comisiones por venta)
-- -----------------------------------------------------------------------------
IF OBJECT_ID('dbo.Comisiones', 'U') IS NOT NULL DROP TABLE dbo.Comisiones;
CREATE TABLE dbo.Comisiones (
    Id BIGINT IDENTITY(1,1) NOT NULL,
    TenantId INT NOT NULL,
    UsuarioId INT NOT NULL, -- Asesor comercial
    VentaId BIGINT NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    Estado VARCHAR(20) NOT NULL CONSTRAINT DF_Comisiones_Estado DEFAULT ('PAGADO'), -- 'PENDIENTE', 'PAGADO', 'RECHAZADO'
    FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Comisiones_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    CreatedAt DATETIME2 NOT NULL CONSTRAINT DF_Comisiones_CreatedAt DEFAULT (SYSUTCDATETIME()),
    CreatedBy INT NOT NULL,
    CONSTRAINT PK_Comisiones PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Comisiones_Tenants FOREIGN KEY (TenantId) REFERENCES dbo.Tenants(Id),
    CONSTRAINT FK_Comisiones_Usuarios FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios(Id)
);
GO

-- =============================================================================
-- ÍNDICES DE ALTO RENDIMIENTO JUSTIFICADOS (Para soportar 50M de registros)
-- =============================================================================

-- ÍNDICE 1: Cobertura Multi-tenant y rango por Fecha (Optimiza Query C y reportes anuales)
CREATE NONCLUSTERED INDEX IX_Comisiones_Tenant_Estado_Fecha
ON dbo.Comisiones (TenantId, Estado, FechaCreacion)
INCLUDE (UsuarioId, Monto);
GO

-- ÍNDICE 2: Optimización de búsqueda de comisiones pendientes y antigüedad (Optimiza Query B)
CREATE NONCLUSTERED INDEX IX_Comisiones_Estado_Fecha_Tenant
ON dbo.Comisiones (Estado, FechaCreacion, TenantId)
INCLUDE (UsuarioId, Monto);
GO

-- ÍNDICE 3: Garantía de unicidad de liquidación por Tenant y Período
CREATE UNIQUE NONCLUSTERED INDEX IX_Liquidaciones_Tenant_Periodo
ON dbo.Liquidaciones (TenantId, Periodo)
WHERE Estado = 'Aprobada';
GO
