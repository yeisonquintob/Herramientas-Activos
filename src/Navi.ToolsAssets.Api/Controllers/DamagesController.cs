using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Damages;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/damages")]
public class DamagesController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public DamagesController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetDamages()
    {
        var damages = await _context.DamageReports
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .OrderByDescending(x => x.ReportedAt)
            .Select(x => new
            {
                x.Id,
                x.ReportNumber,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                Severity = x.Severity.ToString(),
                SeverityLabel = GetSeverityLabel(x.Severity),
                x.Description,
                x.ReportedAt,
                x.ReportedBy,
                x.ActionTaken,
                x.BlocksLoan,
                IsClosed = !string.IsNullOrWhiteSpace(x.ActionTaken),
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync();

        return Ok(damages);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDamageById(Guid id)
    {
        var damage = await _context.DamageReports
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ReportNumber,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                Severity = x.Severity.ToString(),
                SeverityLabel = GetSeverityLabel(x.Severity),
                x.Description,
                x.ReportedAt,
                x.ReportedBy,
                x.ActionTaken,
                x.BlocksLoan,
                IsClosed = !string.IsNullOrWhiteSpace(x.ActionTaken),
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .FirstOrDefaultAsync();

        if (damage is null)
        {
            return NotFound(new { Message = $"No se encontró el reporte de daño con Id {id}." });
        }

        return Ok(damage);
    }

    [HttpGet("by-tool/{toolId:guid}")]
    public async Task<IActionResult> GetDamagesByTool(Guid toolId)
    {
        var damages = await _context.DamageReports
            .AsNoTracking()
            .Where(x => x.ToolAssetId == toolId)
            .OrderByDescending(x => x.ReportedAt)
            .Select(x => new
            {
                x.Id,
                x.ReportNumber,
                x.ToolAssetId,
                Severity = x.Severity.ToString(),
                SeverityLabel = GetSeverityLabel(x.Severity),
                x.Description,
                x.ReportedAt,
                x.ReportedBy,
                x.ActionTaken,
                x.BlocksLoan,
                IsClosed = !string.IsNullOrWhiteSpace(x.ActionTaken)
            })
            .ToListAsync();

        return Ok(damages);
    }

    [HttpGet("by-code/{internalCode}")]
    public async Task<IActionResult> GetDamagesByToolInternalCode(string internalCode)
    {
        var code = NormalizeCode(internalCode);

        var damages = await _context.DamageReports
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Where(x => x.ToolAsset != null && x.ToolAsset.InternalCode == code)
            .OrderByDescending(x => x.ReportedAt)
            .Select(x => new
            {
                x.Id,
                x.ReportNumber,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                Severity = x.Severity.ToString(),
                SeverityLabel = GetSeverityLabel(x.Severity),
                x.Description,
                x.ReportedAt,
                x.ReportedBy,
                x.ActionTaken,
                x.BlocksLoan,
                IsClosed = !string.IsNullOrWhiteSpace(x.ActionTaken)
            })
            .ToListAsync();

        return Ok(damages);
    }

    [HttpPost("report")]
    public async Task<IActionResult> ReportDamage([FromBody] ReportDamageRequest request)
    {
        if (request.ToolAssetId is null && string.IsNullOrWhiteSpace(request.ToolInternalCode))
        {
            return BadRequest(new { Message = "Debe indicar el Id de la herramienta o el código interno." });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new { Message = "La descripción del daño o novedad es obligatoria." });
        }

        var toolQuery = _context.ToolAssets.AsQueryable();

        var tool = request.ToolAssetId is not null
            ? await toolQuery.FirstOrDefaultAsync(x => x.Id == request.ToolAssetId.Value)
            : await toolQuery.FirstOrDefaultAsync(x => x.InternalCode == NormalizeCode(request.ToolInternalCode!));

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        if (tool.OperationalStatus == ToolOperationalStatus.Disposed)
        {
            return BadRequest(new { Message = "No se puede reportar daño operativo sobre una herramienta dada de baja." });
        }

        var severity = ParseSeverity(request.Severity);
        var changedBy = string.IsNullOrWhiteSpace(request.ReportedBy)
            ? "api"
            : request.ReportedBy.Trim();

        var previousOperationalStatus = tool.OperationalStatus;

        var newOperationalStatus = severity == ToolDamageSeverity.Critical
            ? ToolOperationalStatus.NotSuitable
            : ToolOperationalStatus.Damaged;

        var report = new DamageReport
        {
            ReportNumber = $"DAN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ToolAssetId = tool.Id,
            Severity = severity,
            Description = request.Description.Trim(),
            ReportedAt = DateTime.UtcNow,
            ReportedBy = changedBy,
            BlocksLoan = request.BlocksLoan ?? true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = changedBy
        };

        tool.OperationalStatus = newOperationalStatus;
        tool.PhysicalStatus = ToolPhysicalStatus.Damaged;
        tool.UpdatedAt = DateTime.UtcNow;
        tool.UpdatedBy = changedBy;

        _context.DamageReports.Add(report);

        AddToolLifeCycleEvent(
            tool.Id,
            "DamageReported",
            "Daño o novedad reportada",
            report.Description,
            previousOperationalStatus.ToString(),
            newOperationalStatus.ToString(),
            changedBy);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetDamageById), new { id = report.Id }, new
        {
            report.Id,
            report.ReportNumber,
            report.ToolAssetId,
            ToolInternalCode = tool.InternalCode,
            ToolName = tool.Name,
            Severity = report.Severity.ToString(),
            SeverityLabel = GetSeverityLabel(report.Severity),
            report.Description,
            report.ReportedAt,
            report.ReportedBy,
            report.BlocksLoan,
            ToolOperationalStatus = tool.OperationalStatus.ToString(),
            ToolPhysicalStatus = tool.PhysicalStatus.ToString()
        });
    }

    [HttpPatch("{id:guid}/close")]
    public async Task<IActionResult> CloseDamage(Guid id, [FromBody] CloseDamageRequest request)
    {
        var damage = await _context.DamageReports
            .Include(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (damage is null)
        {
            return NotFound(new { Message = $"No se encontró el reporte de daño con Id {id}." });
        }

        if (string.IsNullOrWhiteSpace(request.ActionTaken))
        {
            return BadRequest(new { Message = "Debe indicar la acción tomada para cerrar la novedad." });
        }

        var changedBy = string.IsNullOrWhiteSpace(request.ClosedBy)
            ? "api"
            : request.ClosedBy.Trim();

        damage.ActionTaken = request.ActionTaken.Trim();
        damage.UpdatedAt = DateTime.UtcNow;
        damage.UpdatedBy = changedBy;

        string? previousOperationalStatus = null;
        string? newOperationalStatus = null;

        if (damage.ToolAsset is not null && (request.ChangeToolStatus ?? true))
        {
            var targetStatus = ParseOperationalStatus(request.TargetOperationalStatus);

            if (targetStatus == ToolOperationalStatus.Loaned)
            {
                return BadRequest(new { Message = "No se puede cerrar una novedad cambiando la herramienta directamente a Prestada." });
            }

            previousOperationalStatus = damage.ToolAsset.OperationalStatus.ToString();

            damage.ToolAsset.OperationalStatus = targetStatus;

            if (targetStatus == ToolOperationalStatus.Available)
            {
                damage.ToolAsset.PhysicalStatus = ToolPhysicalStatus.Good;
            }

            damage.ToolAsset.UpdatedAt = DateTime.UtcNow;
            damage.ToolAsset.UpdatedBy = changedBy;

            newOperationalStatus = damage.ToolAsset.OperationalStatus.ToString();
        }

        AddToolLifeCycleEvent(
            damage.ToolAssetId,
            "DamageClosed",
            "Daño o novedad cerrada",
            damage.ActionTaken,
            previousOperationalStatus,
            newOperationalStatus,
            changedBy);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            damage.Id,
            damage.ReportNumber,
            damage.ToolAssetId,
            damage.ActionTaken,
            ClosedBy = changedBy,
            ClosedAt = damage.UpdatedAt,
            ToolOperationalStatus = damage.ToolAsset == null ? null : damage.ToolAsset.OperationalStatus.ToString(),
            ToolPhysicalStatus = damage.ToolAsset == null ? null : damage.ToolAsset.PhysicalStatus.ToString()
        });
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static ToolDamageSeverity ParseSeverity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ToolDamageSeverity.Low;
        }

        if (Enum.TryParse<ToolDamageSeverity>(value.Trim(), ignoreCase: true, out var severity))
        {
            return severity;
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "BAJA" => ToolDamageSeverity.Low,
            "MEDIA" => ToolDamageSeverity.Medium,
            "ALTA" => ToolDamageSeverity.High,
            "CRITICA" => ToolDamageSeverity.Critical,
            "CRÍTICA" => ToolDamageSeverity.Critical,
            _ => ToolDamageSeverity.Low
        };
    }

    private static ToolOperationalStatus ParseOperationalStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ToolOperationalStatus.Available;
        }

        if (Enum.TryParse<ToolOperationalStatus>(value.Trim(), ignoreCase: true, out var status))
        {
            return status;
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "DISPONIBLE" => ToolOperationalStatus.Available,
            "DAÑADA" => ToolOperationalStatus.Damaged,
            "DANADA" => ToolOperationalStatus.Damaged,
            "NO APTA" => ToolOperationalStatus.NotSuitable,
            "EN MANTENIMIENTO" => ToolOperationalStatus.InMaintenance,
            _ => ToolOperationalStatus.Available
        };
    }

    private static string GetSeverityLabel(ToolDamageSeverity severity)
    {
        return severity switch
        {
            ToolDamageSeverity.Low => "Baja",
            ToolDamageSeverity.Medium => "Media",
            ToolDamageSeverity.High => "Alta",
            ToolDamageSeverity.Critical => "Crítica",
            _ => severity.ToString()
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

public sealed class ReportDamageRequest
{
    public Guid? ToolAssetId { get; set; }

    public string? ToolInternalCode { get; set; }

    public string? Severity { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? ReportedBy { get; set; }

    public bool? BlocksLoan { get; set; }
}

public sealed class CloseDamageRequest
{
    public string ActionTaken { get; set; } = string.Empty;

    public string? ClosedBy { get; set; }

    public bool? ChangeToolStatus { get; set; }

    public string? TargetOperationalStatus { get; set; }
}
