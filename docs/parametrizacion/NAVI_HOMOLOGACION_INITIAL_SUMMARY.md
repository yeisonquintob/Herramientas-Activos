# NAVI — resumen de auditoría inicial de homologación

Fecha: 2026-07-13.

Este documento conserva la primera medición obtenida después de congelar las vistas de referencia y antes de aplicar las correcciones de accesibilidad de presentación. Los archivos de auditoría `BASELINE` se usaron como instantáneas de trabajo y por eso no sustituyen este resumen histórico.

| Categoría | Inicial |
|---|---:|
| Campos sin `id` o `name` | 387 |
| Etiquetas de formulario sin asociación | 99 |
| Controles expandibles sin semántica ARIA | 7 |
| **Bloqueantes** | **493** |
| Posición absoluta para revisión | 29 |
| Elementos `label` usados como texto de datos | 126 |
| Gradientes | 1 |
| `white-space: nowrap` para revisión | 186 |
| Overflow horizontal interno para revisión | 33 |
| **Revisión manual** | **375** |

El barrido se amplió durante la implementación para incluir estados de carga. Se añadieron semánticas `role="status"` y `aria-busy="true"` a 54 indicadores no protegidos.

Resultado final: cero hallazgos bloqueantes. El detalle vigente está en `NAVI_HOMOLOGACION_AUDIT_FINAL.json`.
