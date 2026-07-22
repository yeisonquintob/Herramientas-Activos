# Auditoría de apariencia y accesibilidad NAVI

Fecha: 2026-07-16  
Ámbitos: Admin Web y Mobile PWA  
Referencia: `NAVI_VALIDACION_APARIENCIA_ACCESIBILIDAD_CODEX.md`  
Resultado: **aprobación técnica con observaciones manuales documentadas**.

## Alcance respetado

La revisión se limitó a presentación, preferencias visuales, componentes compartidos y accesibilidad. No se modificaron API, base de datos, migraciones, rutas, permisos, autenticación, cálculos de negocio ni contratos de servicios.

Fuentes de verdad verificadas:

- Admin: `SettingsAppearance.razor`, `navi-ui-preferences.js`, `navi-ui-preferences.css`, tokens y componentes compartidos.
- Mobile: `MobileAppearanceSettingsPanel.razor`, `navi-mobile-ui-preferences.js`, `navi-mobile-ui-preferences.css`, tokens y componentes compartidos.
- Almacenamiento independiente por aplicación y atributos independientes en el elemento `html`.

## Línea base y seguridad

- Se generó respaldo completo previo en `backups/NAVI_APARIENCIA_ACCESIBILIDAD_PRE_20260716.tar.gz`.
- SHA-256: `d3381469837ee2dc7c614114622306616caa656298511f41176ab505a3a6abd4`.
- Admin y Mobile compilaban antes de los cambios.
- La API se mantuvo en ejecución durante la revisión visual; solo se reinician los frontends para cargar el resultado final.

## Resultados automáticos

### Integridad de fuentes

| Aplicación | Razor | Rutas | CSS | JS | Variables canónicas sin resolver | Rutas duplicadas |
|---|---:|---:|---:|---:|---:|---:|
| Admin | 96 | 71 | 57 | 4 | 0 | 0 |
| Mobile | 50 | 38 | 23 | 6 | 0 | 0 |

El validador `validate_ui_source_integrity.py` terminó sin errores.

### Homologación estricta

- Bloqueos: **0**.
- Revisiones manuales: 280.
- Posicionamiento absoluto: 34.
- `nowrap`: 212.
- `overflow: auto`: 34.

Estas 280 coincidencias no son errores automáticos: corresponden a patrones que requieren inspección contextual. No se realizó una sustitución masiva porque puede dañar menús, chips, tablas o barras de acción.

### Auditoría histórica de preferencias

El escáner registra 4.271 hallazgos de deuda visual histórica:

| Categoría | Cantidad |
|---|---:|
| Colores directos | 122 |
| Alturas fijas | 1.390 |
| `!important` | 2.706 |
| Estilos dinámicos en línea | 35 |
| `overflow-x: hidden` | 1 |
| Tamaño de fuente definido en vista | 17 |

No hay hallazgos de tono de acción incorrecto ni botones compartidos sin tono. El modo estricto devuelve estado no satisfactorio por esta deuda acumulada, pero no detecta variables sin resolver, rutas duplicadas ni bloqueos de homologación. Debe reducirse por módulos con pruebas visuales, no mediante reemplazo global.

## Preferencias visuales

### Escalas

| Opción | Factor Admin | Factor Mobile | Resultado |
|---|---:|---:|---|
| Compacto | 0,92 | 0,92 | Correcto |
| Normal | 1,00 | 1,00 | Correcto |
| Grande | 1,10 | 1,10 | Correcto |
| Muy grande | 1,20 | 1,20 | Correcto |

Las 16 combinaciones por aplicación —cuatro escalas por cuatro temas— se probaron con componentes reales representativos. El tamaño de cuerpo y KPI aumentó de forma proporcional en todos los casos.

### Temas y contraste

| Tema | Contraste mínimo de botones medido | Contraste mínimo de borde de campo | Estado |
|---|---:|---:|---|
| Normal | 4,82:1 | 9,91:1 | AA |
| Frío | 4,50:1 | 7,19:1 | AA |
| Cálido | 5,19:1 | 8,07:1 | AA |
| Oscuro | 4,77:1 | 8,33:1 | AA |

Se corrigieron los botones de advertencia y peligro, el borde perceptible de campos, el foco del tema frío y el contraste de `placeholder` móvil. Las mediciones se repitieron en las 32 combinaciones Admin/Mobile.

