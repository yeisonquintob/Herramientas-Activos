using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Maintenance;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolMaintenanceScheduleController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public ToolMaintenanceScheduleController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:guid}/maintenance-schedule")]
    public async Task<IActionResult> GetByToolId(Guid id, CancellationToken cancellationToken)
    {
        var toolExists = await _context.ToolAssets
            .AnyAsync(x => x.Id == id, cancellationToken);

        if (!toolExists)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        var records = await _context.Set<MaintenanceRecord>()
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id && !x.IsDeleted)
            .OrderByDescending(x => x.ScheduledAt)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.MaintenanceNumber,
                Type = x.Type.ToString(),
                Status = x.Status.ToString(),
                x.ScheduledAt,
                x.StartedAt,
                x.FinishedAt,
                x.Provider,
                x.Technician,
                x.Description,
                x.MaintenanceActivities,
                x.ExecutionNotes,
                x.InvoiceNumber,
                x.ResponsibleName,
                x.ResponsiblePosition,
                x.IsToolOperational,
                x.EvidenceDocumentId,
                x.Cost,
                x.Result,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return Ok(records);
    }

    [HttpGet("by-code/{internalCode}/maintenance-schedule")]
    public async Task<IActionResult> GetByToolCode(string internalCode, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await GetByToolId(tool.Id, cancellationToken);
    }


    [HttpPost("by-code/{internalCode}/maintenance-schedule")]
    public async Task<IActionResult> CreateByToolCode(string internalCode, [FromBody] CreateToolMaintenanceScheduleRequest request, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await Create(tool.Id, request, cancellationToken);
    }
    [HttpPost("{id:guid}/maintenance-schedule")]
    public async Task<IActionResult> Create(Guid id, [FromBody] CreateToolMaintenanceScheduleRequest request, CancellationToken cancellationToken)
    {
        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        var user = string.IsNullOrWhiteSpace(request.CreatedBy) ? "yquinto" : request.CreatedBy.Trim();

        var type = ParseEnum(request.Type, ToolMaintenanceType.Preventive);
        var status = ParseEnum(request.Status, ToolMaintenanceStatus.Scheduled);

        var maintenanceNumber = string.IsNullOrWhiteSpace(request.MaintenanceNumber)
            ? $"MTTO-{tool.InternalCode}-{DateTime.UtcNow:yyyyMMddHHmmss}"
            : request.MaintenanceNumber.Trim();

        var record = new MaintenanceRecord
        {
            ToolAssetId = tool.Id,
            MaintenanceNumber = maintenanceNumber,
            Type = type,
            Status = status,
            ScheduledAt = request.ScheduledAt,
            StartedAt = request.StartedAt,
            FinishedAt = request.FinishedAt,
            Provider = Normalize(request.Provider),
            Technician = Normalize(request.Technician),
            Description = Normalize(request.Description),
            MaintenanceActivities = Normalize(request.MaintenanceActivities),
            ExecutionNotes = Normalize(request.ExecutionNotes),
            InvoiceNumber = Normalize(request.InvoiceNumber),
            ResponsibleName = Normalize(request.ResponsibleName),
            ResponsiblePosition = Normalize(request.ResponsiblePosition),
            IsToolOperational = request.IsToolOperational,
            EvidenceDocumentId = request.EvidenceDocumentId,
            Cost = request.Cost,
            Result = Normalize(request.Result),
            CreatedBy = user
        };

        _context.Set<MaintenanceRecord>().Add(record);

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = tool.Id,
            EventType = "MaintenanceScheduleCreated",
            Title = "Registro de mantenimiento creado",
            Description = $"Se creó el registro de mantenimiento {record.MaintenanceNumber}.",
            NewValue = $"{record.MaintenanceNumber} | {record.Type} | {record.Status}",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Registro de mantenimiento creado correctamente.",
            record.Id,
            record.ToolAssetId,
            record.MaintenanceNumber,
            Type = record.Type.ToString(),
            Status = record.Status.ToString()
        });
    }

    [HttpPut("maintenance-schedule/{maintenanceId:guid}")]
    public async Task<IActionResult> Update(Guid maintenanceId, [FromBody] UpdateToolMaintenanceScheduleRequest request, CancellationToken cancellationToken)
    {
        var record = await _context.Set<MaintenanceRecord>()
            .FirstOrDefaultAsync(x => x.Id == maintenanceId, cancellationToken);

        if (record is null)
        {
            return NotFound(new { Message = "No se encontró el registro de mantenimiento." });
        }

        var user = string.IsNullOrWhiteSpace(request.UpdatedBy) ? "yquinto" : request.UpdatedBy.Trim();

        var previousValue = $"{record.MaintenanceNumber} | {record.Type} | {record.Status}";

        record.MaintenanceNumber = string.IsNullOrWhiteSpace(request.MaintenanceNumber)
            ? record.MaintenanceNumber
            : request.MaintenanceNumber.Trim();

        record.Type = ParseEnum(request.Type, record.Type);
        record.Status = ParseEnum(request.Status, record.Status);
        record.ScheduledAt = request.ScheduledAt;
        record.StartedAt = request.StartedAt;
        record.FinishedAt = request.FinishedAt;
        record.Provider = Normalize(request.Provider);
        record.Technician = Normalize(request.Technician);
        record.Description = Normalize(request.Description);
        record.MaintenanceActivities = Normalize(request.MaintenanceActivities);
        record.ExecutionNotes = Normalize(request.ExecutionNotes);
        record.InvoiceNumber = Normalize(request.InvoiceNumber);
        record.ResponsibleName = Normalize(request.ResponsibleName);
        record.ResponsiblePosition = Normalize(request.ResponsiblePosition);
        record.IsToolOperational = request.IsToolOperational;
        record.EvidenceDocumentId = request.EvidenceDocumentId;
        record.Cost = request.Cost;
        record.Result = Normalize(request.Result);
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = user;

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = record.ToolAssetId,
            EventType = "MaintenanceScheduleUpdated",
            Title = "Registro de mantenimiento actualizado",
            Description = $"Se actualizó el registro de mantenimiento {record.MaintenanceNumber}.",
            PreviousValue = previousValue,
            NewValue = $"{record.MaintenanceNumber} | {record.Type} | {record.Status}",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Registro de mantenimiento actualizado correctamente.",
            record.Id,
            record.ToolAssetId,
            record.MaintenanceNumber,
            Type = record.Type.ToString(),
            Status = record.Status.ToString()
        });
    }

    [HttpDelete("maintenance-schedule/{maintenanceId:guid}")]
    public async Task<IActionResult> Delete(Guid maintenanceId, [FromQuery] string? deletedBy, CancellationToken cancellationToken)
    {
        var record = await _context.Set<MaintenanceRecord>()
            .FirstOrDefaultAsync(x => x.Id == maintenanceId, cancellationToken);

        if (record is null)
        {
            return NotFound(new { Message = "No se encontró el registro de mantenimiento." });
        }

        var user = string.IsNullOrWhiteSpace(deletedBy) ? "yquinto" : deletedBy.Trim();

        record.Status = ToolMaintenanceStatus.Cancelled;
        record.IsDeleted = true;
        record.UpdatedAt = DateTime.UtcNow;
        record.UpdatedBy = user;

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = record.ToolAssetId,
            EventType = "MaintenanceScheduleDeleted",
            Title = "Registro de mantenimiento desactivado",
            Description = $"Se desactivó el registro de mantenimiento {record.MaintenanceNumber}.",
            PreviousValue = record.MaintenanceNumber,
            NewValue = "Cancelado / desactivado",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Registro de mantenimiento desactivado correctamente.",
            record.Id,
            record.ToolAssetId,
            record.MaintenanceNumber
        });
    }

    private static TEnum ParseEnum<TEnum>(string? value, TEnum defaultValue)
        where TEnum : struct
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        return Enum.TryParse<TEnum>(value.Trim(), true, out var parsed)
            ? parsed
            : defaultValue;
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

