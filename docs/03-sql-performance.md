# Punto 3: SQL Server y Modelo de Datos (20 pts)

## 3.1 Diseño de Esquema Multi-Tenant (10 pts)

### Script DDL
El script de creación del esquema completo se encuentra disponible en [`src/Database/01_DDL_Schema.sql`](file:///d:/pruebas/src/Database/01_DDL_Schema.sql).

---

### Explicación de Decisiones de Diseño

1. **Uso de `DECIMAL(18,2)` en lugar de `FLOAT` / `REAL` para valores monetarios:**
   - **Razón:** Los tipos flotantes (`FLOAT`/`DOUBLE`) son números de coma flotante binaria que sufren imprecisiones de redondeo. En plataformas de gestión de liquidaciones y comisiones financieras, el cálculo debe ser exacto hasta el último centavo para evitar descuadres contables y discrepancias auditables. `DECIMAL(18,2)` es un tipo numérico exacto de punto fijo.

2. **Creación del Índice de Cobertura `IX_Comisiones_Tenant_Estado_Fecha`:**
   - **Razón:** Para tablas masivas (50 millones de filas), las consultas frecuentes filtran por `TenantId`, `Estado` y rango de fechas. Al incluir las columnas `UsuarioId` y `Monto` mediante la cláusula `INCLUDE`, el motor de SQL Server satisface la consulta leyendo únicamente las páginas del índice secundario (Index Seek), evitando costosas búsquedas adicionales por clave en la tabla principal (*Key Lookups*).

---

### Estrategia Multi-Tenant SaaS (Respuesta Breve)

> **¿Por qué `TenantId` en todas las tablas es la estrategia más práctica para SaaS?**
> 
> Mantener `TenantId` como columna en un esquema de base de datos compartida (*Shared Database, Shared Schema*) ofrece el balance óptimo entre **costo operativo y escalabilidad**. Permite administrar miles de clientes en una única instancia reduciendo gastos de infraestructura y facilitando migraciones/mantenimiento centralizado, al tiempo que garantiza el aislamiento lógico estricto mediante índices compuestos `(TenantId, ...)` y filtros globales en la capa de aplicación.

---

## 3.2 Queries y Performance sobre 50 Millones de Filas (10 pts)

### Query A: Resumen Anual por Mes de Comisiones Pagadas por Tenant

```sql
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
```

#### Plan de Ejecución Esperado y Optimización (50M de filas)
- **Plan sin Índice:** Full Table Scan de 50 millones de filas, costando decenas de segundos e I/O masivo.
- **Plan Optimizado:** Al contar con el índice `IX_Comisiones_Tenant_Estado_Fecha`, SQL Server realiza un **Index Seek** filtrando directamente las filas del último año en estado 'PAGADO'. El orden por fecha permite realizar la agregación de manera eficiente en memoria sin derrames a `tempdb`.

---

### Query B: Asesores con Comisiones Pendientes (> 30 Días)

```sql
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
```

#### Plan de Ejecución Esperado y Optimización
- **Estrategia:** SQL Server utiliza el índice `IX_Comisiones_Estado_Fecha_Tenant` para realizar un **Index Seek** seleccionando solo las comisiones 'PENDIENTE' antiguas. Luego efectúa un **Hash Match** o **Nested Loops Join** directo con las tablas de dimensión `Usuarios` y `Tenants` por su Clustered Primary Key `Id`.
- **Rendimiento:** Filtra el conjunto de 50M de filas a solo las pendientes (>30 días) en milisegundos antes de realizar las uniones.

---

### Query C — Análisis de Antipatrones y Solución (Trampa de Performance)

#### Query Original en Producción (Causante de Timeouts):
```sql
SELECT u.Nombre, SUM(c.Monto) as Total
FROM Comisiones c
INNER JOIN Usuarios u ON u.Id = c.UsuarioId
WHERE YEAR(c.FechaCreacion) = 2024
 AND UPPER(c.Estado) = 'PAGADO'
 AND c.TenantId IN (SELECT Id FROM Tenants WHERE Activo = 1)
GROUP BY u.Nombre
ORDER BY Total DESC
```

---

#### 🚨 Diagnóstico de Antipatrones Identificados

1. **Uso de Función Escalar `YEAR(c.FechaCreacion)` en Cláusula `WHERE` (No Sargable):**
   - **Impacto:** Al envolver la columna `FechaCreacion` en la función `YEAR()`, SQL Server no puede utilizar ningún índice existente sobre esa columna. Se ve obligado a evaluar la función fila por fila sobre las 50 millones de registros (**Full Clustered Index Scan**), provocando tiempo de CPU excesivo y timeouts.
   - **Solución:** Reemplazar por un filtro de rango de fechas sargable: `c.FechaCreacion >= '2024-01-01' AND c.FechaCreacion < '2025-01-01'`.

2. **Uso de Función `UPPER(c.Estado)` (No Sargable):**
   - **Impacto:** Invalida el índice sobre la columna `Estado` al forzar el cálculo en tiempo de ejecución para cada fila de la tabla.
   - **Solución:** Almacenar los estados de forma estandarizada en mayúsculas o utilizar la intercalación de base de datos (*Collation Case-Insensitive* `CI_AS`) con la igualdad directa `c.Estado = 'PAGADO'`.

3. **Subconsulta Ineficiente `c.TenantId IN (SELECT Id FROM Tenants WHERE Activo = 1)`:**
   - **Impacto:** Aunque el optimizador puede convertir el `IN` en un Semi-Join, realizar esta consulta dentro de un subselect genera planes de ejecución menos eficientes y evita la simplificación de predicados.
   - **Solución:** Unir explícitamente la tabla mediante `INNER JOIN Tenants t ON t.Id = c.TenantId AND t.Activo = 1`.

4. **Agrupamiento Inseguro Únicamente por `u.Nombre` (`GROUP BY u.Nombre`):**
   - **Riesgo de Negocio:** Si dos asesores diferentes en el sistema comparten el mismo nombre (ej. "Juan Pérez"), la query combinará erróneamente sus montos de comisión en una sola fila.
   - **Solución:** Agrupar por la clave primaria del usuario y su nombre: `GROUP BY u.Id, u.Nombre`.

---

#### 🟢 Query C Reescrita y Optimizada

```sql
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
```

---

#### Comparativa de Desempeño Estimado (En Tabla de 50M Filas)

| Métrica | Query Original (Sin Optimizar) | Query Reescrita (Optimizada) | Mejora Obtenida |
| :--- | :--- | :--- | :--- |
| **Operación Principal** | Clustered Index Scan (50,000,000 filas) | Nonclustered Index Seek (~2,500,000 filas) | **95% menos I/O** |
| **Tiempo de Ejecución** | > 30 - 60 segundos (Timeout) | < 300 milisegundos | **~200x más rápida** |
| **Uso de CPU** | Muy Alto (Evaluación de `YEAR` y `UPPER`) | Mínimo (Uso directo de índices B-Tree) | **Reducción > 90%** |
| **Integridad de Datos** | Riesgo de colisión por nombres duplicados | Garantizada (Agrupa por `u.Id, u.Nombre`) | **100% Exacta** |
