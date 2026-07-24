# Variables de entorno

## API

```text
ConnectionStrings__NaviToolsAssetsDb=<base de seguridad/compatibilidad>
ConnectionStrings__NaviMasterDb=<NaviMasterDb>
Jwt__SigningKey=<mínimo 32 bytes>
Jwt__Issuer=Navi.ToolsAssets
Jwt__Audience=Navi.ToolsAssets.Clients
Minio__Endpoint=<host:puerto>
Minio__AccessKey=<secreto>
Minio__SecretKey=<secreto>
Minio__BucketName=navi-tools-documents
Minio__UseSsl=true
Cors__AllowedOrigins__0=https://admin.ejemplo
Cors__AllowedOrigins__1=https://mobile.ejemplo
Tenancy__Enabled=false
TenantDatabases__Company_EMPRESA1=<conexión operativa>
```

`TenantDatabases__*`, conexiones, JWT y MinIO deben proceder de un gestor de secretos. No deben
guardarse en `appsettings.json`, imágenes Docker, compose ni logs.

## Herramientas EF

```text
NAVI_TOOLS_DB_CONNECTION=<conexión operativa para tooling>
NAVI_MASTER_DB_CONNECTION=<conexión maestra para tooling>
```

Estas variables son locales al proceso de migración y no deben persistirse en scripts
versionados.
