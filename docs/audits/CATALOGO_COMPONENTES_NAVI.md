# Catálogo de componentes NAVI

Fecha de actualización: 2026-07-16  
Alcance: Admin Web y Mobile PWA  
Objetivo: explicar el propósito y contrato de cada componente compartido para facilitar mantenimiento, homologación y reutilización sin alterar la lógica funcional.

## Cómo usar este catálogo

- **En uso**: ya tiene consumidores en vistas o layouts.
- **Disponible**: está implementado y compila, pero aún no fue adoptado por vistas productivas.
- **Estructural**: forma parte del arranque, navegación o layout y no se contabiliza como componente de página común.
- Un componente marcado como **Disponible** no es código muerto confirmado. Se conserva porque forma parte del sistema de diseño previsto y debe migrarse de forma gradual.
- Los parámetros indicados son los nombres públicos más relevantes. Los componentes también pueden aceptar contenido mediante `RenderFragment` y, cuando aplica, atributos HTML adicionales.

## Composición recomendada de una vista

1. Encabezado con `NaviPageHero` o `NaviMobilePageHero`.
2. Indicadores con `NaviKpiToolbar`, `NaviKpiGrid` y `NaviKpiCard`; en móvil, usar `NaviMobileKpiOverflow` cuando no caben todos los KPI principales.
3. Filtros y formularios con `NaviFormSection`, `NaviFormGrid` y `NaviFormField` o sus equivalentes móviles.
4. Resultado con `NaviCard`/`NaviTableShell` o sus equivalentes móviles.
5. Estados y mensajes con `NaviStatusBadge`, `NaviAlert` y `NaviAlertStack`.
6. Acciones con `NaviActionButton` o `NaviMobileActionButton`, usando tonos semánticos.

## Admin Web

