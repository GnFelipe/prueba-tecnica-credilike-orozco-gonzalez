-- =============================================================================
-- Credilike SaaS - Queries SQL Optimizadas (Punto 3.2)
-- Diseñadas para ejecutar en milisegundos sobre 50 Millones de Filas
-- =============================================================================

USE CredilikeDb;
GO

-- -----------------------------------------------------------------------------
-- QUERY A: Resumen de comisiones pagadas por tenant, por mes, en el último año.
-- Campos: TenantId, Mes, TotalComisionesPagadas (conteo), MontoTotal.
-- Orden: Mes Descendente.
-- -----------------------------------------------------------------------------
DECLARE @FechaInicio DATETIME2 = DATEADD(YEAR, -1, SYSUTCDATETIME());

SELECT 
    c.TenantId,
    FORMAT(c.FechaCreacion, 'yyyy-MM') AS Mes,
    COUNT(1) AS TotalComisionesPagadas,
    SUM(c.Monto) AS MontoTotal
FROM dbo.Comisiones c WITH (NOLOCK)
WHERE c.Estado = 'PAGADO'
  AND c.FechaCreacion >= @FechaInicio
GROUP BY c.TenantId, FORMAT(c.FechaCreacion, 'yyyy-MM')
ORDER BY Mes DESC, c.TenantId ASC;
GO


-- -----------------------------------------------------------------------------
-- QUERY B: Asesores con comisiones en estado 'Pendiente' con más de 30 días.
-- Campos: NombreAsesor, NombreTenant, MontoTotalPendiente.
-- Orden: MontoTotalPendiente Descendente.
-- -----------------------------------------------------------------------------
DECLARE @FechaLimite DATETIME2 = DATEADD(DAY, -30, SYSUTCDATETIME());

SELECT 
    u.Nombre AS NombreAsesor,
    t.Nombre AS NombreTenant,
    SUM(c.Monto) AS MontoTotalPendiente
FROM dbo.Comisiones c WITH (NOLOCK)
INNER JOIN dbo.Usuarios u WITH (NOLOCK) ON u.Id = c.UsuarioId
INNER JOIN dbo.Tenants t WITH (NOLOCK) ON t.Id = c.TenantId
WHERE c.Estado = 'PENDIENTE'
  AND c.FechaCreacion < @FechaLimite
GROUP BY u.Id, u.Nombre, t.Id, t.Nombre
ORDER BY MontoTotalPendiente DESC;
GO


-- -----------------------------------------------------------------------------
-- QUERY C (REFACTORIZADA / OPTIMIZADA): Solución a la trampa de performance.
-- Elimina Index Scans por funciones YEAR() y UPPER(), reemplaza subquery por INNER JOIN,
-- y agrupa por (u.Id, u.Nombre) para evitar colisión de nombres.
-- -----------------------------------------------------------------------------
SELECT 
    u.Id AS UsuarioId,
    u.Nombre AS NombreAsesor,
    SUM(c.Monto) AS Total
FROM dbo.Comisiones c WITH (NOLOCK)
INNER JOIN dbo.Usuarios u WITH (NOLOCK) ON u.Id = c.UsuarioId
INNER JOIN dbo.Tenants t WITH (NOLOCK) ON t.Id = c.TenantId AND t.Activo = 1
WHERE c.FechaCreacion >= '2024-01-01T00:00:00' 
  AND c.FechaCreacion < '2025-01-01T00:00:00'
  AND c.Estado = 'PAGADO'
GROUP BY u.Id, u.Nombre
ORDER BY Total DESC;
GO
