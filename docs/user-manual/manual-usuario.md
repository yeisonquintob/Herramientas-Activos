# Manual de usuario NAVI

Este manual describe funciones existentes. La disponibilidad depende del rol,
la sede y la compañía autorizada.

## Acceso y sesión

Inicie sesión con su usuario asignado. No comparta credenciales. Si la sesión
expira, es revocada o el usuario se desactiva, vuelva a autenticarse o contacte
al administrador. Cierre sesión al terminar en equipos compartidos.

Cuando multicompañía esté habilitado y tenga más de un acceso, seleccione la
compañía activa en el encabezado. Cambiarla actualiza el token y el contexto;
confirme siempre el nombre antes de registrar información.

## Navegación común

Use el menú lateral en Admin y el menú operativo en Mobile. Los KPI responden a
los filtros visibles. `Actualizar` vuelve a consultar; `Limpiar` restablece
filtros. Los botones ocultos o deshabilitados dependen de permisos/estado.

## Por rol

### Administrador NAVI

Gestiona usuarios, roles, permisos, configuración y accesos a compañías. Puede
revisar auditoría, importaciones y reportes. La creación física de bases,
restore y migraciones continúan siendo operación técnica documentada.

### Gerencia

Consulta dashboard y reportes ejecutivos por compañía, sede, zona, categoría,
estado, responsable y tipo. Puede exportar si posee `Reports.Export`.

### Administrador de sede

Gestiona inventario y procesos dentro de su alcance. No debe cambiar compañía o
sede para ampliar acceso; el servidor valida el alcance.

### Coordinador de taller

Coordina asignaciones, mantenimiento, tomas y conciliación según su matriz.

### Herramientero

Consulta disponibilidad, asigna/regresa activos y participa en tomas cuando
está autorizado.

### Ingeniero de servicios

Consulta hoja de vida, mantenimiento, documentos y reportes técnicos.

### Técnico

Usa principalmente Mobile para activos asignados, preoperacionales, daños,
préstamos y participación en tomas.

### Auditor

Consulta inventario, historial, documentos, conciliación y reportes sin
modificar cuando su rol es de solo lectura.

## Procesos

### Inventario y hoja de vida

Filtre por código, serial, sede, ubicación, responsable, tipo o estado. Abra el
detalle o la hoja de vida. La edición técnica requiere permiso específico.

### Asignaciones

Cree o revise una solicitud; aprobar/denegar y asignar/regresar son acciones
distintas. Verifique responsable, sede y estado antes de confirmar.

### Compras y mantenimiento

Complete encabezado, activos y cotizaciones según el estado. Las acciones de
aprobar, ejecutar, cerrar o rechazar solo aparecen cuando corresponden.

### Tomas físicas y conciliación

Cree la campaña por sede, genere participantes, inicie y cierre conforme al
avance. Los faltantes, no listados y diferencias pasan a conciliación. No
fusione registros ambiguos sin evidencia.

### Documentos

Seleccione clasificación y activo, cargue un formato permitido y describa el
soporte. Use Ver/Descargar según permisos. Si el objeto no abre, conserve el
trace id y reporte a soporte.

### Importación

1. Confirme compañía activa.
2. Cargue `.xlsx` sin macros.
3. Revise total, válidas, advertencias, errores y duplicados.
4. Corrija o envíe filas a revisión.
5. Cree candidatos por lotes o aplique actualizaciones confirmadas.
6. Exporte el reporte de calidad.

La primera carga nunca crea activos. Duplicados ambiguos requieren decisión.

### Reportes

Use filtros antes de exportar. El CSV refleja el contexto y filtros actuales.
PDF ejecutivo no está habilitado todavía.

## Errores comunes

| Mensaje | Acción |
|---|---|
| 401 / sesión inválida | vuelva a iniciar sesión |
| 403 / sin permiso | solicite revisión de rol y alcance |
| compañía activa requerida | seleccione una compañía autorizada |
| archivo inválido | use `.xlsx`, revise tamaño y encabezados |
| coincidencia ambigua | asocie manualmente o envíe a revisión |
| servicio no disponible | conserve hora/trace id y contacte soporte |

No reintente repetidamente una acción de escritura si no conoce su resultado.
