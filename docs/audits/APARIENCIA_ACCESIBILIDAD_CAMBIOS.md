# Cambios de apariencia y accesibilidad NAVI

Fecha: 2026-07-16  
Tipo: presentación y accesibilidad; sin cambios de lógica funcional.

## Resumen

Se consolidó la aplicación global de temas y escalas, se corrigió contraste/foco, se normalizaron tonos de acciones iconográficas, se reforzó la accesibilidad de la configuración de apariencia y se igualó la sincronización de preferencias del PWA con el Admin.

## Cambios aplicados

### Tokens y contrato visual

- Se añadieron alias canónicos faltantes para actualización, títulos de sección, interlineado, columnas dinámicas e imágenes de inventario.
- Se eliminaron colores directos de `admin-reference-homologation.css` a favor de tokens, sombras y mezclas canónicas.
- Formularios ahora usan el borde fuerte del tema para mantener percepción no textual superior a 3:1.
- Botones `warning` usan superficie suave y texto del tema; botones `danger` usan el rojo oscuro accesible con texto blanco.
- El tema frío dejó de convertir las acciones de peligro en violeta y conserva el significado rojo.
- El foco del tema frío usa color canónico, contorno de 2 px, separación y sombra visible.
- El `placeholder` móvil usa el color secundario sin reducción de opacidad.

Archivos principales:

- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/tokens.css`
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/navi-ui-preferences.css`
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/theme-admin-cold.css`
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/admin-reference-homologation.css`
- `src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/tokens.css`
- `src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/navi-mobile-ui-preferences.css`
- `src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/base.css`
- `src/Navi.ToolsAssets.MobilePwa/wwwroot/index.html`

### Configuración de apariencia

- Admin y Mobile distinguen mensajes de éxito y error con `status/polite` y `alert/assertive`.
- Los estados de carga exponen `role="status"` y `aria-busy="true"`.
- Los selectores de tamaño y tema tienen grupo y nombre accesible.
- El CSS de preferencias móviles se carga al final de las hojas del sistema para preservar su función como capa global de configuración.

Archivos:

- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsAppearance.razor`
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsAppearance.razor.css`
- `src/Navi.ToolsAssets.MobilePwa/Pages/MobileAppearanceSettingsPanel.razor`

### Persistencia Mobile

- Se añadió guardia de inicialización para no registrar eventos duplicados.
- Se centralizó la sincronización de atributos en raíz.
- Se añadieron sincronización por `storage`, foco, `pageshow`, visibilidad y navegación mejorada.
- Se evitó notificar cambios cuando la firma de preferencias no varía.
- Admin y Mobile continúan usando claves y atributos independientes.

Archivo:

- `src/Navi.ToolsAssets.MobilePwa/wwwroot/js/navi-mobile-ui-preferences.js`

### Controles y acciones

- Se agregaron `id` y `name` estables a los controles de carga de imagen de hoja de vida Admin/Mobile.
- Se cambió “Cancelar” de peligro a secundario en seis acciones móviles.
- Se declararon tonos explícitos en 11 botones iconográficos de consulta, zoom y cierre.

Archivos revisados:

- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor`
- `src/Navi.ToolsAssets.Admin/Components/Pages/Tools.razor`
- `src/Navi.ToolsAssets.MobilePwa/Pages/MobileLifeRecord.razor`
- `src/Navi.ToolsAssets.MobilePwa/Pages/MobileLifeRecords.razor`
- `src/Navi.ToolsAssets.MobilePwa/Pages/MyTools.razor`

### Limpieza y documentación

- Se retiraron cinco copias CSS con sufijo ` 2`, dos hojas obsoletas sin carga y 11 archivos de caché Python.
- `.gitignore` ahora excluye caché Python.
- Se creó el catálogo de los 42 componentes compartidos/estructurales identificados.
- Se documentaron componentes disponibles sin eliminarlos.

Documentos:

- `docs/audits/CATALOGO_COMPONENTES_NAVI.md`
- `docs/audits/LIMPIEZA_CODIGO_20260716.md`
- `docs/audits/APARIENCIA_ACCESIBILIDAD_AUDITORIA.md`

## Verificación realizada

### Compilación

| Proyecto | Advertencias | Errores | Resultado |
|---|---:|---:|---|
| Admin | 0 | 0 | Correcto |
| Mobile PWA | 0 | 0 | Correcto |
| API | 0 | 0 | Correcto |

Los archivos JavaScript de preferencias Admin/Mobile superaron comprobación de sintaxis.

### Validadores

- Integridad: 0 variables canónicas sin resolver y 0 rutas duplicadas.
- Homologación estricta: 0 bloqueos.
- Botones sin tono o con tono contradictorio: 0.
- Deuda histórica de CSS: 4.271 hallazgos para migración incremental; no constituye una regresión introducida por este cambio.

### Prueba visual

- 32 combinaciones tema/escala probadas.
- Contraste mínimo de texto en botón: 4,50:1.
- Contraste mínimo de borde de campo: 7,19:1.
- Sin desbordamiento global en Admin 1.024/1.366/1.920 px.
- Sin desbordamiento global en Mobile 360/390/520 px.
- Sin desbordamiento global web a 100/150/200 %; a 200 % la tabla desplaza solo dentro de su viewport accesible.
- Persistencia, recarga, sincronización entre pestañas e independencia Admin/Mobile verificadas.

## Estado operativo

La API continuó activa en el puerto 5218. Admin y Mobile permanecen escuchando en 5264 y 5285 respectivamente, pero sus procesos quedaron desacoplados de las sesiones de terminal. El entorno impidió terminarlos por permisos; por eso la compilación nueva está lista en disco, pero se recomienda reiniciar ambos frontends en una terminal con control de esos procesos antes de la aceptación manual final.

No se intentó forzar el cierre ni iniciar copias en puertos alternos.
