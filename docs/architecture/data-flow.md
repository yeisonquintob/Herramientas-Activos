# Flujo de datos

```text
Admin Web / Mobile PWA
        │ HTTPS + JWT
        ▼
API ── autenticación ── sesión persistida ── permisos
        │
        ├─ resolución de compañía ── NaviMasterDb
        │                              │
        │                              └─ acceso y conexión autorizada
        ▼
NaviToolsAssetsDbContext dinámico ── Base operacional de la compañía
        │
        ├─ SQL Server: inventario, procesos, auditoría e importaciones
        ├─ MinIO: documentos y archivos
        └─ Hangfire/Worker: tareas fuera del request
```

La identidad y `CompanyId` proceden del token validado y de la sesión
persistida. El cliente no selecciona una cadena de conexión. El middleware
valida acceso contra la base maestra y resuelve el contexto operacional.

Las cargas Excel pasan por validación, almacenamiento, staging, reporte de
calidad y confirmación. Crear o actualizar activos nunca ocurre durante la
primera lectura.

Los reportes usan el contexto ya resuelto, proyecciones y paginación. Las
exportaciones grandes se escriben en flujo. Los documentos guardan metadatos
en SQL y contenido en MinIO.

La ruta de recuperación es: detener escrituras, respaldar evidencia actual,
restaurar SQL/MinIO de la misma compañía, validar checksums y ejecutar smoke
tests antes de reabrir.
