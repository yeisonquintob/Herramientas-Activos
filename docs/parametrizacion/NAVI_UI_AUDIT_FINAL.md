# Auditoría final de apariencia NAVI

Generada: `2026-07-16T01:37:42-05:00`

Esta auditoría analiza únicamente código fuente. No modifica archivos y no ejecuta las aplicaciones.

## Resumen

- Hallazgos totales: **4271**.
- Hallazgos bloqueantes: **2846**.
- Observaciones de revisión: **1425**.
- Resultado estricto: **NO APROBADO**.
- Admin: **1897**.
- Mobile: **2374**.

| Categoría | Hallazgos |
|---|---:|
| `direct_color` | 122 |
| `fixed_height` | 1390 |
| `important` | 2706 |
| `inline_style_dynamic` | 35 |
| `overflow_x_hidden` | 1 |
| `view_font_size` | 17 |

## Muestra de hallazgos

### direct_color

- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3346` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3362` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3374` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3377` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3451` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3465` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3606` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3623` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3633` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3647` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3656` — #ffffff
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3662` — #ffffff
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3437` — #c8d0d2
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3441` — #f6f8f7
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3480` — #263840
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3491` — #64767d
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3515` — #879a57
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3519` — #eef4df
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3520` — #455b25
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3555` — #cbd3d5
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3560` — #718743
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3564` — #ffffff
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3565` — #263840
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3571` — rgba(34, 52, 59, 0.07)
- `src/Navi.ToolsAssets.Admin/wwwroot/css/ntx/assignment-web-v24.css:3579` — #879a57
- … 97 adicionales en el JSON.

### fixed_height

- `src/Navi.ToolsAssets.Admin/Components/Layout/NavMenu.razor.css:209` — height: 30px
- `src/Navi.ToolsAssets.Admin/Components/Layout/NavMenu.razor.css:288` — height: 27px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:163` — height: 18px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:173` — height: 17px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:233` — height: 86px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:238` — height: 104px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:284` — height: 8px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:317` — height: 168px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:326` — height: 168px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:420` — height: 8px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:530` — height: 30px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:703` — height: 30px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:729` — height: 32px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:744` — height: 32px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:865` — height: 18px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:880` — height: 24px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:902` — height: 9px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1065` — height: 32px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1097` — height: 18px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1131` — height: 155px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1136` — height: 155px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1216` — height: 34px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1273` — height: 108px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1346` — height: 34px
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor.css:1387` — height: 112px
- … 1365 adicionales en el JSON.

### important

- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:263` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:274` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:276` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:278` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:282` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:306` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:338` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:342` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:344` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:345` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:347` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:349` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:360` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:367` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:369` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:376` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:378` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:379` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:380` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:382` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:385` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:393` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:403` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:404` — !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:413` — !important
- … 2681 adicionales en el JSON.

### inline_style_dynamic

- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:319` — style="@GetBranchChartGridStyle()"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:331` — style="@GetBranchTripleBarStyle(total)" title="@($"Total: {total}")"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:337` — style="@GetBranchTripleBarStyle(assigned)" title="@($"Asignadas: {assigned}")"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:343` — style="@GetBranchTripleBarStyle(available)" title="@($"Disponibles: {available}")"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:408` — style="@GetRingSegmentStyle( GetInUseTotal(), GetOperationalRingTotal(), 0)"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:419` — style="@GetRingSegmentStyle( dashboard.Summary.AvailableTools, GetOperationalRingTotal(), GetInUseTotal())"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:430` — style="@GetRingSegmentStyle( dashboard.Summary.InMaintenanceTools, GetOperationalRingTotal(), GetInUseTotal() + dashboard.Summary.AvailableTools)"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:442` — style="@GetRingSegmentStyle( GetOperationalAlertTotal(), GetOperationalRingTotal(), GetInUseTotal() + dashboard.Summary.AvailableTools + dashboard.Summary.InMaintenanceTools)"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:455` — style="@GetRingSegmentStyle( GetOperationalOtherTotal(), GetOperationalRingTotal(), GetInUseTotal() + dashboard.Summary.AvailableTools + dashboard.Summary.InMaintenanceTools + GetOperationalAlertTotal())"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:541` — style="@GetRingSegmentStyle( dashboard.Summary.FixedAssets, GetAssetIdentificationTotal(), 0)"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:552` — style="@GetRingSegmentStyle( dashboard.Summary.ToolsWithoutFixedAssetCode, GetAssetIdentificationTotal(), dashboard.Summary.FixedAssets)"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:628` — style="@GetDashboardLifeRingSegmentStyle(GetDashboardCompleteLifeRecords(), 0)"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:635` — style="@GetDashboardLifeRingSegmentStyle(GetDashboardIncompleteLifeRecords(), GetDashboardCompleteLifeRecords())"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:714` — style="@GetCostBarStyle( operationCosts.MaintenanceCost)"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:736` — style="@GetCostBarStyle( operationCosts.PurchaseCost)"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:849` — style="@GetProgressStyle(percent)"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Home.razor:914` — style="@GetProgressStyle( (int)GetRingPercentValue( dashboard.Summary.InMaintenanceTools, dashboard.Summary.TotalTools))"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Modules/PhysicalCountDetail.razor:208` — style="width:@GetParticipantProgress(participant)%"
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor:484` — style="width:@GetPercent(record.UsefulLife.UsedPercentage)%"
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor:259` — style="width:@GetPercent(Text("UsefulLife", "UsedPercentage"))%"
- `src/Navi.ToolsAssets.Admin/Components/Pages/Tools.razor:739` — style="@GetInventoryImageZoomStyle()" @onwheel="HandleInventoryImageWheel" @onwheel:preventDefault="true" /
- `src/Navi.ToolsAssets.MobilePwa/Pages/MobileDashboard.razor:187` — style="@GetDashboardBranchBarStyle(item.Total)"
- `src/Navi.ToolsAssets.MobilePwa/Pages/MobileDashboard.razor:192` — style="@GetDashboardBranchBarStyle(GetDashboardBranchInUse(item))"
- `src/Navi.ToolsAssets.MobilePwa/Pages/MobileDashboard.razor:197` — style="@GetDashboardBranchBarStyle(GetDashboardBranchAvailable(item))"
- `src/Navi.ToolsAssets.MobilePwa/Pages/MobileDashboard.razor:247` — style="@GetDashboardRingSegmentStyle(GetDashboardInUseTotal(), DashboardTotalTools, 0)"
- … 10 adicionales en el JSON.

### overflow_x_hidden

- `src/Navi.ToolsAssets.MobilePwa/wwwroot/css/ntx/mobile-life-records-v26.css:2689` — overflow-x: hidden

### view_font_size

- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:473` — font-size: clamp( var(--navi-font-kpi-value), 1.3vw, 1.32rem )
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:1404` — font-size: clamp( 0.95rem, 1.05vw, 1.15rem ) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:2146` — font-size: clamp( 0.88rem, 1vw, 1.08rem ) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:2682` — font-size: clamp( 0.88rem, 1vw, 1.08rem ) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3102` — font-size: inherit
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3272` — font-size: var(--navi-font-badge) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3294` — font-size: var(--navi-font-body) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3348` — font-size: var(--navi-font-button) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3453` — font-size: var(--navi-font-button) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3536` — font-size: var(--navi-font-badge) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3556` — font-size: var(--navi-font-help) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3608` — font-size: var(--navi-font-badge) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3658` — font-size: inherit !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecord.razor.css:3754` — font-size: var(--navi-font-help) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor.css:205` — font-size: var(--navi-font-kpi-title) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor.css:233` — font-size: var(--navi-font-kpi-description) !important
- `src/Navi.ToolsAssets.Admin/Components/Pages/TechnicalLifeRecordReadOnly.razor.css:263` — font-size: clamp( 0.9rem, 1.05vw, 1.12rem ) !important
