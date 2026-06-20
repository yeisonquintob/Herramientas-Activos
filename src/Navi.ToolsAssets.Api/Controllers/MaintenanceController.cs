using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Maintenance;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/maintenance")]
public class MaintenanceController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public MaintenanceController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetMaintenanceRecords()
    {
        var records = await _context.MaintenanceRecords
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.MaintenanceNumber,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                Type = x.Type.ToString(),
                TypeLabel = GetMaintenanceTypeLabel(x.Type),
                Status = x.Status.ToString(),
                StatusLabel = GetMaintenanceStatusLabel(x.Status),
                x.ScheduledAt,
                x.StartedAt,
                x.FinishedAt,
                x.Provider,
                x.Technician,
                x.Description,
                x.Cost,
                x.Result,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMaintenanceById(Guid id)
    {
        var record = await _context.MaintenanceRecords
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.MaintenanceNumber,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                Type = x.Type.ToString(),
                TypeLabel = GetMaintenanceTypeLabel(x.Type),
                Status = x.Status.ToString(),
                StatusLabel = GetMaintenanceStatusLabel(x.Status),
                x.ScheduledAt,
                x.StartedAt,
                x.FinishedAt,
                x.Provider,
                x.Technician,
                x.Description,
                x.Cost,
                x.Result,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .FirstOrDefaultAsync();

        if (record is null)
        {
            return NotFound(new { Message = $"No se encontró el mantenimiento con Id {id}." });
        }

        return Ok(record);
    }

    [HttpGet("by-tool/{toolId:guid}")]
    public async Task<IActionResult> GetMaintenanceByTool(Guid toolId)
    {
        var records = await _context.MaintenanceRecords
            .AsNoTracking()
            .Where(x => x.ToolAssetId == toolId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.MaintenanceNumber,
                x.ToolAssetId,
                Type = x.Type.ToString(),
                TypeLabel = GetMaintenanceTypeLabel(x.Type),
                Status = x.Status.ToString(),
                StatusLabel = GetMaintenanceStatusLabel(x.Status),
                x.ScheduledAt,
                x.StartedAt,
                x.FinishedAt,
                x.Provider,
                x.Technician,
                x.Description,
                x.Cost,
                x.Result
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpGet("by-code/{internalCode}")]
    public async Task<IActionResult> GetMaintenanceByToolInternalCode(string internalCode)
    {
        var code = NormalizeCode(internalCode);

        var records = await _context.MaintenanceRecords
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Where(x => x.ToolAsset != null && x.ToolAsset.InternalCode == code)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.MaintenanceNumber,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                Type = x.Type.ToString(),
                TypeLabel = GetMaintenanceTypeLabel(x.Type),
                Status = x.Status.ToString(),
                StatusLabel = GetMaintenanceStatusLabel(x.Status),
                x.ScheduledAt,
                x.StartedAt,
                x.FinishedAt,
                x.Provider,
                x.Technician,
                x.Description,
                x.Cost,
                x.Result
            })
            .ToListAsync();

        return Ok(records);
    }

    [HttpPost("schedule")]
    public async Task<IActionResult> ScheduleMaintenance([FromBody] ScheduleMaintenanceRequest request)
    {
        if (request.ToolAssetId is null && string.IsNullOrWhiteSpace(request.ToolInternalCode))
        {
            return BadRequest(new { Message = "Debe indicar el Id de la herramienta o el código interno." });
        }

        var tool = request.ToolAssetId is not null
            ? await _context.ToolAssets.FirstOrDefaultAsync(x => x.Id == request.ToolAssetId.Value)
            : await _context.ToolAssets.FirstOrDefaultAsync(x => x.InternalCode == NormalizeCode(request.ToolInternalCode!));

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        if (tool.OperationalStatus == ToolOperationalStatus.Disposed)
        {
            return BadRequest(new { Message = "No se puede programar mantenimiento a una herramienta dada de baja." });
        }

        var createdBy = string.IsNullOrWhiteSpace(request.RequestedBy)
            ? "api"
            : request.RequestedBy.Trim();

        var maintenance = new MaintenanceRecord
        {
            MaintenanceNumber = $"MAN-{DateTime.UtcNow:yyyyMMddHHmmss}",
            ToolAssetId = tool.Id,
            Type = ParseMaintenanceType(request.Type),
            Status = ToolMaintenanceStatus.Scheduled,
            ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow,
            Provider = request.Provider,
            Technician = request.Technician,
            Description = request.Description,
            Cost = request.Cost,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.MaintenanceRecords.Add(maintenance);

        AddToolLifeCycleEvent(
            tool.Id,
            "MaintenanceScheduled",
            "Mantenimiento programado",
            string.IsNullOrWhiteSpace(maintenance.Description)
                ? $"Se programó mantenimiento {GetMaintenanceTypeLabel(maintenance.Type)}."
                : maintenance.Description,
            tool.OperationalStatus.ToString(),
            tool.OperationalStatus.ToString(),
            createdBy);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMaintenanceById), new { id = maintenance.Id }, new
        {
            maintenance.Id,
            maintenance.MaintenanceNumber,
            maintenance.ToolAssetId,
            ToolInternalCode = tool.InternalCode,
            ToolName = tool.Name,
            Type = maintenance.Type.ToString(),
            TypeLabel = GetMaintenanceTypeLabel(maintenance.Type),
            Status = maintenance.Status.ToString(),
            StatusLabel = GetMaintenanceStatusLabel(maintenance.Status),
            maintenance.ScheduledAt,
            maintenance.Provider,
            maintenance.Technician,
            maintenance.Description,
            maintenance.Cost
        });
    }

    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> StartMaintenance(Guid id, [FromBody] MaintenanceActionRequest request)
    {
        var maintenance = await _context.MaintenanceRecords
            .Include(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (maintenance is null)
        {
            return NotFound(new { Message = $"No se encontró el mantenimiento con Id {id}." });
        }

        if (maintenance.Status != ToolMaintenanceStatus.Scheduled)
        {
            return BadRequest(new { Message = $"Solo se puede iniciar un mantenimiento programado. Estado actual: {maintenance.Status}." });
        }

        if (maintenance.ToolAsset is null)
        {
            return BadRequest(new { Message = "El mantenimiento no tiene herramienta asociada." });
        }

        if (maintenance.ToolAsset.OperationalStatus == ToolOperationalStatus.Disposed)
        {
            return BadRequest(new { Message = "No se puede iniciar mantenimiento de una herramienta dada de baja." });
        }

        if (maintenance.ToolAsset.OperationalStatus == ToolOperationalStatus.Loaned)
        {
            return BadRequest(new { Message = "No se puede iniciar mantenimiento porque la herramienta está prestada." });
        }

        var changedBy = GetActionUser(request);
        var previousToolStatus = maintenance.ToolAsset.OperationalStatus;

        maintenance.Status = ToolMaintenanceStatus.InProgress;
        maintenance.StartedAt = DateTime.UtcNow;
        maintenance.UpdatedAt = DateTime.UtcNow;
        maintenance.UpdatedBy = changedBy;

        if (!string.IsNullOrWhiteSpace(request.Provider))
        {
            maintenance.Provider = request.Provider.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Technician))
        {
            maintenance.Technician = request.Technician.Trim();
        }

        maintenance.ToolAsset.OperationalStatus = ToolOperationalStatus.InMaintenance;
        maintenance.ToolAsset.UpdatedAt = DateTime.UtcNow;
        maintenance.ToolAsset.UpdatedBy = changedBy;

        AddToolLifeCycleEvent(
            maintenance.ToolAssetId,
            "MaintenanceStarted",
            "Mantenimiento iniciado",
            string.IsNullOrWhiteSpace(request.Notes)
                ? $"Se inició el mantenimiento {maintenance.MaintenanceNumber}."
                : request.Notes.Trim(),
            previousToolStatus.ToString(),
            ToolOperationalStatus.InMaintenance.ToString(),
            changedBy);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            maintenance.Id,
            maintenance.MaintenanceNumber,
            Status = maintenance.Status.ToString(),
            maintenance.StartedAt,
            ToolOperationalStatus = maintenance.ToolAsset.OperationalStatus.ToString(),
            maintenance.UpdatedBy
        });
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompleteMaintenance(Guid id, [FromBody] MaintenanceActionRequest request)
    {
        var maintenance = await _context.MaintenanceRecords
            .Include(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (maintenance is null)
        {
            return NotFound(new { Message = $"No se encontró el mantenimiento con Id {id}." });
        }

        if (maintenance.Status != ToolMaintenanceStatus.InProgress)
        {
            return BadRequest(new { Message = $"Solo se puede completar un mantenimiento en progreso. Estado actual: {maintenance.Status}." });
        }

        if (maintenance.ToolAsset is null)
        {
            return BadRequest(new { Message = "El mantenimiento no tiene herramienta asociada." });
        }

        var changedBy = GetActionUser(request);
        var previousToolStatus = maintenance.ToolAsset.OperationalStatus;

        var targetStatus = ParseOperationalStatus(request.TargetOperationalStatus);

        if (targetStatus == ToolOperationalStatus.Loaned)
        {
            return BadRequest(new { Message = "No se puede finalizar un mantenimiento cambiando la herramienta directamente a Prestada." });
        }

        maintenance.Status = ToolMaintenanceStatus.Completed;
        maintenance.FinishedAt = DateTime.UtcNow;
        maintenance.Result = string.IsNullOrWhiteSpace(request.Result)
            ? request.Notes
            : request.Result;
        maintenance.UpdatedAt = DateTime.UtcNow;
        maintenance.UpdatedBy = changedBy;

        if (request.Cost is not null)
        {
            maintenance.Cost = request.Cost;
        }

        if (!string.IsNullOrWhiteSpace(request.Provider))
        {
            maintenance.Provider = request.Provider.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Technician))
        {
            maintenance.Technician = request.Technician.Trim();
        }

        maintenance.ToolAsset.OperationalStatus = targetStatus;

        if (targetStatus == ToolOperationalStatus.Available)
        {
            maintenance.ToolAsset.PhysicalStatus = ToolPhysicalStatus.Good;
        }

        maintenance.ToolAsset.UpdatedAt = DateTime.UtcNow;
        maintenance.ToolAsset.UpdatedBy = changedBy;

        AddToolLifeCycleEvent(
            maintenance.ToolAssetId,
            "MaintenanceCompleted",
            "Mantenimiento completado",
            string.IsNullOrWhiteSpace(maintenance.Result)
                ? $"Se completó el mantenimiento {maintenance.MaintenanceNumber}."
                : maintenance.Result,
            previousToolStatus.ToString(),
            targetStatus.ToString(),
            changedBy);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            maintenance.Id,
            maintenance.MaintenanceNumber,
            Status = maintenance.Status.ToString(),
            maintenance.FinishedAt,
            maintenance.Result,
            maintenance.Cost,
            ToolOperationalStatus = maintenance.ToolAsset.OperationalStatus.ToString(),
            ToolPhysicalStatus = maintenance.ToolAsset.PhysicalStatus.ToString(),
            maintenance.UpdatedBy
        });
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelMaintenance(Guid id, [FromBody] MaintenanceActionRequest request)
    {
        var maintenance = await _context.MaintenanceRecords
            .Include(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (maintenance is null)
        {
            return NotFound(new { Message = $"No se encontró el mantenimiento con Id {id}." });
        }

        if (maintenance.Status is ToolMaintenanceStatus.Completed or ToolMaintenanceStatus.Cancelled)
        {
            return BadRequest(new { Message = $"No se puede cancelar un mantenimiento en estado {maintenance.Status}." });
        }

        var changedBy = GetActionUser(request);

        string? previousToolStatus = null;
        string? newToolStatus = null;

        maintenance.Status = ToolMaintenanceStatus.Cancelled;
        maintenance.Result = string.IsNullOrWhiteSpace(request.Notes)
            ? "Mantenimiento cancelado."
            : request.Notes.Trim();
        maintenance.UpdatedAt = DateTime.UtcNow;
        maintenance.UpdatedBy = changedBy;

        if (maintenance.ToolAsset is not null && (request.ChangeToolStatus ?? true))
        {
            previousToolStatus = maintenance.ToolAsset.OperationalStatus.ToString();

            var targetStatus = ParseOperationalStatus(request.TargetOperationalStatus);
            maintenance.ToolAsset.OperationalStatus = targetStatus;

            if (targetStatus == ToolOperationalStatus.Available)
            {
                maintenance.ToolAsset.PhysicalStatus = ToolPhysicalStatus.Good;
            }

            maintenance.ToolAsset.UpdatedAt = DateTime.UtcNow;
            maintenance.ToolAsset.UpdatedBy = changedBy;

            newToolStatus = maintenance.ToolAsset.OperationalStatus.ToString();
        }

        AddToolLifeCycleEvent(
            maintenance.ToolAssetId,
            "MaintenanceCancelled",
            "Mantenimiento cancelado",
            maintenance.Result,
            previousToolStatus,
            newToolStatus,
            changedBy);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            maintenance.Id,
            maintenance.MaintenanceNumber,
            Status = maintenance.Status.ToString(),
            maintenance.Result,
            ToolOperationalStatus = maintenance.ToolAsset == null ? null : maintenance.ToolAsset.OperationalStatus.ToString(),
            maintenance.UpdatedBy
        });
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string GetActionUser(MaintenanceActionRequest request)
    {
        return string.IsNullOrWhiteSpace(request.ActionBy)
            ? "api"
            : request.ActionBy.Trim();
    }

    private static ToolMaintenanceType ParseMaintenanceType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ToolMaintenanceType.Preventive;
        }

        if (Enum.TryParse<ToolMaintenanceType>(value.Trim(), ignoreCase: true, out var type))
        {
            return type;
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "PREVENTIVO" => ToolMaintenanceType.Preventive,
            "CORRECTIVO" => ToolMaintenanceType.Corrective,
            "CALIBRACION" => ToolMaintenanceType.Calibration,
            "CALIBRACIÓN" => ToolMaintenanceType.Calibration,
            "INSPECCION" => ToolMaintenanceType.Inspection,
            "INSPECCIÓN" => ToolMaintenanceType.Inspection,
            _ => ToolMaintenanceType.Preventive
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

    private static string GetMaintenanceTypeLabel(ToolMaintenanceType type)
    {
        return type switch
        {
            ToolMaintenanceType.Preventive => "Preventivo",
            ToolMaintenanceType.Corrective => "Correctivo",
            ToolMaintenanceType.Calibration => "Calibración",
            ToolMaintenanceType.Inspection => "Inspección",
            _ => type.ToString()
        };
    }

    private static string GetMaintenanceStatusLabel(ToolMaintenanceStatus status)
    {
        return status switch
        {
            ToolMaintenanceStatus.Scheduled => "Programado",
            ToolMaintenanceStatus.InProgress => "En progreso",
            ToolMaintenanceStatus.Completed => "Completado",
            ToolMaintenanceStatus.Cancelled => "Cancelado",
            ToolMaintenanceStatus.Failed => "Fallido",
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

public sealed class ScheduleMaintenanceRequest
{
    public Guid? ToolAssetId { get; set; }

    public string? ToolInternalCode { get; set; }

    public string? Type { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public string? Provider { get; set; }

    public string? Technician { get; set; }

    public string? Description { get; set; }

    public decimal? Cost { get; set; }

    public string? RequestedBy { get; set; }
}

public sealed class MaintenanceActionRequest
{
    public string? ActionBy { get; set; }

    public string? Notes { get; set; }

    public string? Provider { get; set; }

    public string? Technician { get; set; }

    public decimal? Cost { get; set; }

    public string? Result { get; set; }

    public bool? ChangeToolStatus { get; set; }

    public string? TargetOperationalStatus { get; set; }
}
