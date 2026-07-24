# Despliegue productivo

## Precondiciones

1. Cerrar todos los pendientes de la lista de readiness.
2. Probar backup y restore en un ambiente aislado.
3. Probar migraciones idempotentes sobre una copia reciente.
4. Proveer TLS mediante balanceador o reverse proxy administrado.
5. Inyectar secretos desde el servicio del entorno.
6. Proveer base maestra, plantilla y bases operacionales.
7. Proveer bucket y usuario MinIO de privilegio mínimo.

## Secuencia

1. Marcar ventana y congelar escrituras.
2. Respaldar SQL, MinIO y configuración no secreta.
3. Aplicar migraciones con una identidad separada.
4. Desplegar API y validar liveness/readiness.
5. Desplegar Worker, Admin y Mobile.
6. Ejecutar login, selector, inventario, documento e importación controlada.
7. Verificar auditoría, sesiones y métricas.
8. Reabrir tráfico.

La API no ejecuta `EnsureCreated`, seed ni migraciones en Production.

## Rollback

1. Retirar tráfico de la versión nueva.
2. Detener Worker y escrituras.
3. Volver a las imágenes de la versión anterior.
4. Si hubo cambio de esquema incompatible, restaurar el backup verificado; no
   ejecutar `Down` sin revisión.
5. Validar integridad SQL/MinIO y smoke tests.
6. Registrar incidente y evidencia.

## Operación

- Rotar JWT, SQL y MinIO mediante el gestor de secretos.
- Conservar backups cifrados fuera del host.
- Alertar por readiness, fallos de jobs, sesiones y auditoría crítica.
- No exponer Swagger ni Hangfire públicamente.
