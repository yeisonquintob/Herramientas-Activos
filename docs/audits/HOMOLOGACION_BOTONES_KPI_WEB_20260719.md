# Homologación de botones y KPI del Admin Web

Fecha: 19 de julio de 2026

## Objetivo

Unificar la geometría horizontal de los botones de acción y el patrón visual de los KPI del Admin Web sin modificar eventos, cálculos, filtros, rutas, servicios, API ni base de datos.

## Contrato de botones

- Acción normal: `168 px`, escalados por la preferencia de tamaño visual.
- Acción dentro de tabla: `96 px`, escalados por la misma preferencia.
- El ancho se limita al espacio disponible para evitar desbordamientos en filtros, paneles y resoluciones reducidas.
- La altura, el `padding`, los eventos y los estados habilitado/deshabilitado se conservan.
- Excepciones intencionales: botones solo icono, actualizar circular, bloques de ancho completo, chips, píldoras, pestañas, disparadores, cierres y controles estructurales de Documentos/Reportes.

## Contrato de KPI

- Componente reutilizado: `NaviKpiGrid` con tarjetas `NaviKpiCard`.
- Borde exterior: `1 px`.
- Radio exterior: píldora canónica (`999 px`).
- Separación entre tarjetas: `0 px`; se usa un único separador interno.
- Relleno exterior: `4 px 7 px`.
- Altura mínima de tarjeta: `50 px`, escalada por Apariencia.
- Tarjetas internas sin borde, radio ni sombra propios.
- Una franja de seis KPI usa seis columnas en escritorio amplio y tres columnas entre `768` y `1439 px` para mantener la lectura; la envoltura crece con el contenido y no invade filtros ni tablas.

## Archivos responsables

- `wwwroot/css/ntx/tokens.css`: medidas y tokens comunes.
- `wwwroot/css/ntx/admin-ui-consistency.css`: contrato visual final y excepciones.
- `Components/App.razor`: orden de carga final de la hoja de consistencia.
- `Components/Pages/Modules/LoginAppearanceSettings.razor.css`: elimina el ancho doble heredado de la tercera acción de imagen.

## Limpieza segura aplicada

- Se eliminaron dos copias JavaScript `.backup_*` duplicadas.
- Se eliminaron registros de ejecución y PID generados dentro de `logs/runtime` y `logs/diagnostics`.
- Se eliminaron archivos `.DS_Store` del árbol activo, sin tocar respaldos ni exportaciones.
- Se conservaron cuatro `Debug.WriteLine` ubicados en bloques de captura de errores de imagen; son diagnóstico útil, no ruido de ejecución normal.

## Validación

- Compilación Admin Web: correcta, `0` advertencias y `0` errores.
- Integridad Razor: sin rutas duplicadas ni delimitadores inválidos.
- Vistas verificadas: Apariencia, Inventario, Documentos, Solicitud de compra, Usuarios y Reportes.
- Resoluciones verificadas: `768`, `1366` y `1920 px`.
- Resultado KPI: borde `1 px`, radio uniforme, `gap 0`, tarjetas internas sin borde y sin superposición con el contenido siguiente.
- Resultado botones: una medida normal por contexto y una medida compacta uniforme en tablas; las alturas previas permanecen intactas.
- Desbordamiento horizontal del documento: `0 px` en las vistas verificadas; las tablas conservan su desplazamiento interno cuando corresponde.

## Deuda técnica observada, no modificada

El auditor de variables CSS mantiene incidencias heredadas anteriores a este cambio (18 referencias en Admin y 6 en Mobile). No afectan la compilación ni forman parte de esta homologación visual. Conviene tratarlas en una limpieza separada para no mezclar alcance visual con refactorización estructural.
