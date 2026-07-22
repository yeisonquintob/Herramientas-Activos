# NAVI — Fase 2: plan e inventario de migración global

## Inventario real analizado

El inventario se obtuvo del árbol fuente, no del archivo exportado. El export consolidado se usó como referencia histórica y el repositorio actual como autoridad.

| Proyecto | Razor | Rutas `@page` | CSS fuente | JavaScript fuente | Rutas duplicadas |
|---|---:|---:|---:|---:|---:|
| Admin Web | 96 | 71 | 59 | 3 | 0 |
| Mobile PWA | 50 | 38 | 25 | 6 | 0 |

Familias Admin incluidas: inicio/dashboard, inventario, detalle y hoja de vida, asignación e historial, disponibilidad, solicitudes SPC/compra/mantenimiento, aprobaciones, mantenimiento, documentos, reportes, toma física, conciliación y configuración.

Familias Mobile incluidas: principal, dashboard, inventario y detalle, asignación, disponibilidad, historial, hojas de vida, solicitudes, mantenimiento, documentos, reportes, toma física, conciliación y configuración.

## Restricciones de ejecución

- No modificar lógica funcional, rutas, permisos, consultas, DTO, filtros o eventos.
- No crear Git branch, commit ni push.
- No iniciar Admin ni Mobile.
- Mantener persistencia independiente.
- Reutilizar las expresiones filtradas existentes para cada KPI.

## Lotes ejecutados

| Lote | Alcance | Estado |
|---|---|---|
| 0 | Auditoría base, rutas, hosts, preferencias y carga CSS | Completado |
| 1 | Tokens, escalas, temas, contraste y alias | Completado |
| 2 | Botones y acciones semánticas | Completado |
| 3 | KPI Admin compartidos | Completado |
| 4 | KPI Mobile y overflow 3 + restantes | Completado |
| 5 | Encabezados Mobile persistentes | Completado |
| 6 | Colores, tipografía, Bootstrap cromático, `!important` y overflow de página | Completado |
| 7 | Gráficas y estilos dinámicos | Completado con excepciones justificadas |
| 8 | Auditoría estricta, integridad y builds | Completado |
| 9 | Matriz visual interactiva | Pendiente por instrucción de no iniciar aplicaciones |

## Estrategia aplicada

1. Crear el núcleo global antes de tocar vistas.
2. Reemplazar decisiones cromáticas locales por tokens semánticos.
3. Clasificar acciones por intención, sin cambiar el evento asociado.
4. Sustituir barras KPI por componentes compartidos y mantener exactamente sus expresiones.
5. En Mobile, dejar tres KPI en la franja principal y pasar el resto a `NaviMobileKpiOverflow`.
6. Conservar estilos inline solo cuando representan un valor de datos imposible de expresar mediante una clase estática.
7. Validar estructura, variables, rutas, auditoría estricta y compilación.

## Matriz de validación prevista

| Superficie | Anchos | Escalas | Temas |
|---|---|---|---|
| Admin Web | 1366, 1920 px | compact, normal, large, extra-large | normal, cold, warm, dark |
| Mobile PWA | 360, 390, 768 px | compact, normal, large, extra-large | normal, cold, warm, dark |

En cada combinación se debe verificar: encabezado visible, ausencia de solapamiento y scroll horizontal, botones legibles, KPI correctos, tablas contenidas, paneles estables, inputs utilizables y tema oscuro completo. Esta matriz requiere ejecución manual posterior.

## Criterios automáticos

El auditor estricto bloquea: color directo fuera de archivos permitidos, color de gráfica rígido, `font-size` fijo de vista, `!important`, bloque `<style>`, estilo inline no justificado, botón sin tono, tono de acción/KPI no canónico, incongruencia texto-tono, KPI manual, Mobile con más de tres KPI sin overflow, encabezado móvil ausente, `overflow-x: hidden`, clases cromáticas Bootstrap y preferencias duplicadas.
