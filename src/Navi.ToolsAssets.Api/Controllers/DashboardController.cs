using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public DashboardController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var tools = await _context.ToolAssets
            .AsNoTracking()
            .ToListAsync();

        var summary = new
        {
            TotalTools = tools.Count,
            AvailableTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.Available),
            LoanedTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.Loaned),
            InMaintenanceTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.InMaintenance),
            DamagedTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.Damaged),
            NotSuitableTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.NotSuitable),
            NotLocatedTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.NotLocated),
            PendingValidationTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.PendingValidation),
            DisposedTools = tools.Count(x => x.OperationalStatus == ToolOperationalStatus.Disposed),
            SpecializedTools = tools.Count(x => x.IsSpecialized),
            NonSpecializedTools = tools.Count(x => !x.IsSpecialized),
            ReconciledTools = tools.Count(x => x.ReconciliationStatus == ToolReconciliationStatus.Validated),
            InconsistentTools = tools.Count(x => x.ReconciliationStatus == ToolReconciliationStatus.Inconsistent),
            SyncErrorTools = tools.Count(x => x.SyncStatus == ToolSyncStatus.SyncError),
            PendingSyncTools = tools.Count(x => x.SyncStatus == ToolSyncStatus.PendingSync),
            LastUpdatedAt = DateTime.UtcNow
        };

        return Ok(summary);
    }

    [HttpGet("tools-by-status")]
    public async Task<IActionResult> GetToolsByStatus()
    {
        var tools = await _context.ToolAssets
            .AsNoTracking()
            .ToListAsync();

        var result = tools
            .GroupBy(x => x.OperationalStatus)
            .Select(x => new
            {
                Status = x.Key.ToString(),
                StatusLabel = GetOperationalStatusLabel(x.Key),
                Count = x.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        return Ok(result);
    }

    [HttpGet("tools-by-branch")]
    public async Task<IActionResult> GetToolsByBranch()
    {
        var result = await _context.Branches
            .AsNoTracking()
            .Include(x => x.Zone)
            .Select(branch => new
            {
                BranchId = branch.Id,
                BranchCode = branch.Code,
                BranchName = branch.Name,
                ZoneCode = branch.Zone == null ? null : branch.Zone.Code,
                ZoneName = branch.Zone == null ? null : branch.Zone.Name,
                TotalTools = _context.ToolAssets.Count(tool => tool.BranchId == branch.Id),
                AvailableTools = _context.ToolAssets.Count(tool => tool.BranchId == branch.Id && tool.OperationalStatus == ToolOperationalStatus.Available),
                LoanedTools = _context.ToolAssets.Count(tool => tool.BranchId == branch.Id && tool.OperationalStatus == ToolOperationalStatus.Loaned),
                InMaintenanceTools = _context.ToolAssets.Count(tool => tool.BranchId == branch.Id && tool.OperationalStatus == ToolOperationalStatus.InMaintenance),
                DamagedTools = _context.ToolAssets.Count(tool => tool.BranchId == branch.Id && tool.OperationalStatus == ToolOperationalStatus.Damaged),
                NotLocatedTools = _context.ToolAssets.Count(tool => tool.BranchId == branch.Id && tool.OperationalStatus == ToolOperationalStatus.NotLocated)
            })
            .OrderByDescending(x => x.TotalTools)
            .ThenBy(x => x.BranchCode)
            .ToListAsync();

        return Ok(result);
    }

    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var damagedTools = await _context.ToolAssets
            .AsNoTracking()
            .Where(x => x.OperationalStatus == ToolOperationalStatus.Damaged
                || x.OperationalStatus == ToolOperationalStatus.NotSuitable)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(10)
            .Select(x => new
            {
                Type = "ToolBlocked",
                Severity = "High",
                ToolId = x.Id,
                x.InternalCode,
                x.Name,
                Status = x.OperationalStatus.ToString(),
                Message = "Herramienta bloqueada para préstamo por estado operativo.",
                Date = x.UpdatedAt ?? x.CreatedAt
            })
            .ToListAsync();

        var notLocatedTools = await _context.ToolAssets
            .AsNoTracking()
            .Where(x => x.OperationalStatus == ToolOperationalStatus.NotLocated)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(10)
            .Select(x => new
            {
                Type = "ToolNotLocated",
                Severity = "Critical",
                ToolId = x.Id,
                x.InternalCode,
                x.Name,
                Status = x.OperationalStatus.ToString(),
                Message = "Herramienta no localizada en toma física.",
                Date = x.UpdatedAt ?? x.CreatedAt
            })
            .ToListAsync();

        var syncErrors = await _context.ToolAssets
            .AsNoTracking()
            .Where(x => x.SyncStatus == ToolSyncStatus.SyncError
                || x.ReconciliationStatus == ToolReconciliationStatus.Inconsistent)
            .OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt)
            .Take(10)
            .Select(x => new
            {
                Type = "SyncOrReconciliationError",
                Severity = "Medium",
                ToolId = x.Id,
                x.InternalCode,
                x.Name,
                Status = x.ReconciliationStatus.ToString(),
                Message = "Herramienta con inconsistencia de conciliación o sincronización.",
                Date = x.UpdatedAt ?? x.CreatedAt
            })
            .ToListAsync();

        var alerts = damagedTools
            .Concat(notLocatedTools)
            .Concat(syncErrors)
            .OrderByDescending(x => x.Date)
            .Take(20)
            .ToList();

        return Ok(alerts);
    }

    [HttpGet("recent-activity")]
    public async Task<IActionResult> GetRecentActivity()
    {
        var events = await _context.ToolLifeCycleEvents
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .OrderByDescending(x => x.CreatedAt)
            .Take(30)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                x.EventType,
                x.Title,
                x.Description,
                x.PreviousValue,
                x.NewValue,
                EventDate = x.CreatedAt,
                PerformedBy = x.CreatedBy
            })
            .ToListAsync();

        return Ok(events);
    }

    private static string GetOperationalStatusLabel(ToolOperationalStatus status)
    {
        return status switch
        {
            ToolOperationalStatus.Available => "Disponible",
            ToolOperationalStatus.Assigned => "Asignada",
            ToolOperationalStatus.Loaned => "Prestada",
            ToolOperationalStatus.InMaintenance => "En mantenimiento",
            ToolOperationalStatus.Damaged => "Dañada",
            ToolOperationalStatus.NotSuitable => "No apta",
            ToolOperationalStatus.PendingValidation => "Pendiente de validación",
            ToolOperationalStatus.Inconsistent => "Inconsistente",
            ToolOperationalStatus.NotLocated => "No localizada",
            ToolOperationalStatus.PendingDisposal => "Pendiente de baja",
            ToolOperationalStatus.Disposed => "Dada de baja",
            _ => status.ToString()
        };
    }
}
