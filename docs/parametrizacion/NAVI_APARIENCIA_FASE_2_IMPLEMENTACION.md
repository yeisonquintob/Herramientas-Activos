# NAVI — Fase 2: cierre de migración global de vistas

## Resultado ejecutivo

La presentación de Admin Web y Mobile PWA quedó conectada al núcleo global de tamaño y tema. La auditoría estricta termina aprobada con cero hallazgos bloqueantes, y ambos proyectos compilan con cero advertencias y cero errores.

No se modificaron rutas, permisos, API, filtros, consultas, DTO ni reglas de negocio. No se usó Git y no se iniciaron las aplicaciones.

## Vistas migradas

- Admin: 96 archivos Razor revisados, 71 rutas cubiertas.
- Mobile: 50 archivos Razor revisados, 38 rutas cubiertas.
- 46 archivos Razor consumen componentes KPI compartidos.
- 17 vistas Mobile con más de tres indicadores, más el panel de vista previa, usan `NaviMobileKpiOverflow`.
- Todos los archivos Mobile con ruta funcional publican un encabezado estándar o pertenecen a la lista explícita de páginas exentas de autenticación/prueba.

Vistas no migradas: ninguna dentro del alcance de presentación. Las métricas auxiliares que forman parte interna de una gráfica o tarjeta analítica no se convirtieron en una segunda barra KPI; sí consumen tokens globales.

## KPI y filtros

Las barras manuales de asignación, historial, disponibilidad, detalle, dashboard, historial móvil y herramientas personales fueron sustituidas por `NaviKpiToolbar`, `NaviKpiGrid`, `NaviKpiCard`, `NaviMobileKpiGrid`, `NaviMobileKpiCard` y `NaviMobileKpiOverflow`.

Los valores continúan invocando las colecciones y funciones existentes —por ejemplo `FilteredTools`, `FilteredHistory`, `FilteredItems`, `FilteredPlans` y los cálculos originales de cada módulo—. Por eso aumentan o disminuyen con los filtros sin introducir estado duplicado.

Mobile muestra tres indicadores principales. Cuando hay más, la pestaña informa exactamente cuántos quedan y abre el contenido adicional; se cierra con Escape o interacción exterior.

## Acciones semánticas

Todos los botones detectados tienen tono canónico. Se corrigieron además 18 asociaciones inconsistentes encontradas por revisión semántica, incluyendo `Filtrar`, `Limpiar`, `Regresar`, `Actualizar`, `Reintentar`, habilitar y deshabilitar. Solo cambió `data-ntx-tone` o `Tone`; cada `@onclick`, `href`, `disabled` y `type` se conservó.

Tonos de acciones: `affirmative`, `refresh`, `operation`, `warning`, `danger`, `consult`, `secondary`, `expand`, `ghost`.

Tonos KPI: `neutral`, `positive`, `warning`, `danger`, `soft`.

## CSS retirado o normalizado

| Deuda inicial | Inicial | Final bloqueante |
|---|---:|---:|
| Colores directos | 3.050 | 0 |
| Colores directos en gráficas | 518 | 0 |
| Botones sin tono | 232 | 0 |
| `!important` | 159 | 0 |
| Clases cromáticas Bootstrap | 95 | 0 |
| Tonos no canónicos | 95 | 0 |
| KPI manual detectado | 92 | 0 |
| `font-size` fijo en CSS de vista | 75 | 0 |
| `style` inline total | 34 | 0 no justificados |
| `overflow-x: hidden` | 31 | 0 |
| Encabezado Mobile ausente | 2 | 0 |

No existían bloques `<style>` al comienzo de estas dos fases, porque habían sido retirados en el trabajo visual anterior; el cierre confirma que siguen en cero.

## Archivos modificados

La migración mecánica procesó 113 archivos de presentación, seguida de revisión manual. Las familias principales son:

