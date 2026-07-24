# NAVI — Informe final de transformación candidata a MVP empresarial

Fecha: 24 de julio de 2026

Repositorio: `/Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos`

Rama: `feature/navi-mvp-empresarial`

Tag de retorno: `navi-pre-mvp-empresarial` (`3d6f5ae`)

## Resumen ejecutivo

Se ejecutaron secuencialmente las cinco fases solicitadas sobre una rama
dedicada y con un respaldo versionado antes de cada fase. La solución conserva
las vistas y componentes visuales existentes, compila en Debug y Release sin
advertencias ni errores, y supera 19 pruebas automatizadas en ambas
configuraciones.

La base técnica incorpora autenticación y autorización reales, sesiones y
auditoría, una base maestra multicompañía, resolución dinámica de base,
importación Excel trazable, reportes eficientes, health checks, contenedores y
documentación operativa.

La salida es una **candidata técnica**, no un MVP empresarial estable. No se
ejecutaron pruebas destructivas o de integración contra bases productivas y
faltan aprovisionamiento de bases, backup/restore real, dos tenants SQL,
integración MinIO, carga y pentest.

## Commits creados

| Fase | Commit |
|---|---|
| Punto inicial | `3d6f5ae` / tag `navi-pre-mvp-empresarial` |
| Fase 1 | `98e4262` — `NAVI Fase 1 - arquitectura y compilación` |
| Fase 2 | `146e5f3` — `NAVI Fase 2 - seguridad` |
| Fase 3 | `2df3dad` — `NAVI Fase 3 - multicompañía` |
| Fase 4 | `64f65ab` — `NAVI Fase 4 - importación y calidad de datos` |
| Fase 5 | `fbc3869` — `NAVI Fase 5 - pruebas y producción` |

No se crea el commit `NAVI MVP empresarial estable`, porque los criterios
empresariales pendientes impiden sostener esa afirmación.

## Arquitectura final

```text
Domain
  ↑
Application ─── Shared
  ↑
Infrastructure
  ↑
API ─── Admin Web / Mobile PWA / Worker
  ↑
Tests
```

- `Domain`: entidades operativas, seguridad, importación y tenancy.
- `Application`: abstracciones, contexto de compañía y resultados.
- `Infrastructure`: EF Core, SQL Server, MinIO, seguridad y contextos.
- `API`: endpoints, autorización, middleware, auditoría y health.
- `Admin` y `MobilePwa`: clientes configurados y componentes reutilizables.
- `Worker`: ejecución Hangfire.
- `Shared`: contratos y catálogo central de permisos.
- `Tests`: reglas, arquitectura, permisos y pruebas HTTP.

## Seguridad implementada

- JWT con firma, issuer, audience, expiración y validación de sesión.
- Permisos derivados de claims emitidos por servidor.
- Eliminación de confianza en headers `X-Navi-*`.
- `PasswordHasher` ASP.NET Core y actualización controlada del hash SHA-256
  heredado.
- Bloqueo temporal de cuenta, sesiones persistidas, revocación y expiración.
- Auditoría de acciones críticas.
- Política global autenticada, permisos por endpoint y rate limiting.
- Swagger/Hangfire no expuestos públicamente en Production.
- Encabezados de seguridad y ProblemDetails.
- CORS por ambiente.
- Restricciones de archivo y neutralización CSV.
- Cero dependencias vulnerables conocidas según NuGet al 24/07/2026.

Pendiente: cookie `HttpOnly`/BFF o estrategia equivalente para evitar conservar
el token en almacenamiento del cliente; refresh/rotación según política; pentest
e integración con infraestructura real.

## Multicompañía implementada

- `NaviMasterDbContext` y migración inicial.
- Entidades de compañía, base, acceso, backup, importación y versión.
- Selector de compañía en Admin.
- Validación de acceso y middleware de resolución.
- `TenantContext` inmutable durante la solicitud.
- `DbContext` operacional dinámico por compañía.
- Endpoints de catálogo y administración de compañías.

Pendiente: mover identidades/sesiones compartidas a la base maestra, crear la
plantilla, automatizar aprovisionamiento/clonación/migración, incorporar tenant
en todos los jobs y probar dos bases reales.

## Importación y reportes

- `.xlsx` validado por extensión, MIME, firma, tamaño y filas.
- Identidad y compañía derivadas de claims/contexto.
- Normalización, staging, reporte de calidad y auditoría.
- Lotes transaccionales e idempotentes.
- Decisiones de crear, actualizar, ignorar, vincular y revisar.
- Calidad exportable en JSON/CSV.
- Reportes ejecutivos con agregados SQL, `AsNoTracking`, paginación y CSV por
  streaming.

Pendiente: pruebas integrales contra SQL Server/MinIO, archivos máximos,
concurrencia e importaciones simultáneas de compañías.

## Migraciones

Operacional:

- `20260724171429_AddEnterpriseSecurity`
- `20260724180238_AddImportQualityTracking`
- `20260724180947_AddImportCompletionTracking`

Maestra:

- `20260724174641_InitialNaviMaster`

`dotnet ef migrations has-pending-model-changes` confirmó que ambos snapshots
están sincronizados. EF reporta una advertencia heredada: `PhysicalCount` tiene
filtro global y es extremo requerido de tres relaciones. Debe evaluarse si las
entidades dependientes necesitan filtro equivalente u opcionalidad.

## Compilación y pruebas

