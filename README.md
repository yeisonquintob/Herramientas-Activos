# NAVI Herramientas y Activos

Solución .NET 8 para inventario, ubicación, asignaciones, hoja de vida,
mantenimiento, compras, tomas físicas, conciliación, documentos, reportes e
importación de herramientas y activos fijos.

## Proyectos

| Proyecto | Responsabilidad |
|---|---|
| `Domain` | Entidades, enums y reglas sin dependencias de infraestructura |
| `Application` | Contratos, abstracciones y contexto de tenant |
| `Infrastructure` | EF Core, SQL Server, seguridad, archivos y tenancy |
| `Api` | HTTP, autenticación, autorización, auditoría y OpenAPI |
| `Admin` | Admin Web Blazor Server |
| `MobilePwa` | PWA Blazor WebAssembly |
| `Worker` | Jobs Hangfire |
| `Tests` | Pruebas unitarias, de arquitectura, autorización y API |

Solución: `Navitrans.ToolsAssets.Management.sln`.

## Requisitos

- .NET SDK 8.
- SQL Server 2022.
- MinIO.
- Docker y Compose, opcionales para infraestructura/contenedores.

No hay credenciales funcionales versionadas. Use variables de entorno o un
gestor de secretos.

## Desarrollo local

```bash
cp docker/env/local.env.example docker/env/local.env
docker compose --env-file docker/env/local.env \
  -f docker/docker-compose.development.yml up -d
dotnet restore Navitrans.ToolsAssets.Management.sln
dotnet build Navitrans.ToolsAssets.Management.sln --configuration Debug --nologo
dotnet test Navitrans.ToolsAssets.Management.sln --configuration Debug --no-build --nologo
```

Configure las conexiones y secretos mediante variables antes de iniciar:

```bash
export ConnectionStrings__NaviToolsAssetsDb='<conexión-operacional>'
export Jwt__SigningKey='<clave-segura-de-64-o-más-caracteres>'
export Minio__AccessKey='<access-key>'
export Minio__SecretKey='<secret-key>'
dotnet run --project src/Navi.ToolsAssets.Api
```

Puertos locales habituales:

- API: `http://localhost:5218`
- Admin: `http://localhost:5264`
- Mobile: `http://localhost:5285`
- MinIO API/consola: `http://localhost:9100` / `http://localhost:9101`

## Base de datos

Las migraciones no se aplican al arrancar Production:

```bash
NAVI_TOOLS_DB_CONNECTION='<conexión-segura>' dotnet ef database update \
  --context NaviToolsAssetsDbContext \
  --project src/Navi.ToolsAssets.Infrastructure \
  --startup-project src/Navi.ToolsAssets.Infrastructure
```

Para el contexto maestro use `NAVI_MASTER_DB_CONNECTION` y
`NaviMasterDbContext`. Consulte [migraciones](docs/database/migrations.md) y
[backup/restore](docs/database/backup-restore.md).

## Producción

La configuración productiva está en
`docker/docker-compose.production.yml`. Antes de desplegar:

1. Provisione SQL, bases, usuario limitado y credenciales MinIO.
2. Genere y aplique scripts idempotentes después de probarlos en una copia.
3. Configure CORS, JWT, URLs públicas y secretos externos.
4. Ejecute la lista de salida de `docs/release/mvp-readiness-checklist.md`.

```bash
docker compose --env-file /ruta/segura/navi.env \
  -f docker/docker-compose.production.yml config
docker compose --env-file /ruta/segura/navi.env \
  -f docker/docker-compose.production.yml up -d --build
```

## Documentación

- [Arquitectura](docs/architecture/overview.md)
- [Seguridad](docs/architecture/security.md)
- [Multicompañía](docs/architecture/multitenancy.md)
- [Importación](docs/imports/excel-imports.md)
- [Despliegue](docs/deployment/production.md)
- [Pruebas](docs/development/testing.md)
- [Manual de usuario](docs/user-manual/manual-usuario.md)
- [Estado de preparación](docs/release/mvp-readiness-checklist.md)

## Estado honesto

La solución compila y cuenta con una base empresarial reforzada. La
habilitación productiva multicompañía permanece condicionada a provisionar dos
bases reales, completar clonación/restore automatizados y ejecutar pruebas de
aislamiento e integración con SQL/MinIO. No debe etiquetarse como MVP estable
hasta cerrar esos puntos.
