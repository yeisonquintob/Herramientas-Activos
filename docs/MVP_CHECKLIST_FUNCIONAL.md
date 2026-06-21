# NAVI Herramientas & Activos — Checklist funcional MVP

## Estado general

- Proyecto: NAVI Herramientas & Activos
- Stack: .NET 8, Blazor Server, Web API, SQL Server, MinIO
- Módulo principal: Gestión de herramientas, activos y hoja de vida técnica

## Servicios principales

| Servicio | URL |
|---|---|
| API | http://localhost:5218 |
| Admin | http://localhost:5264 |
| MinIO API | http://localhost:9100 |
| MinIO Console | http://localhost:9101 |

## Funcionalidades validadas

### Inventario maestro

- [ ] Carga listado de herramientas.
- [ ] Muestra semáforos de estado.
- [ ] Filtro por estado.
- [ ] Filtro por sede.
- [ ] Búsqueda por código, nombre, serial, sede, tipo o categoría.
- [ ] Acceso a detalle.
- [ ] Acceso a hoja de vida técnica.

### Detalle de herramienta

- [ ] Muestra información general.
- [ ] Permite cambiar estado operativo.
- [ ] Permite marcar herramienta especializada/no especializada.
- [ ] Registra cambios.

### Hoja de vida técnica

- [ ] Muestra información del equipo.
- [ ] Muestra especificaciones técnicas.
- [ ] Muestra vida útil.
- [ ] Muestra plan de mantenimiento.
- [ ] Muestra accesorios.
- [ ] Muestra documentos.
- [ ] Muestra cronograma de mantenimiento.
- [ ] Muestra prácticas seguras.
- [ ] Muestra eventos de hoja de vida.
- [ ] Muestra alerta visual de mantenimiento.
- [ ] Exporta a PDF mediante impresión.
- [ ] Exporta a Excel `.xlsx`.

### Documentos

- [ ] Carga documentos.
- [ ] Descarga documentos.
- [ ] Elimina documentos.
- [ ] Limpia evidencia asociada cuando se elimina un documento usado por mantenimiento.

### Mantenimientos

- [ ] Crea mantenimiento.
- [ ] Edita mantenimiento.
- [ ] Asocia evidencia documental.
- [ ] Descarga evidencia desde gestión de mantenimientos.
- [ ] Muestra evidencia en hoja de vida.

### Accesorios

- [ ] Crea accesorio.
- [ ] Edita accesorio.
- [ ] Desactiva accesorio.

### Prácticas seguras

- [ ] Crea práctica segura.
- [ ] Edita práctica segura.
- [ ] Desactiva práctica segura.
- [ ] Carga prácticas por defecto.

### Trazabilidad

- [ ] Lista eventos de hoja de vida.
- [ ] Filtra por texto.
- [ ] Filtra por tipo de evento en español.
- [ ] Limpia filtros.

## Evidencias recomendadas

- Captura de inventario maestro.
- Captura de hoja de vida técnica.
- Captura de alerta de mantenimiento.
- Captura de documentos.
- Captura de mantenimiento con evidencia.
- Captura de trazabilidad.
- Archivo Excel exportado.
- PDF generado desde navegador.

## Resultado esperado

El MVP permite administrar herramientas y activos de taller, consultar su hoja de vida técnica, gestionar documentos, mantenimientos, accesorios, prácticas seguras y trazabilidad, manteniendo una experiencia visual cercana a un sistema corporativo y alineada al flujo de Dynamics/Fenix365.

