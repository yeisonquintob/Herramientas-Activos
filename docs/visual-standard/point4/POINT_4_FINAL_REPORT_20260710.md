# Punto 4 — Estandarización visual del Admin Web

Fecha: 2026-07-10  
Rama: `codex/point4-global-visual-standardization-20260710`  
Base congelada: `604d77943929be95465ea4b063187a5177a74889`

## Resultado implementado

- Se creó un inventario reproducible compatible con UTF-8/BOM para Admin y Mobile.
- Se auditaron 119 archivos Razor: 82 Admin y 37 Mobile.
- Se cubrieron 92 archivos enrutados: 67 Admin y 25 Mobile, con 108 directivas `@page`.
- Se sustituyó la capa visual global por `tokens.css`, `base.css`, `components.css`, `utilities.css` y capas de layout separadas por aplicación.
- Se unificaron acciones, encabezados, KPI, formularios, tablas, estados, alertas, modales, pestañas, progreso y navegación.
- Se clasificaron semánticamente 545 acciones con los tonos `primary`, `positive`, `danger`, `warning`, `secondary` y `ghost`.
- Se extrajeron 48 bloques `<style>` presentes en 40 archivos hacia CSS aislado `.razor.css`.
- Se eliminaron tres hojas legacy redundantes y el contenido monolítico de los dos `app.css`.
- Se normalizaron las clases cromáticas restantes a nombres semánticos.
- Se eliminaron todos los estilos inline estáticos; permanecen 18 estilos dinámicos usados para barras, porcentajes y gráficos calculados.

## Auditoría final

| Control | Resultado |
|---|---:|
| Bloques `<style>` | 0 |
| Acciones sin clasificación | 0 |
| Clases cromáticas no semánticas | 0 |
| Colores fuera de paleta | 0 |
| Colores nombrados en declaraciones | 0 |
| Gradientes | 0 |
| `!important` en CSS personalizado | 0 |
| Estilos inline estáticos | 0 |
| Archivos CSS personalizados inválidos | 0 de 55 |

El CSS personalizado pasó de aproximadamente 3.253.262 a 1.530.677 caracteres, una reducción cercana al 53 %, conservando la estructura específica de las pantallas.

## Contraste

- Encabezados de tabla: blanco sobre gris NAVI, 9,91:1, AA normal.
- Estados positivos: negro NAVI sobre verde, 7,96:1, AA normal.
- Estados de advertencia: negro NAVI sobre amarillo, 6,82:1, AA normal.
- Rojo NAVI con blanco: 4,14:1. Por ello los botones primarios usan texto grande y peso 700; las acciones destructivas normales usan texto negro, fondo blanco y borde rojo.
- Verde y amarillo no se usan como texto sobre blanco porque no alcanzan AA.

## Compilación

Comando:

```bash
/Users/luu/.dotnet/dotnet build Navitrans.ToolsAssets.Management.sln --no-restore --disable-build-servers -m:1 -nr:false --nologo -v:minimal -t:Rebuild
```

Resultado:

- Compilación correcta.
- 0 errores.
- 3 advertencias `CS8602` preexistentes, sin cambio respecto de la línea base:
  - `src/Navi.ToolsAssets.Api/Controllers/ToolsController.cs:304`
  - `src/Navi.ToolsAssets.Api/Controllers/ToolsController.cs:306`
  - `src/Navi.ToolsAssets.Api/Controllers/ToolsController.cs:308`

También pasaron `git diff --check`, la compilación de los scripts Python y la auditoría con `--fail-on-violations`.

## Matriz de vistas y resoluciones

Se prepararon y comprobaron estructuralmente las 92 vistas en 360, 390, 768, 1366 y 1920 px:

- Admin: 67 × 5 = 335 combinaciones.
- Mobile: 25 × 5 = 125 combinaciones.
- Total: 460 combinaciones, 0 fallos estructurales.

La validación visual interactiva no pudo ejecutarse: al intentar levantar API, Admin y Mobile en `localhost`, el entorno rechazó el permiso para abrir los puertos por límite de uso. Por integridad, las 460 filas se registran como `ready_for_runtime` y no como validación visual aprobada.

## Evidencias

- `UI_INVENTORY_CORRECTED_20260710.json`: inventario completo y auditable.
- `UI_INVENTORY_CORRECTED_20260710.md`: resumen humano del inventario.
- `UI_STRUCTURE_VALIDATION_20260710.json`: matriz completa de 460 combinaciones.
- `UI_STRUCTURE_VALIDATION_20260710.md`: resumen de estructura y bloqueo de runtime.
- `scripts/ui-audit/`: generación, migración, clasificación, normalización y validación reproducibles.

## Estado de cierre

La implementación, la compilación y las auditorías automáticas del punto 4 están correctas. El único control pendiente para declarar el punto completamente aceptado es ejecutar la matriz visual en navegador cuando el entorno permita abrir los servicios locales.