| Control | Resultado |
|---|---|
| Restore | Correcto |
| Build Debug | Correcto, 0 advertencias, 0 errores |
| Tests Debug | 19/19, 0 omitidas, 0 fallidas |
| Build Release | Correcto, 0 advertencias, 0 errores |
| Tests Release | 19/19, 0 omitidas, 0 fallidas |
| Modelo operacional | Sin cambios pendientes |
| Modelo maestro | Sin cambios pendientes |
| Paquetes vulnerables | Ninguno reportado |
| Compose development | Sintaxis válida |
| Compose production | Sintaxis válida |
| Imágenes productivas | API, Admin, Mobile y Worker construidas correctamente |

Las pruebas cubren reglas de archivo Excel, arquitectura, permisos,
autorización, contexto de compañía, liveness, headers de seguridad, Swagger y
rechazo de identidad falsificada. No reemplazan las pruebas funcionales con
SQL/MinIO.

## Docker y operación

- Dockerfiles multi-stage para API, Admin, Mobile y Worker.
- API, Admin y Worker ejecutan como usuario `app`; Mobile usa Nginx no
  privilegiado.
- Liveness y readiness separados.
- Health checks de API, Admin, Mobile y proceso Worker.
- Compose productivo con redes, volúmenes, restart policies y secretos
  requeridos por variables.
- Persistencia prevista para SQL, MinIO, backups y Data Protection Keys.
- `.dockerignore` excluye artefactos, respaldos, exportaciones y secretos
  locales.

La terminación TLS corresponde al reverse proxy/balanceador del ambiente.
`Tenancy__Enabled` permanece desactivado por defecto hasta cerrar las pruebas de
aislamiento.

## Archivos principales

- `src/Navi.ToolsAssets.Api/Program.cs`
- `src/Navi.ToolsAssets.Api/Controllers/AuthController.cs`
- `src/Navi.ToolsAssets.Api/Controllers/CompaniesController.cs`
- `src/Navi.ToolsAssets.Api/Controllers/ImportsController.cs`
- `src/Navi.ToolsAssets.Api/Controllers/Reports/ExecutiveReportsController.cs`
- `src/Navi.ToolsAssets.Api/Security/`
- `src/Navi.ToolsAssets.Api/Tenancy/`
- `src/Navi.ToolsAssets.Infrastructure/Tenancy/`
- `src/Navi.ToolsAssets.Infrastructure/Persistence/Migrations/`
- `src/Navi.ToolsAssets.Shared/Security/`
- `tests/Navi.ToolsAssets.Tests/`
- `docker/docker-compose.production.yml`
- `docker/docker-compose.development.yml`
- Dockerfiles de los cuatro proyectos desplegables
- `docs/`

Los cambios visuales locales que ya existían antes de este proceso no se
incluyeron en los commits de las fases.

## Configuración requerida

- `ConnectionStrings__NaviToolsAssetsDb`
- `ConnectionStrings__NaviMasterDb`
- `Jwt__SigningKey`, `Jwt__Issuer`, `Jwt__Audience`
- `Minio__Endpoint`, `Minio__AccessKey`, `Minio__SecretKey`
- `Cors__AllowedOrigins__*`
- `PUBLIC_API_BASE_URL`
- `Tenancy__Enabled`
- conexiones seguras de bases por compañía

La referencia completa está en
[`environment-variables.md`](../deployment/environment-variables.md).

## Inicio de desarrollo

```bash
dotnet restore Navitrans.ToolsAssets.Management.sln
dotnet build Navitrans.ToolsAssets.Management.sln --configuration Debug --nologo
dotnet test Navitrans.ToolsAssets.Management.sln --configuration Debug --no-build --nologo
dotnet run --project src/Navi.ToolsAssets.Api
```

Los servicios de UI se inician por separado con la configuración local
documentada en [`setup.md`](../development/setup.md).

## Despliegue

1. Cerrar todos los ítems NO-GO del checklist.
2. Respaldar SQL/MinIO y comprobar lectura/restauración.
3. Inyectar secretos desde el gestor del ambiente.
4. Probar migraciones sobre una copia.
5. Construir imágenes desde un commit aprobado.
6. Desplegar detrás de TLS.
7. Ejecutar smoke, aislamiento, importación, documentos y auditoría.
8. Habilitar tráfico después de la aprobación.

## Rollback

1. Retirar tráfico y detener escrituras/Worker.
2. Volver a imágenes del commit anterior.
3. Restaurar el backup validado cuando el cambio de esquema lo exija.
4. Verificar login, inventario, documentos, jobs y auditoría.
5. Preservar logs y evidencia.

No se debe aplicar `Down()` automáticamente ni ejecutar comandos Git
destructivos para un rollback de datos.

## Riesgos pendientes y decisión

Riesgos bloqueantes:

1. Aprovisionamiento, plantilla y aislamiento multicompañía no probados en dos
   SQL Server reales.
2. Backup/restore por compañía aún no ejecutable de extremo a extremo.
3. Importación/MinIO sin prueba integral.
4. Sesiones/identidad compartida todavía requieren migración definitiva a
   `NaviMasterDb`.
5. Token en almacenamiento del cliente.
6. Sin prueba de carga, pentest ni recuperación ante desastre.
7. URLs `localhost` heredadas como fallback de desarrollo.

Decisión: **NO-GO para declarar “NAVI MVP empresarial estable”**. El código
queda en condición de candidata técnica compilable y probada, con los pendientes
detallados y sin ocultar las brechas.
