# Informe técnico maestro — NAVI Herramientas & Activos / Fenix365

**Fecha del informe:** 2026-06-28  
**Proyecto:** NAVI Herramientas & Activos / Fenix365  
**Ruta local del proyecto:** `/Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos`  
**Export de código analizado:** `PROYECTO_CODIGO_COMPLETO_20260628_172141.txt`  
**Objetivo:** dejar un documento suficientemente completo para continuar el desarrollo en un nuevo chat, con contexto funcional, técnico, estado actual, pendientes, reglas visuales y próximos pasos.

---

## 0. Prompt recomendado para continuar en un nuevo chat

Copia este bloque en un nuevo chat junto con el archivo de código actualizado:

```text
Estoy trabajando en el proyecto NAVI Herramientas & Activos / Fenix365.

Ruta local:
/Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos

Stack:
.NET 8, ASP.NET Core API, Blazor Server Admin, Blazor/WebAssembly Mobile PWA, SQL Server Docker, MinIO, Swagger, EF Core, arquitectura por capas Domain/Application/Infrastructure/API/Admin/Mobile/Shared.

Objetivo del sistema:
Gestionar herramientas y activos fijos de NAVI/Navitrans con inventario, disponibilidad, asignación, préstamos, hoja de vida técnica, mantenimiento, compras MRO/SPC, toma física, conciliación, documentos, reportes, configuración, usuarios, roles y permisos.

Reglas de trabajo:
1. Responder en español.
2. Dar scripts para macOS/zsh listos para copiar y pegar.
3. Si se cambia Admin, detener puerto 5264 y compilar Admin al final.
4. Si se cambia API, detener puerto 5218 y compilar API al final.
5. Si se cambia Mobile PWA, detener puerto 5285 y compilar Mobile al final.
6. No crear backups ni archivos .sh salvo que se solicite.
7. No exponer contraseñas reales; usar docker/env/local.env.
8. Mantener los banners estandarizados con degradado negro/morado.
9. Mantener formularios compactos, legibles y sin espacios innecesarios.

Estado actual:
El proyecto ya tiene módulos Admin y API funcionales para inventario, disponibilidad, asignación, historial, hoja de vida, compra, mantenimiento, toma física, conciliación, documentos, reportes, configuración, usuarios y roles. Se están realizando ajustes visuales y de flujo.

Prioridades pendientes:
- Renombrar "Consultar Mantenimientos" a "Consultar Solicitudes" o "Consulta de Solicitudes".
- Corregir el formulario de solicitud de asignación: al seleccionar herramienta, no debe quedar mensaje/listado "no se encontraron herramientas".
- Terminar la organización visual compacta del resumen lateral de Reportes si aún se ve montado.
- Fortalecer Documentos: tipos reales en base de datos, no depender de parsing en descripción.
- Fortalecer exportaciones: CSV ya existe en vista, falta PDF/Excel formal por backend.
- Validar permisos por rol en cada acción crítica.
- Revisar que cada acción de cambio cree trazabilidad o evento de ciclo de vida.
```

---

## 1. Resumen ejecutivo del sistema

NAVI Herramientas & Activos / Fenix365 es una solución para controlar el ciclo de vida operativo de herramientas y activos fijos de menor o mayor valor. El sistema busca comportarse como una extensión administrativa alineada con procesos tipo Dynamics/Fenix365, pero especializada en herramientas de taller, activos tecnológicos, accesorios, evidencia documental, mantenimiento, compras, toma física, conciliación y trazabilidad.

El sistema combina tres frentes:

1. **Administración web:** vista principal para coordinadores, administradores, responsables de taller, almacén, compras, mantenimiento y control.
2. **API:** expone endpoints para inventario, documentos, préstamos, mantenimientos, compras, toma física, conciliación, configuración, seguridad y hoja de vida.
3. **PWA móvil:** enfocada en usuario operativo, especialmente inventario físico, disponibilidad, asignación y acciones en campo.

El estado actual es de **MVP funcional en evolución visual y funcional**. Ya existen las bases de dominio, entidades, controladores, vistas Blazor y varias integraciones internas. La conversación más reciente se ha enfocado en estandarizar experiencia visual, compactar vistas, ajustar documentos y cerrar el centro de reportes.

---

## 2. Evidencia del código cargado

El export analizado corresponde al proyecto NAVI Herramientas & Activos / Fenix365, generado el **28 de junio de 2026 a las 17:21:41 -05**, en la ruta local `/Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos`. El export indica que excluye carpetas pesadas o generadas como `bin`, `obj`, `.git`, `node_modules`, `backups` y archivos binarios, además de enmascarar posibles secretos.

La solución contiene proyectos por capas y módulos, incluyendo Admin, API, Application, Domain, Infrastructure, MobilePwa y Shared. El archivo de estructura lista scripts de build, scripts Docker, scripts SQL, documentación, componentes Admin, controladores API, entidades Domain, migraciones, storage MinIO y PWA móvil.

---

## 3. Arquitectura general

### 3.1 Solución y proyectos

La solución `Navitrans.ToolsAssets.Management.sln` agrupa los siguientes proyectos principales:

- `Navi.ToolsAssets.Domain` → `src\Navi.ToolsAssets.Domain\Navi.ToolsAssets.Domain.csproj`
- `Navi.ToolsAssets.Application` → `src\Navi.ToolsAssets.Application\Navi.ToolsAssets.Application.csproj`
- `Navi.ToolsAssets.Infrastructure` → `src\Navi.ToolsAssets.Infrastructure\Navi.ToolsAssets.Infrastructure.csproj`
- `Navi.ToolsAssets.Shared` → `src\Navi.ToolsAssets.Shared\Navi.ToolsAssets.Shared.csproj`
- `Navi.ToolsAssets.Api` → `src\Navi.ToolsAssets.Api\Navi.ToolsAssets.Api.csproj`
- `Navi.ToolsAssets.Admin` → `src\Navi.ToolsAssets.Admin\Navi.ToolsAssets.Admin.csproj`
- `Navi.ToolsAssets.MobilePwa` → `src\Navi.ToolsAssets.MobilePwa\Navi.ToolsAssets.MobilePwa.csproj`
- `Navi.ToolsAssets.Worker` → `src\Navi.ToolsAssets.Worker\Navi.ToolsAssets.Worker.csproj`


### 3.2 Capas

**Domain**
- Entidades de negocio.
- Enumeraciones del estado de herramientas, sincronización, conciliación, mantenimiento y toma física.
- Entidades principales:
- `src/Navi.ToolsAssets.Domain/Entities/Configuration/SettingCatalogItem.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Damages/DamageReport.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Documents/ToolDocument.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Imports/ImportBatch.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Imports/ImportRow.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Inventory/ToolAccessory.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Inventory/ToolAsset.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Inventory/ToolCategory.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Inventory/ToolType.cs`
- `src/Navi.ToolsAssets.Domain/Entities/LifeCycles/ToolLifeCycleEvent.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Loans/ToolLoan.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Loans/ToolLoanItem.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Maintenance/MaintenanceRecord.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Maintenance/ToolMaintenanceRequest.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Organization/Branch.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Organization/ResponsiblePerson.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Organization/SystemParameter.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Organization/ToolLocation.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Organization/Zone.cs`
- `src/Navi.ToolsAssets.Domain/Entities/PhysicalCounts/PhysicalCount.cs`
- `src/Navi.ToolsAssets.Domain/Entities/PhysicalCounts/PhysicalCountExtraItem.cs`
- `src/Navi.ToolsAssets.Domain/Entities/PhysicalCounts/PhysicalCountItem.cs`
- `src/Navi.ToolsAssets.Domain/Entities/PhysicalCounts/PhysicalCountParticipant.cs`
- `src/Navi.ToolsAssets.Domain/Entities/PhysicalCounts/PhysicalCountReportedItem.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Purchases/PurchaseRequest.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Safety/ToolSafePractice.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Security/AppRole.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Security/AppUser.cs`
- `src/Navi.ToolsAssets.Domain/Entities/Sync/FenixReconciliationRecord.cs`