| Componente | Para qué sirve | Parámetros principales | Uso actual | Recomendación |
|---|---|---|---:|---|
| `MainLayout` | Arma la estructura general del Admin: encabezado, menú lateral, contenido y pie. | No expone parámetros de página. | Estructural | Mantener como único layout raíz del Admin. |
| `NavMenu` | Renderiza navegación por módulos y resalta la ruta activa según permisos. | No expone parámetros públicos. | Estructural | No duplicar menús dentro de páginas. |
| `NaviAdminHeader` | Presenta identidad NAVI, contexto de página, notificaciones y usuario actual. | Datos internos del layout. | Estructural | Mantener fuera de la lógica de cada módulo. |
| `NaviPageHero` | Encabezado homogéneo de página con distintivo, título, descripción y acciones. | `Pill`, `Title`, `Description`, `HeaderContent`, `Actions`, `ChildContent`, `Class`. | 59 archivos | Es el encabezado estándar. No crear encabezados manuales nuevos. |
| `NaviKpiToolbar` | Agrupa la banda de KPI y el botón de actualización con estado de carga y nombre accesible. | `ChildContent`, `Refresh`, `IsRefreshing`, `ShowRefresh`, `RefreshTitle`, `RefreshAriaLabel`, `Class`. | 6 archivos | Usar cuando la vista necesite actualizar los KPI. |
| `NaviKpiGrid` | Distribuye KPI por columnas, con modo compacto y divisores. | `Columns`, `Compact`, `Dividers`, `Class`, `ChildContent`. | 30 archivos | No definir grillas KPI manuales. |
| `NaviKpiCard` | Representa un indicador individual enlazado a valores calculados por la vista. | `Title`, `Value`, `ValueContent`, `Description`, `Subtitle`, `Icon`, `IconContent`, `Tone`, `Compact`, `Class`, `ChildContent`. | 30 archivos; aproximadamente 152 instancias | El valor debe provenir del resultado filtrado de la vista; el componente no calcula negocio. |
| `NaviKpiIcon` | Entrega iconografía SVG normalizada para KPI. | `Name`, `Class`. | 8 archivos; aproximadamente 36 instancias | Preferir claves existentes antes de agregar SVG manual. |
| `NaviActionButton` | Botón o enlace de acción con tamaño, estado de carga, iconos y tono semántico. | `Type`, `Href`, `Tone`, `Size`, `Class`, `Block`, `IconOnly`, `Disabled`, `Loading`, `LoadingText`, `OnClick`, `StartContent`, `ChildContent`, `EndContent`. | 3 archivos; 13 instancias | Tonos: `affirmative`, `operation`, `consult`, `warning`, `danger`, `secondary`, `refresh`, `expand`. Si se usa `IconOnly`, debe existir nombre accesible. |
| `NaviFormSection` | Delimita una sección de formulario con título, descripción, acciones y pie. | `Title`, `Description`, `Compact`, `Flush`, `Class`, `HeaderActions`, `ChildContent`, `FooterContent`. | Disponible | Adoptar gradualmente al intervenir formularios; no migrar masivamente sin prueba funcional. |
| `NaviFormGrid` | Organiza campos en una grilla responsive. | `Columns`, `Compact`, `Class`, `ChildContent`. | Disponible | Reemplaza grillas particulares solo cuando la vista sea modificada. |
| `NaviFormField` | Une etiqueta, control, ayuda, error y validación accesible. | `Label`, `FieldId`, `HelpText`, `ErrorText`, `Required`, `Invalid`, `Disabled`, `FullWidth`, `Compact`, `Class`, `ChildContent`, `ValidationContent`. | Disponible | Candidato prioritario para mejorar formularios y asociaciones `label`/`id`. |
| `NaviTableShell` | Contenedor accesible de tabla con encabezado, contador, acciones, viewport desplazable y pie. | `Title`, `Description`, `CountLabel`, `AriaLabel`, `Compact`, `StickyHeader`, `Flush`, `Class`, `HeaderActions`, `ChildContent`, `FooterContent`. | Disponible | Migrar tablas una por una; el viewport ya tiene `role="region"`, nombre y teclado. |
| `NaviStatusBadge` | Muestra un estado como etiqueta semántica con tono y punto opcional. | `Status`, `Label`, `Size`, `ShowDot`, `Class`, `ChildContent`. | Disponible | Usar el catálogo de estados; evitar colores escritos directamente en páginas. |
| `NaviCard` | Superficie reutilizable con encabezado, acciones, cuerpo y pie. | `Eyebrow`, `Title`, `Description`, `Tone`, `Accent`, `Compact`, `Flush`, `Interactive`, `Selected`, `Class`, `HeaderContent`, `HeaderActions`, `ChildContent`, `FooterContent`. | Disponible | Útil para formularios laterales, resúmenes y paneles. |
| `NaviCardGrid` | Distribuye tarjetas con columnas, densidad y alturas equivalentes. | `Columns`, `Compact`, `EqualHeight`, `Class`, `ChildContent`. | Disponible | Usar junto con `NaviCard`; no para tablas. |
| `NaviAlert` | Mensaje de éxito, información, advertencia o error con región viva y cierre opcional. | `Title`, `Message`, `Tone`, `Icon`, `ShowIcon`, `Compact`, `Dismissible`, `DismissLabel`, `Role`, `Live`, `Class`, `OnDismiss`, `ChildContent`, `Actions`. | Disponible | Preferir sobre mensajes sin `role` o `aria-live`. |
| `NaviAlertStack` | Agrupa alertas y les asigna una región accesible común. | `Compact`, `AriaLabel`, `Class`, `ChildContent`. | Disponible | Usar cuando puedan coexistir varios mensajes. |
| `NaviEmptyState` | Presenta un estado vacío con icono, título y explicación. | `Icon`, `Title`, `Message`, `Class`. | Disponible | Sustituye textos sueltos de “sin registros”. |
| `SettingsCatalogCrud` | Implementa el patrón CRUD reutilizado por catálogos de configuración, incluyendo consulta al endpoint indicado. | `Title`, `Pill`, `Description`, `CountLabel`, `Endpoint`. | 8 rutas | Mantener concentrada aquí la presentación común de catálogos; sus endpoints siguen siendo responsabilidad del módulo. |
| `MaintenanceDashboardPanel` | Panel consolidado de mantenimiento preparado para compartir indicadores y resumen. | No expone parámetros públicos. | Disponible | Revisar su contrato antes de adoptarlo; actualmente no tiene consumidores. |

