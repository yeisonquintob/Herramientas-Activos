# Inventario UI corregido

Fecha: 2026-07-11

## Cobertura

- Razor: 119 (82 Admin, 37 Mobile).
- Archivos enrutados: 92 (67 Admin, 25 Mobile).
- Directivas `@page`: 108.
- Controles de formulario: 491 (240 HTML y 251 Blazor `Input*`).
- Elementos de acción auditados: 545.
- Acciones sin clasificación canónica: 0.

## Deuda visual

- Archivos con `<style>`: 0; bloques: 0.
- CSS personalizado, incluidos bloques `<style>`: 1,530,677 caracteres.
- Colores no autorizados: 0 apariciones.
- Colores nombrados en declaraciones: 0.
- Gradientes: 0.
- `!important`: 0.
- Referencias textuales legacy purple/blue/violet/indigo: 0.
- Clases cromáticas no semánticas: 0.
- Estilos inline estáticos: 0; los restantes son valores dinámicos de visualización.

## Contraste de la paleta

| Rol | Frente | Fondo | Ratio | AA normal | AA grande |
|---|---:|---:|---:|---:|---:|
| primary-label | #FFFFFF | #EE2E2F | 4.14:1 | No | Sí |
| destructive-label | #EE2E2F | #FFFFFF | 4.14:1 | No | Sí |
| positive-label | #363534 | #B2E100 | 7.96:1 | Sí | Sí |
| warning-label | #363534 | #FFB301 | 6.82:1 | Sí | Sí |
| table-header | #FFFFFF | #354550 | 9.91:1 | Sí | Sí |
| positive-as-text-on-white | #B2E100 | #FFFFFF | 1.54:1 | No | No |
| warning-as-text-on-white | #FFB301 | #FFFFFF | 1.79:1 | No | No |

El JSON adjunto contiene vistas, rutas, fuentes de mayor riesgo, acciones sin clasificar y candidatos de CSS sin uso.
