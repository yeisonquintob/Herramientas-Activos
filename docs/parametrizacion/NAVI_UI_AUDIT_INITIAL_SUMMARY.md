# Resumen de la auditoría inicial de apariencia NAVI

Fecha de captura: 2026-07-13.

Este resumen conserva los conteos obtenidos en la primera ejecución del auditor, antes de aplicar la migración global. Durante la implementación, los archivos `NAVI_UI_AUDIT_BASELINE.json` y `.md` se regeneraron como instantáneas de trabajo; por eso este documento es la referencia histórica para comparar el estado inicial con el cierre estricto.

| Categoría | Inicial |
|---|---:|
| Colores directos | 3.050 |
| Colores directos en gráficas | 518 |
| Alturas fijas para revisión | 1.271 |
| Botones sin tono | 232 |
| `!important` | 159 |
| Clases cromáticas Bootstrap | 95 |
| Tonos no canónicos | 95 |
| KPI manual detectado | 92 |
| `font-size` fijo en CSS de vista | 75 |
| `style` inline | 34 |
| `overflow-x: hidden` | 31 |
| Vistas móviles sin encabezado estándar | 2 |
| **Total** | **5.654** |

Al cierre, las categorías bloqueantes quedaron en cero. Las únicas observaciones restantes son geometrías fijas sujetas a revisión y 33 estilos dinámicos justificados para anchuras o segmentos de gráficas; el detalle está en `NAVI_UI_AUDIT_FINAL.json`.