## Mobile PWA

| Componente | Para qué sirve | Parámetros principales | Uso actual | Recomendación |
|---|---|---|---:|---|
| `MainLayout` | Contiene la estructura base del PWA, navegación y contenido móvil. | No expone parámetros de página. | Estructural | Mantener como layout móvil único. |
| `NavMenu` | Menú Blazor inicial del proyecto. | No expone parámetros públicos. | Disponible | No usar mientras `NaviMobileFloatingMenu` sea el menú productivo; evaluar eliminación en una fase específica. |
| `NaviMobileFloatingMenu` | Menú flotante productivo con navegación, sesión y opciones disponibles en móvil. | Estado y servicios internos. | 2 consumidores; 1.656 líneas | Funciona, pero conviene dividirlo en subcomponentes en una fase independiente con pruebas de navegación. |
| `NaviMobileAppBanner` | Encabezado móvil común con distintivo, título, descripción, volver, inicio y acciones. | `Pill`, `Title`, `Description`, `Class`, `ShowHomeButton`, `ShowBackButton`, `HomeText`, `BackText`, `HomeUrl`, `BackUrl`, `BackFallbackUrl`, `UseBrowserHistoryWhenNoBackUrl`, `Actions`. | 21 archivos | Es el encabezado principal de módulos móviles. |
| `NaviMobilePageHero` | Encabezado de página móvil con acciones primaria/secundaria opcionales. | `Pill`, `Title`, `Description`, `HeaderContent`, `Actions`, `ChildContent`, `Class`, textos, callbacks y estados de ambos botones. | 2 archivos | Unificar gradualmente con `NaviMobileAppBanner` para evitar dos patrones de encabezado. |
| `NaviMobileKpiGrid` | Distribuye KPI móviles por columnas, con densidad y divisores. | `Columns`, `Compact`, `Dividers`, `Class`, `ChildContent`. | 22 archivos; aproximadamente 45 instancias | Es la grilla KPI estándar móvil. |
| `NaviMobileKpiCard` | KPI individual móvil con valor, descripción, icono y tono. | `Title`, `Value`, `ValueContent`, `Description`, `Subtitle`, `Icon`, `IconKey`, `Tone`, `Compact`, `Class`, `ChildContent`. | 22 archivos; aproximadamente 115 instancias | Recibir valores ya filtrados; no incluir cálculos de negocio en el componente. |
| `NaviMobileKpiIcon` | Iconografía SVG normalizada para KPI móviles. | `Name`, `Class`. | Uso interno/directo puntual | Mantener las mismas claves conceptuales que Admin. |
| `NaviMobileKpiOverflow` | Divide KPI principales y adicionales en una expansión accesible para pantallas pequeñas. | `PrimaryCount`, `OverflowCount`, `ButtonLabel`, `PanelLabel`, `Class`, `PrimaryContent`, `OverflowContent`. | 19 archivos | Usar cuando haya más KPI de los que caben en la franja principal. |
| `NaviMobileKpiToolbar` | Banda móvil de KPI con actualización y estado de carga. | `ChildContent`, `Refresh`, `IsRefreshing`, `ShowRefresh`, `RefreshTitle`, `RefreshAriaLabel`, `Class`. | Disponible | Candidato para homologar las vistas móviles con actualización manual. |
| `NaviMobileActionButton` | Botón/enlace móvil con tono, carga, iconos y modo de ancho completo. | `Type`, `Href`, `Tone`, `Size`, `Class`, `Block`, `IconOnly`, `Disabled`, `Loading`, `LoadingText`, `OnClick`, `StartContent`, `ChildContent`, `EndContent`. | 3 archivos; 9 instancias | Mantener nombres accesibles en acciones de solo icono. |
| `NaviMobileFormSection` | Sección visual de formulario móvil. | `Title`, `Description`, `Compact`, `Flush`, `Class`, `HeaderActions`, `ChildContent`, `FooterContent`. | Disponible | Adoptar de forma gradual. |
| `NaviMobileFormGrid` | Grilla responsive de controles móviles. | `Columns`, `Compact`, `Class`, `ChildContent`. | Disponible | En 360 px debe terminar en una columna cuando el contenido lo requiera. |
| `NaviMobileFormField` | Etiqueta, control, ayuda y error accesibles para móvil. | `Label`, `FieldId`, `HelpText`, `ErrorText`, `Required`, `Invalid`, `Disabled`, `FullWidth`, `Compact`, `Class`, `ChildContent`, `ValidationContent`. | Disponible | Candidato prioritario para formularios nuevos. |
| `NaviMobileTableShell` | Contenedor accesible de tabla móvil con viewport horizontal controlado. | `Title`, `Description`, `CountLabel`, `AriaLabel`, `Compact`, `StickyHeader`, `Flush`, `Class`, `HeaderActions`, `ChildContent`, `FooterContent`. | Disponible | Úsese cuando una tabla sea inevitable; para datos simples, preferir tarjetas. |
| `NaviMobileStatusBadge` | Etiqueta de estado móvil con punto y tamaño configurables. | `Status`, `Label`, `Size`, `ShowDot`, `Class`, `ChildContent`. | Disponible | Reemplazar estados coloreados manualmente de forma gradual. |
| `NaviMobileCard` | Superficie móvil reutilizable para resúmenes, formularios o elementos seleccionables. | Mismos parámetros conceptuales de `NaviCard`. | Disponible | Reutilizar en vez de crear paneles particulares. |
| `NaviMobileCardGrid` | Grilla de tarjetas móviles. | `Columns`, `Compact`, `EqualHeight`, `Class`, `ChildContent`. | Disponible | Limitar columnas en pantallas pequeñas. |
| `NaviMobileAlert` | Mensaje móvil accesible con tono, icono, cierre y acciones. | Mismos parámetros conceptuales de `NaviAlert`. | Disponible | Preferir sobre avisos visuales sin anuncio para lector de pantalla. |
| `NaviMobileAlertStack` | Región que agrupa alertas móviles. | `Compact`, `AriaLabel`, `Class`, `ChildContent`. | Disponible | Usar para mensajes concurrentes. |
| `NaviMobileEmptyState` | Estado vacío móvil con distintivo, título, explicación y contenido opcional. | `Pill`, `Title`, `Description`, `Class`, `ChildContent`. | 2 archivos; 3 instancias | Patrón recomendado para listas sin resultados. |

