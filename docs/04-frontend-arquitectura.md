# Ejercicio 4.2: Análisis de Arquitectura Frontend

## Situación Planteada
Un componente de liquidaciones funciona fluido en desarrollo con 50 registros, pero en producción un cliente con **8,000 liquidaciones** experimenta lentitud extrema. Se proponen dos alternativas:
1. Reescribir el componente en JavaScript puro (Compañero).
2. Comprar un servidor más potente (Líder Técnico).

---

## Respuesta y Diagnóstico Técnico (Máx. 250 palabras)

### 1. Causa Real del Problema
La causa del problema **no es Angular ni la potencia del servidor**, sino un antipatrón de arquitectura web: **intentar descargar y renderizar 8,000 registros simultáneamente en el DOM del cliente**. Insertar miles de nodos HTML de golpe provoca una sobrecarga masiva de memoria en el navegador (*DOM Footprint*) y un bloqueo continuo del hilo principal durante la fase de *Reflow* y *Repaint*.

---

### 2. Evaluación de las Propuestas
**No estoy de acuerdo con ninguna de las dos propuestas:**
- **JS Puro:** Es incorrecto. El cuello de botella lo genera el renderizado de 8,000 elementos en el motor del navegador (`HTMLTableElement`); reescribir en JS Vanilla sufrirá el mismo congelamiento de pantalla.
- **Servidor más potente:** Es un gasto innecesario. El servidor entrega el JSON correctamente; el fallo ocurre en el cliente al procesarlo y dibujarlo.

---

### 3. Solución Propuesta (¿Qué haría yo?)
Implementaría una solución de rendimiento en dos capas:

1. **Paginación en Servidor (*Server-Side Pagination*):** Modificar el API para retornar únicamente páginas de 20 a 50 registros por petición (`page`, `pageSize`), reduciendo la carga de red y el consumo de RAM.
2. **Virtual Scrolling o Detección de Cambios Optimizada:** Si el negocio exige un scroll infinito sin páginas, utilizaría `@angular/cdk/scrolling` (`cdk-virtual-scroll-viewport`), el cual renderiza dinámicamente solo las 15 filas visibles en pantalla. Adicionalmente, aplicaría `ChangeDetectionStrategy.OnPush` y la función `trackBy` por `id`.
