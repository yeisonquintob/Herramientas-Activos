using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Navi.ToolsAssets.Admin.Services.Auth;

public class ReportsIndexBase : ComponentBase
{
    [Inject] protected IHttpClientFactory HttpClientFactory { get; set; } = default!;
    [Inject] protected WebAuthSessionService AuthSession { get; set; } = default!;

    protected bool CanViewReports => ReportsHasPermission("Reports.View");
    protected bool CanExportReports => ReportsHasPermission("Reports.Export");

    protected bool ReportsHasPermission(string permission)
    {
        return AuthSession.HasPermission(permission)
            || string.Equals(AuthSession.RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase)
            || string.Equals(AuthSession.RoleCode, "SUPERADMIN", StringComparison.OrdinalIgnoreCase);
    }



    protected bool isLoading;
    protected string apiBaseUrl = "http://localhost:5218";
    protected string selectedReportCode = "executive";
    protected string searchText = string.Empty;
    protected string branchFilter = string.Empty;
    protected string periodFilter = "all";
    protected string statusFilter = string.Empty;
    protected string? message;
    protected string messageClass = "alert alert-info";

    protected List<ToolAssetOption> assets = new();
    protected List<DocumentViewItem> documents = new();
    protected List<MaintenanceRecordItem> maintenanceRecords = new();
    protected List<MaintenanceRequestItem> maintenanceRequests = new();
    protected List<PurchaseRequestItem> purchaseRequests = new();
    protected List<PhysicalCountItem> physicalCounts = new();
    protected List<ReconciliationItem> reconciliationRecords = new();

    protected List<ReportHistoryItem> History { get; } = new();

    protected int TotalMaintenance => maintenanceRecords.Count + maintenanceRequests.Count;

    protected ReportDefinition SelectedReport =>
        Reports.FirstOrDefault(x => x.Code == selectedReportCode) ?? Reports.First();

    protected List<ReportDefinition> Reports { get; } = new()
    {
        new("executive", "📊", "Resumen ejecutivo", "Ejecutivo", "Vista global", "Resumen general del sistema con activos, documentos, mantenimientos, compras, tomas físicas y conciliación.", "reporte-ejecutivo-navi"),
        new("inventory", "🧰", "Inventario Herramientas y AF", "Operación", "Activos y herramientas", "Listado de herramientas y activos existentes con sede, responsable, serial y estado.", "reporte-inventario-af"),
        new("documents", "📄", "Documentos", "Control", "Soportes cargados", "Documentos asociados a activos, clasificación, responsable, fecha y descarga.", "reporte-documentos"),
        new("maintenance", "🛠️", "Mantenimiento", "Operación", "Costos y solicitudes", "Registros y solicitudes de mantenimiento con estado, activo, proveedor y fechas.", "reporte-mantenimiento"),
        new("purchases", "🛒", "Compras Herramientas y AF", "Operación", "Solicitudes SPC/MRO", "Solicitudes de compra de herramientas, activos y soportes asociados.", "reporte-compras-af"),
        new("physical", "📦", "Toma física", "Control", "Campañas reales", "Tomas físicas creadas, estado, sede, fechas, responsables, encontrados y faltantes.", "reporte-toma-fisica"),
        new("reconciliation", "⚖️", "Conciliación", "Control", "Diferencias", "Registros de conciliación, diferencias, decisiones y trazabilidad.", "reporte-conciliacion")
    };

