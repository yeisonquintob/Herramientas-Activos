# Configuración segura - NAVI Herramientas

## Objetivo

Definir cómo se manejarán las variables de entorno, cadenas de conexión, claves de MinIO, configuración de Hangfire y archivos locales del proyecto NAVI Herramientas.

## Archivos que sí pueden subirse al repositorio

- appsettings.json
- docker-compose.yml
- local.env.example
- minio.example.json
- Documentación técnica

## Archivos que NO deben subirse al repositorio

- appsettings.Development.json
- appsettings.Local.json
- appsettings.Production.json
- local.env
- .env
- Archivos con contraseñas, tokens o secretos reales

## Servicios locales

SQL Server:

- Host: localhost
- Puerto: 1434
- Base de datos: NaviToolsAssetsDb

MinIO:

- API: http://localhost:9100
- Consola: http://localhost:9101
- Bucket: navi-tools-documents

Hangfire:

- Dashboard: /hangfire
- Base de datos: NaviToolsAssetsDb
- Esquema: HangFire

## Variables de entorno recomendadas

ConnectionStrings__NaviToolsAssetsDb
Minio__Endpoint
Minio__AccessKey
Minio__SecretKey
Minio__BucketName
Minio__UseSsl
Worker__Name
Hangfire__DashboardPath

## Regla principal

Las credenciales reales deben vivir en user-secrets, variables de entorno del servidor o archivos locales ignorados por Git.
Nunca deben quedar quemadas en archivos versionados.
