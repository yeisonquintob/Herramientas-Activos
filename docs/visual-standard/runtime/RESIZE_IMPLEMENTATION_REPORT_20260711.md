# Implementación de resize global NAVI

Fecha: 2026-07-11  
Rama: `codex/ui-resize-colors-20260711`  
Commit base congelado: `ba059e3`  
Estado de la puerta de calidad: **BLOCKED — pendiente de validación visual con procesos reiniciados**

## Línea base

- Build completo forzado: correcto, 0 errores.
- Advertencias: 3 `CS8602` preexistentes en `ToolsController.cs:304`, `:306` y `:308`.
- Inventario: 119 Razor, 92 vistas enrutadas, 545 acciones.
- Auditoría inicial: 0 colores fuera de paleta, 0 gradientes, 0 `!important`, 0 estilos inline estáticos, 0 bloques `<style>` y 0 acciones sin tono.

## Causa raíz confirmada

1. Los tokens `--navi-sidebar-width`, `--navi-footer-height`, `--navi-topbar-height` y `--navi-mobile-app-width` se consumían sin estar definidos en `tokens.css`.
2. `MainLayout.razor.css` repetía la estrategia `position:fixed + margin-left + width:calc(...)` varias veces y ocultaba contenido con `overflow-x:hidden`.
3. Admin usaba breakpoints incompatibles de 640, 800 y 900 px sin una estrategia única de drawer.
4. Mobile conservaba reglas de la plantilla Blazor con sidebar de 250 px desde 641 px.
5. El CSS aislado se cargaba después del sistema `ntx` y varias directivas extraídas conservaban `@@media`, sintaxis propia de Razor pero inválida en `.razor.css`.
6. Las 41 tablas no compartían un wrapper de scroll interno universal.

## Cambios de resize aplicados

- Variables de layout centralizadas en los tokens de Admin y Mobile.
- Admin migrado a CSS Grid con columnas `sidebar + minmax(0,1fr)`.
- Sidebar de escritorio convertido en columna sticky; footer integrado al flujo y sin superposición.
- Drawer CSS accesible incorporado para ≤899 px, con control, botón y backdrop.
- `navi-main-area` y `navi-content` usan `min-width:0` y no recortan el eje horizontal.
- `blazor-error-ui` respeta el ancho real del sidebar y vuelve a ancho completo en móvil.
- Menú Admin reorganizado sin alturas rígidas ni truncado forzoso de textos.
- Mobile limitado a `--navi-mobile-app-width:520px`, centrado en escritorio y al 100 % en teléfono.
- Bottom nav y menú flotante ajustados con `safe-area` y reserva inferior de contenido.
- Formularios y detalles se fuerzan a una columna por debajo de 600 px; KPI a un máximo de dos columnas.
- Se añadió `.ntx-table-scroll` y se aplicó a las 41 tablas presentes en 29 archivos Razor.
- Se normalizaron directivas `@@media`, `@@supports` y `@@keyframes` de CSS aislado.
- Se reordenó la carga para que los estilos canónicos `ntx` se apliquen después del CSS aislado.
- Se agregó versionado `?v=20260711` para evitar caché visual antigua.

## Verificaciones después de implementar

- Build solución: correcto, 0 errores y las mismas 3 advertencias preexistentes.
- Auditoría UI: correcta, 0 violaciones.
- CSS estructural: 59 archivos válidos.
- Tablas detectadas: 41; wrappers `ntx-table-scroll`: 41.
- `git diff --check`: correcto.

## Validación visual

Los puertos `5218`, `5264` y `5285` estaban ocupados por API, Admin y Mobile de este mismo repositorio y respondían HTTP 200. Sin embargo, esos procesos fueron iniciados antes del build actual. El entorno rechazó el permiso para reiniciarlos por límite temporal de uso de operaciones elevadas.

No se ejecutaron capturas sobre esos procesos porque podrían presentar markup y estilos compilados anteriores. Según el checklist, no se puede marcar PASS con evidencia obsoleta.

## Matriz de resize

| App | Rutas | 360×800 | 390×844 | 768×1024 | 1024×768 | 1366×768 | 1920×1080 |
|---|---:|---:|---:|---:|---:|---:|---:|
| Admin | 15 críticas | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED |
| Mobile | 11 críticas | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED | BLOCKED |

## Pendiente para abrir la siguiente fase

1. Reiniciar API, Admin y Mobile con el build de esta rama.
2. Ejecutar las 156 combinaciones críticas de resize.
3. Guardar capturas en `docs/visual-standard/runtime/screenshots/resize/`.
4. Corregir cualquier FAIL.
5. Solo después iniciar colores y jerarquía semántica.
