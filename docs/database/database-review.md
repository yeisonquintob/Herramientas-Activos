# Revisión de base de datos NAVI

Fecha: 2026-07-24

Contexto operativo: `NaviToolsAssetsDbContext`

Proveedor: SQL Server / Entity Framework Core 8.0.28

## Migraciones encontradas

1. `20260620023050_InitialCreate`
2. `20260620034701_CreateCoreDomainEntities`
3. `20260620044650_AddToolSpecializedFlag`
4. `20260620224637_AddImportBatches`
5. `20260621004539_AddTechnicalLifeRecordDetails`
6. `20260621004836_FixTechnicalLifeRecordSchemasAndFilters`
7. `20260621010937_AddMaintenanceTechnicalScheduleFields`
8. `20260621222750_AddSettingsUsersAndRoles`
9. `20260622001914_AddSettingCatalogItems`

El snapshot existe y corresponde al contexto operativo. No se agregó una
migración durante esta revisión porque todavía no se ha comparado el esquema
real de cada ambiente y no se autoriza recrear o borrar datos.

## Tablas y áreas

| Esquema | Tablas principales |
|---|---|
| `Organization` | `SystemParameters`, `SettingCatalogItems`, `Zones`, `Branches`, `ToolLocations`, `ResponsiblePeople` |
| `Inventory` | `ToolTypes`, `ToolCategories`, `ToolAssets`, `ToolAccessories` |
| `Safety` | `ToolSafePractices` |
| `LifeCycle` | `ToolLifeCycleEvents` |
| `Documents` | `ToolDocuments` |
| `Loans` | `ToolLoans`, `ToolLoanItems` |
| `Damages` | `DamageReports` |
| `Maintenance` | `MaintenanceRecords`, `MaintenanceRequests` |
| `PhysicalCounts` | `PhysicalCounts`, `PhysicalCountItems` y entidades de participantes/reportes/evidencias |
| `Purchases` | `PurchaseRequests`, `PurchaseRequestEvidences` |
| `Imports` | `ImportBatches`, `ImportRows` |
| `Sync` | `FenixReconciliationRecords` |
| Seguridad | `AppUsers`, `AppRoles` según la migración de seguridad |

## Relaciones y cascadas

- Organización, activos y procesos operativos usan `DeleteBehavior.Restrict`
  para evitar borrar históricos por cascada.
- Accesorios y prácticas seguras se eliminan en cascada con su activo.
- Filas de importación se eliminan en cascada con su lote.
- La mayoría de entidades implementa borrado lógico mediante `IsDeleted`.

No se detectó una cascada directa que borre masivamente activos, préstamos,
mantenimientos, documentos o tomas físicas.

## Índices existentes relevantes

- códigos únicos de zonas, sedes, tipos y categorías;
- sede + código de ubicación;
- código interno de activo;
- códigos Fénix y activo fijo filtrados cuando no son nulos;
- serial de activo;
- números únicos de préstamo, daño, mantenimiento, toma, compra e importación;
- lote + número de fila de importación;
- índices de evidencia por workspace, solicitud y composición funcional.

## Índices candidatos

Requieren medición y plan de ejecución antes de crear migración:

- `ToolAssets(BranchId, OperationalStatus, IsDeleted)`;
- `ToolAssets(ResponsiblePersonId, IsDeleted)`;
- `ToolLifeCycleEvents(ToolAssetId, RegisteredAt)`;
- `ToolDocuments(ToolAssetId, CreatedAt)`;
- `PhysicalCountParticipants(PhysicalCountId, UserId/ResponsiblePersonId)`;
- `PhysicalCountItems(PhysicalCountId, ToolAssetId)`;
- `MaintenanceRequests(BranchId, Status, CreatedAt)`;
- `PurchaseRequests(BranchId, Status, CreatedAt)`;
- `ImportRows(ImportBatchId, ResultStatus)`.

No se crean todavía para evitar índices redundantes y bloqueo inesperado sobre
una base con datos.

## Riesgos

1. Varias entidades de toma física y seguridad dependen parcialmente de
   convenciones de EF; conviene mover su configuración a clases
   `IEntityTypeConfiguration` durante una modificación funcional futura.
2. El login ejecuta comprobaciones/ajustes de esquema en tiempo de request.
   Deben reemplazarse por migraciones antes de producción.
3. `BaseEntity` usa `DateTime`; los nuevos eventos/auditorías deben usar
   `DateTimeOffset` UTC y migrarse gradualmente.
4. La base actual no tiene `CompanyId`; el aislamiento empresarial se diseñará
   con base maestra y una base operativa por compañía.
5. No debe usarse `EnsureCreated` en producción. No se encontró su uso en el
   contexto revisado.

## Validaciones pendientes por ambiente

- ejecutar `dotnet ef migrations list` contra una cadena autorizada;
- comparar `__EFMigrationsHistory` con esta lista;
- revisar tamaño y selectividad antes de agregar índices;
- ejecutar `dotnet ef migrations script --idempotent` y revisar que no haya
  `DROP COLUMN` o `DROP TABLE` inesperados;
- verificar backups antes de aplicar cualquier migración.

## Recomendación

Mantener las migraciones operativas separadas de las futuras migraciones de
`NaviMasterDbContext`. Ninguna base se recreará automáticamente.
