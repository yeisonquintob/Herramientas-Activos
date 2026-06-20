using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Sync;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/reconciliation")]
public class ReconciliationController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public ReconciliationController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetReconciliationRecords()
    {
        var records = await _context.FenixReconciliationRecords
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .OrderByDescending(x => x.ProcessedAt)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                NaviFenixCode = x.ToolAsset == null ? null : x.ToolAsset.FenixCode,
                NaviFixedAssetCode = x.ToolAsset == null ? null : x.ToolAsset.FixedAssetCode,
                NaviBranchCode = x.ToolAsset == null || x.ToolAsset.Branch == null ? null : x.ToolAsset.Branch.Code,
                NaviOperationalStatus = x.ToolAsset == null ? null : x.ToolAsset.OperationalStatus.ToString(),
                x.SourceSystem,
                x.FenixCode,
                x.FixedAssetCode,
                x.FenixStatus,
                x.FenixBranch,
                x.FenixResponsible,
                ResultStatus = x.ResultStatus.ToString(),
                ResultStatusLabel = GetReconciliationStatusLabel(x.ResultStatus),
                x.Differences,
                x.ProcessedAt,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReconciliationById(Guid id)
    {
        var record = await _context.FenixReconciliationRecords
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                NaviFenixCode = x.ToolAsset == null ? null : x.ToolAsset.FenixCode,
                NaviFixedAssetCode = x.ToolAsset == null ? null : x.ToolAsset.FixedAssetCode,
                NaviBranchCode = x.ToolAsset == null || x.ToolAsset.Branch == null ? null : x.ToolAsset.Branch.Code,
                NaviOperationalStatus = x.ToolAsset == null ? null : x.ToolAsset.OperationalStatus.ToString(),
                NaviReconciliationStatus = x.ToolAsset == null ? null : x.ToolAsset.ReconciliationStatus.ToString(),
                NaviSyncStatus = x.ToolAsset == null ? null : x.ToolAsset.SyncStatus.ToString(),
                x.SourceSystem,
                x.FenixCode,
                x.FixedAssetCode,
                x.FenixStatus,
                x.FenixBranch,
                x.FenixResponsible,
                ResultStatus = x.ResultStatus.ToString(),
                ResultStatusLabel = GetReconciliationStatusLabel(x.ResultStatus),
                x.Differences,
                x.ProcessedAt,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .FirstOrDefaultAsync();

        if (record is null)
        {
            return NotFound(new { Message = $"No se encontró la conciliación con Id {id}." });
        }

        return Ok(record);
    }

    [HttpGet("by-tool/{toolId:guid}")]
    public async Task<IActionResult> GetReconciliationByTool(Guid toolId)
    {
        var records = await _context.FenixReconciliationRecords
            .AsNoTracking()
            .Where(x => x.ToolAssetId == toolId)
            .OrderByDescending(x => x.ProcessedAt)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.SourceSystem,
                x.FenixCode,
                x.FixedAssetCode,
                x.FenixStatus,
                x.FenixBranch,
                x.FenixResponsible,
                ResultStatus = x.ResultStatus.ToString(),
                ResultStatusLabel = GetReconciliationStatusLabel(x.ResultStatus),
                x.Differences,
                x.ProcessedAt
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("by-code/{internalCode}")]
    public async Task<IActionResult> GetReconciliationByToolInternalCode(string internalCode)
    {
        var code = NormalizeCode(internalCode);

        var records = await _context.FenixReconciliationRecords
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Where(x => x.ToolAsset != null && x.ToolAsset.InternalCode == code)
            .OrderByDescending(x => x.ProcessedAt)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                x.SourceSystem,
                x.FenixCode,
                x.FixedAssetCode,
                x.FenixStatus,
                x.FenixBranch,
                x.FenixResponsible,
                ResultStatus = x.ResultStatus.ToString(),
                ResultStatusLabel = GetReconciliationStatusLabel(x.ResultStatus),
                x.Differences,
                x.ProcessedAt
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpPost("manual")]
    public async Task<IActionResult> ManualReconciliation([FromBody] ManualReconciliationRequest request)
    {
        if (request.ToolAssetId is null && string.IsNullOrWhiteSpace(request.ToolInternalCode))
        {
            return BadRequest(new { Message = "Debe indicar el Id de la herramienta o el código interno." });
        }

        var tool = request.ToolAssetId is not null
            ? await _context.ToolAssets
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.Id == request.ToolAssetId.Value)
            : await _context.ToolAssets
                .Include(x => x.Branch)
                .FirstOrDefaultAsync(x => x.InternalCode == NormalizeCode(request.ToolInternalCode!));

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        var processedBy = string.IsNullOrWhiteSpace(request.ProcessedBy)
            ? "api"
            : request.ProcessedBy.Trim();

        var differences = BuildDifferences(
            naviFenixCode: tool.FenixCode,
            fenixCode: request.FenixCode,
            naviFixedAssetCode: tool.FixedAssetCode,
            fixedAssetCode: request.FixedAssetCode,
            naviBranchCode: tool.Branch?.Code,
            fenixBranch: request.FenixBranch,
            naviOperationalStatus: tool.OperationalStatus.ToString(),
            fenixStatus: request.FenixStatus);

        var hasAnyFenixData = HasAnyFenixData(request);

        var resultStatus = !hasAnyFenixData
            ? ToolReconciliationStatus.NotFoundInFenix365
            : differences.Count == 0
                ? ToolReconciliationStatus.Validated
                : ToolReconciliationStatus.Inconsistent;

        var previousReconciliationStatus = tool.ReconciliationStatus;
        var previousSyncStatus = tool.SyncStatus;

        tool.ReconciliationStatus = resultStatus;

        tool.SyncStatus = resultStatus == ToolReconciliationStatus.Validated
            ? ToolSyncStatus.Synced
            : ToolSyncStatus.SyncError;

        tool.UpdatedAt = DateTime.UtcNow;
        tool.UpdatedBy = processedBy;

        var record = new FenixReconciliationRecord
        {
            ToolAssetId = tool.Id,
            SourceSystem = string.IsNullOrWhiteSpace(request.SourceSystem)
                ? "DynamicsExport"
                : request.SourceSystem.Trim(),
            FenixCode = request.FenixCode,
            FixedAssetCode = request.FixedAssetCode,
            FenixStatus = request.FenixStatus,
            FenixBranch = request.FenixBranch,
            FenixResponsible = request.FenixResponsible,
            ResultStatus = resultStatus,
            Differences = differences.Count == 0 ? null : string.Join(" | ", differences),
            ProcessedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = processedBy
        };

        _context.FenixReconciliationRecords.Add(record);

        AddToolLifeCycleEvent(
            tool.Id,
            "FenixReconciliationProcessed",
            "Conciliación Fenix365/Dynamics procesada",
            record.Differences is null
                ? $"Conciliación validada desde {record.SourceSystem}."
                : $"Conciliación con diferencias desde {record.SourceSystem}: {record.Differences}",
            previousReconciliationStatus.ToString(),
            tool.ReconciliationStatus.ToString(),
            processedBy);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReconciliationById), new { id = record.Id }, new
        {
            record.Id,
            record.ToolAssetId,
            ToolInternalCode = tool.InternalCode,
            ToolName = tool.Name,
            record.SourceSystem,
            record.FenixCode,
            record.FixedAssetCode,
            record.FenixStatus,
            record.FenixBranch,
            record.FenixResponsible,
            ResultStatus = record.ResultStatus.ToString(),
            ResultStatusLabel = GetReconciliationStatusLabel(record.ResultStatus),
            record.Differences,
            record.ProcessedAt,
            ToolReconciliationStatus = tool.ReconciliationStatus.ToString(),
            ToolSyncStatus = tool.SyncStatus.ToString(),
            PreviousReconciliationStatus = previousReconciliationStatus.ToString(),
            PreviousSyncStatus = previousSyncStatus.ToString()
        });
    }

    [HttpPatch("{id:guid}/validate")]
    public async Task<IActionResult> ValidateReconciliation(Guid id, [FromBody] ReconciliationActionRequest request)
    {
        var record = await _context.FenixReconciliationRecords
            .Include(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (record is null)
        {
            return NotFound(new { Message = $"No se encontró la conciliación con Id {id}." });
        }

        if (record.ToolAsset is null)
        {
            return BadRequest(new { Message = "La conciliación no tiene herramienta asociada." });
        }

        var changedBy = GetActionUser(request);

        var previousStatus = record.ResultStatus;
        var previousToolReconciliation = record.ToolAsset.ReconciliationStatus;
        var previousToolSync = record.ToolAsset.SyncStatus;

        record.ResultStatus = ToolReconciliationStatus.Validated;
        record.Differences = null;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = changedBy;

        record.ToolAsset.ReconciliationStatus = ToolReconciliationStatus.Validated;
        record.ToolAsset.SyncStatus = ToolSyncStatus.Synced;
        record.ToolAsset.UpdatedAt = DateTime.UtcNow;
        record.ToolAsset.UpdatedBy = changedBy;

        AddToolLifeCycleEvent(
            record.ToolAssetId,
            "FenixReconciliationValidated",
            "Conciliación validada",
            string.IsNullOrWhiteSpace(request.Notes)
                ? "La conciliación fue marcada como validada."
                : request.Notes.Trim(),
            previousToolReconciliation.ToString(),
            record.ToolAsset.ReconciliationStatus.ToString(),
            changedBy);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            record.Id,
            PreviousRecordStatus = previousStatus.ToString(),
            ResultStatus = record.ResultStatus.ToString(),
            PreviousToolReconciliationStatus = previousToolReconciliation.ToString(),
            ToolReconciliationStatus = record.ToolAsset.ReconciliationStatus.ToString(),
            PreviousToolSyncStatus = previousToolSync.ToString(),
            ToolSyncStatus = record.ToolAsset.SyncStatus.ToString(),
            record.UpdatedBy
        });
    }

    [HttpPatch("{id:guid}/mark-inconsistent")]
    public async Task<IActionResult> MarkReconciliationInconsistent(Guid id, [FromBody] ReconciliationActionRequest request)
    {
        var record = await _context.FenixReconciliationRecords
            .Include(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (record is null)
        {
            return NotFound(new { Message = $"No se encontró la conciliación con Id {id}." });
        }

        if (record.ToolAsset is null)
        {
            return BadRequest(new { Message = "La conciliación no tiene herramienta asociada." });
        }

        if (string.IsNullOrWhiteSpace(request.Differences) && string.IsNullOrWhiteSpace(request.Notes))
        {
            return BadRequest(new { Message = "Debe indicar la diferencia o una observación." });
        }

        var changedBy = GetActionUser(request);

        var previousStatus = record.ResultStatus;
        var previousToolReconciliation = record.ToolAsset.ReconciliationStatus;
        var previousToolSync = record.ToolAsset.SyncStatus;

        record.ResultStatus = ToolReconciliationStatus.Inconsistent;
        record.Differences = string.IsNullOrWhiteSpace(request.Differences)
            ? request.Notes!.Trim()
            : request.Differences.Trim();
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = changedBy;

        record.ToolAsset.ReconciliationStatus = ToolReconciliationStatus.Inconsistent;
        record.ToolAsset.SyncStatus = ToolSyncStatus.SyncError;
        record.ToolAsset.UpdatedAt = DateTime.UtcNow;
        record.ToolAsset.UpdatedBy = changedBy;

        AddToolLifeCycleEvent(
            record.ToolAssetId,
            "FenixReconciliationInconsistent",
            "Conciliación marcada como inconsistente",
            record.Differences,
            previousToolReconciliation.ToString(),
            record.ToolAsset.ReconciliationStatus.ToString(),
            changedBy);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            record.Id,
            PreviousRecordStatus = previousStatus.ToString(),
            ResultStatus = record.ResultStatus.ToString(),
            record.Differences,
            PreviousToolReconciliationStatus = previousToolReconciliation.ToString(),
            ToolReconciliationStatus = record.ToolAsset.ReconciliationStatus.ToString(),
            PreviousToolSyncStatus = previousToolSync.ToString(),
            ToolSyncStatus = record.ToolAsset.SyncStatus.ToString(),
            record.UpdatedBy
        });
    }

    private static bool HasAnyFenixData(ManualReconciliationRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.FenixCode)
            || !string.IsNullOrWhiteSpace(request.FixedAssetCode)
            || !string.IsNullOrWhiteSpace(request.FenixStatus)
            || !string.IsNullOrWhiteSpace(request.FenixBranch)
            || !string.IsNullOrWhiteSpace(request.FenixResponsible);
    }

    private static List<string> BuildDifferences(
        string? naviFenixCode,
        string? fenixCode,
        string? naviFixedAssetCode,
        string? fixedAssetCode,
        string? naviBranchCode,
        string? fenixBranch,
        string? naviOperationalStatus,
        string? fenixStatus)
    {
        var differences = new List<string>();

        AddDifference(differences, "Código Fenix365", naviFenixCode, fenixCode);
        AddDifference(differences, "Activo fijo", naviFixedAssetCode, fixedAssetCode);
        AddDifference(differences, "Sede", naviBranchCode, fenixBranch);
        AddDifference(differences, "Estado operativo", naviOperationalStatus, fenixStatus);

        return differences;
    }

    private static void AddDifference(List<string> differences, string fieldName, string? naviValue, string? fenixValue)
    {
        if (string.IsNullOrWhiteSpace(fenixValue))
        {
            return;
        }

        var navi = string.IsNullOrWhiteSpace(naviValue) ? "" : naviValue.Trim();
        var fenix = fenixValue.Trim();

        if (!string.Equals(navi, fenix, StringComparison.OrdinalIgnoreCase))
        {
            differences.Add($"{fieldName}: NAVI='{navi}' / Fenix='{fenix}'");
        }
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string GetActionUser(ReconciliationActionRequest request)
    {
        return string.IsNullOrWhiteSpace(request.ActionBy)
            ? "api"
            : request.ActionBy.Trim();
    }

    private static string GetReconciliationStatusLabel(ToolReconciliationStatus status)
    {
        return status switch
        {
            ToolReconciliationStatus.Pending => "Pendiente",
            ToolReconciliationStatus.Validated => "Validado",
            ToolReconciliationStatus.Inconsistent => "Inconsistente",
            ToolReconciliationStatus.PossibleDuplicate => "Posible duplicado",
            ToolReconciliationStatus.Duplicate => "Duplicado",
            ToolReconciliationStatus.NotFoundInFenix365 => "No existe en Fenix365",
            ToolReconciliationStatus.ExistsInFenix365WithoutLifeCycle => "Existe en Fenix365 sin hoja de vida",
            ToolReconciliationStatus.LifeCycleWithoutFenix365Asset => "Hoja de vida sin activo Fenix365",
            _ => status.ToString()
        };
    }

    private void AddToolLifeCycleEvent(
        Guid toolAssetId,
        string eventType,
        string title,
        string description,
        string? previousValue,
        string? newValue,
        string changedBy)
    {
        _context.ToolLifeCycleEvents.Add(new ToolLifeCycleEvent
        {
            ToolAssetId = toolAssetId,
            EventType = eventType,
            Title = title,
            Description = description,
            PreviousValue = previousValue,
            NewValue = newValue,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = changedBy
        });
    }
}

public sealed class ManualReconciliationRequest
{
    public Guid? ToolAssetId { get; set; }

    public string? ToolInternalCode { get; set; }

    public string? SourceSystem { get; set; }

    public string? FenixCode { get; set; }

    public string? FixedAssetCode { get; set; }

    public string? FenixStatus { get; set; }

    public string? FenixBranch { get; set; }

    public string? FenixResponsible { get; set; }

    public string? ProcessedBy { get; set; }
}

public sealed class ReconciliationActionRequest
{
    public string? ActionBy { get; set; }

    public string? Differences { get; set; }

    public string? Notes { get; set; }
}