## Reglas de mantenimiento

- No introducir lógica de negocio en componentes visuales. Las páginas/servicios calculan datos; los componentes los representan.
- No crear colores directos en las vistas. Usar tonos y tokens semánticos.
- Todo campo debe tener `id` estable y etiqueta asociada; los archivos deben tener nombre accesible aunque la etiqueta visual sea personalizada.
- Todo botón de solo icono debe incluir un nombre accesible mediante contenido textual oculto o `aria-label`.
- Los KPI deben recibir el subconjunto ya filtrado para aumentar o disminuir con los filtros de la vista.
- Las tablas con desplazamiento deben tener viewport enfocable y `aria-label` descriptivo.
- Antes de retirar un componente sin consumidores, confirmar que no esté previsto por una migración pendiente y realizar una compilación completa de ambos frontends.

## Prioridades técnicas siguientes

1. Adoptar componentes de formularios y tabla en una vista piloto de bajo riesgo.
2. Consolidar `NaviMobileAppBanner` y `NaviMobilePageHero` en un único patrón móvil.
3. Dividir `NaviMobileFloatingMenu` en navegación, sesión y presentación sin cambiar rutas.
4. Añadir validación automática del nombre accesible cuando `IconOnly=true`.
5. Medir nuevamente consumidores después de la migración global y decidir qué componentes disponibles pueden retirarse.
