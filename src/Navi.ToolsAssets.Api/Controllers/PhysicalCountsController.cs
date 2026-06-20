using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.PhysicalCounts;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/physical-counts")]
public class PhysicalCountsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public PhysicalCountsController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPhysicalCounts()
    {
        var counts = await _context.PhysicalCounts
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.CountNumber,
                x.BranchId,
                BranchCode = x.Branch == null ? null : x.Branch.Code,
                BranchName = x.Branch == null ? null : x.Branch.Name,
                Status = x.Status.ToString(),
                StatusLabel = GetPhysicalCountStatusLabel(x.Status),
                x.StartedAt,
                x.FinishedAt,
                x.ResponsibleBy,
                x.Notes,
                TotalItems = x.Items.Count,
                FoundItems = x.Items.Count(i => i.WasFound),
                MissingItems = x.Items.Count(i => !i.WasFound),
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync();

        return Ok(counts);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPhysicalCountById(Guid id)
    {
        var count = await _context.PhysicalCounts
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Items)
                .ThenInclude(x => x.ToolAsset)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.CountNumber,
                x.BranchId,
                BranchCode = x.Branch == null ? null : x.Branch.Code,
                BranchName = x.Branch == null ? null : x.Branch.Name,
                Status = x.Status.ToString(),
                StatusLabel = GetPhysicalCountStatusLabel(x.Status),
                x.StartedAt,
                x.FinishedAt,
                x.ResponsibleBy,
                x.Notes,
                TotalItems = x.Items.Count,
                FoundItems = x.Items.Count(i => i.WasFound),
                MissingItems = x.Items.Count(i => !i.WasFound),
                Items = x.Items
                    .OrderByDescending(i => i.CountedAt)
                    .Select(i => new
                    {
                        i.Id,
                        i.ToolAssetId,
                        ToolInternalCode = i.ToolAsset == null ? null : i.ToolAsset.InternalCode,
                        ToolName = i.ToolAsset == null ? null : i.ToolAsset.Name,
                        i.WasFound,
                        i.ExpectedLocation,
                        i.FoundLocation,
                        i.Observation,
                        i.CountedAt,
                        i.CreatedAt,
                        i.CreatedBy,
                        i.UpdatedAt,
                        i.UpdatedBy
                    }),
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .FirstOrDefaultAsync();

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        return Ok(count);
    }

    [HttpGet("by-branch/{branchCode}")]
    public async Task<IActionResult> GetPhysicalCountsByBranch(string branchCode)
    {
        var code = NormalizeCode(branchCode);

        var counts = await _context.PhysicalCounts
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Items)
            .Where(x => x.Branch != null && x.Branch.Code == code)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.CountNumber,
                x.BranchId,
                BranchCode = x.Branch == null ? null : x.Branch.Code,
                BranchName = x.Branch == null ? null : x.Branch.Name,
                Status = x.Status.ToString(),
                StatusLabel = GetPhysicalCountStatusLabel(x.Status),
                x.StartedAt,
                x.FinishedAt,
                x.ResponsibleBy,
                x.Notes,
                TotalItems = x.Items.Count,
                FoundItems = x.Items.Count(i => i.WasFound),
                MissingItems = x.Items.Count(i => !i.WasFound)
            })
            .ToListAsync();

        return Ok(counts);
    }

    [HttpPost]
    public async Task<IActionResult> CreatePhysicalCount([FromBody] CreatePhysicalCountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BranchCode))
        {
            return BadRequest(new { Message = "La sede es obligatoria." });
        }

        var branchCode = NormalizeCode(request.BranchCode);

        var branch = await _context.Branches
            .FirstOrDefaultAsync(x => x.Code == branchCode);

        if (branch is null)
        {
            return BadRequest(new { Message = $"No existe la sede {branchCode}." });
        }

        var createdBy = string.IsNullOrWhiteSpace(request.ResponsibleBy)
            ? "api"
            : request.ResponsibleBy.Trim();

        var count = new PhysicalCount
        {
            CountNumber = $"TF-{DateTime.UtcNow:yyyyMMddHHmmss}",
            BranchId = branch.Id,
            Status = PhysicalCountStatus.Draft,
            StartedAt = DateTime.UtcNow,
            ResponsibleBy = createdBy,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.PhysicalCounts.Add(count);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPhysicalCountById), new { id = count.Id }, new
        {
            count.Id,
            count.CountNumber,
            count.BranchId,
            BranchCode = branch.Code,
            BranchName = branch.Name,
            Status = count.Status.ToString(),
            StatusLabel = GetPhysicalCountStatusLabel(count.Status),
            count.StartedAt,
            count.ResponsibleBy,
            count.Notes
        });
    }

    [HttpPatch("{id:guid}/start")]
    public async Task<IActionResult> StartPhysicalCount(Guid id, [FromBody] PhysicalCountActionRequest request)
    {
        var count = await _context.PhysicalCounts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        if (count.Status != PhysicalCountStatus.Draft)
        {
            return BadRequest(new { Message = $"Solo se puede iniciar una toma física en borrador. Estado actual: {count.Status}." });
        }

        var changedBy = GetActionUser(request);

        count.Status = PhysicalCountStatus.InProgress;
        count.StartedAt = DateTime.UtcNow;
        count.UpdatedAt = DateTime.UtcNow;
        count.UpdatedBy = changedBy;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            count.Notes = request.Notes.Trim();
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            count.Id,
            count.CountNumber,
            Status = count.Status.ToString(),
            StatusLabel = GetPhysicalCountStatusLabel(count.Status),
            count.StartedAt,
            count.UpdatedBy
        });
    }

    [HttpPost("{id:guid}/items")]
    public async Task<IActionResult> RegisterPhysicalCountItem(Guid id, [FromBody] RegisterPhysicalCountItemRequest request)
    {
        var count = await _context.PhysicalCounts
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        if (count.Status != PhysicalCountStatus.InProgress)
        {
            return BadRequest(new { Message = $"Solo se pueden registrar herramientas en una toma física en progreso. Estado actual: {count.Status}." });
        }

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

        if (tool.BranchId != count.BranchId)
        {
            return BadRequest(new
            {
                Message = "La herramienta no pertenece a la sede de la toma física.",
                ToolInternalCode = tool.InternalCode,
                CountBranchId = count.BranchId,
                ToolBranchId = tool.BranchId
            });
        }

        var changedBy = string.IsNullOrWhiteSpace(request.CountedBy)
            ? "api"
            : request.CountedBy.Trim();

        var existingItem = await _context.PhysicalCountItems
            .FirstOrDefaultAsync(x => x.PhysicalCountId == count.Id && x.ToolAssetId == tool.Id);

        var previousToolStatus = tool.OperationalStatus;

        if (existingItem is null)
        {
            existingItem = new PhysicalCountItem
            {
                PhysicalCountId = count.Id,
                ToolAssetId = tool.Id,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = changedBy
            };

            _context.PhysicalCountItems.Add(existingItem);
        }
        else
        {
            existingItem.UpdatedAt = DateTime.UtcNow;
            existingItem.UpdatedBy = changedBy;
        }

        existingItem.WasFound = request.WasFound;
        existingItem.ExpectedLocation = request.ExpectedLocation;
        existingItem.FoundLocation = request.FoundLocation;
        existingItem.Observation = request.Observation;
        existingItem.CountedAt = DateTime.UtcNow;

        if (!request.WasFound)
        {
            tool.OperationalStatus = ToolOperationalStatus.NotLocated;
            tool.PhysicalStatus = ToolPhysicalStatus.Lost;
        }
        else if (tool.OperationalStatus == ToolOperationalStatus.NotLocated)
        {
            tool.OperationalStatus = ToolOperationalStatus.Available;
            tool.PhysicalStatus = ToolPhysicalStatus.Good;
        }

        tool.UpdatedAt = DateTime.UtcNow;
        tool.UpdatedBy = changedBy;

        AddToolLifeCycleEvent(
            tool.Id,
            request.WasFound ? "PhysicalCountFound" : "PhysicalCountNotFound",
            request.WasFound ? "Herramienta encontrada en toma física" : "Herramienta no localizada en toma física",
            string.IsNullOrWhiteSpace(request.Observation)
                ? $"Registro de toma física {count.CountNumber}."
                : request.Observation.Trim(),
            previousToolStatus.ToString(),
            tool.OperationalStatus.ToString(),
            changedBy);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            existingItem.Id,
            existingItem.PhysicalCountId,
            existingItem.ToolAssetId,
            ToolInternalCode = tool.InternalCode,
            ToolName = tool.Name,
            existingItem.WasFound,
            existingItem.ExpectedLocation,
            existingItem.FoundLocation,
            existingItem.Observation,
            existingItem.CountedAt,
            ToolOperationalStatus = tool.OperationalStatus.ToString(),
            ToolPhysicalStatus = tool.PhysicalStatus.ToString()
        });
    }

    [HttpPatch("{id:guid}/complete")]
    public async Task<IActionResult> CompletePhysicalCount(Guid id, [FromBody] PhysicalCountActionRequest request)
    {
        var count = await _context.PhysicalCounts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        if (count.Status != PhysicalCountStatus.InProgress)
        {
            return BadRequest(new { Message = $"Solo se puede completar una toma física en progreso. Estado actual: {count.Status}." });
        }

        if (count.Items.Count == 0)
        {
            return BadRequest(new { Message = "No se puede completar una toma física sin registros de herramientas." });
        }

        var changedBy = GetActionUser(request);

        count.Status = PhysicalCountStatus.Completed;
        count.FinishedAt = DateTime.UtcNow;
        count.UpdatedAt = DateTime.UtcNow;
        count.UpdatedBy = changedBy;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            count.Notes = request.Notes.Trim();
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            count.Id,
            count.CountNumber,
            Status = count.Status.ToString(),
            StatusLabel = GetPhysicalCountStatusLabel(count.Status),
            count.StartedAt,
            count.FinishedAt,
            TotalItems = count.Items.Count,
            FoundItems = count.Items.Count(x => x.WasFound),
            MissingItems = count.Items.Count(x => !x.WasFound),
            count.UpdatedBy
        });
    }

    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> CancelPhysicalCount(Guid id, [FromBody] PhysicalCountActionRequest request)
    {
        var count = await _context.PhysicalCounts
            .FirstOrDefaultAsync(x => x.Id == id);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        if (count.Status is PhysicalCountStatus.Completed or PhysicalCountStatus.Cancelled)
        {
            return BadRequest(new { Message = $"No se puede cancelar una toma física en estado {count.Status}." });
        }

        var changedBy = GetActionUser(request);

        count.Status = PhysicalCountStatus.Cancelled;
        count.FinishedAt = DateTime.UtcNow;
        count.UpdatedAt = DateTime.UtcNow;
        count.UpdatedBy = changedBy;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            count.Notes = request.Notes.Trim();
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            count.Id,
            count.CountNumber,
            Status = count.Status.ToString(),
            StatusLabel = GetPhysicalCountStatusLabel(count.Status),
            count.FinishedAt,
            count.Notes,
            count.UpdatedBy
        });
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string GetActionUser(PhysicalCountActionRequest request)
    {
        return string.IsNullOrWhiteSpace(request.ActionBy)
            ? "api"
            : request.ActionBy.Trim();
    }

    private static string GetPhysicalCountStatusLabel(PhysicalCountStatus status)
    {
        return status switch
        {
            PhysicalCountStatus.Draft => "Borrador",
            PhysicalCountStatus.InProgress => "En progreso",
            PhysicalCountStatus.Completed => "Completada",
            PhysicalCountStatus.Reconciled => "Conciliada",
            PhysicalCountStatus.Cancelled => "Cancelada",
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

public sealed class CreatePhysicalCountRequest
{
    public string BranchCode { get; set; } = string.Empty;

    public string? ResponsibleBy { get; set; }

    public string? Notes { get; set; }
}

public sealed class RegisterPhysicalCountItemRequest
{
    public Guid? ToolAssetId { get; set; }

    public string? ToolInternalCode { get; set; }

    public bool WasFound { get; set; }

    public string? ExpectedLocation { get; set; }

    public string? FoundLocation { get; set; }

    public string? Observation { get; set; }

    public string? CountedBy { get; set; }
}

public sealed class PhysicalCountActionRequest
{
    public string? ActionBy { get; set; }

    public string? Notes { get; set; }
}
