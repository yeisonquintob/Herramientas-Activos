# Limpieza conservadora de código NAVI

Fecha: 2026-07-16  
Estado: aplicada y pendiente únicamente de la validación final registrada al cierre  
Principio: retirar solo artefactos regenerables, duplicados inequívocos o archivos sin carga/consumidor; conservar cualquier elemento cuyo retiro pueda afectar comportamiento.

## Respaldo previo

- Archivo: `backups/NAVI_APARIENCIA_ACCESIBILIDAD_PRE_20260716.tar.gz`
- SHA-256: `d3381469837ee2dc7c614114622306616caa656298511f41176ab505a3a6abd4`
- Contiene el árbol de trabajo previo, incluidos archivos no rastreados, excluyendo `.git`, `bin`, `obj`, cachés y respaldos anidados.

## Elementos retirados

### Copias accidentales no compiladas

- `src/Navi.ToolsAssets.Admin/Components/Layout/MainLayout.razor 2.css`
- `src/Navi.ToolsAssets.Admin/Components/Layout/NavMenu.razor 2.css`
- `src/Navi.ToolsAssets.MobilePwa/Layout/MainLayout.razor 2.css`
- `src/Navi.ToolsAssets.MobilePwa/Layout/NavMenu.razor 2.css`
- `src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/mobile-life-records-v26 2.css`

Los nombres con sufijo ` 2.css` no cumplen la convención de aislamiento `Componente.razor.css`, no estaban enlazados desde HTML y no eran parte de la salida productiva.

### Hojas de estilo obsoletas sin carga

- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-history-web-v25.css`: reemplazada por `assignment-history-web-v26.css`, que es la versión enlazada en `App.razor` y usada por la clase de la vista.
- `src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/home-mobile-form-fixes.css`: no estaba enlazada por `index.html`, componentes ni imports CSS.

### Caché regenerable

- Se eliminó `scripts/ui-audit/__pycache__/`, con 11 archivos `.pyc` detectados durante la auditoría.
- Se añadieron `__pycache__/` y `*.py[cod]` a `.gitignore` para evitar que la caché vuelva a contaminar el árbol.

### Higiene de fuente

- Se retiró un espacio final detectado en `ToolDetail.razor`.
- `git diff --check` finalizó sin incidencias.

## Elementos revisados y conservados

- Todos los componentes `.razor`, incluso los que todavía no tienen consumidores.
- `MobileSpcRequest` y demás páginas extensas o modificadas: la falta de referencias directas no demuestra que sean prescindibles en un proyecto Blazor con rutas y navegación dinámica.
- `MaintenanceDashboardPanel`: se documentó como disponible, sin eliminarlo.
- `NaviMobileFloatingMenu`: se conserva porque es productivo; su tamaño amerita refactor posterior, no eliminación.
- API, controladores, DTO, base de datos, migraciones, rutas, permisos, autenticación y servicios HTTP.
- Marcadores y comentarios históricos: no se hizo una eliminación masiva porque algunos delimitan migraciones visuales vigentes.
- `bin/` y `obj/`: están ignorados y son regenerables, pero se mantuvieron mientras las aplicaciones estaban ejecutándose para no interrumpir pruebas.

## Comprobaciones de seguridad realizadas

- Búsqueda de consumidores por nombre de archivo y clase CSS antes de retirar hojas de estilo.
- Confirmación de la versión productiva `assignment-history-web-v26.css` en `Components/App.razor`.
- Verificación de rutas duplicadas: 0.
- Verificación de variables CSS canónicas sin resolver: 0.
- Verificación de bloques semánticos estrictos de homologación: 0.
- Compilación de Admin, Mobile y API, registrada en los informes de auditoría al finalizar.

## Lo que deliberadamente no se hizo

- No se ejecutó una limpieza automática masiva.
- No se reorganizaron namespaces, carpetas ni nombres públicos.
- No se combinaron componentes parecidos sin una prueba funcional por módulo.
- No se sustituyeron miles de declaraciones `!important` o alturas fijas de una sola vez.
- No se cambió texto funcional, comportamiento de botones, contratos API ni persistencia de datos.

## Próxima limpieza recomendada

La siguiente fase debe ser incremental: seleccionar una vista, migrarla a `FormSection/FormGrid/FormField`, `TableShell`, `StatusBadge` y `Alert`, ejecutar prueba funcional y solo entonces retirar CSS particular que quede sin consumidores. Esto reduce el riesgo de romper layouts que hoy dependen de capas históricas de especificidad.
