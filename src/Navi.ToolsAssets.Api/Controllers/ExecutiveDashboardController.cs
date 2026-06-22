using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Maintenance;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class ExecutiveDashboardController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public ExecutiveDashboardController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet("executive")]
    public async Task<IActionResult> GetExecutiveDashboard(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
        var startDay = now.Date.AddDays(-13);

        var tools = await _context.ToolAssets
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Location)
            .Include(x => x.ResponsiblePerson)
            .Include(x => x.ToolType)
            .Include(x => x.ToolCategory)
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var branches = await _context.Branches
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var responsibles = await _context.ResponsiblePeople
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var maintenanceRecords = await _context.MaintenanceRecords
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var documents = await _context.ToolDocuments
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.UploadedAt)
            .ThenByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var events = await _context.ToolLifeCycleEvents
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.RegisteredAt)
            .ThenByDescending(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        static DateTime GetMaintenanceDate(MaintenanceRecord item)
        {
            return item.FinishedAt
                ?? item.StartedAt
                ?? item.ScheduledAt;
        }

        var totalTools = tools.Count;

        var availableTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.Available);
        var pendingValidationTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.PendingValidation);
        var assignedTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.Assigned);
        var loanedTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.Loaned);
        var inMaintenanceTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.InMaintenance);

        var damagedTools = tools.Count(x =>
            x.OperationalStatus == ToolOperationalStatus.Damaged ||
            x.OperationalStatus == ToolOperationalStatus.NotSuitable);

        var notLocatedTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.NotLocated);

        var disposedTools = tools.Count(x =>
            x.OperationalStatus == ToolOperationalStatus.Disposed ||
            x.OperationalStatus == ToolOperationalStatus.PendingDisposal);

        var fixedAssets = tools.Count(x => !string.IsNullOrWhiteSpace(x.FixedAssetCode));
        var toolsWithoutFixedAssetCode = tools.Count(x => string.IsNullOrWhiteSpace(x.FixedAssetCode));

        var toolsOnly = tools.Count(x =>
            (x.ToolType != null && x.ToolType.Name.Contains("herramienta", StringComparison.OrdinalIgnoreCase)) ||
            (x.ToolType != null && x.ToolType.Code.Contains("HERR", StringComparison.OrdinalIgnoreCase)) ||
            (x.ToolType != null && x.ToolType.Code.Contains("TOOL", StringComparison.OrdinalIgnoreCase)));

        var nonToolAssets = Math.Max(0, totalTools - toolsOnly);

        var missingTechnicalData = tools
            .Where(x =>
                string.IsNullOrWhiteSpace(x.Brand) ||
                string.IsNullOrWhiteSpace(x.Model) ||
                string.IsNullOrWhiteSpace(x.SerialNumber) ||
                string.IsNullOrWhiteSpace(x.FixedAssetCode))
            .OrderBy(x => x.InternalCode)
            .Select(x => new
            {
                x.Id,
                x.InternalCode,
                x.Name,
                x.SerialNumber,
                x.FixedAssetCode,
                BranchCode = x.Branch != null ? x.Branch.Code : null,
                Missing = string.Join(", ", new[]
                {
                    string.IsNullOrWhiteSpace(x.Brand) ? "Marca" : null,
                    string.IsNullOrWhiteSpace(x.Model) ? "Modelo" : null,
                    string.IsNullOrWhiteSpace(x.SerialNumber) ? "Serial" : null,
                    string.IsNullOrWhiteSpace(x.FixedAssetCode) ? "Activo fijo" : null
                }.Where(v => !string.IsNullOrWhiteSpace(v)))
            })
            .Take(10)
            .ToList();

        var maintenanceByMonth = maintenanceRecords
            .Where(x => GetMaintenanceDate(x) >= startMonth)
            .GroupBy(x =>
            {
                var date = GetMaintenanceDate(x);
                return new { date.Year, date.Month };
            })
            .OrderBy(x => x.Key.Year)
            .ThenBy(x => x.Key.Month)
            .Select(x => new
            {
                Label = $"{x.Key.Month:00}/{x.Key.Year}",
                Count = x.Count()
            })
            .ToList();

        var maintenanceByDay = maintenanceRecords
            .Where(x => GetMaintenanceDate(x).Date >= startDay)
            .GroupBy(x => GetMaintenanceDate(x).Date)
            .OrderBy(x => x.Key)
            .Select(x => new
            {
                Label = x.Key.ToString("dd/MM"),
                Count = x.Count()
            })
            .ToList();

        var toolsByBranch = tools
            .GroupBy(x => new
            {
                BranchCode = x.Branch != null ? x.Branch.Code : "SIN SEDE",
                BranchName = x.Branch != null ? x.Branch.Name : "Sin sede"
            })
            .OrderByDescending(x => x.Count())
            .Select(x => new
            {
                x.Key.BranchCode,
                x.Key.BranchName,
                Total = x.Count(),
                Available = x.Count(t => t.OperationalStatus == ToolOperationalStatus.Available),
                PendingValidation = x.Count(t => t.OperationalStatus == ToolOperationalStatus.PendingValidation),
                Assigned = x.Count(t => t.OperationalStatus == ToolOperationalStatus.Assigned),
                InMaintenance = x.Count(t => t.OperationalStatus == ToolOperationalStatus.InMaintenance)
            })
            .ToList();

        var toolsByStatus = tools
            .GroupBy(x => x.OperationalStatus.ToString())
            .OrderByDescending(x => x.Count())
            .Select(x => new
            {
                Status = x.Key,
                Count = x.Count()
            })
            .ToList();

        var toolsByType = tools
            .GroupBy(x => x.ToolType != null ? x.ToolType.Name : "Sin tipo")
            .OrderByDescending(x => x.Count())
            .Select(x => new
            {
                Type = x.Key,
                Count = x.Count()
            })
            .ToList();

        var toolsByCategory = tools
            .GroupBy(x => x.ToolCategory != null ? x.ToolCategory.Name : "Sin categoría")
            .OrderByDescending(x => x.Count())
            .Take(10)
            .Select(x => new
            {
                Category = x.Key,
                Count = x.Count()
            })
            .ToList();

        var lifeRecords = new
        {
            WithLifeRecord = totalTools,
            WithoutLifeRecord = 0,
            MissingTechnicalData = missingTechnicalData.Count,
            MissingTechnicalDataItems = missingTechnicalData
        };

        var assignmentSummary = new
        {
            Assigned = assignedTools,
            InWarehouse = tools.Count(x => x.CustodyStatus == ToolCustodyStatus.InWarehouse),
            WithResponsible = tools.Count(x => x.ResponsiblePersonId != null),
            WithoutResponsible = tools.Count(x => x.ResponsiblePersonId == null),
            NotLocated = tools.Count(x => x.CustodyStatus == ToolCustodyStatus.NotLocated)
        };

        var maintenanceSummary = new
        {
            Total = maintenanceRecords.Count,
            ThisMonth = maintenanceRecords.Count(x =>
                GetMaintenanceDate(x).Year == now.Year &&
                GetMaintenanceDate(x).Month == now.Month),
            Today = maintenanceRecords.Count(x => GetMaintenanceDate(x).Date == now.Date),
            Pending = maintenanceRecords.Count(x =>
                x.Status == ToolMaintenanceStatus.Scheduled ||
                x.Status == ToolMaintenanceStatus.InProgress),
            Finished = maintenanceRecords.Count(x =>
                x.Status == ToolMaintenanceStatus.Completed ||
                x.FinishedAt.HasValue),
            Overdue = maintenanceRecords.Count(x =>
                x.ScheduledAt.Date < now.Date &&
                x.Status != ToolMaintenanceStatus.Completed &&
                !x.FinishedAt.HasValue)
        };

        var documentSummary = new
        {
            Total = documents.Count,
            ThisMonth = documents.Count(x =>
                x.UploadedAt.Year == now.Year &&
                x.UploadedAt.Month == now.Month),
            Recent = documents
                .Take(8)
                .Select(x => new
                {
                    x.Id,
                    x.ToolAssetId,
                    x.FileName,
                    DocumentType = x.DocumentType.ToString(),
                    x.UploadedAt,
                    x.UploadedBy
                })
                .ToList()
        };

        var purchaseSummary = new
        {
            Total = 0,
            Completed = 0,
            Pending = 0,
            Rejected = 0,
            ModuleStatus = "Pendiente de implementar",
            Note = "Cuando se cree el módulo Solicitar Compra AF, estos indicadores saldrán de solicitudes reales."
        };

        var recentActivity = events
            .Select(x => new
            {
                x.Id,
                x.EventType,
                x.Title,
                x.Description,
                ToolInternalCode = x.ToolAsset != null ? x.ToolAsset.InternalCode : null,
                ToolName = x.ToolAsset != null ? x.ToolAsset.Name : null,
                RegisteredAt = x.RegisteredAt == default ? x.CreatedAt : x.RegisteredAt,
                User = x.RegisteredBy ?? x.CreatedBy ?? x.UpdatedBy ?? "sistema"
            })
            .ToList();

        var response = new
        {
            GeneratedAt = now,
            Summary = new
            {
                TotalTools = totalTools,
                AvailableTools = availableTools,
                PendingValidationTools = pendingValidationTools,
                AssignedTools = assignedTools,
                LoanedTools = loanedTools,
                InMaintenanceTools = inMaintenanceTools,
                DamagedTools = damagedTools,
                NotLocatedTools = notLocatedTools,
                DisposedTools = disposedTools,
                FixedAssets = fixedAssets,
                ToolsWithoutFixedAssetCode = toolsWithoutFixedAssetCode,
                ToolsOnly = toolsOnly,
                NonToolAssets = nonToolAssets,
                SpecializedTools = tools.Count(x => x.IsSpecialized),
                RequiresMaintenance = tools.Count(x => x.RequiresMaintenance),
                RequiresPreOperationalCheck = tools.Count(x => x.RequiresPreOperationalCheck),
                RequiresCertification = tools.Count(x => x.RequiresCertification),
                ResponsiblePeople = responsibles.Count,
                ActiveResponsibles = responsibles.Count(x => x.IsActive),
                InactiveResponsibles = responsibles.Count(x => !x.IsActive),
                Branches = branches.Count,
                ActiveBranches = branches.Count(x => x.IsActive),
                InactiveBranches = branches.Count(x => !x.IsActive),
                WithLifeRecord = totalTools,
                WithoutLifeRecord = 0
            },
            ToolsByStatus = toolsByStatus,
            ToolsByBranch = toolsByBranch,
            ToolsByType = toolsByType,
            ToolsByCategory = toolsByCategory,
            MaintenanceByMonth = maintenanceByMonth,
            MaintenanceByDay = maintenanceByDay,
            LifeRecordCoverage = lifeRecords,
            AssignmentSummary = assignmentSummary,
            MaintenanceSummary = maintenanceSummary,
            DocumentSummary = documentSummary,
            PurchaseSummary = purchaseSummary,
            RecentActivity = recentActivity
        };

        return Ok(response);
    }
}