### Persistencia e independencia

- Admin conservó `extra-large` + `dark` después de recargar.
- Mobile conservó `extra-large` + `dark` después de recargar.
- Un cambio Admin a `normal` + `warm` se sincronizó en una segunda pestaña Admin.
- Mobile permaneció sin cambios durante la modificación Admin.
- Un cambio Mobile a `large` + `cold` se sincronizó en una segunda pestaña Mobile.
- Admin permaneció sin cambios durante la modificación Mobile.

Esto confirma persistencia, sincronización entre pestañas y separación de las claves de cada aplicación.

## Validación responsive y zoom

Se probó el caso más exigente (`extra-large` + tema oscuro):

| Medio | Ancho | Desbordamiento global | KPI | Resultado |
|---|---:|---|---|---|
| Admin | 1.024 px | No | Sin corte | Correcto |
| Admin | 1.366 px | No | Sin corte | Correcto |
| Admin | 1.920 px | No | Sin corte | Correcto |
| Mobile | 360 px | No | Sin corte | Correcto |
| Mobile | 390 px | No | Sin corte | Correcto |
| Mobile | 520 px | No | Sin corte | Correcto |

Zoom web sobre 1.366 px:

| Zoom | Desbordamiento global | Resultado |
|---:|---|---|
| 100 % | No | Correcto |
| 150 % | No | Correcto |
| 200 % | No | Correcto; la tabla usa desplazamiento interno enfocable y nombrado |

La emulación de zoom se aplicó en una página de prueba temporal que cargó las hojas reales. El arnés temporal se retiró después de registrar las mediciones.

## Accesibilidad corregida

- Mensajes de éxito: `role="status"` y `aria-live="polite"`.
- Mensajes de error: `role="alert"` y `aria-live="assertive"`.
- Estados de carga: `role="status"` y `aria-busy="true"`.
- Grupos de selección de tamaño y tema con `role="group"` y nombre accesible.
- Controles de archivo de hoja de vida con `id` y `name` estables.
- Botones iconográficos revisados con tono semántico explícito.
- Acciones “Cancelar” dejaron de presentarse como peligro.
- Foco móvil verificado con anillo y sombra perceptibles.
- Foco Admin frío reforzado en CSS con color canónico, 2 px de contorno, separación y sombra. La última repetición interactiva de este punto quedó impedida por la restricción de seguridad del navegador local; queda como comprobación manual puntual, aunque el contrato CSS y la compilación son correctos.

## Observaciones que requieren una fase funcional autorizada

No se modificaron porque implican manejo de foco, teclado o navegación y el alcance prohibía alterar lógica:

- Diálogos visuales que todavía no implementan completamente semántica `dialog`, foco inicial, encierro de foco y cierre con Escape:
  - Admin `MaintenanceConsultation.razor`.
  - Admin `DocumentsIndex.razor`.
  - Mobile `MaintenancePlans.razor`.
  - Mobile `PurchasesMaintenanceConsultation.razor`.
  - Mobile `Documents.razor`.
- Previsualizadores de imagen con rol/nombre, pero sin gestión completa de foco/Escape.
- Tablas históricas aún no migradas a `NaviTableShell`/`NaviMobileTableShell`.
- Sexto color de algunas gráficas con contraste bajo cuando se usa como único diferenciador; debe acompañarse siempre de etiqueta, patrón o valor textual.

## Contrato de homologación

`homologation_contract.py verify` informa 31 diferencias frente a una línea base anterior. La mayoría ya existía en el árbol de trabajo actual y corresponde a la evolución visual realizada después de crear esa línea base; una diferencia adicional es la retirada documentada del CSS v25 sustituido por v26. No se regeneró la línea base automáticamente porque hacerlo sin revisión convertiría cambios existentes en referencia oficial.

Recomendación: revisar el conjunto actual, aceptar expresamente la nueva versión visual y solo entonces regenerar el contrato de hashes.

## Dictamen

La configuración global de apariencia, contraste, persistencia, sincronización y reflow queda técnicamente validada. La aplicación puede continuar trabajando con esta base. Permanecen tareas manuales/accesibles que requieren autorización para tocar comportamiento de diálogos y una migración incremental de deuda CSS; no deben resolverse con cambios masivos.