- `src/Navi.ToolsAssets.Admin/Components/**/*.razor` y `*.razor.css`.
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/*.css`.
- `src/Navi.ToolsAssets.MobilePwa/Pages/*.razor` y `*.razor.css`.
- `src/Navi.ToolsAssets.MobilePwa/Components/Shared/*.razor`.
- `src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/*.css`.
- Hosts `Components/App.razor` y `wwwroot/index.html`.
- Preferencias JS de Admin/Mobile y el nuevo controlador de overflow KPI.
- Scripts e informes de `scripts/ui-audit/` y `docs/parametrizacion/`.

Archivos centrales: `tokens.css`, `base.css`, `components.css`, `utilities.css`, `kpi-standard.css`, `layouts-admin.css`, `layouts-mobile.css`, `navi-ui-preferences.css`, `navi-mobile-ui-preferences.css` y `theme-admin-cold.css`.

## Gráficas y tema

Los arreglos y segmentos visuales consumen `--navi-chart-1` a `--navi-chart-6` o estilos resueltos desde tokens. El evento `navi:appearancechange` queda disponible para redibujado. No quedan hexadecimales de gráfica fuera de los archivos de tema permitidos.

Los pares principales de botones fueron verificados matemáticamente: contraste mínimo 4,50:1. El tema oscuro redefine superficies, texto, bordes, acciones y gráficas, no solamente el fondo de la página.

## Excepciones justificadas

Responsable: Codex. Fecha: 2026-07-13. Motivo común: estos estilos expresan porcentajes, ángulos o anchuras calculados con datos en tiempo de ejecución. Reemplazarlos por clases estáticas requeriría discretizar el dato o cambiar la lógica de la gráfica. Plan: mantenerlos aislados y reevaluarlos si el proyecto adopta SVG/canvas con propiedades tipadas.

| Archivo | Líneas | Uso |
|---|---|---|
| `Admin/Components/Pages/Home.razor` | 323, 329, 335 | Barras por sede |
| `Admin/Components/Pages/Home.razor` | 400, 411, 422, 434, 447 | Segmentos de anillo operativo |
| `Admin/Components/Pages/Home.razor` | 533, 544, 620, 627 | Anillos de identificación y hojas de vida |
| `Admin/Components/Pages/Home.razor` | 706, 728, 841, 906 | Costos y barras de progreso |
| `Admin/Components/Pages/Modules/PhysicalCountDetail.razor` | 207 | Avance del participante |
| `Admin/Components/Pages/TechnicalLifeRecord.razor` | 239 | Porcentaje de vida útil |
| `Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor` | 170 | Porcentaje de vida útil de solo lectura |
| `MobilePwa/Pages/MobileDashboard.razor` | 181, 186, 191 | Barras por sede |
| `MobilePwa/Pages/MobileDashboard.razor` | 241, 249, 257, 265 | Anillo operativo |
| `MobilePwa/Pages/MobileDashboard.razor` | 332, 340, 395, 403 | Identificación y hojas de vida |
| `MobilePwa/Pages/MobileDashboard.razor` | 490, 540 | Progreso de sede y mantenimiento |
| `MobilePwa/Pages/MobileLifeRecordView.razor` | 98 | Porcentaje de vida útil |

Total: 33 estilos dinámicos justificados. El auditor los clasifica como `inline_style_dynamic`; cualquier nuevo estilo inline que no cumpla el patrón permitido vuelve a bloquear el modo estricto.

## Deuda de revisión no bloqueante

El auditor señala 1.251 declaraciones de altura fija. La mayoría corresponde a iconos, líneas, controles, miniaturas o geometrías de gráficas. No se eliminaron mecánicamente porque hacerlo sin ejecución visual podría deformar componentes existentes. En la matriz interactiva deben priorizarse las alturas aplicadas a contenedores de texto con escala `extra-large`.

## Auditoría e integridad

- Auditoría estricta: **APROBADA**.
- Hallazgos bloqueantes: 0.
- Observaciones: 1.251 alturas para revisión y 33 estilos de datos justificados.
- Variables canónicas sin definición: 0.
- Desbalance de delimitadores CSS/JS: 0.
- Rutas duplicadas: 0.
- Carga duplicada de preferencias: 0.
- Patrones corruptos `navi-navi`, `ntx-ntx` o inserción Razor inválida: 0.

Informes:

- `docs/parametrizacion/NAVI_UI_AUDIT_INITIAL_SUMMARY.md`
- `docs/parametrizacion/NAVI_UI_AUDIT_FINAL.json`
- `docs/parametrizacion/NAVI_UI_AUDIT_FINAL.md`

## Compilación final

| Proyecto | Puerto previo | Resultado | Advertencias | Errores | Tiempo |
|---|---|---|---:|---:|---:|
| Admin Web | 5264 detenido | Correcta | 0 | 0 | 24,77 s |
| Mobile PWA | 5285 detenido | Correcta | 0 | 0 | 29,87 s |

## Matriz visual

| Superficie | 360 | 390 | 768 | 1366 | 1920 |
|---|---|---|---|---|---|
| Reglas responsive y sin `overflow-x: hidden` | Validación estática | Validación estática | Validación estática | Validación estática | Validación estática |
| Prueba interactiva en navegador | Pendiente | Pendiente | Pendiente | Pendiente | Pendiente |

Las combinaciones de cuatro escalas y cuatro temas están implementadas en CSS. La aprobación visual interactiva no se ejecutó porque la Fase 1 exige no iniciar aplicaciones. Debe hacerse posteriormente con datos y permisos representativos; no se presenta como validada.

## Confirmaciones

- Lógica funcional modificada: **No**.
- Permisos modificados: **No**.
- API modificada: **No**.
- Rutas modificadas: **No**.
- Preferencias Admin/Mobile independientes: **Sí**.
- Persistencia tras recarga implementada: **Sí**.
- Git, rama, commit o push: **No utilizados**.
