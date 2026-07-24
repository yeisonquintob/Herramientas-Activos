# MinIO

MinIO almacena archivos; SQL conserva metadatos y claves de objeto.

Variables:

```text
Minio__Endpoint
Minio__AccessKey
Minio__SecretKey
Minio__BucketName
Minio__UseSsl
```

Use un usuario de aplicación, no la cuenta root. Limite permisos al bucket de
NAVI, habilite TLS fuera de la red interna y configure ciclo de vida/retención.
No registre URLs firmadas, claves ni contenido.

Backup y restore de documentos deben coordinarse con la misma marca temporal de
SQL. Una restauración parcial puede dejar metadatos sin objeto o archivos
huérfanos.