**Application**
- Contratos de aplicación.
- Servicio de almacenamiento documental abstraído: `IDocumentStorageService`.

**Infrastructure**
- `NaviToolsAssetsDbContext`.
- Migraciones EF Core.
- Seeders.
- Integración con MinIO.
- Persistencia y configuración.

**API**
- Controladores REST por módulo.
- Swagger.
- Seguridad por permisos.
- Operaciones de negocio: inventario, documentos, mantenimiento, compras, toma física, conciliación, configuración, seguridad.

**Admin**
- Blazor Server.
- Componentes visuales por módulo.
- Permisos con `PermissionGuard`.
- Vistas de escritorio para operación y administración.

**MobilePwa**
- PWA móvil.
- Vistas móviles para operación, consulta y toma física.
- Consume la API NAVI.

**Shared**
- DLL compartida.

---

## 4. Mapa de controladores API

| Controlador | Ruta base | Métodos detectados | Primeros métodos |
|---|---:|---:|---|
| `AuthController.cs` | `api/auth` | 2 | HttpPost("login"), HttpGet("permissions") |
| `CatalogsController.cs` | `api/catalogs` | 3 | HttpGet("tool-types"), HttpGet("tool-categories"), HttpGet("responsible-people") |
| `DamagesController.cs` | `api/damages` | 6 | HttpGet, HttpGet("{id:guid}"), HttpGet("by-tool/{toolId:guid}"), HttpGet("by-code/{internalCode}"), HttpPost("report"), HttpPatch("{id:guid}/close") |
| `DashboardController.cs` | `api/dashboard` | 5 | HttpGet("summary"), HttpGet("tools-by-status"), HttpGet("tools-by-branch"), HttpGet("alerts"), HttpGet("recent-activity") |
| `DevDemoAssignmentDatesController.cs` | `api/dev/demo` | 1 | HttpPost("apply-branch-assignment-dates") |
| `DevDemoResetController.cs` | `api/dev/demo` | 1 | HttpPost("reset-and-seed") |
| `DevDemoToolOnlyAssignmentDatesController.cs` | `api/dev/demo` | 1 | HttpPost("apply-tool-only-assignment-dates") |
| `ExecutiveDashboardController.cs` | `api/dashboard` | 1 | HttpGet("executive") |
| `ImportsController.cs` | `api/imports` | 10 | HttpGet, HttpGet("{id:guid}"), HttpGet("{id:guid}/rows"), HttpPost("excel"), HttpPost("{id:guid}/apply-new-candidates"), HttpGet("{id:guid}/summary"), HttpGet("{id:guid}/rows/by-status/{status}"), HttpPatch("rows/{rowId:guid}/mark-reviewed")... |
| `LoansController.cs` | `api/loans` | 12 | HttpGet, HttpGet("{id:guid}"), HttpGet("by-tool/{toolId:guid}"), HttpGet("by-code/{internalCode}"), HttpPost("request"), HttpPatch("{id:guid}/reject"), HttpPatch("{id:guid}/deny"), HttpPatch("{id:guid}/cancel")... |
| `MaintenanceController.cs` | `api/maintenance` | 8 | HttpGet, HttpGet("{id:guid}"), HttpGet("by-tool/{toolId:guid}"), HttpGet("by-code/{internalCode}"), HttpPost("schedule"), HttpPatch("{id:guid}/start"), HttpPatch("{id:guid}/complete"), HttpPatch("{id:guid}/cancel") |
| `MaintenanceRequestsController.cs` | `api/maintenance-requests` | 12 | HttpGet, HttpGet("dashboard-summary"), HttpGet("plans-board"), HttpPost, HttpPost("{id:guid}/submit"), HttpPost("{id:guid}/approve"), HttpPost("{id:guid}/reject"), HttpPost("{id:guid}/schedule")... |
| `OrganizationController.cs` | `api/organization` | 6 | HttpGet("zones"), HttpGet("branches"), HttpGet("branches/{code}"), HttpGet("branches/by-zone/{zoneCode}"), HttpGet("locations"), HttpGet("locations/by-branch/{branchCode}") |
| `PhysicalCountsController.cs` | `api/physical-counts` | 34 | HttpGet, HttpGet("{id:guid}"), HttpGet("by-branch/{branchCode}"), HttpPost, HttpPatch("{id:guid}/start"), HttpPost("{id:guid}/items"), HttpPatch("{id:guid}/complete"), HttpPatch("{id:guid}/cancel")... |
| `PurchaseRequestsController.cs` | `api/purchase-requests` | 9 | HttpGet, HttpGet("{id:guid}"), HttpPost, HttpPost("{id:guid}/submit"), HttpPost("{id:guid}/approve"), HttpPost("{id:guid}/reject"), HttpPost("{id:guid}/close"), HttpPost("{id:guid}/mark-sent-dynamics")... |
| `ReconciliationController.cs` | `api/reconciliation` | 7 | HttpGet, HttpGet("{id:guid}"), HttpGet("by-tool/{toolId:guid}"), HttpGet("by-code/{internalCode}"), HttpPost("manual"), HttpPatch("{id:guid}/validate"), HttpPatch("{id:guid}/mark-inconsistent") |
| `SecurityRolesEduardoController.cs` | `api/security/roles` | 1 | HttpPost("eduardo/sync") |
| `SettingsManagementController.cs` | `api/settings` | 57 | HttpGet("roles"), HttpPost("roles"), HttpPut("roles/{id:guid}"), HttpDelete("roles/{id:guid}"), HttpGet("users"), HttpPost("users"), HttpPut("users/{id:guid}"), HttpPut("users/{id:guid}/password")... |
| `TechnicalLifeRecordExcelExportController.cs` | `api/tools` | 2 | HttpGet("{id:guid}/technical-life-record/export-excel"), HttpGet("by-code/{internalCode}/technical-life-record/export-excel") |
| `TechnicalLifeRecordMockDataController.cs` | `api/tools` | 1 | HttpPost("technical-life-record/mock-data/apply-all") |
| `TechnicalLifeRecordsController.cs` | `api/tools` | 2 | HttpGet("{id:guid}/technical-life-record"), HttpGet("by-code/{internalCode}/technical-life-record") |
| `ToolAccessoriesController.cs` | `api/tools` | 6 | HttpGet("{id:guid}/accessories"), HttpGet("by-code/{internalCode}/accessories"), HttpPost("{id:guid}/accessories"), HttpPost("by-code/{internalCode}/accessories"), HttpPut("accessories/{accessoryId:guid}"), HttpDelete("accessories/{accessoryId:guid}") |
| `ToolAssetControlController.cs` | `api/tools` | 4 | HttpGet("{id:guid}/reported-source"), HttpPatch("{id:guid}/master-data"), HttpPatch("{id:guid}/void"), HttpPatch("{id:guid}/reset-custody-to-warehouse") |
| `ToolDocumentsController.cs` | `api/tools` | 6 | HttpGet("{id:guid}/documents"), HttpGet("by-code/{internalCode}/documents"), HttpPost("{id:guid}/documents"), HttpPost("by-code/{internalCode}/documents"), HttpGet("documents/{documentId:guid}/download"), HttpDelete("documents/{documentId:guid}") |
| `ToolMaintenanceScheduleController.cs` | `api/tools` | 6 | HttpGet("{id:guid}/maintenance-schedule"), HttpGet("by-code/{internalCode}/maintenance-schedule"), HttpPost("by-code/{internalCode}/maintenance-schedule"), HttpPost("{id:guid}/maintenance-schedule"), HttpPut("maintenance-schedule/{maintenanceId:guid}"), HttpDelete("maintenance-schedule/{maintenanceId:guid}") |
| `ToolSafePracticesController.cs` | `api/tools` | 8 | HttpGet("{id:guid}/safe-practices"), HttpGet("by-code/{internalCode}/safe-practices"), HttpPost("{id:guid}/safe-practices"), HttpPost("by-code/{internalCode}/safe-practices"), HttpPost("{id:guid}/safe-practices/defaults"), HttpPost("by-code/{internalCode}/safe-practices/defaults"), HttpPut("safe-practices/{safePracticeId:guid}"), HttpDelete("safe-practices/{safePracticeId:guid}") |
| `ToolTechnicalDataController.cs` | `api/tools` | 2 | HttpPatch("{id:guid}/technical-data"), HttpPatch("by-code/{internalCode}/technical-data") |
| `ToolsController.cs` | `api/tools` | 21 | HttpGet, HttpGet("{id:guid}"), HttpGet("by-code/{internalCode}"), HttpGet("by-branch/{branchCode}"), HttpGet("specialized"), HttpGet("non-specialized"), HttpPost, HttpPut("{id:guid}")... |

