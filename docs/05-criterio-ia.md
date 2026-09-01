# Punto 5: IA y Criterio Técnico (15 pts)

## 5.1 Revisión de Código Generado por IA (8 pts)

### Código Ingestado (Generado por Asistente de IA para Desarrollador Junior)

```csharp
[HttpPost("calcular")]
public async Task<IActionResult> CalcularComisiones([FromBody] ComisionRequest request)
{
 var comisiones = await _context.Comisiones
 .Where(c => c.PeriodoId == request.PeriodoId)
 .ToListAsync();
 foreach (var comision in comisiones)
 {
 comision.MontoFinal = comision.Monto * request.Porcentaje / 100;
 comision.Estado = "calculado";
 comision.FechaCalculo = DateTime.Now;
 await _context.SaveChangesAsync();
 }
 return Ok(new { message = "Comisiones calculadas", total = comisiones.Count });
}
```

---

### Diagnóstico de Problemas Clasificados por Categoría

1. **Performance Crítico (`SaveChangesAsync` dentro de Bucle `foreach`):**
   - **Fallo:** Invocar `await _context.SaveChangesAsync()` dentro del bucle `foreach` realiza N llamadas I/O de red y N transacciones individuales a la base de datos (problema N+1).
   - **Consecuencia:** Para 10,000 comisiones, se ejecutan 10,000 conexiones y escrituras individuales, congelando la ejecución por minutos.

2. **Seguridad y Multi-Tenancy (Ausencia de `TenantId`):**
   - **Fallo:** La consulta `_context.Comisiones.Where(c => c.PeriodoId == request.PeriodoId)` no filtra por `TenantId`.
   - **Consecuencia:** Un usuario del Tenant A puede procesar y alterar las comisiones de los Tenants B, C y D si comparten el mismo `PeriodoId`.

3. **Inprecisión Numérica (División Entera en C#):**
   - **Fallo:** La expresión `request.Porcentaje / 100` realizará una división entera si ambos operandos son enteros. Ej: `15 / 100` resulta en `0`, multiplicando el monto por cero y truncando las comisiones.
   - **Consecuencia:** Pérdida de dinero y errores graves en la liquidación de comisiones.

4. **Buenas Prácticas y Auditoría (`DateTime.Now` vs `DateTime.UtcNow`):**
   - **Fallo:** Usar `DateTime.Now` guarda la hora local del servidor web. Si los servidores están en zonas horarias distintas o en la nube (UTC), los registros quedan desincronizados.

---

### Código Corregido y Refactorizado

```csharp
[HttpPost("calcular")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> CalcularComisiones([FromBody] ComisionRequest request)
{
    // 1. Extraer TenantId del contexto de seguridad JWT del usuario autenticado
    int tenantId = GetTenantIdFromUser();

    if (request.Porcentaje <= 0 || request.Porcentaje > 100)
    {
        return BadRequest(new { codigo = "INVALID_PERCENTAGE", mensaje = "El porcentaje debe estar entre 0.1 y 100." });
    }

    // 2. Consulta con aislamiento estricto por TenantId
    var comisiones = await _context.Comisiones
        .Where(c => c.TenantId == tenantId && c.PeriodoId == request.PeriodoId && c.Estado == "Pendiente")
        .ToListAsync();

    if (!comisiones.Any())
    {
        return Ok(new { message = "No hay comisiones pendientes para procesar", total = 0 });
    }

    DateTime fechaProcesamiento = DateTime.UtcNow;
    decimal factorPorcentaje = (decimal)request.Porcentaje / 100m; // Uso explícito de decimales

    foreach (var comision in comisiones)
    {
        comision.MontoFinal = comision.Monto * factorPorcentaje;
        comision.Estado = "Calculado";
        comision.FechaCalculo = fechaProcesamiento;
    }

    // 3. FIX PERFORMANCE: Guardado masivo fuera del bucle en una única transacción atómica
    await _context.SaveChangesAsync();

    return Ok(new { message = "Comisiones calculadas exitosamente", total = comisiones.Count });
}
```

---

### Reflexión Técnica (105 palabras)

> **¿Qué riesgos concretos tiene usar código generado por IA sin revisión en una plataforma que maneja pagos de comisiones?**
> 
> Utilizar código generado por IA sin la revisión de un desarrollador experimentado representa un **riesgo financiero y de seguridad crítico**. La IA tiende a generar código superficial funcionalmente aparente pero ignorante de contextos complejos como el **aislamiento multi-tenant**, **transaccionalidad** y **precisión numérico-contable**. 
> 
> En un sistema de liquidación de comisiones, un error como la división entera o la omisión del filtro `TenantId` puede causar la pérdida masiva de datos, pagar saldos erróneos a asesores o filtrar información confidencial entre empresas clientes. La IA es una excelente herramienta de productividad, pero la responsabilidad técnica y el criterio de arquitectura son indelegables.

---

## 5.2 Tu Proceso con IA (7 pts)

### Análisis del Proceso en la Construcción de la Prueba

Para el desarrollo del **Backend (.NET Core)** y el **Esquema SQL Server**, me apoyé en la herramienta **Antigravity (Gemini 3.6 Flash)** como asistente de código.

1. **Uso Concreto de la IA:**
   - La IA fue utilizada para generar el *boilerplate* inicial de la arquitectura en capas (Controllers, DTOs, Entidades) y estructurar las sintaxis DDL de SQL Server en milisegundos.

2. **Detección de Sugerencias Incorrectas o No Aplicables:**
   - Al proponer el esquema de base de datos inicial, la IA sugirió usar tipos de datos `FLOAT` para la columna `MontoComision` y omitir el campo `TenantId` en la tabla de detalle `LiquidacionDetalles` asumiendo que bastaba con tenerlo en la cabecera.
   - **¿Cómo me di cuenta?** Detecté inmediatamente que `FLOAT` introduce imprecisiones de coma flotante en cálculos de dinero y que omitir `TenantId` en las tablas hijas rompe la consulta indexada de aislamiento directo multi-tenant. Corregí la sugerencia a `DECIMAL(18,2)` e incluí `TenantId` en todas las entidades requeridas por el estándar SaaS.