    protected IEnumerable<string> BranchOptions =>
        assets.Select(x => x.BranchCode)
            .Concat(physicalCounts.Select(x => x.BranchCode))
            .Concat(reconciliationRecords.Select(x => x.BranchCode))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)!;

    protected IEnumerable<string> StatusOptions =>
        RawRows.Select(x => x.Status)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)!;

    protected List<string> CurrentHeaders => selectedReportCode switch
    {
        "inventory" => new() { "Código", "Nombre", "Sede", "Responsable", "Serial", "Estado" },
        "documents" => new() { "Documento", "Clasificación", "Activo", "Responsable", "Fecha", "Tamaño" },
        "maintenance" => new() { "Número", "Activo", "Tipo", "Estado", "Responsable", "Fecha", "Costo" },
        "purchases" => new() { "Solicitud", "Activo / herramienta", "Sede", "Estado", "Responsable", "Fecha", "Total" },
        "physical" => new() { "Toma", "Sede", "Estado", "Inicio", "Cierre", "Responsable", "Inventario" },
        "reconciliation" => new() { "Código", "Activo", "Sede", "Estado", "Decisión", "Fecha", "Responsable" },
        _ => new() { "Módulo", "Indicador", "Resultado", "Estado", "Observación" }
    };

    protected List<ReportRow> RawRows => selectedReportCode switch
    {
        "inventory" => BuildInventoryRows(),
        "documents" => BuildDocumentRows(),
        "maintenance" => BuildMaintenanceRows(),
        "purchases" => BuildPurchaseRows(),
        "physical" => BuildPhysicalRows(),
        "reconciliation" => BuildReconciliationRows(),
        _ => BuildExecutiveRows()
    };

    protected List<ReportRow> FilteredRows
    {
        get
        {
            var query = RawRows.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(branchFilter))
            {
                query = query.Where(x => Contains(x.BranchCode, branchFilter));
            }

            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                query = query.Where(x => string.Equals(x.Status, statusFilter, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var value = searchText.Trim();
                query = query.Where(x => x.SearchText.Contains(value, StringComparison.OrdinalIgnoreCase));
            }

            if (periodFilter != "all")
            {
                var from = periodFilter switch
                {
                    "today" => DateTime.Today,
                    "7" => DateTime.Now.AddDays(-7),
                    "30" => DateTime.Now.AddDays(-30),
                    "90" => DateTime.Now.AddDays(-90),
                    _ => DateTime.MinValue
                };

                query = query.Where(x => x.Date is null || x.Date.Value >= from);
            }

            return query.ToList();
        }
    }

    protected List<SummaryItem> SelectedSummary => selectedReportCode switch
    {
        "inventory" => new()
        {
            new("Total activos", assets.Count.ToString(), "Inventario cargado", ""),
            new("Con sede", assets.Count(x => !string.IsNullOrWhiteSpace(x.BranchCode)).ToString(), "Ubicación asignada", "success"),
            new("Sin responsable", assets.Count(x => string.IsNullOrWhiteSpace(x.ResponsiblePersonName)).ToString(), "Pendiente responsable", "warning"),
            new("Con documentos", documents.Select(x => x.ToolAssetId).Distinct().Count().ToString(), "Soporte documental", "purple")
        },
        "documents" => new()
        {
            new("Total documentos", documents.Count.ToString(), "Soportes cargados", ""),
            new("Activos con docs", documents.Select(x => x.ToolAssetId).Distinct().Count().ToString(), "Con evidencia", "success"),
            new("Ficha técnica", documents.Count(x => GetDocumentCategory(x) == "Ficha técnica").ToString(), "Técnicos", "warning"),
            new("Mantenimiento", documents.Count(x => GetDocumentCategory(x) == "Evidencia mantenimiento").ToString(), "Evidencias", "purple")
        },
        "maintenance" => new()
        {
            new("Total", TotalMaintenance.ToString(), "Registros y solicitudes", ""),
            new("Solicitudes", maintenanceRequests.Count.ToString(), "Nuevas solicitudes", "success"),
            new("Registros", maintenanceRecords.Count.ToString(), "Histórico", "warning"),
            new("Pendientes", maintenanceRequests.Count(x => !IsClosedStatus(x.Status)).ToString(), "Por cerrar", "purple")
        },
        "purchases" => new()
        {
            new("Solicitudes", purchaseRequests.Count.ToString(), "SPC/MRO", ""),
            new("Con activo", purchaseRequests.Count(x => !string.IsNullOrWhiteSpace(x.ToolInternalCode)).ToString(), "Asociadas", "success"),
            new("Pendientes", purchaseRequests.Count(x => !IsClosedStatus(x.Status)).ToString(), "Por gestionar", "warning"),
            new("Cerradas", purchaseRequests.Count(x => IsClosedStatus(x.Status)).ToString(), "Finalizadas", "purple")
        },
        "physical" => new()
        {
            new("Tomas", physicalCounts.Count.ToString(), "Campañas reales", ""),
            new("En progreso", physicalCounts.Count(x => x.Status == "InProgress").ToString(), "Activas", "success"),
            new("Canceladas", physicalCounts.Count(x => x.Status.Contains("Cancel", StringComparison.OrdinalIgnoreCase)).ToString(), "Canceladas", "warning"),
            new("Faltantes", physicalCounts.Sum(x => x.MissingItems).ToString(), "Pendientes", "purple")
        },
        "reconciliation" => new()
        {
            new("Registros", reconciliationRecords.Count.ToString(), "Conciliación", ""),
            new("Con activo", reconciliationRecords.Count(x => x.ToolAssetId != Guid.Empty).ToString(), "Asociados", "success"),
            new("Pendientes", reconciliationRecords.Count(x => !IsClosedStatus(x.Status)).ToString(), "Por decidir", "warning"),
            new("Procesados", reconciliationRecords.Count(x => IsClosedStatus(x.Status)).ToString(), "Cerrados", "purple")
        },
        _ => new()
        {
            new("Activos", assets.Count.ToString(), "Inventario", ""),
            new("Documentos", documents.Count.ToString(), "Soportes", "success"),
            new("Mantenimientos", TotalMaintenance.ToString(), "Operación", "warning"),
            new("Tomas físicas", physicalCounts.Count.ToString(), "Control", "purple")
        }
    };

    protected List<InsightItem> SelectedInsights => selectedReportCode switch
    {
        "physical" => new()
        {
            new("📦", "Tomas reales", $"{physicalCounts.Count} campaña(s) registradas."),
            new("✅", "Encontrados", $"{physicalCounts.Sum(x => x.FoundItems)} activo(s) encontrados."),
            new("⚠️", "Faltantes", $"{physicalCounts.Sum(x => x.MissingItems)} activo(s) faltantes."),
            new("🔎", "Seguimiento", "Cruzar con Conciliación para resolver diferencias.")
        },
        "documents" => new()
        {
            new("📄", "Soportes", $"{documents.Count} documento(s) cargados."),
            new("🧰", "Activos", $"{documents.Select(x => x.ToolAssetId).Distinct().Count()} activo(s) con documento."),
            new("🛠️", "Mantenimiento", $"{documents.Count(x => GetDocumentCategory(x) == "Evidencia mantenimiento")} evidencia(s)."),
            new("📘", "Técnicos", $"{documents.Count(x => GetDocumentCategory(x) == "Ficha técnica")} ficha(s) técnica(s).")
        },
        "maintenance" => new()
        {
            new("🛠️", "Mantenimiento", $"{TotalMaintenance} registro(s) total."),
            new("📝", "Solicitudes", $"{maintenanceRequests.Count} solicitud(es)."),
            new("⏳", "Pendientes", $"{maintenanceRequests.Count(x => !IsClosedStatus(x.Status))} por cerrar."),
            new("📄", "Soporte", "Las evidencias deben cruzar con Documentos.")
        },
        _ => new()
        {
            new("📊", "Reporte", SelectedReport.Title),
            new("🔎", "Filtrado", $"{FilteredRows.Count} registro(s) visibles."),
            new("🏢", "Sedes", $"{BranchOptions.Count()} sede(s) detectadas."),
            new("⬇️", "Exportación", CanExportReports ? "Descarga CSV con los filtros actuales." : "Requiere permiso Reports.Export.")
        }
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadAsync();
    }

    protected HttpClient CreateApiClient()
    {
        var api = HttpClientFactory.CreateClient("NaviApi");
        apiBaseUrl = api.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5218";
        return api;
    }

    protected async Task LoadAsync()
    {
        isLoading = true;
        message = null;

        try
        {
            var api = CreateApiClient();

            await LoadAssetsAsync(api);
            await LoadDocumentsAsync(api);
            await LoadMaintenanceAsync(api);
            await LoadPurchaseRequestsAsync(api);
            await LoadPhysicalCountsAsync(api);
            await LoadReconciliationAsync(api);

            History.Insert(0, new ReportHistoryItem(SelectedReport.Title, DateTime.Now, AuthSession.UserName ?? "admin"));

            messageClass = "alert alert-success";
            message = "Reportes actualizados correctamente con información real de NAVI.";
        }
        catch (Exception ex)
        {
            messageClass = "alert alert-danger";
            message = $"No se pudieron cargar todos los reportes. Valida que la API esté activa. Detalle: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    protected async Task LoadAssetsAsync(HttpClient api)
    {
        try
        {
            assets = await api.GetFromJsonAsync<List<ToolAssetOption>>("api/tools") ?? new();
        }
        catch
        {
            assets = new();
        }
    }

    protected async Task LoadDocumentsAsync(HttpClient api)
    {
        var result = new List<DocumentViewItem>();

        foreach (var asset in assets)
        {
            try
            {
                var toolDocuments = await api.GetFromJsonAsync<List<DocumentItemModel>>($"api/tools/{asset.Id}/documents") ?? new();

                foreach (var document in toolDocuments)
                {
                    result.Add(new DocumentViewItem
                    {
                        Id = document.Id,
                        ToolAssetId = document.ToolAssetId,
                        DocumentType = document.DocumentType,
                        FileName = document.FileName,
                        ObjectKey = document.ObjectKey,
                        ContentType = document.ContentType,
                        SizeBytes = document.SizeBytes,
                        UploadedBy = document.UploadedBy,
                        UploadedAt = document.UploadedAt,
                        Description = document.Description,
                        DownloadUrl = document.DownloadUrl,
                        ToolInternalCode = asset.InternalCode,
                        ToolName = asset.Name,
                        BranchCode = asset.BranchCode,
                        ResponsiblePersonName = asset.ResponsiblePersonName
                    });
                }
            }
            catch
            {
            }
        }

        documents = result;
    }

    protected async Task LoadMaintenanceAsync(HttpClient api)
    {
        try
        {
            maintenanceRecords = await api.GetFromJsonAsync<List<MaintenanceRecordItem>>("api/maintenance") ?? new();
        }
        catch
        {
            maintenanceRecords = new();
        }

        try
        {
            maintenanceRequests = await api.GetFromJsonAsync<List<MaintenanceRequestItem>>("api/maintenance-requests") ?? new();
        }
        catch
        {
            maintenanceRequests = new();
        }
    }

    protected async Task LoadPurchaseRequestsAsync(HttpClient api)
    {
        try
        {
            purchaseRequests = await api.GetFromJsonAsync<List<PurchaseRequestItem>>("api/purchase-requests") ?? new();
        }
        catch
        {
            purchaseRequests = new();
        }
    }

    protected async Task LoadPhysicalCountsAsync(HttpClient api)
    {
        try
        {
            physicalCounts = await api.GetFromJsonAsync<List<PhysicalCountItem>>("api/physical-counts") ?? new();
        }
        catch
        {
            physicalCounts = new();
        }
    }

    protected async Task LoadReconciliationAsync(HttpClient api)
    {
        try
        {
            reconciliationRecords = await api.GetFromJsonAsync<List<ReconciliationItem>>("api/reconciliation") ?? new();
        }
        catch
        {
            reconciliationRecords = new();
        }
    }

    protected void SelectReport(string code)
    {
        selectedReportCode = code;
        searchText = string.Empty;
        branchFilter = string.Empty;
        statusFilter = string.Empty;
        periodFilter = "all";

        var selected = Reports.FirstOrDefault(x => x.Code == code);
        History.Insert(0, new ReportHistoryItem(selected?.Title ?? "Reporte", DateTime.Now, AuthSession.UserName ?? "admin"));
    }

    protected void ClearFilters()
    {
        searchText = string.Empty;
        branchFilter = string.Empty;
        periodFilter = "all";
        statusFilter = string.Empty;
    }

    protected string GetReportClass(string code)
    {
        return selectedReportCode == code ? "active" : string.Empty;
    }

    protected List<ReportRow> BuildExecutiveRows()
    {
        return new()
        {
            new(new() { new("Inventario"), new("Total activos"), new(assets.Count.ToString()), new(assets.Count > 0 ? "Con datos" : "Sin datos", true), new("Herramientas y activos registrados.") }, "", assets.Count > 0 ? "Con datos" : "Sin datos", DateTime.Now),
            new(new() { new("Documentos"), new("Total soportes"), new(documents.Count.ToString()), new(documents.Count > 0 ? "Con datos" : "Sin datos", true), new("Documentos cargados por activo.") }, "", documents.Count > 0 ? "Con datos" : "Sin datos", DateTime.Now),
            new(new() { new("Mantenimiento"), new("Registros y solicitudes"), new(TotalMaintenance.ToString()), new(TotalMaintenance > 0 ? "Con datos" : "Sin datos", true), new("Incluye históricos y solicitudes.") }, "", TotalMaintenance > 0 ? "Con datos" : "Sin datos", DateTime.Now),
            new(new() { new("Compras"), new("Solicitudes SPC/MRO"), new(purchaseRequests.Count.ToString()), new(purchaseRequests.Count > 0 ? "Con datos" : "Sin datos", true), new("Solicitudes de compra registradas.") }, "", purchaseRequests.Count > 0 ? "Con datos" : "Sin datos", DateTime.Now),
            new(new() { new("Toma física"), new("Campañas reales"), new(physicalCounts.Count.ToString()), new(physicalCounts.Count > 0 ? "Con datos" : "Sin datos", true), new("Campañas creadas desde control.") }, "", physicalCounts.Count > 0 ? "Con datos" : "Sin datos", DateTime.Now),
            new(new() { new("Conciliación"), new("Registros conciliación"), new(reconciliationRecords.Count.ToString()), new(reconciliationRecords.Count > 0 ? "Con datos" : "Sin datos", true), new("Diferencias y decisiones.") }, "", reconciliationRecords.Count > 0 ? "Con datos" : "Sin datos", DateTime.Now)
        };
    }

    protected List<ReportRow> BuildInventoryRows()
    {
        return assets.Select(x => new ReportRow(new()
        {
            new(x.InternalCode),
            new(x.Name),
            new(Show(x.BranchCode)),
            new(Show(x.ResponsiblePersonName)),
            new(Show(x.SerialNumber)),
            new(Show(x.OperationalStatus ?? x.Status), true)
        }, x.BranchCode, x.OperationalStatus ?? x.Status, x.CreatedAt, $"{x.InternalCode} {x.Name} {x.BranchCode} {x.ResponsiblePersonName} {x.SerialNumber} {x.OperationalStatus} {x.Status}")).ToList();
    }

    protected List<ReportRow> BuildDocumentRows()
    {
        return documents.Select(x => new ReportRow(new()
        {
            new(GetDocumentDisplayName(x)),
            new(GetDocumentCategory(x), true),
            new($"{x.ToolInternalCode} - {x.ToolName}"),
            new(Show(x.UploadedBy)),
            new(x.UploadedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            new(FormatBytes(x.SizeBytes))
        }, x.BranchCode, GetDocumentCategory(x), x.UploadedAt, $"{x.FileName} {x.Description} {x.ToolInternalCode} {x.ToolName} {x.UploadedBy} {GetDocumentCategory(x)}")).ToList();
    }

    protected List<ReportRow> BuildMaintenanceRows()
    {
        var rows = new List<ReportRow>();

        rows.AddRange(maintenanceRecords.Select(x => new ReportRow(new()
        {
            new(Show(x.MaintenanceNumber)),
            new($"{Show(x.ToolInternalCode)} - {Show(x.ToolName)}"),
            new(Show(x.MaintenanceType)),
            new(Show(x.Status), true),
            new(Show(x.ResponsibleBy ?? x.TechnicianName)),
            new((x.CompletedAt ?? x.ScheduledAt ?? x.CreatedAt).ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            new(FormatMoney(x.Cost))
        }, x.BranchCode, x.Status, x.CompletedAt ?? x.ScheduledAt ?? x.CreatedAt, $"{x.MaintenanceNumber} {x.ToolInternalCode} {x.ToolName} {x.Status} {x.MaintenanceType} {x.ResponsibleBy} {x.TechnicianName}")));

        rows.AddRange(maintenanceRequests.Select(x => new ReportRow(new()
        {
            new(Show(x.RequestNumber ?? x.Title)),
            new($"{Show(x.ToolInternalCode)} - {Show(x.ToolName)}"),
            new(Show(x.RequestType ?? x.MaintenanceType)),
            new(Show(x.Status), true),
            new(Show(x.RequestedBy ?? x.ResponsibleBy)),
            new(x.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            new(FormatMoney(x.EstimatedCost ?? x.TotalBeforeTax))
        }, x.BranchCode, x.Status, x.CreatedAt, $"{x.RequestNumber} {x.Title} {x.ToolInternalCode} {x.ToolName} {x.Status} {x.RequestedBy} {x.ResponsibleBy}")));

        return rows;
    }

    protected List<ReportRow> BuildPurchaseRows()
    {
        return purchaseRequests.Select(x => new ReportRow(new()
        {
            new(Show(x.RequestNumber ?? x.Title)),
            new($"{Show(x.ToolInternalCode)} - {Show(x.ToolName)}"),
            new(Show(x.BranchCode)),
            new(Show(x.Status), true),
            new(Show(x.RequestedBy ?? x.ResponsibleBy)),
            new(x.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            new(FormatMoney(x.TotalBeforeTax ?? x.EstimatedCost))
        }, x.BranchCode, x.Status, x.CreatedAt, $"{x.RequestNumber} {x.Title} {x.ToolInternalCode} {x.ToolName} {x.Status} {x.RequestedBy} {x.ResponsibleBy} {x.BranchCode}")).ToList();
    }

    protected List<ReportRow> BuildPhysicalRows()
    {
        return physicalCounts.Select(x => new ReportRow(new()
        {
            new(x.CountNumber),
            new($"{Show(x.BranchCode)} - {Show(x.BranchName)}"),
            new(Show(x.StatusLabel ?? x.Status), true),
            new(x.StartedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            new(x.FinishedAt is null ? "—" : x.FinishedAt.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            new(Show(x.ResponsibleBy)),
            new($"Total: {x.TotalItems} · Encontradas: {x.FoundItems} · Faltantes: {x.MissingItems}")
        }, x.BranchCode, x.StatusLabel ?? x.Status, x.StartedAt, $"{x.CountNumber} {x.BranchCode} {x.BranchName} {x.Status} {x.StatusLabel} {x.ResponsibleBy}")).ToList();
    }

    protected List<ReportRow> BuildReconciliationRows()
    {
        return reconciliationRecords.Select(x => new ReportRow(new()
        {
            new(Show(x.ToolInternalCode)),
            new(Show(x.ToolName)),
            new(Show(x.BranchCode)),
            new(Show(x.Status), true),
            new(Show(x.Decision ?? x.Action)),
            new(x.ProcessedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")),
            new(Show(x.ProcessedBy))
        }, x.BranchCode, x.Status, x.ProcessedAt, $"{x.ToolInternalCode} {x.ToolName} {x.BranchCode} {x.Status} {x.Decision} {x.Action} {x.ProcessedBy}")).ToList();
    }
protected string BuildCsvDataUrl()
    {
        if (!CanExportReports)
        {
            messageClass = "alert alert-warning";
            message = "No tienes permiso para exportar reportes.";
            return "#";
        }

        var csv = new StringBuilder();

        csv.AppendLine(string.Join(",", CurrentHeaders.Select(Csv)));

        foreach (var row in FilteredRows)
        {
            csv.AppendLine(string.Join(",", row.Cells.Select(x => Csv(x.Value))));
        }

        return "data:text/csv;charset=utf-8," + Uri.EscapeDataString(csv.ToString());
    }


    protected static string Csv(string? value)
    {
        var safe = (value ?? string.Empty).Replace("\"", "\"\"");
        return "\"" + safe + "\"";
    }

    protected static string GetBadgeClass(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "reports-badge";
        }

        if (value.Contains("cerr", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("final", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("vigente", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("completo", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("ok", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("concili", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("datos", StringComparison.OrdinalIgnoreCase))
        {
            return "reports-badge ok";
        }

        if (value.Contains("cancel", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("rechaz", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("baja", StringComparison.OrdinalIgnoreCase))
        {
            return "reports-badge danger";
        }

        return "reports-badge warn";
    }

    protected static bool IsClosedStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Contains("closed", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("cerr", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("finish", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("final", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("complete", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("cancel", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("reject", StringComparison.OrdinalIgnoreCase);
    }

    protected static string GetDocumentCategory(DocumentViewItem item)
    {
        var fromDescription = TryGetDescriptionValue(item.Description, "Clasificación");

        if (!string.IsNullOrWhiteSpace(fromDescription))
        {
            return fromDescription;
        }

        return item.DocumentType switch
        {
            "TechnicalDocument" => "Ficha técnica",
            "MaintenanceSupport" => "Evidencia mantenimiento",
            "PhysicalCountEvidence" => "Toma física",
            "DeliveryAct" => "Acta",
            "ReturnAct" => "Acta",
            "Other" => "Factura",
            _ => "Ficha técnica"
        };
    }

    protected static string GetDocumentDisplayName(DocumentViewItem item)
    {
        var fromDescription = TryGetDescriptionValue(item.Description, "Nombre");

        return string.IsNullOrWhiteSpace(fromDescription)
            ? item.FileName
            : fromDescription;
    }

    protected static string? TryGetDescriptionValue(string? description, string key)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var token = $"[{key}:";
        var start = description.IndexOf(token, StringComparison.OrdinalIgnoreCase);

        if (start < 0)
        {
            return null;
        }

        start += token.Length;
        var end = description.IndexOf(']', start);

        if (end < 0)
        {
            return null;
        }

        return description[start..end].Trim();
    }

    protected static bool Contains(string? source, string value)
    {
        return !string.IsNullOrWhiteSpace(source)
            && source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    protected static string Show(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "—" : value;
    }

    protected static string FormatBytes(long value)
    {
        if (value >= 1024 * 1024)
        {
            return $"{value / 1024d / 1024d:0.00} MB";
        }

        if (value >= 1024)
        {
            return $"{value / 1024d:0.00} KB";
        }

        return $"{value} bytes";
    }

    protected static string FormatMoney(decimal? value)
    {
        return value is null ? "$0" : $"${value.Value:N0}";
    }

    protected sealed record ReportDefinition(string Code, string Icon, string Title, string Area, string Description, string LongDescription, string FileName);
    protected sealed record ReportHistoryItem(string Title, DateTime Date, string User);
    protected sealed record SummaryItem(string Label, string Value, string Description, string CssClass);
    protected sealed record InsightItem(string Icon, string Title, string Detail);
    protected sealed record ReportCell(string Value, bool IsBadge = false);

    protected sealed record ReportRow(List<ReportCell> Cells, string? BranchCode, string? Status, DateTime? Date, string? ExtraSearch = null)
    {
        public string SearchText => string.Join(" ", Cells.Select(x => x.Value)) + " " + ExtraSearch;
    }

    protected sealed class ToolAssetOption
    {
        public Guid Id { get; set; }
        public string InternalCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string? FixedAssetCode { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? ResponsiblePersonName { get; set; }
        public string? OperationalStatus { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    protected class DocumentItemModel
    {
        public Guid Id { get; set; }
        public Guid ToolAssetId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? ObjectKey { get; set; }
        public string? ContentType { get; set; }
        public long SizeBytes { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? Description { get; set; }
        public string? DownloadUrl { get; set; }
    }

    protected sealed class DocumentViewItem : DocumentItemModel
    {
        public string ToolInternalCode { get; set; } = string.Empty;
        public string ToolName { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? ResponsiblePersonName { get; set; }
    }

    protected sealed class MaintenanceRecordItem
    {
        public Guid Id { get; set; }
        public string? MaintenanceNumber { get; set; }
        public Guid? ToolAssetId { get; set; }
        public string? ToolInternalCode { get; set; }
        public string? ToolName { get; set; }
        public string? BranchCode { get; set; }
        public string? MaintenanceType { get; set; }
        public string? Status { get; set; }
        public string? ResponsibleBy { get; set; }
        public string? TechnicianName { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal? Cost { get; set; }
    }

    protected sealed class MaintenanceRequestItem
    {
        public Guid Id { get; set; }
        public string? RequestNumber { get; set; }
        public string? Title { get; set; }
        public string? ToolInternalCode { get; set; }
        public string? ToolName { get; set; }
        public string? BranchCode { get; set; }
        public string? RequestType { get; set; }
        public string? MaintenanceType { get; set; }
        public string? Status { get; set; }
        public string? RequestedBy { get; set; }
        public string? ResponsibleBy { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? TotalBeforeTax { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    protected sealed class PurchaseRequestItem
    {
        public Guid Id { get; set; }
        public string? RequestNumber { get; set; }
        public string? Title { get; set; }
        public string? ToolInternalCode { get; set; }
        public string? ToolName { get; set; }
        public string? BranchCode { get; set; }
        public string? Status { get; set; }
        public string? RequestedBy { get; set; }
        public string? ResponsibleBy { get; set; }
        public decimal? EstimatedCost { get; set; }
        public decimal? TotalBeforeTax { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    protected sealed class PhysicalCountItem
    {
        public Guid Id { get; set; }
        public string CountNumber { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? StatusLabel { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? ResponsibleBy { get; set; }
        public string? Notes { get; set; }
        public int TotalItems { get; set; }
        public int FoundItems { get; set; }
        public int MissingItems { get; set; }
    }

    protected sealed class ReconciliationItem
    {
        public Guid Id { get; set; }
        public Guid ToolAssetId { get; set; }
        public string? ToolInternalCode { get; set; }
        public string? ToolName { get; set; }
        public string? BranchCode { get; set; }
        public string? Status { get; set; }
        public string? Decision { get; set; }
        public string? Action { get; set; }
        public string? ProcessedBy { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