public sealed class CreateToolMaintenanceScheduleRequest
{
    public string? MaintenanceNumber { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public DateTime ScheduledAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? Provider { get; set; }

    public string? Technician { get; set; }

    public string? Description { get; set; }

    public string? MaintenanceActivities { get; set; }

    public string? ExecutionNotes { get; set; }

    public string? InvoiceNumber { get; set; }

    public string? ResponsibleName { get; set; }

    public string? ResponsiblePosition { get; set; }

    public bool? IsToolOperational { get; set; }

    public Guid? EvidenceDocumentId { get; set; }

    public decimal? Cost { get; set; }

    public string? Result { get; set; }

    public string? CreatedBy { get; set; }
}

public sealed class UpdateToolMaintenanceScheduleRequest
{
    public string? MaintenanceNumber { get; set; }

    public string? Type { get; set; }

    public string? Status { get; set; }

    public DateTime ScheduledAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? Provider { get; set; }

    public string? Technician { get; set; }

    public string? Description { get; set; }

    public string? MaintenanceActivities { get; set; }

    public string? ExecutionNotes { get; set; }

    public string? InvoiceNumber { get; set; }

    public string? ResponsibleName { get; set; }

    public string? ResponsiblePosition { get; set; }

    public bool? IsToolOperational { get; set; }

    public Guid? EvidenceDocumentId { get; set; }

    public decimal? Cost { get; set; }

    public string? Result { get; set; }

    public string? UpdatedBy { get; set; }
}

