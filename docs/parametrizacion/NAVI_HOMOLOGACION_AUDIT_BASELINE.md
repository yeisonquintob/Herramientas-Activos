# Auditoría de homologación NAVI — baseline

Generada: `2026-07-13T21:23:30-05:00`

- Bloqueantes: **7**.
- Revisión manual: **375**.
- Resultado: **NO APROBADO**.

| Categoría | Total |
|---|---:|
| `absolute_position_review` | 29 |
| `display_label_element` | 126 |
| `expand_without_aria` | 7 |
| `gradient` | 1 |
| `nowrap_review` | 186 |
| `overflow_auto_review` | 33 |

## Hallazgos

### absolute_position_review

- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:243` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:351` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1558` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:2322` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3390` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Login.razor.css:519` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:666` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1050` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1276` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1597` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:117` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:608` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:740` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:1240` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountReportedItems.razor.css:317` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountsIndex.razor.css:428` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReconciliationIndex.razor.css:239` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReconciliationIndex.razor.css:695` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReportsIndex.razor.css:554` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReportsIndex.razor.css:938` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReportsIndex.razor.css:1164` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReportsIndex.razor.css:1485` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReportsIndex.razor.css:1593` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsIndex.razor.css:250` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsIndex.razor.css:634` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsIndex.razor.css:860` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsUsers.razor.css:315` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SpcRequestWorkspace.razor.css:1617` — position: absolute
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SpcRequestWorkspace.razor.css:1847` — position: absolute

### display_label_element

- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsBranches.razor:61` — <label class="form-check-label">Sede piloto</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsBranches.razor:66` — <label class="form-check-label">Activa</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsLocations.razor:50` — <label class="form-label">Descripción</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsLocations.razor:56` — <label class="form-check-label">Activa</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsResponsibles.razor:60` — <label class="form-check-label">Activo</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsRoles.razor:83` — <label>Descripción</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsRoles.razor:95` — <label>Rol activo</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsWarehouses.razor:50` — <label class="form-label">Descripción</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsWarehouses.razor:56` — <label class="form-check-label">Activo</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsZones.razor:39` — <label class="form-label">Descripción</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsZones.razor:45` — <label class="form-check-label">Activa</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SpcRequestWorkspace.razor:162` — <label>Descripción general de la necesidad</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SpcRequestWorkspace.razor:220` — <label>Detalles</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SpcRequestWorkspace.razor:225` — <label>Novedades / falla / observación</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SpcRequestWorkspace.razor:382` — <label>Detalles adicionales de cotización</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:178` — <label>Código interno</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:179` — <label>Nombre</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:180` — <label>Marca</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:181` — <label>Modelo</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:182` — <label>Serial</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:183` — <label>Activo fijo</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:184` — <label>Código Fenix365</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:185` — <label>Fecha adquisición</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:186` — <label>Zona</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:187` — <label>Sede</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:188` — <label>Ubicación</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:189` — <label>Responsable</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:190` — <label>Tipo</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:191` — <label>Categoría</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:192` — <label>Unidad</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:193` — <label>Cantidad</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:204` — <label>Voltaje</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:205` — <label>Capacidad de carga</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:206` — <label>Proveedor</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:207` — <label>Requiere mantenimiento</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:208` — <label>Requiere preoperacional</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:209` — <label>Requiere certificación</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:210` — <label>Vence certificación</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:211` — <label>Garantía</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:212` — <label>Tipo garantía</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:225` — <label>Fecha inicial vida útil</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:226` — <label>Fecha final vida útil</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:227` — <label>Vida útil meses</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:228` — <label>Vida útil días</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:229` — <label>Días transcurridos</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:230` — <label>Días restantes</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:251` — <label>Último mantenimiento</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:252` — <label>Próximo mantenimiento</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:253` — <label>Periodicidad meses</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:254` — <label>Total mantenimientos</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:255` — <label>Completados</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:256` — <label>Pendientes</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordEdit.razor:115` — <label>Requiere mantenimiento</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordEdit.razor:120` — <label>Requiere preoperacional</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordEdit.razor:125` — <label>Requiere certificación</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordEdit.razor:146` — <label>Tiene garantía</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:109` — <label>Código interno</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:110` — <label>Nombre</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:111` — <label>Marca</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:112` — <label>Modelo</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:113` — <label>Serial</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:114` — <label>Activo fijo</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:115` — <label>Código Fenix365</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:116` — <label>Fecha adquisición</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:117` — <label>Zona</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:118` — <label>Sede</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:119` — <label>Ubicación</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:120` — <label>Responsable</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:121` — <label>Tipo</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:122` — <label>Categoría</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:123` — <label>Unidad</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:124` — <label>Cantidad</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:135` — <label>Voltaje</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:136` — <label>Capacidad de carga</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:137` — <label>Proveedor</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:138` — <label>Requiere mantenimiento</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:139` — <label>Requiere preoperacional</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:140` — <label>Requiere certificación</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:141` — <label>Vence certificación</label>
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:142` — <label>Garantía</label>
- … 46 adicionales en JSON.

### expand_without_aria

- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor:346` — <button type="button" class="pc-action pc-action-primary" @onclick="ToggleQuickReportForm" data-ntx-tone="expand" > @(showQuickReportForm ? "Ocultar formulario" : "Agregar no listada") </button>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsRoles.razor:242` — <button type="button" class="soft" @onclick="ExpandAllPermissionGroups" data-ntx-tone="expand" >Mostrar todo</button>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsRoles.razor:243` — <button type="button" class="soft" @onclick="CollapseAllPermissionGroups" data-ntx-tone="expand" >Ocultar todo</button>
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsRoles.razor:270` — <button type="button" class="rolesx-collapse-btn" @onclick="() => TogglePermissionGroup(group)" data-ntx-tone="expand" > @(IsGroupCollapsed(group) ? "Ver" : "Ocultar") </button>
- `src/Navi.ToolsAssets.MobilePwa/Pages/Home.razor:555` — <button class="navi-operational-menu-toggle navi-operational-menu-toggle-primary-v7" type="button" @onclick="ToggleOperationalMenu" data-ntx-tone="expand" > @(showOperationalMenu ? "Ocultar" : "Mostrar") </button>
- `src/Navi.ToolsAssets.MobilePwa/Pages/MobileLifeRecords.razor:121` — <button type="button" class="@(selected ? "selected" : "")" @onclick="() => ToggleStatus(status.Value)" data-ntx-tone="expand" > @status.Text </button>
- `src/Navi.ToolsAssets.MobilePwa/Pages/MobileSpcRequest.razor:129` — <button type="button" class="@(selectedStatuses.Contains(status.Value) ? "selected" : "")" @onclick="() => ToggleStatus(status.Value)" data-ntx-tone="expand" > @status.Text </button>

### gradient

- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/components.css:207` — linear-gradient(

### nowrap_review

- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:291` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:368` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:388` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:505` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:767` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:782` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1284` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1302` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1398` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1416` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1474` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1583` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1648` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1676` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1771` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1813` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1945` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1972` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:2235` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:2241` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:2336` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:2493` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:2499` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:2649` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3109` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3174` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3180` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3405` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3414` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3434` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3682` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3790` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3886` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3923` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3930` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/Approvals.razor.css:37` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:125` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:260` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:535` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:721` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:841` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:856` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:946` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:990` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1018` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1296` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1323` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1386` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1401` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1493` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1627` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1652` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1793` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1813` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1854` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:1865` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenanceConsultation.razor.css:131` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenanceConsultation.razor.css:383` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenanceConsultation.razor.css:650` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenanceConsultation.razor.css:1084` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:214` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:239` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:390` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:657` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:1099` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:1171` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:1180` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:1250` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:1259` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:1268` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:137` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:156` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:389` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:455` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:463` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:491` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:632` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:650` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:668` — white-space: nowrap
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:763` — white-space: nowrap
- … 106 adicionales en JSON.

### overflow_auto_review

- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:580` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:957` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1446` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1517` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1840` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:3605` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:249` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/DocumentsIndex.razor.css:928` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenanceConsultation.razor.css:90` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenanceConsultation.razor.css:639` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:202` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:646` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/MaintenancePlans.razor.css:1209` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:124` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:678` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:807` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor.css:1432` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountReportedItems.razor.css:136` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountReportedItems.razor.css:433` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountsIndex.razor.css:104` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountsIndex.razor.css:218` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountsIndex.razor.css:620` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountsIndex.razor.css:1033` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReconciliationIndex.razor.css:887` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReportsIndex.razor.css:216` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/ReportsIndex.razor.css:816` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsIndex.razor.css:512` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsRoles.razor.css:363` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsUsers.razor.css:155` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsUsers.razor.css:709` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SettingsUsers.razor.css:1025` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SpcRequestWorkspace.razor.css:105` — overflow-x: auto
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/SpcRequestWorkspace.razor.css:744` — overflow-x: auto
