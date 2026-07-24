# Docker

## Archivos

- `docker/docker-compose.development.yml`: SQL y MinIO para desarrollo.
- `docker/docker-compose.production.yml`: API, Admin, Mobile, Worker, SQL y MinIO.
- `.env.example`: nombres de variables, sin secretos funcionales.
- Dockerfiles multi-stage en cada proyecto desplegable.

Las imágenes .NET ejecutan con el usuario `app`; Mobile usa Nginx sin
privilegios. API y Admin conservan Data Protection Keys en volúmenes. SQL,
MinIO y backups tienen volúmenes independientes.

## Validación

```bash
docker compose --env-file /ruta/segura/navi.env \
  -f docker/docker-compose.production.yml config --quiet
```

No ejecute `up` antes de provisionar bases, usuario SQL limitado, bucket y
credenciales MinIO. Compose no aplica migraciones.

## Health

- API liveness: `/health/live`.
- API readiness SQL: `/health/ready`.
- Contenedores: health checks propios y dependencias condicionadas.
- Worker: comprueba que el proceso administrado permanezca como PID 1.

Liveness no consulta dependencias. Readiness no crea esquemas. En modo
multicompañía valida la conexión base del despliegue, no todas las compañías;
la monitorización productiva debe añadir una comprobación autorizada por tenant.

## Redes

`frontend` comunica Admin/Mobile/API. `data` es interna y comunica API/Worker
con SQL/MinIO. SQL y MinIO no publican puertos en el compose productivo.