### Observación técnica

El API ya cuenta con endpoints suficientes para sostener el MVP. Sin embargo, algunos reportes y vistas recientes consumen modelos simplificados desde el Admin. Conviene validar con Swagger que los DTO reales devuelvan exactamente los nombres de propiedades que las vistas esperan, especialmente en:
- `api/maintenance`
- `api/maintenance-requests`
- `api/purchase-requests`
- `api/physical-counts`
- `api/reconciliation`
- `api/tools/{id}/documents`

---

## 5. Mapa de vistas Admin relevantes

| Archivo | Ruta(s) | Título | Permiso |
|---|---|---|---|
| `src/Navi.ToolsAssets.Admin/Components/Pages/AccessDenied.razor` | `/access-denied` | Acceso denegado - NAVI | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Counter.razor` | `/counter` | Counter | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Error.razor` | `/Error` | Error | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor` | `/, /dashboard` | Dashboard - NAVI | Dashboard.View (Dashboard) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Login.razor` | `/login` | Ingreso - NAVI Herramientas y AF | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AccessoriesIndex.razor` | `/accessories` | Accesorios | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/Approvals.razor` | `/approvals` | Aprobaciones | Purchases.Approve (Aprobaciones) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAssignment.razor` | `/asset-assignment` | Asignar AF - NAVI | AssetAssignment.View (Asignar AF) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAssignmentHistory.razor` | `/asset-assignment-history` | Historial de asignaciones - NAVI | AssetAssignment.History (Historial de asignaciones) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetAvailability.razor` | `/asset-availability` | Disponible y Ubicación de Herramientas - NAVI | AssetAvailability.View (Disponible y Ubicación de Herramientas) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/AssetsIndex.razor` | `/assets` | Otros activos | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/CatalogsSettings.razor` | `/settings/catalogs` | Tipos y categorías | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DamagesIndex.razor` | `/damages` | Daños y novedades | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DisposalsIndex.razor` | `/disposals` | Bajas | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor` | `/documents` | Documentos - NAVI | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/FenixIntegration.razor` | `/integrations/fenix365` | Integración Fenix365 / Dynamics | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ImportConsolidated.razor` | `/imports/consolidated` | Importar consolidado Excel | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ImportDynamics.razor` | `/imports/dynamics` | Importar DynamicsExport | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/LoansIndex.razor` | `/loans` | Préstamos | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenanceConsultation.razor` | `/maintenance` | Consultar Mantenimientos | Maintenance.View (Consultar Mantenimientos) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenanceIndex.razor` | `/maintenance-old` | Mantenimientos | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor` | `/maintenance-plans` | Planes de Mantenimiento | Maintenance.View (Planes de Mantenimiento) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenanceRequest.razor` | `/maintenance-request` | Solicitar Mantenimiento | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MobilePreview.razor` | `/mobile-preview` | PWA móvil | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MroPurchasesIndex.razor` | `/mro-purchases, /purchases/mro` | Solicitar Compra AF | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/OrganizationSettings.razor` | `/settings/organization` | Sedes, zonas y ubicaciones | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor` | `/physical-counts/{Id:guid}` | Detalle Toma Física | PhysicalCounts.View (Detalle de Toma Física) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountPendingToolDetail.razor` | `/physical-counts/{CountId:guid}/participants/{ParticipantId:guid}/pending-tools/{ToolAssetId:guid}` | Validar herramienta asignada | PhysicalCounts.View (Validar herramienta asignada) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountReportedItems.razor` | `/physical-counts/{Id:guid}/reported-items` | Registros reportados por usuario | PhysicalCounts.View (Registros reportados) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountsIndex.razor` | `/physical-counts` | Tomas Físicas | PhysicalCounts.View (Tomas Físicas) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PreoperationalsIndex.razor` | `/preoperationals` | Preoperacionales | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReconciliationIndex.razor` | `/reconciliation` | Conciliación | PhysicalCounts.View |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReportsIndex.razor` | `/reports` | Reportes - NAVI | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReturnsIndex.razor` | `/returns` | Devoluciones | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/RolesSecurity.razor` | `/security/roles` | Roles y permisos | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SafePracticesIndex.razor` | `/safe-practices` | Prácticas seguras | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsAssetTypes.razor` | `/settings/asset-types` | Tipos de activo - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsBranches.razor` | `/settings/branches` | Sedes - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsCategories.razor` | `/settings/categories` | Categorías - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsCauses.razor` | `/settings/causes` | Causales - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsDocumentTypes.razor` | `/settings/document-types` | Tipos de documento - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsIndex.razor` | `/settings` | Configuración - NAVI Herramientas | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsLocations.razor` | `/settings/locations` | Ubicaciones - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsMaintenancePlans.razor` | `/settings/maintenance-plans` | Planes de mantenimiento - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsMaintenanceTypes.razor` | `/settings/maintenance-types` | Tipos de mantenimiento - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsResponsibles.razor` | `/settings/responsibles` | Responsables - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsRoles.razor` | `/settings/roles` | Roles y permisos - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsStatuses.razor` | `/settings/statuses` | Estados - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsSystemParameters.razor` | `/settings/system-parameters` | Parámetros - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsUsers.razor` | `/settings/users` | Usuarios - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsWarehouses.razor` | `/settings/warehouses` | Almacenes / Talleres - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsZones.razor` | `/settings/zones` | Zonas - Configuración | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/TechnicalLifeRecordsIndex.razor` | `/technical-life-records` | Hoja de Vida del AF - NAVI | TechnicalLifeRecord.View (Hoja de Vida del AF) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/UsersSecurity.razor` | `/security/users` | Usuarios | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/Workbench.razor` | `/workbench` | Bandeja de trabajo | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor` | `/tools/{ToolId:guid}/technical-life-record` | Hoja de vida técnica - NAVI Herramientas | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordEdit.razor` | `/tools/{ToolId:guid}/technical-life-record/edit` | Editar datos técnicos - NAVI Herramientas | TechnicalLifeRecord.Edit (Editar hoja de vida) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor` | `/tools/{ToolId:guid}/technical-life-record-view, /tools/{ToolId:guid}/life-record-view` | Consulta hoja de vida técnica - NAVI Herramientas | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/ToolAccessoriesManage.razor` | `/tools/{ToolId:guid}/accessories` | Accesorios - NAVI Herramientas | TechnicalLifeRecord.Edit (Accesorios) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/ToolDetail.razor` | `/tools/{ToolId:guid}` | Detalle herramienta - NAVI Herramientas | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/ToolDocumentsManage.razor` | `/tools/{ToolId:guid}/documents` | Documentos - NAVI Herramientas | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/ToolLifeCycleEvents.razor` | `/tools/{ToolId:guid}/life-cycle-events` | Trazabilidad de hoja de vida | — |
| `src/Navi.ToolsAssets.Admin/Components/Pages/ToolMaintenanceScheduleManage.razor` | `/tools/{ToolId:guid}/maintenance-schedule` | Cronograma mantenimiento - NAVI Herramientas | Maintenance.Execute (Plan de mantenimiento) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/ToolSafePracticesManage.razor` | `/tools/{ToolId:guid}/safe-practices` | Prácticas seguras - NAVI Herramientas | SafePractices.Manage (Prácticas seguras) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Tools.razor` | `/tools` | Inventario maestro - NAVI Herramientas | Tools.View (Inventario de AF) |
| `src/Navi.ToolsAssets.Admin/Components/Pages/Weather.razor` | `/weather` | Weather | — |

---

## 6. Reglas visuales acordadas

Durante el trabajo se definió una línea visual común:

1. **Banners principales**
   - Degradado negro/morado.
   - Bordes redondeados.
   - Título grande.
   - Subtítulo claro.
   - Botones a la derecha, alineados y del mismo tamaño.
   - No incluir contadores innecesarios dentro del banner.

2. **Menú lateral**
   - Fondo oscuro/morado.
   - Íconos tipo emoji o visuales compactos.
   - Scroll independiente del menú lateral.
   - No depender del scroll general de la pantalla.
   - Opción activa con borde o resaltado.

3. **Tarjetas/KPI**
   - Compactas.
   - Con barra lateral de color.
   - Valor visible, descripción corta.
   - Evitar duplicidad de información.

4. **Tablas**
   - Compactas.
   - Encabezado claro.
   - Evitar textos montados.
   - Permitir filtros arriba.
   - Acciones claras por fila.

5. **Dashboard**
   - Debe mostrar información ejecutiva.
   - No debe tener demasiado espacio vacío.
   - Debe agrupar indicadores y gráficas de manera armónica.
   - Los gráficos de dona deben llenarse con porcentaje real, no solo mostrar total.
   - Gráficos de barras deben permitir lectura por sede/mes/día.

6. **Reportes**
   - Panel lateral izquierdo tipo menú.
   - Panel central para filtros y tabla.
   - Panel derecho de resumen compacto, en una línea, parecido al menú izquierdo.
   - Banner sin botón “Descargar CSV”.
   - En resultado del reporte, dejar “Limpiar filtros”; quitar exportar junto al filtro si ya existe exportación lateral.

---

## 7. Flujo funcional por módulo

## 7.1 Dashboard

### Objetivo
Mostrar una visión general de todos los procesos del sistema:
- Inventario.
- Activos disponibles/asignados.
- Mantenimientos.
- Diferencias.
- Toma física.
- Documentación.
- Avance operativo.
- Reportes ejecutivos.

### Estado actual
Se ha rediseñado varias veces para hacerlo más parecido a la referencia visual entregada. Se buscó compactar:
- Local de herramientas.
- Mantenimientos realizados.
- Herramientas vs activos.
- Herramientas por sede.
- Distribución de activos.
- Herramientas por mes.
- Hojas de vida.
- Resumen por sede/responsable.
- Histórico de procesos.

### Pendiente
- Confirmar que el dashboard final compila y refleja datos reales de API.
- Revisar que los gráficos de dona se llenen por porcentaje real.
- Validar que el resumen por sede tenga la columna adicional solicitada.
- Validar que el historial inferior muestre procesos reales o al menos eventos trazables.

---

## 7.2 Inventario de AF

### Objetivo
Consultar todos los activos fijos/herramientas sin importar sede, responsable o agencia.

### Datos visibles requeridos
- Código del activo.
- Herramienta / descripción / nombre.
- Sede.
- Tipo.
- Categoría.
- Serial.
- Estado.
- Clase.
- Botón Detalles.

### Estado actual
Existe la vista `Tools.razor` con ruta `/tools`, título “Inventario maestro - NAVI Herramientas” y permiso `Tools.View`. La vista declara el banner “Inventario de AF” y la descripción “Consulta general de herramientas y activos fijos registrados en NAVI”.

### Botón Detalles
Debe llevar a `/tools/{ToolId}`, donde se visualizan:
- Datos principales.
- Ubicación.
- Acciones rápidas.
- Estados.
- Reglas operativas.
- Actividades recientes de hoja de vida.

### Acciones rápidas permitidas
Solo desde acciones rápidas se permite modificar:
- Estado operativo:
  - Disponible.
  - Pendiente de validación.
  - Asignación.
  - Prestada.
  - En mantenimiento.
  - Detallada.
  - No apta.
  - Inconsistencia.
  - No localizada.
  - Pendiente de baja.
  - Dada de baja.
- Observación.
- Marcar/desmarcar herramienta especializada.
- Ver hoja de vida.

### Pendientes
- Validar que todos los estados de negocio estén sincronizados con `ToolEnums`.
- Validar que cada cambio de estado cree evento de ciclo de vida.
- Validar que “clase” y “especializada” estén claramente diferenciados.

---

## 7.3 Detalle de activo/herramienta

### Objetivo
Visualizar y controlar datos operativos de un activo específico.

### Vista actual
`ToolDetail.razor` usa rutas:
- `/tools/{ToolId:guid}`

### Funciones esperadas
- Mostrar datos principales.
- Mostrar ubicación.
- Mostrar estado.
- Mostrar reglas operativas.
- Mostrar actividades recientes.
- Permitir cambios de estado solo en acciones rápidas.
- Permitir observación.
- Permitir marcar como especializada.
- Botón “Ver hoja de vida”.

### Pendientes
- Confirmar que el botón de hoja de vida vaya a la vista correcta: solo lectura o edición, según módulo de origen.
- Validar permisos: no todos los usuarios deben poder cambiar estado.
- Confirmar que los cambios persistan en API y se reflejen en Dashboard, Inventario, Disponibilidad y Reportes.

---

## 7.4 Hoja de vida del AF

### Objetivo
Centralizar toda la vida técnica y documental del activo.

### Vista de consulta
Desde detalle de inventario, el botón “Ver hoja de vida” debe llevar a una hoja de vida principalmente de visualización:
- Fechas programadas de mantenimiento.
- Último mantenimiento.
- Propósito de mantenimiento.
- Días restantes.
- Información del equipo.
- Especificaciones técnicas.
- Vida útil.
- Plan de mantenimiento.
- Accesorios.
- Registros fotográficos.
- Ficha técnica y documentación.
- Cronograma de mantenimiento.
- Prácticas seguras.
- Eventos.

### Vista editable
Desde el módulo “Hoja de Vida del AF” o historial de usuario, la hoja de vida debe permitir:
- Editar información principal.
- Agregar accesorios.
- Agregar documentación.
- Agregar mantenimiento.
- Agregar seguridad/prácticas seguras.
- Exportar PDF o Excel.

### Estado actual
Existen vistas separadas:
- `TechnicalLifeRecordReadOnly.razor`
- `TechnicalLifeRecordEdit.razor`
- `TechnicalLifeRecordsIndex.razor`
- `ToolAccessoriesManage.razor`
- `ToolDocumentsManage.razor`
- `ToolLifeCycleEvents.razor`
- `ToolMaintenanceScheduleManage.razor`
- `ToolSafePracticesManage.razor`

### Pendientes
- Confirmar que la vista read-only no permita edición accidental.
- Confirmar exportación PDF; actualmente se identifica controlador Excel (`TechnicalLifeRecordExcelExportController`), pero PDF debe validarse.
- Unificar experiencia visual entre read-only y editable.
- Confirmar que documentación de ficha técnica se sincronice con módulo Documentos.

---

## 7.5 Disponible y ubicación

### Objetivo
Saber dónde está cada activo/herramienta y gestionar su ubicación.

### Funciones
- Filtrar por sede.
- Filtrar por estado.
- Filtrar por código, nombre o referencia.
- Ver disponibilidad.
- Botón “Gestionar”.

### Botón Gestionar
Debe permitir:
- Ver información de la herramienta.
- Cambiar sede.
- Cambiar taller.
- Cambiar almacén o ubicación.
- Cambiar estado operativo.
- Registrar observación.
- Guardar trazabilidad.

### Estados requeridos
- Disponible.
- Asignada.
- Prestada.
- Mantenimiento.
- Dañada.
- Apta.
- Pendiente.
- No localizada.
- Pendiente de baja.
- Dada de baja.

### Escenario de préstamo entre sedes
Si una herramienta está en AGU y se presta a GIR, se debe:
- Cambiar sede destino.
- Definir ubicación destino.
- Registrar observación.
- Guardar movimiento.
- Mantener trazabilidad.

### Pendientes
- Validar si la responsabilidad queda en responsable de sede, almacén o sin responsable.
- Validar trazabilidad al cambiar ubicación.
- Validar que “prestada” y “asignada” tengan reglas distintas.

---

## 7.6 Asignación de AF

### Objetivo
Gestionar asignaciones, préstamos, devoluciones y solicitudes.

### Nombre del módulo
Actualmente se usa “Asignar AF”. Se discutió si debe llamarse:
- Asignar AF.
- Asignar herramientas.
- Asignación de activos fijos/herramientas.

Recomendación: mantener **Asignar AF** en menú y usar subtítulo: “herramientas y activos fijos”.

### Nueva asignación
Debe permitir:
- Buscar herramienta.
- Seleccionar responsable, almacén o usuario.
- Fecha de asignación.
- Observación.
- Confirmar o limpiar.

### Solicitud de asignación / préstamo
Usuario técnico puede:
- Buscar herramienta disponible.
- Solicitar préstamo.
- Indicar fecha estimada de devolución.
- Agregar observación.
- Enviar solicitud.

La herramienta queda en estado de solicitud pendiente, hasta que coordinador:
- Apruebe.
- Deniegue.

### Pendiente específico detectado
En el formulario de solicitud, después de seleccionar una herramienta, no debe seguir apareciendo “no se encontraron herramientas” o resultados filtrados. Una vez seleccionada, el bloque debe ocultarse o mostrar solo la herramienta seleccionada.

### Solicitudes pendientes
Deben aparecer abajo:
- Aprobar.
- Denegar.
- Regresar.

### Regresar activo
Permite devolver al taller/almacén y quitar responsabilidad personal:
- Observación.
- Confirmar regreso.
- Cambiar estado/ubicación.
- Crear trazabilidad.

### Panel derecho
Debe mostrar herramientas asignadas o en asignación:
- Pendientes primero.
- Denegar.
- Regresar.
- Aprobación rápida.

### Pendientes
- Ajustar visualmente el bloque de solicitud seleccionada.
- Confirmar que “regresar” realmente quite responsabilidad del usuario.
- Confirmar trazabilidad completa en `ToolLoan` / eventos de ciclo de vida.

---

## 7.7 Historial de asignaciones

### Objetivo
Visualizar movimientos históricos de herramientas por usuario o por activo.

### Funciones
- Filtrar por usuario.
- Filtrar por herramienta.
- Ver movimientos.
- Ver historial de una persona.
- Botón volver atrás desde historial.

### Pendiente
El botón de volver atrás desde historial de usuario quedó mencionado como pendiente. Debe revisarse navegación y estado de filtros.

---

## 7.8 Solicitud de compra AF / MRO

### Objetivo
Crear solicitudes de compra para herramientas o activos.

### Flujo definido
1. Crear encabezado:
   - Nombre solicitud.
   - Sede.
   - Solicitante.
   - Responsable que aprueba.
   - Tipo solicitud.
   - Prioridad.
   - Fecha requerida.
   - Ubicación/área.
   - Descripción.
2. Agregar herramientas o activos solicitados:
   - Foto/evidencia.
   - Nombre herramienta/equipo.
   - Información técnica/serial.
   - Cantidad.
   - Detalles.
   - Novedad/observación.
3. Guardar borrador o confirmar.
4. Si está en borrador, confirmar.
5. Realizar cotización:
   - Proveedor.
   - Herramienta cotizada.
   - Fecha cotización.
   - Tiempo entrega.
   - Precio antes de IVA.
   - Detalles.
6. Agregar cotización.
7. Generar solicitud final.

### Estado técnico
La vista `MroPurchasesIndex.razor` usa el componente común `SpcRequestWorkspace.razor` con `Kind="purchase"`. El componente común usa permisos, API, localStorage, cotizaciones y envío a endpoints.

### Pendientes
- Validar persistencia real contra API, no solo localStorage.
- Confirmar que la solicitud final se liste en el módulo de consulta.
- Validar estados: borrador, confirmada, cotizando, generada, aprobada, rechazada, enviada a Dynamics.
- Validar trazabilidad de cotizaciones por herramienta.

---

## 7.9 Solicitud de mantenimiento

### Objetivo
Solicitar mantenimiento de herramientas/equipos con cotización y generación de solicitud.

### Flujo
Muy similar a compra:
1. Encabezado de mantenimiento.
2. Agregar herramienta/equipo a intervenir.
3. Guardar borrador o confirmar.
4. Cotización de mantenimiento:
   - Proveedor.
   - Herramienta cotizada.
   - Fecha.
   - Tiempo entrega/reparación.
   - Precio antes de IVA.
   - Detalles.
5. Generar solicitud de mantenimiento.

### Pendiente conceptual
En conversaciones previas se dijo que al generar solicitud de mantenimiento aparece relacionada con consulta de mantenimientos. El usuario aclaró que ese módulo debería llamarse **Consulta de Solicitudes**, porque allí se consulta trazabilidad de compra y mantenimiento, aprobadas o rechazadas.

### Cambio recomendado inmediato
- Renombrar menú “Consultar Mantenimientos” a **Consultar Solicitudes**.
- Cambiar título, banner, `PageTitle`, texto y rutas si aplica.
- Mantener ruta anterior `/maintenance` temporalmente para compatibilidad o crear alias.

---

## 7.10 Consulta de solicitudes

### Objetivo esperado
Consultar trazabilidad de solicitudes de compra y mantenimiento:
- Solicitud.
- Tipo.
- Estado.
- Cotización.
- Aprobación.
- Rechazo.
- Programación.
- Costo.
- Responsable.
- Fechas.

### Estado actual
Existe `MaintenanceConsultation.razor` con permiso `Maintenance.View`. Se ajustó visualmente para evitar duplicidad de KPIs y reorganizar costos/mantenimientos.

### Pendientes
- Renombrar módulo.
- Dividir en dos columnas: una columna principal con solicitudes y otra con resumen de costos.
- Evitar que el resumen de costos ocupe demasiado espacio superior cuando haya muchas solicitudes.
- Validar que compra y mantenimiento convivan o que se separen por tabs/filtros.

---

## 7.11 Planes de mantenimiento

### Objetivo
Visualizar planes, distribución, KPIs y programación.

### Ajustes ya discutidos
- Quitar “atención prioritaria”.
- Conservar distribución del plan.
- Mover KPIs para ocupar el espacio de atención prioritaria.
- Evitar duplicidad con KPIs superiores.

### Pendientes
- Validar funcionalidad real contra API.
- Ver si planes son configuración o ejecución operativa.
- Conectar con cronograma de hoja de vida.

---

## 7.12 Toma física

### Objetivo
Crear campañas de inventario físico por sede.

### Flujo de campaña
1. Usuario crea toma física por sede.
2. Define responsable/creador.
3. Agrega notas/alcance.
4. Se muestra en campañas registradas.
5. Acciones:
   - Detalle.
   - Iniciar.
   - Cerrar.
   - Cancelar.

### Detalle de toma
En detalle se debe visualizar por participante:
- Asignadas.
- Validadas.
- Pendientes.
- Diferencias.
- Avance.
- Fechas.
- Acciones: iniciar, finalizar, pendientes.

### Pendientes por reportar
Para cada herramienta asignada, el usuario puede:
- Confirmar.
- Rechazar.
- Entrar al detalle.
- Reportar estado físico.
- Reportar ubicación.
- Reportar estado operativo.
- Agregar comentarios.
- Subir evidencia/fotos.
- Agregar accesorios no considerados.

### Herramienta no listada
El usuario puede registrar activos no listados:
- Tipo de reporte.
- Tipo de activo.
- Código visible si existe.
- Nombre.
- Categoría.
- Sede.
- Estado físico.
- Marca.
- Observaciones.
- Guardar.

### Resultado esperado
La toma debe mostrar:
- Completados.
- No completados.
- Ubicación.
- Observaciones.
- Fecha.
- Usuario que reportó.
- Registros no listados.

### Estado técnico
El API `PhysicalCountsController` tiene ruta `api/physical-counts` y es uno de los controladores más extensos, con operaciones para listar, consultar, iniciar, completar, cancelar, generar participantes, registros reportados, board de participantes y board de registros.

### Pendientes
- Asegurar que Documentos muestre la toma real, no el inventario completo.
- Ya se ajustó concepto: toma física en Documentos no sube archivo; visualiza campañas y registros reportados.
- Confirmar que los reportes de toma física en Documentos y Reportes coincidan con las campañas reales.

---

## 7.13 Conciliación

### Objetivo
Cerrar diferencias de toma física.

### Filtros requeridos
- Toma.
- Sede.
- Usuario.
- Tipo de reporte.
- Estado.
- Datos mínimos completos/incompletos.
- Fecha desde/hasta.
- Texto.

### Acciones por registro
- Aclarar.
- Conciliar.
- Aprobar creación.
- Rechazar.

### Reglas funcionales
- Aclarar cuando el reporte no concuerda.
- Conciliar cuando el reporte es correcto.
- Aprobar creación cuando es herramienta nueva/no listada y procede registrarla.
- Rechazar cuando no tiene coherencia.

### Estado técnico
Existe `ReconciliationIndex.razor` y `ReconciliationController`. El controlador permite acciones como validar y marcar inconsistente.

### Pendientes
- Validar que las acciones de conciliación actualicen `ToolAsset`, `FenixReconciliationRecord` y eventos de ciclo de vida.
- Validar que los filtros no dupliquen sede si la toma ya es por sede, aunque se puede conservar por consistencia.
- Validar mensajes de aclaración al usuario.

---

## 7.14 Documentos

### Objetivo
Gestionar documentos asociados a activos y procesos.

### Clasificaciones
- Ficha técnica.
- Factura.
- Evidencia mantenimiento.
- Toma física.
- Acta.
- Garantía.

### Reglas definidas
- Ficha técnica, factura, evidencia de mantenimiento, acta y garantía se cargan a un activo existente.
- No se permite cargar documento a un activo inexistente.
- El activo se busca en campo único tipo búsqueda/desplegable.
- Toma física no permite subir documentos; muestra consolidado real de campañas y registros reportados.
- Debe permitir ver y descargar documentos.
- Debe mostrar manual por opción.
- Manual abre modal de descripción, no edición.

### Estado técnico
`DocumentsIndex.razor.cs` usa `api/tools`, documentos por activo y `api/physical-counts`. Tiene categorías y manuales embebidos. También valida que `Toma física` no permita carga, usa máximo de archivo de 50 MB y filtra activos por código, nombre, serial, sede y responsable.

### Pendientes técnicos
- Crear tipos documentales reales en base de datos para factura, garantía y acta, en lugar de mapear varias categorías a `Other`, `TechnicalDocument` o depender de la descripción.
- Normalizar clasificación documental como campo real, no como texto dentro de `Description`.
- Mejorar descarga PDF/Excel de consolidado de toma física desde backend, no solo CSV en vista.
- Conectar facturas de compra y evidencias de mantenimiento automáticamente desde procesos origen.
- Validar carga de documentos desde Hoja de Vida contra el mismo repositorio documental.

---

## 7.15 Reportes

### Objetivo
Generar reportes de:
- Resumen ejecutivo.
- Inventario AF.
- Documentos.
- Mantenimiento.
- Compras AF.
- Toma física.
- Conciliación.

### Funciones actuales
- Selección de reporte.
- Filtros:
  - Buscar.
  - Sede.
  - Periodo.
  - Estado.
- KPIs por reporte.
- Tabla de resultados.
- Resumen lateral.
- Histórico de uso.
- Exportación CSV.

### Estado técnico
`ReportsIndex.razor.cs` consume:
- `api/tools`
- documentos por activo.
- `api/maintenance`
- `api/maintenance-requests`
- `api/purchase-requests`
- `api/physical-counts`
- `api/reconciliation`

El código define los reportes `executive`, `inventory`, `documents`, `maintenance`, `purchases`, `physical` y `reconciliation`, con headers, filas, KPIs, insights y CSV.

### Ajustes recientes
- Se quitó `Descargar CSV` del banner.
- Se quitó `Exportar` junto a limpiar filtros.
- Se compactó el resumen lateral porque el texto se montaba.
- Se dejó exportación lateral.

### Pendientes
- Confirmar compilación después del último ajuste visual.
- Validar que el resumen lateral quede en una línea y no se monte.
- Mover exportación si visualmente sigue ocupando espacio excesivo.
- Implementar exportación backend PDF/Excel, no solo CSV data URL.
- Revisar si `api/reconciliation` devuelve propiedades exactamente compatibles con `ReconciliationItem`.

---

## 7.16 Configuración

### Objetivo
Administrar catálogos y parámetros base:
- Usuarios.
- Roles.
- Sedes.
- Zonas.
- Talleres.
- Ubicaciones.
- Responsables.
- Tipos de activos.
- Categorías.
- Estados.
- Tipos de documentos.
- Tipos de mantenimiento.
- Plan de trabajo.
- Causales.
- Parámetros.

### Estado técnico
Existen múltiples vistas:
- `SettingsIndex.razor`
- `SettingsUsers.razor`
- `SettingsRoles.razor`
- `SettingsBranches.razor`
- `SettingsZones.razor`
- `SettingsWarehouses.razor`
- `SettingsLocations.razor`
- `SettingsResponsibles.razor`
- `SettingsAssetTypes.razor`
- `SettingsCategories.razor`
- `SettingsStatuses.razor`
- `SettingsDocumentTypes.razor`
- `SettingsMaintenanceTypes.razor`
- `SettingsMaintenancePlans.razor`
- `SettingsCauses.razor`
- `SettingsSystemParameters.razor`

### Pendientes
- Terminar restricciones por rol.
- En usuarios, permitir cambiar contraseña solo cuando corresponda.
- Validar que roles y permisos controlen acciones, no solo navegación.
- Revisar CRUD real de todas las configuraciones.
- Asociar catálogos con formularios de inventario, toma física y documentos.

---

## 7.17 Usuarios y roles

### Objetivo
Controlar acceso por perfil y matriz de permisos.

### Estado actual
Hay módulos de usuarios, roles y seguridad. La exportación muestra `SettingsUsers.razor`, `SettingsRoles.razor`, `RolesSecurity.razor`, controladores de seguridad y matriz de roles previa en documentación.

### Pendiente importante
`RolesSecurity.razor` aparece como “Módulo en construcción”. Debe consolidarse con `SettingsRoles.razor` o reemplazarse por la matriz real de permisos.

### Reglas
- No todos los usuarios pueden cambiar estados.
- No todos pueden aprobar solicitudes.
- No todos pueden conciliar.
- No todos pueden editar hoja de vida.
- Los técnicos deben tener acceso a toma física, solicitudes y consulta limitada.
- Administrador NAVI debe tener control total.
- Coordinador/taller debe aprobar, asignar, regresar y validar.

---

## 7.18 Mobile PWA

### Objetivo
Permitir operación móvil:
- Consulta de activos.
- Disponibilidad.
- Asignación/préstamo.
- Reportes de toma física.
- Evidencias/fotos.
- Consulta de hoja de vida.
- Flujo de usuario técnico.

### Estado actual
El proyecto `Navi.ToolsAssets.MobilePwa` existe con vistas, modelos y CSS. Se han hecho ajustes visuales previos para dashboard móvil, menú, búsqueda, disponibilidad, detalle y asignación.

### Pendientes
- Validar que los cambios funcionales web también se reflejen en móvil.
- Alinear toma física móvil con detalle de pendiente por reportar.
- Validar carga de fotos/evidencias desde móvil.
- Validar permisos y sesión.

---

## 8. Modelo de dominio actual

Entidades principales detectadas en el proyecto:

### Inventario
- `ToolAsset`
- `ToolAccessory`
- `ToolCategory`
- `ToolType`

### Documentos
- `ToolDocument`

### Mantenimiento
- `MaintenanceRecord`
- `ToolMaintenanceRequest`

### Compras
- `PurchaseRequest`

### Toma física
- `PhysicalCount`
- `PhysicalCountItem`
- `PhysicalCountParticipant`
- `PhysicalCountReportedItem`
- `PhysicalCountExtraItem`

### Organización
- `Branch`
- `Zone`
- `ToolLocation`
- `ResponsiblePerson`
- `SystemParameter`

### Préstamos/asignación
- `ToolLoan`
- `ToolLoanItem`

### Seguridad
- `AppUser`
- `AppRole`

### Ciclo de vida
- `ToolLifeCycleEvent`

### Conciliación/sync
- `FenixReconciliationRecord`

---

## 9. Estado de implementación por módulo

| Módulo | Estado | Observación |
|---|---|---|
| Dashboard | En mejora visual | Falta validar porcentaje real de donas y datos finales. |
| Inventario AF | Funcional base | Listado, detalle, acciones rápidas. |
| Detalle herramienta | Funcional base | Cambios de estado y especializada requieren trazabilidad robusta. |
| Disponible/Ubicación | Funcional base | Falta validar trazabilidad de cambios de sede/taller/almacén. |
| Asignar AF | Funcional base | Pendiente corregir bloque de solicitud seleccionada y regreso. |
| Historial asignaciones | Funcional base | Pendiente botón volver y UX. |
| Hoja de vida AF | Funcional base | Read-only y edit separadas; revisar PDF. |
| Solicitud compra AF/MRO | Funcional visual y lógica local/API parcial | Validar persistencia y consulta. |
| Solicitud mantenimiento | Funcional visual y lógica local/API parcial | Validar estados y generación final. |
| Consulta mantenimientos/solicitudes | En ajuste | Renombrar y reorganizar costos. |
| Planes mantenimiento | En ajuste | Reorganización visual realizada; validar datos reales. |
| Toma física | Funcional base avanzada | Campañas, participantes, registros; consolidado real en documentos. |
| Conciliación | Funcional base | Acciones y filtros; validar impacto completo. |
| Documentos | Funcional base avanzada | Carga a activos, consolidado toma física, manuales; mejorar tipos reales. |
| Reportes | Funcional base avanzada | Filtros, KPIs, CSV; falta PDF/Excel backend y validar resumen final. |
| Configuración | Funcional base | CRUDs y catálogos; validar restricciones. |
| Usuarios/Roles | En construcción/ajuste | Completar matriz real y permisos por acción. |
| Mobile PWA | Funcional base | Alinear con web y probar toma física/evidencias. |

---

## 10. Problemas y errores resueltos durante el proceso

1. **Pérdida de opciones del menú**
   - Ocurrió al ajustar NavMenu.
   - Se corrigió restaurando estructura y compactando íconos.

2. **Dashboard con error `pending`**
   - Error: nombre `pending` no existía en contexto.
   - Se corrigió el código del dashboard.

3. **Error de `StringSplitOptions`**
   - En `MaintenanceConsultation.razor`, error por conversión de argumento a `char`.
   - Se corrigió usando overload correcto.

4. **Error `media` en Razor**
   - Causado por `@media` sin escape.
   - Debe usarse `@@media` dentro de `<style>` en Razor.

5. **Error `sealed` en Documents**
   - `DocumentViewItem` no podía heredar de `DocumentItemModel` porque este era `sealed`.
   - Se corrigió quitando `sealed`.

6. **Error Razor por mezcla de C# y HTML**
   - `DocumentsIndex.razor` quedó demasiado grande y Razor interpretó mal bloques.
   - Se separó en `.razor` y `.razor.cs`.

7. **zsh `event not found`**
   - Causado por `!` en Razor al pegar heredoc.
   - Solución: `setopt NO_BANG_HIST` y/o generar archivos desde Python.

8. **zsh `bquote>`**
   - Causado por comillas/heredoc sin cierre.
   - Solución: `Ctrl+C` y usar script Python con strings raw.

9. **Reportes lateral montado**
   - El resumen lateral se montaba letra por letra.
   - Se compactó diseño a una línea tipo menú.

---

## 11. Pendientes técnicos priorizados

### Prioridad alta

1. **Compilar después del último ajuste de Reportes**
   - Comando:
     ```bash
     dotnet build src/Navi.ToolsAssets.Admin/Navi.ToolsAssets.Admin.csproj -c Debug
     ```

2. **Renombrar Consulta de Mantenimientos**
   - Nuevo nombre sugerido: `Consulta de Solicitudes`.
   - Afecta:
     - NavMenu.
     - `MaintenanceConsultation.razor`.
     - `PageTitle`.
     - Banner.
     - Texto de permisos si aplica.
     - Posible ruta alias.

3. **Corregir solicitud de asignación**
   - Cuando se selecciona herramienta, ocultar resultados filtrados.
   - No mostrar “no se encontraron herramientas”.

4. **Validar permisos de acciones**
   - Cambiar estado.
   - Aprobar préstamo.
   - Denegar préstamo.
   - Regresar herramienta.
   - Aprobar compra.
   - Aprobar mantenimiento.
   - Conciliar.
   - Cambiar contraseña.
   - Editar hoja de vida.

5. **Trazabilidad obligatoria**
   - Cada cambio de estado, ubicación, responsable, conciliación o mantenimiento debe crear evento.

### Prioridad media

6. **Documentos: normalizar clasificación**
   - Agregar campo real de clasificación si no existe.
   - Evitar depender de `[Clasificación: X]` dentro de descripción.

7. **Reportes: exportación formal**
   - Agregar endpoints de exportación:
     - CSV.
     - Excel.
     - PDF.
   - Evitar data URL para reportes grandes.

8. **Toma física: consolidado formal**
   - Descargar consolidado desde API.
   - Incluir:
     - Toma.
     - Sede.
     - Usuario.
     - Estado.
     - Código.
     - Nombre.
     - Ubicación encontrada/esperada.
     - Conciliación.
     - Datos mínimos.
     - Fecha.

9. **Hoja de vida PDF**
   - Si solo existe Excel, implementar PDF.

10. **Validaciones de estados**
   - Alinear estados de UI con enums reales.

### Prioridad baja

11. **Limpieza de backups**
   - Hay muchos `.bak_*` listados en export.
   - No eliminarlos sin decisión, pero sí evaluar limpieza o exclusión.

12. **Mejorar documentación interna**
   - Convertir este informe en `/docs/DOCUMENTO_MAESTRO_NAVI.md`.
   - Agregar manual por módulo.
   - Agregar checklist de QA.

13. **Automatizar pruebas**
   - Pruebas de API.
   - Pruebas de build.
   - Pruebas de navegación Blazor.

---

## 12. Checklist de validación funcional

### Inventario
- [ ] Lista todos los activos.
- [ ] Filtra correctamente.
- [ ] Detalle abre.
- [ ] Cambia estado.
- [ ] Guarda observación.
- [ ] Marca especializada.
- [ ] Crea evento.

### Disponibilidad
- [ ] Cambia sede.
- [ ] Cambia ubicación.
- [ ] Cambia estado.
- [ ] Guarda observación.
- [ ] Se refleja en inventario.
- [ ] Se refleja en reportes.

### Asignación
- [ ] Nueva asignación.
- [ ] Solicitud de préstamo.
- [ ] Aprobación.
- [ ] Denegación.
- [ ] Regreso.
- [ ] Historial.
- [ ] No muestra resultados vacíos después de seleccionar herramienta.

### Hoja de vida
- [ ] Consulta read-only.
- [ ] Edición desde módulo correcto.
- [ ] Accesorios.
- [ ] Documentos.
- [ ] Mantenimiento.
- [ ] Seguridad.
- [ ] Eventos.
- [ ] Exportación.

### Compra
- [ ] Encabezado.
- [ ] Herramientas.
- [ ] Borrador.
- [ ] Confirmación.
- [ ] Cotización.
- [ ] Generación.
- [ ] Consulta.

### Mantenimiento
- [ ] Encabezado.
- [ ] Herramientas.
- [ ] Borrador.
- [ ] Confirmación.
- [ ] Cotización.
- [ ] Generación.
- [ ] Consulta.
- [ ] Costos.

### Toma física
- [ ] Crear toma.
- [ ] Iniciar.
- [ ] Cancelar.
- [ ] Cerrar.
- [ ] Ver detalle.
- [ ] Participantes.
- [ ] Pendientes.
- [ ] Reportar herramienta.
- [ ] Agregar no listada.
- [ ] Evidencias.
- [ ] Consolidado.

### Conciliación
- [ ] Filtrar por toma.
- [ ] Filtrar por usuario.
- [ ] Filtrar por tipo.
- [ ] Aclarar.
- [ ] Conciliar.
- [ ] Aprobar creación.
- [ ] Rechazar.
- [ ] Actualizar herramienta o estado.

### Documentos
- [ ] Cargar ficha técnica.
- [ ] Cargar factura.
- [ ] Cargar evidencia mantenimiento.
- [ ] Toma física no carga archivo.
- [ ] Toma física muestra campañas reales.
- [ ] Ver documento.
- [ ] Descargar documento.
- [ ] Manual abre modal.

### Reportes
- [ ] Ejecutivo.
- [ ] Inventario.
- [ ] Documentos.
- [ ] Mantenimiento.
- [ ] Compras.
- [ ] Toma física.
- [ ] Conciliación.
- [ ] Filtros.
- [ ] CSV.
- [ ] Resumen lateral compacto.

### Configuración y seguridad
- [ ] CRUD usuarios.
- [ ] Cambio contraseña con permisos.
- [ ] CRUD roles.
- [ ] Matriz permisos.
- [ ] CRUD sedes.
- [ ] CRUD zonas.
- [ ] CRUD almacenes.
- [ ] CRUD ubicaciones.
- [ ] CRUD tipos y categorías.
- [ ] Parámetros.

---

## 13. Comandos operativos

### Levantar API

```bash
cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos
./run-api.sh
```

### Levantar Admin

```bash
cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos
./run-admin.sh
```

### Levantar Mobile PWA

```bash
cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos
./run-mobile.sh
```

### Compilar solución

```bash
cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos
./build-solution.sh
```

### Compilar Admin

```bash
cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos
dotnet build src/Navi.ToolsAssets.Admin/Navi.ToolsAssets.Admin.csproj -c Debug
```

### Compilar API

```bash
cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos
dotnet build src/Navi.ToolsAssets.Api/Navi.ToolsAssets.Api.csproj -c Debug
```

### Compilar Mobile

```bash
cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos
dotnet build src/Navi.ToolsAssets.MobilePwa/Navi.ToolsAssets.MobilePwa.csproj -c Debug
```

### Detener Admin en puerto 5264

```bash
PORT=5264
PIDS="$(lsof -tiTCP:$PORT -sTCP:LISTEN || true)"
if [ -n "$PIDS" ]; then
  kill $PIDS || true
  sleep 1
  PIDS_FORCE="$(lsof -tiTCP:$PORT -sTCP:LISTEN || true)"
  if [ -n "$PIDS_FORCE" ]; then kill -9 $PIDS_FORCE || true; fi
