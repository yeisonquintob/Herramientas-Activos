# Componentes reutilizables NAVI

## Propósito

Este catálogo define qué componente debe reutilizarse antes de agregar marcado o
CSS específico en Admin Web y Mobile PWA. La consolidación es incremental para
conservar el diseño validado, la navegación y la lógica existente.

## Admin Web

| Necesidad | Componente oficial | Responsabilidad |
|---|---|---|
| KPI | `NaviKpiGrid`, `NaviKpiCard`, `NaviKpiIcon` | Estructura, separación, icono, valor y adaptación a Apariencia. |
| KPI con acciones | `NaviKpiToolbar` | KPI y acción de actualización alineada. |
| Botón | `NaviActionButton` | Tono, tamaño, carga, disabled, enlace y foco. |
| Actualizar | `NaviRefreshButton` | Botón circular oficial, estado ocupado y etiqueta accesible. |
| Estado | `NaviStatusBadge` | Color semántico y texto normalizado. |
| Tabla | `NaviDataTable` / `NaviTableShell` | Encabezado, viewport accesible, scroll y pie. |
| Filtros | `NaviFilterBar` | Región de búsqueda compacta y aislada. |
| Buscar | `NaviSearchBox` | Entrada de búsqueda y acción con Enter. |
| Seleccionar | `NaviSelect` | Etiqueta, estado disabled y opciones. |
| Paginación | `NaviPagination` | Navegación anterior/siguiente y resumen. |
| Modal | `NaviModal` | Diálogo accesible con título, cuerpo y pie. |
| Confirmación | `NaviConfirmDialog` | Confirmación reutilizable y tonos semánticos. |
| Vacío | `NaviEmptyState` | Mensaje sin datos. |
| Cargando | `NaviLoadingState` | Estado `aria-live` y `aria-busy`. |
| Archivo | `NaviFileUploader` | Selección accesible y restricciones declarativas. |
| Imagen | `NaviImageViewer` | Carga diferida, texto alternativo y estado vacío. |
| Formulario | `NaviFormField`, `NaviFormGrid`, `NaviFormSection` | Etiquetas, agrupación y validación visual. |
| Tarjeta | `NaviCard`, `NaviCardGrid` | Superficie reutilizable y distribución. |
| Alertas | `NaviAlert`, `NaviAlertStack` | Mensajes y anuncios accesibles. |

`NaviDataTable` conserva `NaviTableShell` como implementación base para no
romper las vistas existentes. `NaviKpiToolbar` utiliza internamente
`NaviRefreshButton`; de esta manera el icono y sus estados se mantienen en una
sola implementación.

## Mobile PWA

Mobile conserva componentes con prefijo `NaviMobile` porque su densidad,
disposición táctil y puntos de quiebre son distintos:

- `NaviMobileKpiGrid`, `NaviMobileKpiCard`, `NaviMobileKpiIcon`.
- `NaviMobileKpiToolbar`, que reutiliza `NaviMobileRefreshButton`.
- `NaviMobileActionButton`.
- `NaviMobileStatusBadge`.
- `NaviMobileTableShell`.
- `NaviMobileEmptyState`.
- `NaviMobileFormField`, `NaviMobileFormGrid`, `NaviMobileFormSection`.
- `NaviMobileCard`, `NaviMobileCardGrid`.
- `NaviMobileAlert`, `NaviMobileAlertStack`.
- `NaviMobileAppBanner` y `NaviMobileFloatingMenu`.

No se deben reemplazar por componentes Web: comparten tokens y semántica, pero
no necesariamente el mismo marcado táctil.

## Reglas de uso

1. Buscar primero por componente y por `data-ntx-component`.
2. Extender parámetros del componente existente antes de duplicar CSS.
3. Mantener CSS aislado cuando una regla pertenezca a un módulo.
4. Usar tokens globales de Apariencia; no fijar colores o tamaños en línea.
5. Proporcionar `aria-label` a controles sin texto visible.
6. Conservar `disabled`, foco visible, teclado, escala de texto y temas.
7. No mover lógica de negocio a componentes visuales.

## Compatibilidad

Las vistas existentes pueden migrarse progresivamente. Los componentes nuevos
no modifican por sí solos el HTML de páginas que todavía usan marcado legado.
Cada migración debe compilar y validarse visualmente antes de retirar el marcado
anterior.
