# NAVI — Fase 1: núcleo global de apariencia

## Resultado

Se consolidó un núcleo visual parametrizable e independiente para Admin Web y Mobile PWA. La configuración permite seleccionar cuatro escalas tipográficas y cuatro temas sin modificar rutas, permisos, eventos, consultas HTTP, DTO, filtros ni reglas de negocio.

No se creó rama, commit ni cambio remoto. No se iniciaron las aplicaciones.

## Contrato implementado

| Proyecto | Persistencia | Atributo de escala | Atributo de tema |
|---|---|---|---|
| Admin | `navi-admin-ui-preferences-v1` | `data-navi-font` | `data-navi-theme` |
| Mobile | `navi-mobile-ui-preferences-v1` | `data-navi-mobile-font` | `data-navi-mobile-theme` |

Valores de escala conservados: `compact` 0,92; `normal` 1; `large` 1,10; `extra-large` 1,20. Temas conservados: `normal`, `cold`, `warm` y `dark`.

El JavaScript aplica la preferencia antes de Blazor, valida valores persistidos, se recupera si `localStorage` no está disponible, actualiza `color-scheme` y publica `navi:appearancechange` para consumidores visuales como gráficas.

## Tokens

Los archivos canónicos son:

- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/tokens.css`
- `src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/tokens.css`

Contienen tokens para página, superficies, texto, bordes, foco, backdrop, seis colores de gráficas, acciones semánticas, tipografía semántica y escalas de espacio/control. Admin y Mobile conservan escalas tipográficas independientes.

Los alias históricos `--ntx-*` apuntan a los tokens `--navi-*`; el núcleo nuevo no depende de los alias antiguos. Se añadió `--ntx-space-7`, detectado como la única referencia sin definición durante la validación estructural.

Los fondos sólidos fueron ajustados para contraste. Los pares principales validados quedan entre 4,50:1 y 6,67:1; advertencia usa texto oscuro cuando el fondo naranja no permite texto blanco suficiente.

## Componentes compartidos

- `NaviActionButton` y `NaviMobileActionButton`: tonos `affirmative`, `refresh`, `operation`, `warning`, `danger`, `consult`, `secondary`, `expand` y `ghost`, con alias compatibles.
- `NaviKpiGrid`, `NaviKpiCard`, `NaviKpiToolbar` y sus equivalentes Mobile: estructura KPI real usada también por las vistas previas.
- `NaviMobileKpiOverflow`: mantiene tres KPI principales y presenta los restantes en una pestaña accesible; cierra con Escape, foco o pulsación exterior.
- `NaviKpiIcon`: iconografía reutilizable del estándar.

El overflow móvil no carga datos ni controla encabezados; solo recibe fragmentos y conteos. Por tanto, no introduce una segunda fuente de estado funcional.

## Configuración de apariencia

Admin mantiene la ruta `/settings/appearance`. Mobile conserva Apariencia dentro de `/mobile/settings`, debajo de Seguridad personal y en un panel cerrado inicialmente.

Las vistas previas usan componentes KPI y botones reales. Las cuatro muestras tipográficas tienen tokens propios para representar correctamente 0,92/1/1,10/1,20 sin valores rígidos en la vista.

## Orden de carga

Admin:

1. Preferencias JS previas al render.
2. Bootstrap.
3. Tokens y base.
4. CSS del host y compatibilidad de tema.
5. CSS aislado.
6. Layouts, componentes y utilidades.
7. KPI y hojas específicas.
8. Preferencias CSS como capa final parametrizable.

Mobile sigue el mismo orden conceptual, con `layouts-mobile.css`, encabezado móvil, módulos Mobile y `navi-mobile-ui-preferences.css` al final. Los recursos modificados usan versión de caché `20260713_05`.

## Archivos creados

- `src/Navi.ToolsAssets.MobilePwa/Components/Shared/NaviMobileKpiOverflow.razor`
- `src/Navi.ToolsAssets.MobilePwa/wwwroot/js/navi-mobile-kpi-overflow.js`
- `scripts/ui-audit/audit_ui_preferences.py`
- `scripts/ui-audit/validate_ui_source_integrity.py`
- `scripts/ui-audit/migrate_ui_preferences_phase2.py`
- `scripts/ui-audit/replace_manual_kpis_phase2.py`
- Informes bajo `docs/parametrizacion/`.

## Archivos principales modificados

- Hosts `Components/App.razor` e `wwwroot/index.html`.
- Tokens, `base.css`, `components.css`, `utilities.css`, `kpi-standard.css`, layouts y capas de preferencias de ambos proyectos.
- `theme-admin-cold.css`, ahora como compatibilidad estructural y no como paleta fija.
- Componentes de acciones, KPI y paneles de apariencia.

La migración posterior alcanzó CSS y Razor de presentación; no se modificaron servicios, controladores, clientes HTTP ni proyectos de dominio.

## Validación

- Admin: compilación correcta, 0 advertencias, 0 errores; 24,77 s.
- Mobile: compilación correcta, 0 advertencias, 0 errores; 29,87 s.
- Puertos 5264 y 5285 detenidos antes de los builds finales.
- 224 definiciones CSS canónicas en Admin y 229 en Mobile; 0 referencias canónicas sin definición.
- 71 rutas Admin y 38 rutas Mobile; 0 rutas duplicadas.
- Auditoría estricta: aprobada con 0 hallazgos bloqueantes.

## Riesgos y deuda

- La matriz visual interactiva sigue pendiente porque los documentos ordenan no iniciar aplicaciones. No se afirma aprobación visual en navegador.
- El auditor conserva 1.251 alturas fijas como deuda de revisión. Incluyen principalmente iconos, controles, barras y geometría de gráficas; no son bloqueantes, pero deben revisarse durante pruebas visuales con `extra-large`.
- Hay 33 estilos inline dinámicos justificados para segmentos y anchos calculados desde datos. Se documentan en el cierre de Fase 2.
