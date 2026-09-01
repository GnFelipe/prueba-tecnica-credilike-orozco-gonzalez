# Prueba Técnica Credilike — Desarrollador Fullstack Semi-Senior
**Fisapay S.A.S.** | Plataforma Credilike

---

## 📌 Descripción General

Este repositorio contiene la solución oficial a la **Prueba Técnica de Desarrollador Fullstack Semi-Senior** para **Fisapay S.A.S. (Plataforma Credilike)**. 

La solución abarca desde el análisis de negocio y especificación técnica, pasando por el desarrollo del backend en **.NET Core**, diseño de base de datos relacional y consultas de alta performance en **SQL Server**, hasta la construcción del frontend en **Angular Standalone** y análisis crítico del uso de IA.

---

## 🗂️ Estructura de Entregables por Secciones

La prueba se ha estructurado de forma modular y progresiva:

| Sección / Punto | Nombre | Estado | Enlace al Entregable |
| :--- | :--- | :---: | :--- |
| **Punto 0** | Instrucciones Generales y Log IA | 🟢 Completado | [README.md (este archivo)](#-log-de-uso-de-ia-obligatorio) |
| **Punto 1** | Análisis y Especificación (20 pts) | 🟢 Completado | [docs/01-analisis-especificacion.md](file:///d:/pruebas/docs/01-analisis-especificacion.md) |
| **Punto 2** | Backend .NET Core (25 pts) | 🟢 Completado | [src/Backend/](file:///d:/pruebas/src/Backend/) & [docs/02-backend-refactoring.md](file:///d:/pruebas/docs/02-backend-refactoring.md) |
| **Punto 3** | SQL Server y Modelo de Datos (20 pts) | 🟢 Completado | [src/Database/](file:///d:/pruebas/src/Database/) & [docs/03-sql-performance.md](file:///d:/pruebas/docs/03-sql-performance.md) |
| **Punto 4** | Frontend Angular (20 pts) | 🟢 Completado | [src/Frontend/](file:///d:/pruebas/src/Frontend/) & [docs/04-frontend-arquitectura.md](file:///d:/pruebas/docs/04-frontend-arquitectura.md) |
| **Punto 5** | IA y Criterio Técnico (15 pts) | 🟢 Completado | [docs/05-criterio-ia.md](file:///d:/pruebas/docs/05-criterio-ia.md) |

---

## 🚀 Instrucciones de Instalación y Ejecución

### 1. Base de Datos (SQL Server)
1. Abrir SQL Server Management Studio (SSMS) o Azure Data Studio.
2. Ejecutar el script **[`src/Database/01_DDL_Schema.sql`](file:///d:/pruebas/src/Database/01_DDL_Schema.sql)** para crear la base de datos `CredilikeDb` y sus tablas relacionales.
3. Ejecutar las consultas de reporte y optimización en **[`src/Database/02_Queries_Performance.sql`](file:///d:/pruebas/src/Database/02_Queries_Performance.sql)**.

---

### 2. Backend (.NET Core 8)
1. Navegar al directorio de la API Backend:
   ```bash
   cd d:\pruebas\src\Backend\Credilike.Api
   ```
2. Ejecutar la aplicación web:
   ```bash
   dotnet run
   ```
   *La API estará escuchando en `http://localhost:5000` (o `https://localhost:5001`).*
3. Para ejecutar la suite de pruebas unitarias de xUnit:
   ```bash
   cd d:\pruebas\src\Backend\Credilike.Tests
   dotnet test
   ```

---

### 3. Frontend (Angular Standalone)
1. Navegar al directorio del proyecto Angular:
   ```bash
   cd d:\pruebas\src\Frontend\credilike-frontend
   ```
2. Instalar las dependencias de Node.js:
   ```bash
   npm install
   ```
3. Iniciar el servidor de desarrollo local:
   ```bash
   npm start
   # o bien: ng serve
   ```
4. Abrir en el navegador de preferencia la URL:
   `http://localhost:4200`
5. Para ejecutar las pruebas unitarias de Angular (Jasmine/Karma):
   ```bash
   npm test
   ```

---

## 🤖 Log de Uso de IA (Obligatorio)

En cumplimiento estricto del numeral **0.3 Log de uso de IA** de las instrucciones generales, se documenta de forma honesta y transparente cada interacción significativa con asistentes de IA durante la realización de esta prueba:

| Herramienta | Prompt / Consulta resumida | Resultado obtenido | Decisión tomada |
| :--- | :--- | :--- | :--- |
| **Antigravity (Gemini 3.6 Flash)** | Lectura, OCR y análisis completo del documento PDF `Prueba_Tecnica_Desarrollador MID-LEVEL.pdf`. | Extracción precisa del texto, estructura de 9 páginas, rúbrica de 100 puntos y criterios de descalificación. | Aceptado. Se estructuró el plan de trabajo iterativo punto por punto. |
| **Antigravity (Gemini 3.6 Flash)** | Generación del borrador de preguntas de refinamiento al PO y especificación técnica para US-042. | Generó 7 preguntas orientadas a lógica SaaS multi-tenant y la plantilla de spec con contrato de API y criterios Given-When-Then. | Aceptado y ajustado. Se incorporó diagrama Mermaid de secuencia y matriz de campos con `TenantId`, `CreatedAt` y `CreatedBy`. |
| **Antigravity (Gemini 3.6 Flash)** | Construcción del Backend en .NET Core en capas, DTOs, Middleware de errores, Controller, Service y Pruebas Unitarias xUnit, junto al refactoring del Ejercicio 2.2. | Generó las entidades base, endpoints REST protegidos por rol, middleware de error uniforme, 4 xUnit tests y análisis de 9 fallos de seguridad/multi-tenancy. | Aceptado. Se verificó que todas las entidades heredaran de `BaseEntity` (`TenantId`, `CreatedAt`, `CreatedBy`) y que los errores retornaran el JSON consistente. |
| **Antigravity (Gemini 3.6 Flash)** | Diseño del esquema DDL con `TenantId`, PKs, FKs, Índices de alto rendimiento para 50M de filas y resolución de la trampa de rendimiento Query C. | Generó scripts SQL `01_DDL_Schema.sql`, `02_Queries_Performance.sql` y análisis técnico de sargabilidad en `03-sql-performance.md`. | Aceptado. Se confirmó el uso de rangos de fechas sargables para evitar Index Scans y se incluyó `WITH (NOLOCK)` para lectura concurrente. |
| **Antigravity (Gemini 3.6 Flash)** | Desarrollo del componente Standalone en Angular con Signals, badges de estados visuales por color, paginación en servidor, suite de pruebas Karma/Jasmine para `LiquidacionService` y documento de arquitectura. | Generó la aplicación en `src/Frontend/credilike-frontend/` y el diagnóstico técnico de 8,000 registros en `docs/04-frontend-arquitectura.md`. | Aceptado. Se definió la estrategia de paginación en servidor e inmutabilidad visual de la tabla al aprobar. |
| **Antigravity (Gemini 3.6 Flash)** | Revisión del código generado por IA para el desarrollador junior (`CalcularComisiones`), corrección de la división entera y transacciones N+1, y redacción de la reflexión técnica. | Generó el documento `docs/05-criterio-ia.md` resolviendo los 4 fallos del endpoint y detallando el proceso de uso de IA. | Aceptado. Se consolidaron todas las secciones en el `README.md` final. |

---

*Fisapay S.A.S. — Plataforma Credilike | Desarrollador Fullstack Semi-Senior*
