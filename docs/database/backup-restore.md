# Backup y restauración por compañía

## Estado actual

`POST /api/companies/{companyId}/backups` registra una solicitud protegida por
`Companies.Backup`. La solicitud incluye compañía, usuario, timestamp, nombre lógico, objeto
privado, versión y campos para checksum/retención.

El endpoint no ejecuta `BACKUP DATABASE` dentro del request. El procesamiento productivo debe
realizarlo un worker con una identidad SQL limitada y un `CompanyId` explícito.

## Reglas del worker

1. Revalidar compañía, acceso y estado.
2. Resolver la conexión mediante `ConnectionKey`, nunca mediante valores del request.
3. Validar el nombre de base contra el registro maestro.
4. Crear un archivo en una ruta controlada por infraestructura.
5. Calcular SHA-256.
6. Cargarlo a almacenamiento privado.
7. Actualizar tamaño, checksum, versión, estado y expiración.
8. Registrar auditoría.
9. Eliminar de forma controlada el archivo temporal.

Los nombres de base y rutas no se concatenan desde entrada libre. Las descargas deben usar URL
temporal o streaming autorizado y nunca hacer público el bucket.

## Restauración

La restauración no está automatizada en esta fase. Procedimiento recomendado:

1. Suspender la compañía.
2. Crear backup de seguridad.
3. Verificar checksum y versión del archivo seleccionado.
4. Restaurar en una base nueva, no sobre la activa.
5. Ejecutar comprobaciones de integridad y migraciones pendientes.
6. Probar acceso con una cuenta técnica.
7. Cambiar la referencia maestra en una operación controlada.
8. Reactivar y auditar.
9. Conservar la base anterior durante la ventana de rollback.

No se debe exponer un endpoint de restauración hasta contar con pruebas de integración, control
de concurrencia y procedimiento de compensación.
