# Migraciones

## Contexto operativo

Las migraciones de `NaviToolsAssetsDbContext` están en
`src/Navi.ToolsAssets.Infrastructure/Persistence/Migrations`.

La migración `20260724171429_AddEnterpriseSecurity`:

- valida que no existan nombres de usuario duplicados ni mayores a 150 caracteres;
- normaliza la longitud e índice único de `AppUsers.UserName`;
- incorpora bloqueo, fecha de cambio y `SecurityStamp`;
- preserva una columna `PasswordHash` heredada si ya existe;
- crea `Security.UserSessions`;
- crea `Security.AuditLogs`;
- crea índices para validación de sesión y consulta de auditoría.

No borra contraseñas heredadas ni datos operativos. El `Down` conserva `PasswordHash` de forma
intencional para evitar pérdida de hashes que pudieran haber sido creados por el adaptador
legado.

## Ejecución segura

Antes de Production:

1. Respaldar la base.
2. Consultar duplicados y longitudes de `AppUsers.UserName`.
3. Probar la migración sobre una copia restaurada.
4. Revisar el SQL generado.
5. Aplicar con una identidad de despliegue limitada.
6. Validar login, sesiones y auditoría.

```bash
NAVI_TOOLS_DB_CONNECTION='<conexión-segura>' dotnet ef migrations script \
  --project src/Navi.ToolsAssets.Infrastructure \
  --startup-project src/Navi.ToolsAssets.Infrastructure \
  --idempotent
```

La fábrica de diseño de EF usa exclusivamente `NAVI_TOOLS_DB_CONNECTION`. Así las herramientas
no arrancan la API, Hangfire ni integraciones externas durante una inspección o generación de
migración.

La migración fue generada y validada contra el modelo, pero no se aplicó automáticamente a
ninguna base de datos.

## Advertencias conocidas del modelo

EF Core informa relaciones requeridas hacia `PhysicalCount` cuyos principales tienen filtro
global de borrado lógico. Es un comportamiento preexistente que debe corregirse con filtros
compatibles en las entidades dependientes o convirtiendo la navegación en opcional mediante una
migración revisada. No se cambió en la fase de seguridad para evitar alterar el resultado de
consultas operativas sin pruebas de regresión.

`EstimatedDowntimeHours` está configurado explícitamente como `decimal(18,2)`.

## Política

- No usar `EnsureCreated` en Production.
- No ejecutar migraciones al arrancar una instancia productiva.
- No mezclar migraciones del futuro contexto maestro con las operativas.
- Conservar scripts idempotentes y evidencia del respaldo por despliegue.
