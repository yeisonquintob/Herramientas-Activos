# NAVI — homologación visual global Admin Web y Mobile PWA

## Resumen ejecutivo

Se cerró la homologación estructural y accesible del sistema visual existente sin crear un segundo núcleo. Las 66 rutas objetivo de Admin y las 28 rutas objetivo de Mobile conservan el sistema previamente migrado de encabezados, KPI, acciones semánticas, formularios, tarjetas, tablas, temas y escalas. Esta intervención completó las asociaciones de formularios, la semántica de expansión y los estados de carga que faltaban.

No se iniciaron aplicaciones, no se utilizó Git y no se modificaron API, base de datos, permisos, rutas ni lógica de negocio.

## Protección y no regresión

- 25 archivos de referencia congelados y verificados por SHA-256.
- 5 vistas protegidas Admin y 5 vistas protegidas Mobile sin cambios.
- 146 archivos Razor comparados por proyección funcional.
- Diferencias en cuerpos `@code`, rutas, directivas, eventos, `@bind`, enlaces, permisos, flujo de control y llamadas de servicios: 0.
- Rutas actuales: 71 Admin y 38 Mobile; duplicadas: 0.

La línea base verificable está en `NAVI_HOMOLOGACION_BASELINE_CONTRACT.json` y se valida con `scripts/ui-audit/homologation_contract.py verify`.

## Cobertura de vistas

| Superficie | Archivos con ruta | Rutas | Protegidas | Objetivo homologado |
|---|---:|---:|---:|---:|
| Admin Web | 68 | 71 | 5 archivos / 5 rutas | 63 archivos / 66 rutas |
| Mobile PWA | 25 | 38 | 5 archivos / 10 rutas | 20 archivos / 28 rutas |

Las familias cubiertas incluyen dashboard, compras, préstamos, devoluciones, aprobaciones, mantenimiento, tomas físicas, conciliación, documentos, reportes, integraciones, hojas de vida, configuración, seguridad, páginas especiales, inicio Mobile, solicitudes, consultas, reportes y páginas técnicas.

## Componentes y núcleo reutilizados

Admin conserva `NaviPageHero`, `NaviKpiToolbar`, `NaviKpiGrid`, `NaviKpiCard`, `NaviActionButton`, `NaviStatusBadge`, `NaviTableShell`, tarjetas, alertas, estados vacíos y formularios compartidos.

Mobile conserva `NaviMobileAppBanner`, `NaviMobilePageHero`, la familia KPI Mobile con `NaviMobileKpiOverflow`, acciones, estados, tarjetas, alertas, estados vacíos y tablas compartidas.

Los cuatro tamaños (`compact`, `normal`, `large`, `extra-large`) y los cuatro temas (`normal`, `cold`, `warm`, `dark`) continúan gobernados por los tokens y preferencias independientes de Admin y Mobile.

## Accesibilidad completada

- 387 campos sin identidad iniciales: 0 finales.
- 99 etiquetas de formulario sin asociación iniciales: 0 finales.
- 74 etiquetas semánticamente incorrectas de datos de solo lectura convertidas a texto descriptivo sin cambiar su apariencia.
- 7 controles expandibles o toggle sin estado ARIA: 0 finales.
- 54 indicadores de carga no protegidos con `role="status"` y `aria-busy="true"`.
- Botones de solo icono sin nombre accesible: 0.
- Se conservaron íntegros todos los eventos y enlaces.

La intervención de accesibilidad alcanzó 35 archivos Razor Admin y 22 Mobile. Los cambios son exclusivamente `id`, `name`, `for`, `aria-*`, `role`, tono visual de dos toggles y etiquetas HTML de presentación.

## CSS y auditoría

| Regla | Resultado final |
|---|---:|
| Colores rígidos en vistas | 0 |
| Colores rígidos de gráficas | 0 |
| `!important` | 0 |
| Gradientes | 0 |
| Bootstrap cromático local | 0 |
| Botones sin tono | 0 |
| Tonos no canónicos | 0 |
| KPI manual detectado | 0 |
| `font-size` fijo de vista | 0 |
| `overflow-x: hidden` | 0 |
| Encabezados Mobile ausentes | 0 |
| Variables canónicas sin definición | 0 |

El único gradiente residual era código muerto del encabezado Admin, ya neutralizado posteriormente por una regla más específica de superficie blanca. Se sustituyó por el mismo color efectivo, de modo que el resultado normal de las vistas protegidas no cambia.

## Excepciones justificadas

- 29 posiciones absolutas se conservan para pseudo-elementos decorativos, centros de anillos y etiquetas de gráficas; no estructuran formularios, tablas ni navegación.
- 33 `overflow-x: auto` permanecen confinados a contenedores internos de tablas o matrices. No existe ocultamiento ni overflow horizontal aplicado a la página.
- 186 usos de `nowrap` corresponden principalmente a controles compactos, identificadores, estados y etiquetas breves. Permanecen marcados para confirmación visual con datos reales y escala `extra-large`.
- 1.251 alturas fijas siguen catalogadas para revisión; predominan iconos, controles y geometría de gráficas.
- 33 estilos inline dinámicos representan porcentajes o segmentos calculados con datos. Sustituirlos requeriría cambiar lógica, por lo que se mantienen como excepción documentada.

## Validaciones

- Auditoría estricta de homologación: aprobada, 0 bloqueantes.
- Auditoría estricta de preferencias: aprobada, 0 bloqueantes.
- Integridad estructural: 96 Razor Admin, 50 Razor Mobile, 84 CSS, 0 CSS inválidos.
- Matriz estática: 92 vistas × 5 anchos = 460 comprobaciones, 0 fallos estructurales.
- Build Admin Debug: correcto, 0 advertencias, 0 errores, 24,73 s.
- Build Mobile Debug: correcto, 0 advertencias, 0 errores, 28,33 s.
- Puertos 5264 y 5285: detenidos antes de compilar.

## Prueba manual pendiente

La inspección interactiva con datos y permisos representativos queda pendiente en 360, 390, 520/768, 1366 y 1920 px, combinando los cuatro tamaños y cuatro temas. No se presenta como ejecutada porque el encargo prohíbe iniciar las aplicaciones.

## Confirmación funcional

No se modificó lógica de negocio, API, base de datos, DTO, entidades, clientes HTTP, filtros, cálculos KPI, navegación, rutas, permisos, roles, eventos, métodos ni cuerpos `@code`.