fi
```

---

## 14. Recomendación de siguiente sprint

### Sprint 1: estabilización
1. Compilar Admin/API/Mobile.
2. Corregir errores restantes.
3. Validar navegación completa.
4. Renombrar consulta de mantenimientos.
5. Corregir solicitud de asignación.

### Sprint 2: trazabilidad y permisos
1. Revisar permisos por acción.
2. Revisar eventos de ciclo de vida.
3. Validar usuarios/roles.
4. Bloquear acciones no autorizadas.

### Sprint 3: documentos y reportes
1. Normalizar tipos documentales.
2. Exportación PDF/Excel.
3. Consolidado toma física API.
4. Reportes por backend.

### Sprint 4: pruebas y documentación
1. Checklist QA.
2. Manual por módulo.
3. Documento técnico final.
4. Validación con usuarios.

---

## 15. Decisiones funcionales tomadas

1. Todo se trata como activo fijo/herramienta, no solo herramienta.
2. El detalle de inventario permite cambios rápidos.
3. Hoja de vida puede ser read-only o editable según origen.
4. Toma física no carga documentos manuales; documenta campañas reales.
5. Documentos deben asociarse a activos existentes.
6. Solicitud de compra y mantenimiento usan flujo común de encabezado, líneas, cotización y generación.
7. Consulta de mantenimientos debe evolucionar a consulta de solicitudes.
8. Reportes debe ser centro ejecutivo y operativo.
9. Configuración debe alimentar todos los formularios.
10. Seguridad por rol debe controlar navegación y acciones.

---

## 16. Riesgos actuales

1. **Riesgo de UI divergente:** algunos módulos pueden no estar visualmente estandarizados.
2. **Riesgo de modelo temporal:** Documentos usa descripción para clasificación; eso debe normalizarse.
3. **Riesgo de API mismatch:** los DTO simplificados de reportes pueden no coincidir exactamente con respuestas reales.
4. **Riesgo de trazabilidad incompleta:** algunos cambios pueden no crear eventos.
5. **Riesgo de permisos incompletos:** roles pueden proteger menú, pero no todas las acciones.
6. **Riesgo de exportación insuficiente:** CSV en frontend no es suficiente para reportes grandes o auditoría.
7. **Riesgo de acumulación de backups:** muchos `.bak` pueden confundir futuras modificaciones.

---

## 17. Glosario

**AF:** Activo fijo.  
**MRO:** Maintenance, Repair and Operations. Compra o abastecimiento operativo.  
**SPC:** Solicitud/proceso de compra usado en el flujo de compra AF/MRO.  
**Hoja de vida:** Expediente técnico y operativo de una herramienta/activo.  
**Toma física:** Inventario físico por sede y responsable.  
**Conciliación:** Proceso de resolver diferencias entre inventario esperado y reportado.  
**Herramienta no listada:** Activo reportado por usuario que no existe en inventario.  
**Evidencia:** Foto, documento o soporte cargado.  
**MinIO:** Almacenamiento de archivos/documentos.  
**Fenix365/Dynamics:** Sistema corporativo de referencia/integración.

---

## 18. Cierre

Este documento consolida el estado funcional y técnico del proyecto NAVI Herramientas & Activos / Fenix365 a partir de la conversación y del código exportado el 28 de junio de 2026. Sirve como documento puente para continuar en un nuevo chat, retomar desarrollo, explicar el sistema a otra persona o preparar un documento maestro más formal.

El siguiente paso recomendado es tomar este documento, abrir un nuevo chat, adjuntar el export actualizado del código y pedir: “continúa desde este informe, primero valida compilación, luego aplica pendientes en orden de prioridad”.
