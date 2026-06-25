using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.PhysicalCounts;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.Organization;
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


    [HttpGet("{id:guid}/participants-board")]
    public async Task<IActionResult> GetParticipantsBoard(Guid id, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var count = await _context.PhysicalCounts
            .AsNoTracking()
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        var participants = await _context.Set<PhysicalCountParticipant>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PhysicalCountId == id)
            .OrderBy(x => x.DisplayName)
            .Select(x => new
            {
                x.Id,
                x.PhysicalCountId,
                x.ResponsiblePersonId,
                x.UserId,
                x.UserName,
                x.DisplayName,
                x.Area,
                x.Position,
                x.BranchId,
                x.ZoneId,
                x.LocationId,
                x.Status,
                StatusLabel = GetParticipantStatusLabel(x.Status),
                x.ExpectedItems,
                x.CountedItems,
                x.PendingItems,
                x.FoundItems,
                x.MissingItems,
                x.DifferentItems,
                x.DamagedItems,
                x.ExtraItems,
                Progress = x.ExpectedItems == 0 ? 0 : Math.Round((decimal)x.CountedItems * 100 / x.ExpectedItems, 2),
                x.StartedAt,
                x.FinishedAt,
                x.LastActivityAt,
                x.Observation,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        var items = await _context.PhysicalCountItems
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Where(x => x.PhysicalCountId == id)
            .OrderByDescending(x => x.CountedAt)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,
                x.WasFound,
                x.ExpectedLocation,
                x.FoundLocation,
                x.Observation,
                x.CountedAt,
                x.CreatedBy,
                x.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        var totalExpected = participants.Sum(x => x.ExpectedItems);
        var totalCounted = participants.Sum(x => x.CountedItems);
        var totalPending = participants.Sum(x => x.PendingItems);

        return Ok(new
        {
            count.Id,
            count.CountNumber,
            count.BranchId,
            BranchCode = count.Branch == null ? null : count.Branch.Code,
            BranchName = count.Branch == null ? null : count.Branch.Name,
            Status = count.Status.ToString(),
            StatusLabel = GetPhysicalCountStatusLabel(count.Status),
            count.StartedAt,
            count.FinishedAt,
            count.ResponsibleBy,
            count.Notes,
            Participants = participants,
            Items = items,
            Summary = new
            {
                TotalParticipants = participants.Count,
                NotStarted = participants.Count(x => x.Status == "NotStarted"),
                InProgress = participants.Count(x => x.Status == "InProgress"),
                Finished = participants.Count(x => x.Status == "Finished" || x.Status == "FinishedWithDifferences"),
                FinishedWithDifferences = participants.Count(x => x.Status == "FinishedWithDifferences"),
                Expired = participants.Count(x => x.Status == "Expired"),
                TotalExpected = totalExpected,
                TotalCounted = totalCounted,
                TotalPending = totalPending,
                FoundItems = participants.Sum(x => x.FoundItems),
                MissingItems = participants.Sum(x => x.MissingItems),
                DifferentItems = participants.Sum(x => x.DifferentItems),
                DamagedItems = participants.Sum(x => x.DamagedItems),
                ExtraItems = participants.Sum(x => x.ExtraItems),
                Progress = totalExpected == 0 ? 0 : Math.Round((decimal)totalCounted * 100 / totalExpected, 2)
            }
        });
    }

    [HttpPost("{id:guid}/generate-participants")]
    public async Task<IActionResult> GenerateParticipants(Guid id, [FromBody] GeneratePhysicalCountParticipantsRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var count = await _context.PhysicalCounts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        if (count.Status is PhysicalCountStatus.Completed or PhysicalCountStatus.Reconciled or PhysicalCountStatus.Cancelled)
        {
            return BadRequest(new { Message = $"No se pueden generar participantes para una toma en estado {count.Status}." });
        }

        var actionBy = string.IsNullOrWhiteSpace(request.ActionBy)
            ? "admin"
            : request.ActionBy.Trim();

        var createdFromThisCountIds = await _context.Set<PhysicalCountReportedItem>()
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.PhysicalCountId == count.Id &&
                x.CreatedToolAssetId.HasValue)
            .Select(x => x.CreatedToolAssetId!.Value)
            .ToListAsync(cancellationToken);

        var query = _context.ToolAssets
            .AsNoTracking()
            .Include(x => x.ResponsiblePerson)
            .Where(x =>
                !x.IsDeleted &&
                x.BranchId == count.BranchId &&
                !createdFromThisCountIds.Contains(x.Id) &&
                (x.Description == null || !x.Description.Contains(count.CountNumber)));

        if (request.ResponsiblePersonIds is not null && request.ResponsiblePersonIds.Any())
        {
            query = query.Where(x => x.ResponsiblePersonId.HasValue && request.ResponsiblePersonIds.Contains(x.ResponsiblePersonId.Value));
        }

        if (request.LocationId.HasValue)
        {
            query = query.Where(x => x.LocationId == request.LocationId.Value);
        }

        var tools = await query
            .Select(x => new
            {
                x.Id,
                x.ResponsiblePersonId,
                ResponsibleName = x.ResponsiblePerson == null ? null : x.ResponsiblePerson.FullName,
                ResponsibleEmployeeCode = x.ResponsiblePerson == null ? null : x.ResponsiblePerson.EmployeeCode,
                ResponsibleArea = x.ResponsiblePerson == null ? null : x.ResponsiblePerson.Area,
                ResponsiblePosition = x.ResponsiblePerson == null ? null : x.ResponsiblePerson.Position
            })
            .ToListAsync(cancellationToken);

        var grouped = tools
            .Where(x => x.ResponsiblePersonId.HasValue || request.IncludeToolsWithoutResponsible)
            .GroupBy(x => x.ResponsiblePersonId)
            .ToList();

        var created = 0;
        var updated = 0;
        var now = DateTime.UtcNow;

        foreach (var group in grouped)
        {
            var first = group.FirstOrDefault();
            if (first is null)
            {
                continue;
            }

            var responsibleId = group.Key;
            var displayName = responsibleId.HasValue
                ? (first.ResponsibleName ?? "Responsable sin nombre")
                : "Sin responsable asignado";

            var userName = responsibleId.HasValue
                ? (!string.IsNullOrWhiteSpace(first.ResponsibleEmployeeCode) ? first.ResponsibleEmployeeCode! : displayName)
                : "SIN-RESPONSABLE";

            var participant = await _context.Set<PhysicalCountParticipant>()
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.PhysicalCountId == count.Id &&
                    x.ResponsiblePersonId == responsibleId,
                    cancellationToken);

            if (participant is null)
            {
                participant = new PhysicalCountParticipant
                {
                    Id = Guid.NewGuid(),
                    PhysicalCountId = count.Id,
                    ResponsiblePersonId = responsibleId,
                    BranchId = count.BranchId,
                    UserName = userName,
                    DisplayName = displayName,
                    Area = first.ResponsibleArea,
                    Position = first.ResponsiblePosition,
                    Status = "NotStarted",
                    CreatedAt = now,
                    CreatedBy = actionBy
                };

                _context.Set<PhysicalCountParticipant>().Add(participant);
                created++;
            }
            else
            {
                participant.UserName = userName;
                participant.DisplayName = displayName;
                participant.Area = first.ResponsibleArea;
                participant.Position = first.ResponsiblePosition;
                participant.UpdatedAt = now;
                participant.UpdatedBy = actionBy;
                updated++;
            }

            participant.ExpectedItems = group.Count();
            participant.CountedItems = await CountParticipantItemsAsync(count.Id, responsibleId, participant.UserName, cancellationToken);
            participant.PendingItems = Math.Max(participant.ExpectedItems - participant.CountedItems, 0);

            RefreshParticipantDerivedStatus(participant);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            count.Id,
            count.CountNumber,
            CreatedParticipants = created,
            UpdatedParticipants = updated,
            TotalParticipants = created + updated,
            TotalExpectedItems = grouped.Sum(x => x.Count()),
            Message = "Participantes generados correctamente para la toma física."
        });
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> ActivatePhysicalCount(Guid id, [FromBody] PhysicalCountActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var count = await _context.PhysicalCounts
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        if (count.Status == PhysicalCountStatus.Completed || count.Status == PhysicalCountStatus.Reconciled || count.Status == PhysicalCountStatus.Cancelled)
        {
            return BadRequest(new { Message = $"No se puede activar una toma física en estado {count.Status}." });
        }

        var participants = await _context.Set<PhysicalCountParticipant>()
            .Where(x => !x.IsDeleted && x.PhysicalCountId == id)
            .ToListAsync(cancellationToken);

        if (!participants.Any())
        {
            return BadRequest(new { Message = "Debe generar participantes antes de activar la toma física." });
        }

        var actionBy = GetActionUser(request);

        count.Status = PhysicalCountStatus.InProgress;
        count.StartedAt = DateTime.UtcNow;
        count.UpdatedAt = DateTime.UtcNow;
        count.UpdatedBy = actionBy;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            count.Notes = request.Notes.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            count.Id,
            count.CountNumber,
            Status = count.Status.ToString(),
            StatusLabel = GetPhysicalCountStatusLabel(count.Status),
            count.StartedAt,
            Participants = participants.Count
        });
    }

    [HttpPatch("{id:guid}/close-with-participants")]
    public async Task<IActionResult> ClosePhysicalCountWithParticipants(Guid id, [FromBody] PhysicalCountActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var count = await _context.PhysicalCounts
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        if (count.Status != PhysicalCountStatus.InProgress)
        {
            return BadRequest(new { Message = $"Solo se puede cerrar una toma física activa/en progreso. Estado actual: {count.Status}." });
        }

        var participants = await _context.Set<PhysicalCountParticipant>()
            .Where(x => !x.IsDeleted && x.PhysicalCountId == id)
            .ToListAsync(cancellationToken);

        if (!participants.Any())
        {
            return BadRequest(new { Message = "La toma física no tiene participantes generados." });
        }

        var pendingParticipants = participants.Where(x => x.PendingItems > 0 && x.Status != "Finished" && x.Status != "FinishedWithDifferences").ToList();

        if (pendingParticipants.Any() && !request.ForceClose)
        {
            return BadRequest(new
            {
                Message = "Aún hay participantes con herramientas pendientes. Use cierre forzado si desea cerrar de todas formas.",
                PendingParticipants = pendingParticipants.Select(x => new
                {
                    x.Id,
                    x.DisplayName,
                    x.ExpectedItems,
                    x.CountedItems,
                    x.PendingItems,
                    x.Status
                })
            });
        }

        var actionBy = GetActionUser(request);
        var now = DateTime.UtcNow;

        foreach (var participant in participants.Where(x => x.PendingItems > 0 && x.Status != "Finished" && x.Status != "FinishedWithDifferences"))
        {
            participant.Status = "Expired";
            participant.UpdatedAt = now;
            participant.UpdatedBy = actionBy;
        }

        count.Status = PhysicalCountStatus.Completed;
        count.FinishedAt = now;
        count.UpdatedAt = now;
        count.UpdatedBy = actionBy;

        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            count.Notes = request.Notes.Trim();
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            count.Id,
            count.CountNumber,
            Status = count.Status.ToString(),
            StatusLabel = GetPhysicalCountStatusLabel(count.Status),
            count.FinishedAt,
            Participants = participants.Count,
            FinishedParticipants = participants.Count(x => x.Status == "Finished" || x.Status == "FinishedWithDifferences"),
            PendingParticipants = participants.Count(x => x.PendingItems > 0),
            ExpiredParticipants = participants.Count(x => x.Status == "Expired")
        });
    }

    [HttpPatch("participants/{participantId:guid}/start")]
    public async Task<IActionResult> StartParticipant(Guid participantId, [FromBody] ParticipantActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var participant = await _context.Set<PhysicalCountParticipant>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == participantId, cancellationToken);

        if (participant is null)
        {
            return NotFound(new { Message = $"No se encontró el participante {participantId}." });
        }

        var now = DateTime.UtcNow;
        var actionBy = string.IsNullOrWhiteSpace(request.ActionBy) ? participant.UserName : request.ActionBy.Trim();

        participant.Status = "InProgress";
        participant.StartedAt ??= now;
        participant.LastActivityAt = now;
        participant.UpdatedAt = now;
        participant.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            participant.Id,
            participant.DisplayName,
            participant.Status,
            StatusLabel = GetParticipantStatusLabel(participant.Status),
            participant.StartedAt,
            participant.LastActivityAt
        });
    }


    [HttpPost("participants/{participantId:guid}/report-assigned-tool")]
    public async Task<IActionResult> ReportAssignedToolByParticipant(
        Guid participantId,
        [FromBody] ParticipantAssignedToolReportRequest request,
        CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var participant = await _context.Set<PhysicalCountParticipant>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == participantId, cancellationToken);

        if (participant is null)
        {
            return NotFound(new { Message = $"No se encontró el participante {participantId}." });
        }

        var count = await _context.PhysicalCounts
            .FirstOrDefaultAsync(x => x.Id == participant.PhysicalCountId, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = "No se encontró la toma física asociada." });
        }

        if (count.Status != PhysicalCountStatus.InProgress)
        {
            return BadRequest(new { Message = $"La toma física debe estar activa/en progreso. Estado actual: {count.Status}." });
        }

        var tool = await _context.ToolAssets
            .Include(x => x.Branch)
            .Include(x => x.Location)
            .Include(x => x.ResponsiblePerson)
            .Include(x => x.ToolType)
            .Include(x => x.ToolCategory)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ToolAssetId, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = $"No se encontró el activo/herramienta {request.ToolAssetId}." });
        }

        var now = DateTime.UtcNow;
        var actionBy = string.IsNullOrWhiteSpace(request.ActionBy)
            ? participant.DisplayName
            : request.ActionBy.Trim();

        var observation = string.IsNullOrWhiteSpace(request.Observation)
            ? null
            : request.Observation.Trim();

        var wasFound = request.ReportAction.Equals("Confirmed", StringComparison.OrdinalIgnoreCase);

        var countItem = await _context.PhysicalCountItems
            .FirstOrDefaultAsync(x =>
                x.PhysicalCountId == participant.PhysicalCountId &&
                x.ToolAssetId == tool.Id,
                cancellationToken);

        if (countItem is null)
        {
            countItem = new PhysicalCountItem
            {
                PhysicalCountId = participant.PhysicalCountId,
                ToolAssetId = tool.Id,
                CreatedAt = now,
                CreatedBy = actionBy
            };

            _context.PhysicalCountItems.Add(countItem);
        }
        else
        {
            countItem.UpdatedAt = now;
            countItem.UpdatedBy = actionBy;
        }

        countItem.WasFound = wasFound;
        countItem.ExpectedLocation = tool.Location?.Name;
        countItem.FoundLocation = string.IsNullOrWhiteSpace(request.FoundLocation)
            ? tool.Location?.Name
            : request.FoundLocation.Trim();
        countItem.Observation = observation;
        countItem.CountedAt = now;

        var existingReport = await _context.Set<PhysicalCountReportedItem>()
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                x.PhysicalCountId == participant.PhysicalCountId &&
                x.PhysicalCountParticipantId == participant.Id &&
                x.MatchedToolAssetId == tool.Id,
                cancellationToken);

        if (existingReport is null)
        {
            existingReport = new PhysicalCountReportedItem
            {
                PhysicalCountId = participant.PhysicalCountId,
                PhysicalCountParticipantId = participant.Id,
                MatchedToolAssetId = tool.Id,
                ToolAssetId = tool.Id,
                ReportedAt = now,
                ReportedBy = actionBy,
                CreatedAt = now,
                CreatedBy = actionBy
            };

            _context.Set<PhysicalCountReportedItem>().Add(existingReport);
        }
        else
        {
            existingReport.UpdatedAt = now;
            existingReport.UpdatedBy = actionBy;
        }

        existingReport.ReportedCode = tool.InternalCode;
        existingReport.ReportedName = tool.Name;
        existingReport.SerialNumber = tool.SerialNumber;
        existingReport.Brand = tool.Brand;
        existingReport.Model = tool.Model;
        existingReport.AssetTypeId = tool.ToolTypeId;
        existingReport.AssetTypeName = tool.ToolType?.Name;
        existingReport.CategoryId = tool.ToolCategoryId;
        existingReport.CategoryName = tool.ToolCategory?.Name;
        existingReport.BranchId = tool.BranchId;
        existingReport.BranchCode = tool.Branch?.Code;
        existingReport.LocationId = tool.LocationId;
        existingReport.FoundLocation = countItem.FoundLocation;
        existingReport.ResponsiblePersonId = participant.ResponsiblePersonId ?? tool.ResponsiblePersonId;
        existingReport.ResponsibleName = participant.DisplayName;
        existingReport.PhysicalStatus = string.IsNullOrWhiteSpace(request.PhysicalStatus)
            ? tool.PhysicalStatus.ToString()
            : request.PhysicalStatus.Trim();
        existingReport.OperationalStatus = tool.OperationalStatus.ToString();
        existingReport.Observation = observation;
        existingReport.RequiresUserClarification = false;
        existingReport.MinimumDataCompleted = true;
        existingReport.MissingRequiredData = null;

        if (wasFound)
        {
            existingReport.ReportType = "Found";
            existingReport.ReconciliationStatus = "Reconciled";
            existingReport.ReconciliationObservation = "Usuario confirmó que tiene físicamente la herramienta asignada y validó su información.";
            existingReport.ReconciledAt = now;
            existingReport.ReconciledBy = actionBy;
            existingReport.SuggestedAction = "Validación completa";

            tool.PhysicalStatus = ToolPhysicalStatus.Good;
        }
        else
        {
            existingReport.ReportType = "NotFound";
            existingReport.ReconciliationStatus = "MatchedInSystem";
            existingReport.ReconciliationObservation = "Usuario indicó que esta herramienta aparece asignada a él, pero no la tiene físicamente. Requiere conciliación.";
            existingReport.ReconciledAt = null;
            existingReport.ReconciledBy = null;
            existingReport.SuggestedAction = "Revisar trazabilidad, reasignar, enviar a bodega o dar de baja según análisis.";
        }

        participant.CountedItems = await CountParticipantItemsAsync(participant.PhysicalCountId, participant.ResponsiblePersonId, participant.UserName, cancellationToken);
        participant.PendingItems = Math.Max(participant.ExpectedItems - participant.CountedItems, 0);
        participant.LastActivityAt = now;
        RefreshParticipantDerivedStatus(participant);
        participant.UpdatedAt = now;
        participant.UpdatedBy = actionBy;

        _context.ToolLifeCycleEvents.Add(new ToolLifeCycleEvent
        {
            ToolAssetId = tool.Id,
            EventType = wasFound ? "PhysicalCountAssignedToolConfirmed" : "PhysicalCountAssignedToolRejected",
            Title = wasFound ? "Herramienta asignada confirmada en toma física" : "Herramienta asignada rechazada en toma física",
            Description = observation ?? (wasFound
                ? "El usuario confirmó que tiene la herramienta físicamente."
                : "El usuario indicó que no tiene la herramienta físicamente."),
            PreviousValue = tool.OperationalStatus.ToString(),
            NewValue = wasFound ? "Confirmada por usuario" : "Pendiente conciliación",
            CreatedAt = now,
            CreatedBy = actionBy
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = wasFound
                ? "Herramienta reportada correctamente como completa."
                : "Herramienta rechazada y enviada a pendiente de conciliación.",
            participant.Id,
            participant.DisplayName,
            ToolAssetId = tool.Id,
            ToolInternalCode = tool.InternalCode,
            ToolName = tool.Name,
            WasFound = wasFound,
            ReportedItemId = existingReport.Id,
            existingReport.ReportType,
            existingReport.ReconciliationStatus,
            existingReport.ReconciliationObservation,
            participant.ExpectedItems,
            participant.CountedItems,
            participant.PendingItems
        });
    }
    [HttpPatch("participants/{participantId:guid}/finish")]
    public async Task<IActionResult> FinishParticipant(Guid participantId, [FromBody] ParticipantActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var participant = await _context.Set<PhysicalCountParticipant>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == participantId, cancellationToken);

        if (participant is null)
        {
            return NotFound(new { Message = $"No se encontró el participante {participantId}." });
        }

        var now = DateTime.UtcNow;
        var actionBy = string.IsNullOrWhiteSpace(request.ActionBy) ? participant.UserName : request.ActionBy.Trim();

        participant.CountedItems = await CountParticipantItemsAsync(participant.PhysicalCountId, participant.ResponsiblePersonId, participant.UserName, cancellationToken);
        participant.PendingItems = Math.Max(participant.ExpectedItems - participant.CountedItems, 0);

        participant.Status = participant.PendingItems == 0
            ? (participant.DifferentItems > 0 || participant.MissingItems > 0 || participant.DamagedItems > 0 || participant.ExtraItems > 0 ? "FinishedWithDifferences" : "Finished")
            : "InProgress";

        participant.FinishedAt = participant.PendingItems == 0 ? now : null;
        participant.LastActivityAt = now;
        participant.Observation = request.Observation;
        participant.UpdatedAt = now;
        participant.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            participant.Id,
            participant.DisplayName,
            participant.Status,
            StatusLabel = GetParticipantStatusLabel(participant.Status),
            participant.ExpectedItems,
            participant.CountedItems,
            participant.PendingItems,
            participant.FinishedAt
        });
    }

    [HttpPost("participants/{participantId:guid}/count-tool")]
    public async Task<IActionResult> CountToolByParticipant(Guid participantId, [FromBody] ParticipantCountToolRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var participant = await _context.Set<PhysicalCountParticipant>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == participantId, cancellationToken);

        if (participant is null)
        {
            return NotFound(new { Message = $"No se encontró el participante {participantId}." });
        }

        var count = await _context.PhysicalCounts
            .FirstOrDefaultAsync(x => x.Id == participant.PhysicalCountId, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = "No se encontró la toma física asociada." });
        }

        if (count.Status != PhysicalCountStatus.InProgress)
        {
            return BadRequest(new { Message = $"La toma física debe estar activa/en progreso. Estado actual: {count.Status}." });
        }

        var tool = request.ToolAssetId.HasValue
            ? await _context.ToolAssets.FirstOrDefaultAsync(x => x.Id == request.ToolAssetId.Value, cancellationToken)
            : await _context.ToolAssets.FirstOrDefaultAsync(x => x.InternalCode == NormalizeCode(request.ToolInternalCode ?? string.Empty), cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta/activo a validar." });
        }

        var now = DateTime.UtcNow;
        var actionBy = string.IsNullOrWhiteSpace(request.CountedBy) ? participant.UserName : request.CountedBy.Trim();

        var existingItem = await _context.PhysicalCountItems
            .FirstOrDefaultAsync(x => x.PhysicalCountId == count.Id && x.ToolAssetId == tool.Id, cancellationToken);

        if (existingItem is null)
        {
            existingItem = new PhysicalCountItem
            {
                Id = Guid.NewGuid(),
                PhysicalCountId = count.Id,
                ToolAssetId = tool.Id,
                CreatedAt = now,
                CreatedBy = actionBy
            };

            _context.PhysicalCountItems.Add(existingItem);
        }
        else
        {
            existingItem.UpdatedAt = now;
            existingItem.UpdatedBy = actionBy;
        }

        var expectedLocation = request.ExpectedLocation;
        if (string.IsNullOrWhiteSpace(expectedLocation))
        {
            expectedLocation = tool.LocationId.HasValue ? tool.LocationId.Value.ToString() : null;
        }

        existingItem.WasFound = request.WasFound;
        existingItem.ExpectedLocation = expectedLocation;
        existingItem.FoundLocation = request.FoundLocation;
        existingItem.Observation = request.Observation;
        existingItem.CountedAt = now;

        participant.StartedAt ??= now;
        participant.LastActivityAt = now;
        participant.Status = "InProgress";
        participant.CountedItems = await CountParticipantItemsAsync(participant.PhysicalCountId, participant.ResponsiblePersonId, participant.UserName, cancellationToken);
        participant.CountedItems = Math.Max(participant.CountedItems, 0);

        if (existingItem.CreatedAt == now)
        {
            participant.CountedItems++;
        }

        participant.PendingItems = Math.Max(participant.ExpectedItems - participant.CountedItems, 0);

        if (request.WasFound)
        {
            participant.FoundItems++;
        }
        else
        {
            participant.MissingItems++;
        }

        if (!string.IsNullOrWhiteSpace(request.Result) && request.Result.Contains("Diferente", StringComparison.OrdinalIgnoreCase))
        {
            participant.DifferentItems++;
        }

        if (!string.IsNullOrWhiteSpace(request.PhysicalStatus) &&
            (request.PhysicalStatus.Contains("Malo", StringComparison.OrdinalIgnoreCase) ||
             request.PhysicalStatus.Contains("Dañado", StringComparison.OrdinalIgnoreCase)))
        {
            participant.DamagedItems++;
        }

        RefreshParticipantDerivedStatus(participant);

        participant.UpdatedAt = now;
        participant.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            participant.Id,
            participant.DisplayName,
            participant.Status,
            StatusLabel = GetParticipantStatusLabel(participant.Status),
            participant.ExpectedItems,
            participant.CountedItems,
            participant.PendingItems,
            existingItem.ToolAssetId,
            ToolInternalCode = tool.InternalCode,
            ToolName = tool.Name,
            existingItem.WasFound,
            existingItem.FoundLocation,
            existingItem.Observation,
            existingItem.CountedAt
        });
    }

    [HttpGet("participants/{participantId:guid}/pending-tools")]
    public async Task<IActionResult> GetParticipantPendingTools(Guid participantId, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var participant = await _context.Set<PhysicalCountParticipant>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == participantId, cancellationToken);

        if (participant is null)
        {
            return NotFound(new { Message = $"No se encontró el participante {participantId}." });
        }

        var alreadyCounted = await _context.PhysicalCountItems
            .AsNoTracking()
            .Where(x => x.PhysicalCountId == participant.PhysicalCountId)
            .Select(x => x.ToolAssetId)
            .ToListAsync(cancellationToken);
        var currentCount = await _context.PhysicalCounts
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == participant.PhysicalCountId, cancellationToken);

        var createdFromThisCountIds = await _context.Set<PhysicalCountReportedItem>()
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.PhysicalCountId == participant.PhysicalCountId &&
                x.CreatedToolAssetId.HasValue)
            .Select(x => x.CreatedToolAssetId!.Value)
            .ToListAsync(cancellationToken);

        var query = _context.ToolAssets
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Location)
            .Include(x => x.ResponsiblePerson)
            .Where(x =>
                !x.IsDeleted &&
                x.BranchId == participant.BranchId &&
                !createdFromThisCountIds.Contains(x.Id) &&
                (currentCount == null || x.Description == null || !x.Description.Contains(currentCount.CountNumber)));

        if (participant.ResponsiblePersonId.HasValue)
        {
            query = query.Where(x => x.ResponsiblePersonId == participant.ResponsiblePersonId.Value);
        }
        else
        {
            query = query.Where(x => !x.ResponsiblePersonId.HasValue);
        }

        var tools = await query
            .Where(x => !alreadyCounted.Contains(x.Id))
            .OrderBy(x => x.InternalCode)
            .Select(x => new
            {
                x.Id,
                x.InternalCode,
                x.Name,
                BranchCode = x.Branch == null ? null : x.Branch.Code,
                LocationName = x.Location == null ? null : x.Location.Name,
                ResponsibleName = x.ResponsiblePerson == null ? null : x.ResponsiblePerson.FullName,
                OperationalStatus = x.OperationalStatus.ToString(),
                PhysicalStatus = x.PhysicalStatus.ToString()
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            participant.Id,
            participant.DisplayName,
            participant.ExpectedItems,
            participant.CountedItems,
            PendingItems = tools.Count,
            Tools = tools
        });
    }

    private async Task<int> CountParticipantItemsAsync(Guid physicalCountId, Guid? responsiblePersonId, string userName, CancellationToken cancellationToken)
    {
        var query = _context.PhysicalCountItems
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Where(x => x.PhysicalCountId == physicalCountId);

        if (responsiblePersonId.HasValue)
        {
            query = query.Where(x => x.ToolAsset != null && x.ToolAsset.ResponsiblePersonId == responsiblePersonId.Value);
        }
        else
        {
            query = query.Where(x => x.ToolAsset != null && !x.ToolAsset.ResponsiblePersonId.HasValue);
        }

        return await query.CountAsync(cancellationToken);
    }

    private static void RefreshParticipantDerivedStatus(PhysicalCountParticipant participant)
    {
        participant.PendingItems = Math.Max(participant.ExpectedItems - participant.CountedItems, 0);

        if (participant.PendingItems <= 0 && participant.CountedItems > 0)
        {
            participant.Status = participant.DifferentItems > 0 || participant.MissingItems > 0 || participant.DamagedItems > 0 || participant.ExtraItems > 0
                ? "FinishedWithDifferences"
                : "Finished";

            participant.FinishedAt ??= DateTime.UtcNow;
        }
        else if (participant.CountedItems > 0)
        {
            participant.Status = "InProgress";
        }
        else if (string.IsNullOrWhiteSpace(participant.Status))
        {
            participant.Status = "NotStarted";
        }
    }

    private static string GetParticipantStatusLabel(string? status)
    {
        return status switch
        {
            "NotStarted" => "No iniciada",
            "InProgress" => "En proceso",
            "Finished" => "Finalizada",
            "FinishedWithDifferences" => "Finalizada con diferencias",
            "Expired" => "Vencida",
            "Canceled" => "Cancelada",
            _ => string.IsNullOrWhiteSpace(status) ? "No iniciada" : status
        };
    }

    private async Task EnsurePhysicalCountParticipantSchemaAsync(CancellationToken cancellationToken)
    {
        var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PhysicalCounts')
BEGIN
    EXEC('CREATE SCHEMA PhysicalCounts');
END;

IF OBJECT_ID(N'[PhysicalCounts].[PhysicalCountParticipants]', N'U') IS NULL
BEGIN
    CREATE TABLE [PhysicalCounts].[PhysicalCountParticipants](
        [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PhysicalCountParticipants] PRIMARY KEY,
        [PhysicalCountId] uniqueidentifier NOT NULL,
        [ResponsiblePersonId] uniqueidentifier NULL,
        [BranchId] uniqueidentifier NULL,
        [ZoneId] uniqueidentifier NULL,
        [LocationId] uniqueidentifier NULL,
        [UserId] nvarchar(150) NULL,
        [UserName] nvarchar(180) NOT NULL,
        [DisplayName] nvarchar(250) NOT NULL,
        [Area] nvarchar(150) NULL,
        [Position] nvarchar(150) NULL,
        [Status] nvarchar(80) NOT NULL,
        [ExpectedItems] int NOT NULL CONSTRAINT [DF_PhysicalCountParticipants_ExpectedItems] DEFAULT 0,
        [CountedItems] int NOT NULL CONSTRAINT [DF_PhysicalCountParticipants_CountedItems] DEFAULT 0,
        [PendingItems] int NOT NULL CONSTRAINT [DF_PhysicalCountParticipants_PendingItems] DEFAULT 0,
        [FoundItems] int NOT NULL CONSTRAINT [DF_PhysicalCountParticipants_FoundItems] DEFAULT 0,
        [MissingItems] int NOT NULL CONSTRAINT [DF_PhysicalCountParticipants_MissingItems] DEFAULT 0,
        [DifferentItems] int NOT NULL CONSTRAINT [DF_PhysicalCountParticipants_DifferentItems] DEFAULT 0,
        [DamagedItems] int NOT NULL CONSTRAINT [DF_PhysicalCountParticipants_DamagedItems] DEFAULT 0,
        [ExtraItems] int NOT NULL CONSTRAINT [DF_PhysicalCountParticipants_ExtraItems] DEFAULT 0,
        [StartedAt] datetime2 NULL,
        [FinishedAt] datetime2 NULL,
        [LastActivityAt] datetime2 NULL,
        [Observation] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(150) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(150) NULL,
        [IsDeleted] bit NOT NULL CONSTRAINT [DF_PhysicalCountParticipants_IsDeleted] DEFAULT 0
    );

    ALTER TABLE [PhysicalCounts].[PhysicalCountParticipants]
    ADD CONSTRAINT [FK_PhysicalCountParticipants_PhysicalCounts]
    FOREIGN KEY ([PhysicalCountId]) REFERENCES [PhysicalCounts].[PhysicalCounts]([Id]);

    CREATE INDEX [IX_PhysicalCountParticipants_PhysicalCountId]
    ON [PhysicalCounts].[PhysicalCountParticipants]([PhysicalCountId]);

    CREATE INDEX [IX_PhysicalCountParticipants_ResponsiblePersonId]
    ON [PhysicalCounts].[PhysicalCountParticipants]([ResponsiblePersonId]);

    CREATE UNIQUE INDEX [UX_PhysicalCountParticipants_Count_Responsible]
    ON [PhysicalCounts].[PhysicalCountParticipants]([PhysicalCountId], [ResponsiblePersonId])
    WHERE [IsDeleted] = 0 AND [ResponsiblePersonId] IS NOT NULL;
END;
";
        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public sealed class GeneratePhysicalCountParticipantsRequest
    {
        public string ScopeMode { get; set; } = "Branch";
        public Guid? LocationId { get; set; }
        public List<Guid>? ResponsiblePersonIds { get; set; }
        public bool IncludeToolsWithoutResponsible { get; set; } = true;
        public string? ActionBy { get; set; }
    }

    public sealed class ParticipantActionRequest
    {
        public string? ActionBy { get; set; }
        public string? Observation { get; set; }
    }

    public sealed class ParticipantCountToolRequest
    {
        public Guid? ToolAssetId { get; set; }
        public string? ToolInternalCode { get; set; }
        public bool WasFound { get; set; } = true;
        public string? ExpectedLocation { get; set; }
        public string? FoundLocation { get; set; }
        public string? PhysicalStatus { get; set; }
        public string? Result { get; set; }
        public string? Observation { get; set; }
        public string? CountedBy { get; set; }
    }

    [HttpPost("participants/{participantId:guid}/report-extra-tool")]
    public async Task<IActionResult> ReportExtraTool(Guid participantId, [FromBody] ReportExtraToolRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountExtraItemsSchemaAsync(cancellationToken);

        var participant = await _context.Set<PhysicalCountParticipant>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == participantId, cancellationToken);

        if (participant is null)
        {
            return NotFound(new { Message = $"No se encontró el participante {participantId}." });
        }

        var count = await _context.PhysicalCounts
            .FirstOrDefaultAsync(x => x.Id == participant.PhysicalCountId, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = "No se encontró la toma física asociada." });
        }

        if (count.Status != PhysicalCountStatus.InProgress)
        {
            return BadRequest(new { Message = $"La toma física debe estar activa/en progreso. Estado actual: {count.Status}." });
        }

        if (string.IsNullOrWhiteSpace(request.ReportedName) && string.IsNullOrWhiteSpace(request.ReportedCode))
        {
            return BadRequest(new { Message = "Debe indicar al menos el nombre o código de la herramienta reportada." });
        }

        var now = DateTime.UtcNow;
        var reportedBy = string.IsNullOrWhiteSpace(request.ReportedBy)
            ? participant.UserName
            : request.ReportedBy.Trim();

        var existingTool = await TryFindReportedToolAsync(request.ReportedCode, request.ReportedSerial, cancellationToken);

        var extra = new PhysicalCountExtraItem
        {
            Id = Guid.NewGuid(),
            PhysicalCountId = count.Id,
            PhysicalCountParticipantId = participant.Id,
            MatchedToolAssetId = existingTool?.Id,
            ReportedCode = NormalizeOptional(request.ReportedCode),
            ReportedName = NormalizeOptional(request.ReportedName) ?? existingTool?.Name ?? "Herramienta no listada",
            ReportedSerial = NormalizeOptional(request.ReportedSerial),
            ReportedBrand = NormalizeOptional(request.ReportedBrand),
            ReportedModel = NormalizeOptional(request.ReportedModel),
            FoundLocation = NormalizeOptional(request.FoundLocation),
            PhysicalStatus = NormalizeOptional(request.PhysicalStatus),
            Observation = NormalizeOptional(request.Observation),
            ReportedBy = reportedBy,
            ReportedAt = now,
            ReconciliationStatus = existingTool is null ? "NotFoundInSystem" : "MatchedInSystem",
            ReconciliationObservation = existingTool is null
                ? "Herramienta reportada por usuario, no encontrada automáticamente en el sistema."
                : $"Coincidencia automática con activo {existingTool.InternalCode}.",
            RequiresUserClarification = request.RequiresUserClarification,
            SuggestedAction = existingTool is null ? "Validar si debe crearse en inventario" : "Validar si cambia ubicación o responsable",
            CreatedAt = now,
            CreatedBy = reportedBy
        };

        _context.Set<PhysicalCountExtraItem>().Add(extra);

        participant.ExtraItems++;
        participant.DifferentItems++;
        participant.LastActivityAt = now;
        participant.UpdatedAt = now;
        participant.UpdatedBy = reportedBy;

        if (participant.Status == "NotStarted")
        {
            participant.Status = "InProgress";
            participant.StartedAt = now;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            extra.Id,
            extra.PhysicalCountId,
            extra.PhysicalCountParticipantId,
            extra.ReportedCode,
            extra.ReportedName,
            extra.ReportedSerial,
            extra.FoundLocation,
            extra.PhysicalStatus,
            extra.ReportedBy,
            extra.ReportedAt,
            extra.ReconciliationStatus,
            ReconciliationStatusLabel = GetExtraReconciliationStatusLabel(extra.ReconciliationStatus),
            extra.RequiresUserClarification,
            extra.MatchedToolAssetId,
            MatchedToolInternalCode = existingTool?.InternalCode,
            MatchedToolName = existingTool?.Name,
            Message = "Herramienta no listada reportada correctamente para conciliación."
        });
    }

    [HttpGet("{id:guid}/extra-items")]
    public async Task<IActionResult> GetExtraItems(Guid id, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountExtraItemsSchemaAsync(cancellationToken);

        var exists = await _context.PhysicalCounts
            .AsNoTracking()
            .AnyAsync(x => x.Id == id, cancellationToken);

        if (!exists)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        var extras = await _context.Set<PhysicalCountExtraItem>()
            .AsNoTracking()
            .Include(x => x.Participant)
            .Include(x => x.MatchedToolAsset)
            .Where(x => !x.IsDeleted && x.PhysicalCountId == id)
            .OrderByDescending(x => x.ReportedAt)
            .Select(x => new
            {
                x.Id,
                x.PhysicalCountId,
                x.PhysicalCountParticipantId,
                ParticipantName = x.Participant == null ? null : x.Participant.DisplayName,
                ParticipantUserName = x.Participant == null ? null : x.Participant.UserName,
                x.MatchedToolAssetId,
                MatchedToolInternalCode = x.MatchedToolAsset == null ? null : x.MatchedToolAsset.InternalCode,
                MatchedToolName = x.MatchedToolAsset == null ? null : x.MatchedToolAsset.Name,
                x.ReportedCode,
                x.ReportedName,
                x.ReportedSerial,
                x.ReportedBrand,
                x.ReportedModel,
                x.FoundLocation,
                x.PhysicalStatus,
                x.Observation,
                x.ReportedBy,
                x.ReportedAt,
                x.ReconciliationStatus,
                ReconciliationStatusLabel = GetExtraReconciliationStatusLabel(x.ReconciliationStatus),
                x.ReconciliationObservation,
                x.RequiresUserClarification,
                x.ClarificationRequestedAt,
                x.ClarificationRequestedBy,
                x.ReconciledAt,
                x.ReconciledBy,
                x.SuggestedAction,
                x.ApprovedForCreation,
                x.ApprovedForCreationAt,
                x.ApprovedForCreationBy,
                x.Rejected,
                x.RejectedAt,
                x.RejectedBy,
                x.RejectionReason
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            PhysicalCountId = id,
            Total = extras.Count,
            PendingReview = extras.Count(x => x.ReconciliationStatus == "PendingReview" || x.ReconciliationStatus == "NotFoundInSystem"),
            MatchedInSystem = extras.Count(x => x.ReconciliationStatus == "MatchedInSystem"),
            RequiresUserClarification = extras.Count(x => x.RequiresUserClarification || x.ReconciliationStatus == "RequiresUserClarification"),
            ApprovedForCreation = extras.Count(x => x.ReconciliationStatus == "ApprovedForCreation"),
            Reconciled = extras.Count(x => x.ReconciliationStatus == "Reconciled"),
            Rejected = extras.Count(x => x.ReconciliationStatus == "Rejected"),
            Items = extras
        });
    }

    [HttpPatch("extra-items/{extraItemId:guid}/request-clarification")]
    public async Task<IActionResult> RequestExtraItemClarification(Guid extraItemId, [FromBody] ExtraItemReconciliationRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountExtraItemsSchemaAsync(cancellationToken);

        var extra = await _context.Set<PhysicalCountExtraItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == extraItemId, cancellationToken);

        if (extra is null)
        {
            return NotFound(new { Message = $"No se encontró la herramienta reportada {extraItemId}." });
        }

        var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        extra.RequiresUserClarification = true;
        extra.ClarificationRequestedAt = now;
        extra.ClarificationRequestedBy = actionBy;
        extra.ReconciliationStatus = "RequiresUserClarification";
        extra.ReconciliationObservation = NormalizeOptional(request.Observation)
            ?? "Se requiere contactar al usuario para ampliar información de la herramienta reportada.";
        extra.UpdatedAt = now;
        extra.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            extra.Id,
            extra.ReconciliationStatus,
            ReconciliationStatusLabel = GetExtraReconciliationStatusLabel(extra.ReconciliationStatus),
            extra.RequiresUserClarification,
            extra.ClarificationRequestedAt,
            extra.ClarificationRequestedBy,
            extra.ReconciliationObservation
        });
    }

    [HttpPatch("extra-items/{extraItemId:guid}/match-existing")]
    public async Task<IActionResult> MatchExtraItemWithExistingTool(Guid extraItemId, [FromBody] MatchExtraToolRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountExtraItemsSchemaAsync(cancellationToken);

        if (!request.MatchedToolAssetId.HasValue)
        {
            return BadRequest(new { Message = "Debe indicar el activo/herramienta del sistema con el que se conciliará el reporte." });
        }

        var extra = await _context.Set<PhysicalCountExtraItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == extraItemId, cancellationToken);

        if (extra is null)
        {
            return NotFound(new { Message = $"No se encontró la herramienta reportada {extraItemId}." });
        }

        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == request.MatchedToolAssetId.Value, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró el activo/herramienta seleccionado para conciliación." });
        }

        var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        extra.MatchedToolAssetId = tool.Id;
        extra.ReconciliationStatus = request.MarkAsReconciled ? "Reconciled" : "MatchedInSystem";
        extra.ReconciliationObservation = NormalizeOptional(request.Observation)
            ?? $"Conciliado con herramienta existente {tool.InternalCode}.";
        extra.ReconciledAt = request.MarkAsReconciled ? now : null;
        extra.ReconciledBy = request.MarkAsReconciled ? actionBy : null;
        extra.UpdatedAt = now;
        extra.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            extra.Id,
            extra.MatchedToolAssetId,
            MatchedToolInternalCode = tool.InternalCode,
            MatchedToolName = tool.Name,
            extra.ReconciliationStatus,
            ReconciliationStatusLabel = GetExtraReconciliationStatusLabel(extra.ReconciliationStatus),
            extra.ReconciliationObservation,
            extra.ReconciledAt,
            extra.ReconciledBy
        });
    }

    [HttpPatch("extra-items/{extraItemId:guid}/approve-creation")]
    public async Task<IActionResult> ApproveExtraItemForCreation(Guid extraItemId, [FromBody] ExtraItemReconciliationRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountExtraItemsSchemaAsync(cancellationToken);

        var extra = await _context.Set<PhysicalCountExtraItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == extraItemId, cancellationToken);

        if (extra is null)
        {
            return NotFound(new { Message = $"No se encontró la herramienta reportada {extraItemId}." });
        }

        
        if (extra.Rejected || extra.ReconciliationStatus == "Rejected")
        {
            return BadRequest(new { Message = "La herramienta reportada está rechazada y no puede aprobarse para creación." });
        }

        if (extra.RequiresUserClarification || extra.ReconciliationStatus == "RequiresUserClarification")
        {
            return BadRequest(new { Message = "La herramienta reportada requiere aclaración antes de aprobar creación." });
        }
var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        extra.ApprovedForCreation = true;
        extra.ApprovedForCreationAt = now;
        extra.ApprovedForCreationBy = actionBy;
        extra.ReconciliationStatus = "ApprovedForCreation";
        extra.ReconciliationObservation = NormalizeOptional(request.Observation)
            ?? "Herramienta aprobada para creación en inventario después de conciliación.";
        extra.SuggestedAction = "Crear herramienta en inventario";
        extra.UpdatedAt = now;
        extra.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            extra.Id,
            extra.ApprovedForCreation,
            extra.ApprovedForCreationAt,
            extra.ApprovedForCreationBy,
            extra.ReconciliationStatus,
            ReconciliationStatusLabel = GetExtraReconciliationStatusLabel(extra.ReconciliationStatus),
            extra.ReconciliationObservation
        });
    }

    [HttpPatch("extra-items/{extraItemId:guid}/reject")]
    public async Task<IActionResult> RejectExtraItem(Guid extraItemId, [FromBody] ExtraItemReconciliationRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountExtraItemsSchemaAsync(cancellationToken);

        var extra = await _context.Set<PhysicalCountExtraItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == extraItemId, cancellationToken);

        if (extra is null)
        {
            return NotFound(new { Message = $"No se encontró la herramienta reportada {extraItemId}." });
        }

        var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        extra.Rejected = true;
        extra.RejectedAt = now;
        extra.RejectedBy = actionBy;
        extra.RejectionReason = NormalizeOptional(request.Observation) ?? "Reporte rechazado en conciliación.";
        extra.ReconciliationStatus = "Rejected";
        extra.ReconciliationObservation = extra.RejectionReason;
        extra.UpdatedAt = now;
        extra.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            extra.Id,
            extra.Rejected,
            extra.RejectedAt,
            extra.RejectedBy,
            extra.RejectionReason,
            extra.ReconciliationStatus,
            ReconciliationStatusLabel = GetExtraReconciliationStatusLabel(extra.ReconciliationStatus)
        });
    }

    [HttpPatch("extra-items/{extraItemId:guid}/reconcile")]
    public async Task<IActionResult> ReconcileExtraItem(Guid extraItemId, [FromBody] ExtraItemReconciliationRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountExtraItemsSchemaAsync(cancellationToken);

        var extra = await _context.Set<PhysicalCountExtraItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == extraItemId, cancellationToken);

        if (extra is null)
        {
            return NotFound(new { Message = $"No se encontró la herramienta reportada {extraItemId}." });
        }

        
        if (extra.Rejected || extra.ReconciliationStatus == "Rejected")
        {
            return BadRequest(new { Message = "La herramienta reportada está rechazada y no puede conciliarse." });
        }

        if (extra.RequiresUserClarification || extra.ReconciliationStatus == "RequiresUserClarification")
        {
            return BadRequest(new { Message = "La herramienta reportada requiere aclaración antes de conciliarse." });
        }
var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        extra.ReconciliationStatus = "Reconciled";
        extra.ReconciliationObservation = NormalizeOptional(request.Observation)
            ?? "Herramienta conciliada manualmente.";
        extra.ReconciledAt = now;
        extra.ReconciledBy = actionBy;
        extra.RequiresUserClarification = false;
        extra.UpdatedAt = now;
        extra.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            extra.Id,
            extra.ReconciliationStatus,
            ReconciliationStatusLabel = GetExtraReconciliationStatusLabel(extra.ReconciliationStatus),
            extra.ReconciliationObservation,
            extra.ReconciledAt,
            extra.ReconciledBy
        });
    }

    private async Task<ToolAsset?> TryFindReportedToolAsync(string? reportedCode, string? reportedSerial, CancellationToken cancellationToken)
    {
        var code = NormalizeOptional(reportedCode);
        var serial = NormalizeOptional(reportedSerial);

        if (!string.IsNullOrWhiteSpace(code))
        {
            var byCode = await _context.ToolAssets
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    (x.InternalCode == code || x.FixedAssetCode == code || x.FenixCode == code),
                    cancellationToken);

            if (byCode is not null)
            {
                return byCode;
            }
        }

        if (!string.IsNullOrWhiteSpace(serial))
        {
            var bySerial = await _context.ToolAssets
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.SerialNumber != null &&
                    x.SerialNumber == serial,
                    cancellationToken);

            if (bySerial is not null)
            {
                return bySerial;
            }
        }

        return null;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string GetExtraReconciliationStatusLabel(string? status)
    {
        return status switch
        {
            "PendingReview" => "Pendiente de revisión",
            "MatchedInSystem" => "Existe en sistema",
            "DifferentResponsible" => "Existe con otro responsable",
            "DifferentLocation" => "Existe en otra ubicación",
            "NotFoundInSystem" => "No existe en sistema",
            "RequiresUserClarification" => "Requiere aclaración del usuario",
            "ApprovedForCreation" => "Aprobada para crear",
            "Rejected" => "Rechazada",
            "Reconciled" => "Conciliada",
            _ => string.IsNullOrWhiteSpace(status) ? "Pendiente de revisión" : status
        };
    }

    private async Task EnsurePhysicalCountExtraItemsSchemaAsync(CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PhysicalCounts')
BEGIN
    EXEC('CREATE SCHEMA PhysicalCounts');
END;

IF OBJECT_ID(N'[PhysicalCounts].[PhysicalCountExtraItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [PhysicalCounts].[PhysicalCountExtraItems](
        [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PhysicalCountExtraItems] PRIMARY KEY,
        [PhysicalCountId] uniqueidentifier NOT NULL,
        [PhysicalCountParticipantId] uniqueidentifier NULL,
        [MatchedToolAssetId] uniqueidentifier NULL,
        [ReportedCode] nvarchar(120) NULL,
        [ReportedName] nvarchar(250) NOT NULL,
        [ReportedSerial] nvarchar(160) NULL,
        [ReportedBrand] nvarchar(160) NULL,
        [ReportedModel] nvarchar(160) NULL,
        [FoundLocation] nvarchar(250) NULL,
        [PhysicalStatus] nvarchar(120) NULL,
        [Observation] nvarchar(2000) NULL,
        [ReportedBy] nvarchar(180) NULL,
        [ReportedAt] datetime2 NOT NULL,
        [ReconciliationStatus] nvarchar(80) NOT NULL,
        [ReconciliationObservation] nvarchar(2000) NULL,
        [RequiresUserClarification] bit NOT NULL CONSTRAINT [DF_PhysicalCountExtraItems_RequiresUserClarification] DEFAULT 0,
        [ClarificationRequestedAt] datetime2 NULL,
        [ClarificationRequestedBy] nvarchar(180) NULL,
        [ReconciledAt] datetime2 NULL,
        [ReconciledBy] nvarchar(180) NULL,
        [SuggestedAction] nvarchar(300) NULL,
        [ApprovedForCreation] bit NOT NULL CONSTRAINT [DF_PhysicalCountExtraItems_ApprovedForCreation] DEFAULT 0,
        [ApprovedForCreationAt] datetime2 NULL,
        [ApprovedForCreationBy] nvarchar(180) NULL,
        [Rejected] bit NOT NULL CONSTRAINT [DF_PhysicalCountExtraItems_Rejected] DEFAULT 0,
        [RejectedAt] datetime2 NULL,
        [RejectedBy] nvarchar(180) NULL,
        [RejectionReason] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(150) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(150) NULL,
        [IsDeleted] bit NOT NULL CONSTRAINT [DF_PhysicalCountExtraItems_IsDeleted] DEFAULT 0
    );

    ALTER TABLE [PhysicalCounts].[PhysicalCountExtraItems]
    ADD CONSTRAINT [FK_PhysicalCountExtraItems_PhysicalCounts]
    FOREIGN KEY ([PhysicalCountId]) REFERENCES [PhysicalCounts].[PhysicalCounts]([Id]);

    ALTER TABLE [PhysicalCounts].[PhysicalCountExtraItems]
    ADD CONSTRAINT [FK_PhysicalCountExtraItems_Participants]
    FOREIGN KEY ([PhysicalCountParticipantId]) REFERENCES [PhysicalCounts].[PhysicalCountParticipants]([Id]);

    CREATE INDEX [IX_PhysicalCountExtraItems_PhysicalCountId]
    ON [PhysicalCounts].[PhysicalCountExtraItems]([PhysicalCountId]);

    CREATE INDEX [IX_PhysicalCountExtraItems_ParticipantId]
    ON [PhysicalCounts].[PhysicalCountExtraItems]([PhysicalCountParticipantId]);

    CREATE INDEX [IX_PhysicalCountExtraItems_ReconciliationStatus]
    ON [PhysicalCounts].[PhysicalCountExtraItems]([ReconciliationStatus]);
END;
";
        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public sealed class ReportExtraToolRequest
    {
        public string? ReportedCode { get; set; }
        public string? ReportedName { get; set; }
        public string? ReportedSerial { get; set; }
        public string? ReportedBrand { get; set; }
        public string? ReportedModel { get; set; }
        public string? FoundLocation { get; set; }
        public string? PhysicalStatus { get; set; }
        public string? Observation { get; set; }
        public string? ReportedBy { get; set; }
        public bool RequiresUserClarification { get; set; }
    }

    public sealed class ExtraItemReconciliationRequest
    {
        public string? ActionBy { get; set; }
        public string? Observation { get; set; }
    }

    public sealed class MatchExtraToolRequest
    {
        public Guid? MatchedToolAssetId { get; set; }
        public bool MarkAsReconciled { get; set; }
        public string? ActionBy { get; set; }
        public string? Observation { get; set; }
    }

    [HttpGet("{id:guid}/reported-items-board")]
    public async Task<IActionResult> GetReportedItemsBoard(Guid id, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var count = await _context.PhysicalCounts
            .AsNoTracking()
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        var items = await _context.Set<PhysicalCountReportedItem>()
            .AsNoTracking()
            .Include(x => x.Participant)
            .Include(x => x.ToolAsset)
            .Include(x => x.MatchedToolAsset)
            .Where(x => !x.IsDeleted && x.PhysicalCountId == id)
            .OrderByDescending(x => x.ReportedAt)
            .Select(x => new
            {
                x.Id,
                x.PhysicalCountId,
                x.PhysicalCountParticipantId,
                ParticipantName = x.Participant == null ? null : x.Participant.DisplayName,
                ParticipantUserName = x.Participant == null ? null : x.Participant.UserName,

                x.ReportType,
                ReportTypeLabel = GetReportedItemTypeLabel(x.ReportType),

                x.ToolAssetId,
                ToolInternalCode = x.ToolAsset == null ? null : x.ToolAsset.InternalCode,
                ToolName = x.ToolAsset == null ? null : x.ToolAsset.Name,

                x.MatchedToolAssetId,
                MatchedToolInternalCode = x.MatchedToolAsset == null ? null : x.MatchedToolAsset.InternalCode,
                MatchedToolName = x.MatchedToolAsset == null ? null : x.MatchedToolAsset.Name,

                x.CreatedToolAssetId,

                x.ReportedCode,
                x.ReportedName,
                x.SerialNumber,
                x.Brand,
                x.Model,

                x.AssetTypeId,
                x.AssetTypeName,
                x.CategoryId,
                x.CategoryName,

                x.BranchId,
                x.BranchCode,
                x.LocationId,
                x.FoundLocation,

                x.ResponsiblePersonId,
                x.ResponsibleName,

                x.PhysicalStatus,
                x.OperationalStatus,
                x.Observation,
                x.EvidenceDocumentId,

                x.ReportedBy,
                x.ReportedAt,

                x.ReconciliationStatus,
                ReconciliationStatusLabel = GetReportedItemReconciliationStatusLabel(x.ReconciliationStatus),
                x.ReconciliationObservation,

                x.RequiresUserClarification,
                x.ClarificationRequestedAt,
                x.ClarificationRequestedBy,

                x.MinimumDataCompleted,
                x.MissingRequiredData,

                x.ApprovedForCreation,
                x.ApprovedForCreationAt,
                x.ApprovedForCreationBy,

                x.Rejected,
                x.RejectedAt,
                x.RejectedBy,
                x.RejectionReason,

                x.ReconciledAt,
                x.ReconciledBy,

                x.SuggestedAction
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            count.Id,
            count.CountNumber,
            count.BranchId,
            BranchCode = count.Branch == null ? null : count.Branch.Code,
            BranchName = count.Branch == null ? null : count.Branch.Name,
            Status = count.Status.ToString(),
            StatusLabel = GetPhysicalCountStatusLabel(count.Status),
            Items = items,
            Summary = new
            {
                Total = items.Count,
                Found = items.Count(x => x.ReportType == "Found"),
                NotFound = items.Count(x => x.ReportType == "NotFound"),
                ReturnedOrDelivered = items.Count(x => x.ReportType == "ReturnedOrDelivered"),
                Damaged = items.Count(x => x.ReportType == "Damaged"),
                DifferentLocation = items.Count(x => x.ReportType == "DifferentLocation"),
                DifferentResponsible = items.Count(x => x.ReportType == "DifferentResponsible"),
                ExtraNotListed = items.Count(x => x.ReportType == "ExtraNotListed"),
                PendingReview = items.Count(x => x.ReconciliationStatus == "PendingReview" || x.ReconciliationStatus == "NotFoundInSystem"),
                RequiresUserClarification = items.Count(x => x.RequiresUserClarification || x.ReconciliationStatus == "RequiresUserClarification"),
                ApprovedForCreation = items.Count(x => x.ApprovedForCreation || x.ReconciliationStatus == "ApprovedForCreation"),
                Reconciled = items.Count(x => x.ReconciliationStatus == "Reconciled"),
                Rejected = items.Count(x => x.ReconciliationStatus == "Rejected"),
                MissingMinimumData = items.Count(x => !x.MinimumDataCompleted && x.ReportType == "ExtraNotListed")
            }
        });
    }

    [HttpPost("participants/{participantId:guid}/reported-items")]
    public async Task<IActionResult> CreateReportedItem(Guid participantId, [FromBody] CreatePhysicalCountReportedItemRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var participant = await _context.Set<PhysicalCountParticipant>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == participantId, cancellationToken);

        if (participant is null)
        {
            return NotFound(new { Message = $"No se encontró el participante {participantId}." });
        }

        var count = await _context.PhysicalCounts
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == participant.PhysicalCountId, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = "No se encontró la toma física asociada." });
        }

        if (count.Status == PhysicalCountStatus.Cancelled || count.Status == PhysicalCountStatus.Reconciled)
        {
            return BadRequest(new { Message = $"No se pueden registrar novedades en una toma física en estado {count.Status}." });
        }

        var reportType = NormalizeOptional(request.ReportType) ?? "ExtraNotListed";

        if (string.IsNullOrWhiteSpace(request.ReportedName) && string.IsNullOrWhiteSpace(request.ReportedCode))
        {
            return BadRequest(new { Message = "Debe indicar al menos el nombre o código del registro reportado." });
        }

        var now = DateTime.UtcNow;
        var reportedBy = NormalizeOptional(request.ReportedBy) ?? participant.UserName ?? "admin";

        ToolAsset? matchedTool = null;

        if (request.ToolAssetId.HasValue)
        {
            matchedTool = await _context.ToolAssets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == request.ToolAssetId.Value, cancellationToken);
        }

        if (matchedTool is null)
        {
            matchedTool = await TryFindReportedToolAsync(request.ReportedCode, request.SerialNumber, cancellationToken);
        }

        var minimum = ValidateMinimumDataForReportedItem(reportType, request, count, participant);

        var item = new PhysicalCountReportedItem
        {
            Id = Guid.NewGuid(),
            PhysicalCountId = count.Id,
            PhysicalCountParticipantId = participant.Id,

            ReportType = reportType,
            ToolAssetId = request.ToolAssetId,
            MatchedToolAssetId = matchedTool?.Id,

            ReportedCode = NormalizeOptional(request.ReportedCode),
            ReportedName = NormalizeOptional(request.ReportedName) ?? matchedTool?.Name ?? "Registro reportado",
            SerialNumber = NormalizeOptional(request.SerialNumber),
            Brand = NormalizeOptional(request.Brand),
            Model = NormalizeOptional(request.Model),

            AssetTypeId = request.AssetTypeId,
            AssetTypeName = NormalizeOptional(request.AssetTypeName),
            CategoryId = request.CategoryId,
            CategoryName = NormalizeOptional(request.CategoryName),

            BranchId = request.BranchId ?? count.BranchId,
            BranchCode = NormalizeOptional(request.BranchCode) ?? count.Branch?.Code,
            LocationId = request.LocationId,
            FoundLocation = NormalizeOptional(request.FoundLocation),

            ResponsiblePersonId = request.ResponsiblePersonId ?? participant.ResponsiblePersonId,
            ResponsibleName = NormalizeOptional(request.ResponsibleName) ?? participant.DisplayName,

            PhysicalStatus = NormalizeOptional(request.PhysicalStatus),
            OperationalStatus = NormalizeOptional(request.OperationalStatus),
            Observation = NormalizeOptional(request.Observation),
            EvidenceDocumentId = request.EvidenceDocumentId,

            ReportedBy = reportedBy,
            ReportedAt = now,

            ReconciliationStatus = BuildInitialReportedItemStatus(reportType, matchedTool),
            ReconciliationObservation = BuildInitialReportedItemObservation(reportType, matchedTool),
            RequiresUserClarification = request.RequiresUserClarification || !minimum.IsComplete,

            MinimumDataCompleted = minimum.IsComplete,
            MissingRequiredData = minimum.MissingData,

            SuggestedAction = BuildSuggestedAction(reportType, matchedTool, minimum.IsComplete),

            CreatedAt = now,
            CreatedBy = reportedBy
        };

        _context.Set<PhysicalCountReportedItem>().Add(item);

        participant.LastActivityAt = now;
        participant.UpdatedAt = now;
        participant.UpdatedBy = reportedBy;

        if (participant.Status == "NotStarted")
        {
            participant.Status = "InProgress";
            participant.StartedAt = now;
        }

        switch (reportType)
        {
            case "Found":
                participant.FoundItems++;
                break;
            case "NotFound":
            case "ReturnedOrDelivered":
                participant.MissingItems++;
                participant.DifferentItems++;
                break;
            case "Damaged":
            case "DifferentLocation":
            case "DifferentResponsible":
                participant.DifferentItems++;
                if (reportType == "Damaged")
                {
                    participant.DamagedItems++;
                }
                break;
            case "ExtraNotListed":
                participant.ExtraItems++;
                participant.DifferentItems++;
                break;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            item.Id,
            item.PhysicalCountId,
            item.PhysicalCountParticipantId,
            item.ReportType,
            ReportTypeLabel = GetReportedItemTypeLabel(item.ReportType),
            item.ReportedCode,
            item.ReportedName,
            item.SerialNumber,
            item.AssetTypeName,
            item.CategoryName,
            item.BranchCode,
            item.FoundLocation,
            item.ResponsibleName,
            item.PhysicalStatus,
            item.Observation,
            item.MinimumDataCompleted,
            item.MissingRequiredData,
            item.ReconciliationStatus,
            ReconciliationStatusLabel = GetReportedItemReconciliationStatusLabel(item.ReconciliationStatus),
            item.RequiresUserClarification,
            item.MatchedToolAssetId,
            MatchedToolInternalCode = matchedTool?.InternalCode,
            MatchedToolName = matchedTool?.Name,
            item.SuggestedAction,
            Message = "Registro reportado guardado correctamente para conciliación."
        });
    }

    [HttpPatch("reported-items/{reportedItemId:guid}/request-clarification")]
    public async Task<IActionResult> RequestReportedItemClarification(Guid reportedItemId, [FromBody] ReportedItemActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var item = await _context.Set<PhysicalCountReportedItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == reportedItemId, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = $"No se encontró el registro reportado {reportedItemId}." });
        }

        var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        item.RequiresUserClarification = true;
        item.ClarificationRequestedAt = now;
        item.ClarificationRequestedBy = actionBy;
        item.ReconciliationStatus = "RequiresUserClarification";
        item.ReconciliationObservation = NormalizeOptional(request.Observation)
            ?? "Se requiere contactar al usuario para ampliar información del registro reportado.";
        item.UpdatedAt = now;
        item.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            item.Id,
            item.ReconciliationStatus,
            ReconciliationStatusLabel = GetReportedItemReconciliationStatusLabel(item.ReconciliationStatus),
            item.RequiresUserClarification,
            item.ClarificationRequestedAt,
            item.ClarificationRequestedBy,
            item.ReconciliationObservation
        });
    }

    [HttpPatch("reported-items/{reportedItemId:guid}/approve-creation")]
    public async Task<IActionResult> ApproveReportedItemForCreation(Guid reportedItemId, [FromBody] ReportedItemActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var item = await _context.Set<PhysicalCountReportedItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == reportedItemId, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = $"No se encontró el registro reportado {reportedItemId}." });
        }

        if (item.ReportType != "ExtraNotListed")
        {
            return BadRequest(new { Message = "Solo las herramientas no listadas pueden aprobarse para creación en Inventario de AF." });
        }

        if (!item.MinimumDataCompleted)
        {
            return BadRequest(new
            {
                Message = "No se puede aprobar creación porque faltan datos mínimos.",
                item.MissingRequiredData
            });
        }

        
        if (item.Rejected || item.ReconciliationStatus == "Rejected")
        {
            return BadRequest(new { Message = "El registro está rechazado y no puede aprobarse para creación." });
        }

        if (item.RequiresUserClarification || item.ReconciliationStatus == "RequiresUserClarification")
        {
            return BadRequest(new { Message = "El registro requiere aclaración del usuario antes de aprobar creación." });
        }
var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        item.ApprovedForCreation = true;
        item.ApprovedForCreationAt = now;
        item.ApprovedForCreationBy = actionBy;
        item.ReconciliationStatus = "ApprovedForCreation";
        item.ReconciliationObservation = NormalizeOptional(request.Observation)
            ?? "Registro aprobado para creación en Inventario de AF después de conciliación.";
        item.SuggestedAction = "Crear activo/herramienta en Inventario de AF";
        item.RequiresUserClarification = false;
        item.UpdatedAt = now;
        item.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            item.Id,
            item.ApprovedForCreation,
            item.ApprovedForCreationAt,
            item.ApprovedForCreationBy,
            item.ReconciliationStatus,
            ReconciliationStatusLabel = GetReportedItemReconciliationStatusLabel(item.ReconciliationStatus),
            item.ReconciliationObservation
        });
    }

    [HttpPatch("reported-items/{reportedItemId:guid}/reject")]
    public async Task<IActionResult> RejectReportedItem(Guid reportedItemId, [FromBody] ReportedItemActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var item = await _context.Set<PhysicalCountReportedItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == reportedItemId, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = $"No se encontró el registro reportado {reportedItemId}." });
        }

        var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        item.Rejected = true;
        item.RejectedAt = now;
        item.RejectedBy = actionBy;
        item.RejectionReason = NormalizeOptional(request.Observation) ?? "Reporte rechazado en conciliación.";
        item.ReconciliationStatus = "Rejected";
        item.ReconciliationObservation = item.RejectionReason;
        item.UpdatedAt = now;
        item.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            item.Id,
            item.Rejected,
            item.RejectedAt,
            item.RejectedBy,
            item.RejectionReason,
            item.ReconciliationStatus,
            ReconciliationStatusLabel = GetReportedItemReconciliationStatusLabel(item.ReconciliationStatus)
        });
    }

    [HttpPatch("reported-items/{reportedItemId:guid}/reconcile")]
    public async Task<IActionResult> ReconcileReportedItem(Guid reportedItemId, [FromBody] ReportedItemActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var item = await _context.Set<PhysicalCountReportedItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == reportedItemId, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = $"No se encontró el registro reportado {reportedItemId}." });
        }

        
        if (item.Rejected || item.ReconciliationStatus == "Rejected")
        {
            return BadRequest(new { Message = "El registro está rechazado y no puede conciliarse." });
        }

        if (item.RequiresUserClarification || item.ReconciliationStatus == "RequiresUserClarification")
        {
            return BadRequest(new { Message = "El registro requiere aclaración del usuario antes de conciliarse." });
        }
var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        item.ReconciliationStatus = "Reconciled";
        item.ReconciliationObservation = NormalizeOptional(request.Observation)
            ?? "Registro conciliado manualmente desde administración.";
        item.ReconciledAt = now;
        item.ReconciledBy = actionBy;
        item.RequiresUserClarification = false;
        item.UpdatedAt = now;
        item.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            item.Id,
            item.ReconciliationStatus,
            ReconciliationStatusLabel = GetReportedItemReconciliationStatusLabel(item.ReconciliationStatus),
            item.ReconciliationObservation,
            item.ReconciledAt,
            item.ReconciledBy
        });
    }

    private static string BuildInitialReportedItemStatus(string reportType, ToolAsset? matchedTool)
    {
        return reportType switch
        {
            "Found" => "PendingReview",
            "NotFound" => "PendingReview",
            "ReturnedOrDelivered" => "PendingReview",
            "Damaged" => "PendingReview",
            "DifferentLocation" => "PendingReview",
            "DifferentResponsible" => "PendingReview",
            "ExtraNotListed" => matchedTool is null ? "NotFoundInSystem" : "MatchedInSystem",
            _ => "PendingReview"
        };
    }

    private static string BuildInitialReportedItemObservation(string reportType, ToolAsset? matchedTool)
    {
        if (reportType == "ExtraNotListed")
        {
            return matchedTool is null
                ? "Herramienta reportada por usuario, no encontrada automáticamente en el sistema."
                : $"Coincidencia automática con activo {matchedTool.InternalCode}.";
        }

        return "Registro reportado por usuario durante la toma física.";
    }

    private static string BuildSuggestedAction(string reportType, ToolAsset? matchedTool, bool minimumDataCompleted)
    {
        return reportType switch
        {
            "Found" => "Validar y conciliar registro encontrado",
            "NotFound" => "Validar faltante con responsable",
            "ReturnedOrDelivered" => "Validar devolución o entrega contra historial de asignaciones",
            "Damaged" => "Evaluar creación de solicitud de mantenimiento",
            "DifferentLocation" => "Validar cambio de ubicación",
            "DifferentResponsible" => "Validar cambio de responsable",
            "ExtraNotListed" when matchedTool is not null => "Validar si corresponde a activo existente",
            "ExtraNotListed" when !minimumDataCompleted => "Solicitar datos mínimos antes de crear en Inventario de AF",
            "ExtraNotListed" => "Validar si debe crearse en Inventario de AF",
            _ => "Revisar en conciliación"
        };
    }

    private static MinimumDataValidationResult ValidateMinimumDataForReportedItem(
        string reportType,
        CreatePhysicalCountReportedItemRequest request,
        PhysicalCount count,
        PhysicalCountParticipant participant)
    {
        if (reportType != "ExtraNotListed")
        {
            return new MinimumDataValidationResult(true, null);
        }

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(request.ReportedName))
        {
            missing.Add("Nombre del activo/herramienta");
        }

        if (string.IsNullOrWhiteSpace(request.AssetTypeName) && !request.AssetTypeId.HasValue)
        {
            missing.Add("Tipo de activo");
        }

        if (string.IsNullOrWhiteSpace(request.CategoryName) && !request.CategoryId.HasValue)
        {
            missing.Add("Categoría");
        }

        if (!request.BranchId.HasValue && count.BranchId == Guid.Empty && string.IsNullOrWhiteSpace(request.BranchCode))
        {
            missing.Add("Sede");
        }

        if (string.IsNullOrWhiteSpace(request.FoundLocation) && !request.LocationId.HasValue)
        {
            missing.Add("Ubicación encontrada");
        }

        if (string.IsNullOrWhiteSpace(request.PhysicalStatus))
        {
            missing.Add("Estado físico");
        }

        if (string.IsNullOrWhiteSpace(request.Observation))
        {
            missing.Add("Observación");
        }

        return new MinimumDataValidationResult(
            missing.Count == 0,
            missing.Count == 0 ? null : string.Join(", ", missing));
    }

    private static string GetReportedItemTypeLabel(string? type)
    {
        return type switch
        {
            "Found" => "Encontrada",
            "NotFound" => "No encontrada",
            "ReturnedOrDelivered" => "Devuelta / entregada",
            "Damaged" => "Dañada",
            "DifferentLocation" => "Ubicación diferente",
            "DifferentResponsible" => "Responsable diferente",
            "ExtraNotListed" => "Herramienta no listada",
            _ => string.IsNullOrWhiteSpace(type) ? "Reporte" : type
        };
    }

    private static string GetReportedItemReconciliationStatusLabel(string? status)
    {
        return status switch
        {
            "PendingReview" => "Pendiente de revisión",
            "MatchedInSystem" => "Existe en sistema",
            "NotFoundInSystem" => "No existe en sistema",
            "RequiresUserClarification" => "Requiere aclaración",
            "ApprovedForCreation" => "Aprobada para crear",
            "Rejected" => "Rechazada",
            "Reconciled" => "Conciliada",
            _ => string.IsNullOrWhiteSpace(status) ? "Pendiente de revisión" : status
        };
    }

    private async Task EnsurePhysicalCountReportedItemsSchemaAsync(CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountParticipantSchemaAsync(cancellationToken);

        var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'PhysicalCounts')
BEGIN
    EXEC('CREATE SCHEMA PhysicalCounts');
END;

IF OBJECT_ID(N'[PhysicalCounts].[PhysicalCountReportedItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [PhysicalCounts].[PhysicalCountReportedItems](
        [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_PhysicalCountReportedItems] PRIMARY KEY,
        [PhysicalCountId] uniqueidentifier NOT NULL,
        [PhysicalCountParticipantId] uniqueidentifier NULL,
        [ReportType] nvarchar(80) NOT NULL,
        [ToolAssetId] uniqueidentifier NULL,
        [MatchedToolAssetId] uniqueidentifier NULL,
        [CreatedToolAssetId] uniqueidentifier NULL,
        [ReportedCode] nvarchar(120) NULL,
        [ReportedName] nvarchar(250) NOT NULL,
        [SerialNumber] nvarchar(160) NULL,
        [Brand] nvarchar(160) NULL,
        [Model] nvarchar(160) NULL,
        [AssetTypeId] uniqueidentifier NULL,
        [AssetTypeName] nvarchar(180) NULL,
        [CategoryId] uniqueidentifier NULL,
        [CategoryName] nvarchar(180) NULL,
        [BranchId] uniqueidentifier NULL,
        [BranchCode] nvarchar(80) NULL,
        [LocationId] uniqueidentifier NULL,
        [FoundLocation] nvarchar(250) NULL,
        [ResponsiblePersonId] uniqueidentifier NULL,
        [ResponsibleName] nvarchar(250) NULL,
        [PhysicalStatus] nvarchar(120) NULL,
        [OperationalStatus] nvarchar(120) NULL,
        [Observation] nvarchar(2000) NULL,
        [EvidenceDocumentId] uniqueidentifier NULL,
        [ReportedBy] nvarchar(180) NULL,
        [ReportedAt] datetime2 NOT NULL,
        [ReconciliationStatus] nvarchar(80) NOT NULL,
        [ReconciliationObservation] nvarchar(2000) NULL,
        [RequiresUserClarification] bit NOT NULL CONSTRAINT [DF_PhysicalCountReportedItems_RequiresUserClarification] DEFAULT 0,
        [ClarificationRequestedAt] datetime2 NULL,
        [ClarificationRequestedBy] nvarchar(180) NULL,
        [MinimumDataCompleted] bit NOT NULL CONSTRAINT [DF_PhysicalCountReportedItems_MinimumDataCompleted] DEFAULT 0,
        [MissingRequiredData] nvarchar(2000) NULL,
        [ApprovedForCreation] bit NOT NULL CONSTRAINT [DF_PhysicalCountReportedItems_ApprovedForCreation] DEFAULT 0,
        [ApprovedForCreationAt] datetime2 NULL,
        [ApprovedForCreationBy] nvarchar(180) NULL,
        [Rejected] bit NOT NULL CONSTRAINT [DF_PhysicalCountReportedItems_Rejected] DEFAULT 0,
        [RejectedAt] datetime2 NULL,
        [RejectedBy] nvarchar(180) NULL,
        [RejectionReason] nvarchar(2000) NULL,
        [ReconciledAt] datetime2 NULL,
        [ReconciledBy] nvarchar(180) NULL,
        [SuggestedAction] nvarchar(300) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] nvarchar(150) NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] nvarchar(150) NULL,
        [IsDeleted] bit NOT NULL CONSTRAINT [DF_PhysicalCountReportedItems_IsDeleted] DEFAULT 0
    );

    ALTER TABLE [PhysicalCounts].[PhysicalCountReportedItems]
    ADD CONSTRAINT [FK_PhysicalCountReportedItems_PhysicalCounts]
    FOREIGN KEY ([PhysicalCountId]) REFERENCES [PhysicalCounts].[PhysicalCounts]([Id]);

    ALTER TABLE [PhysicalCounts].[PhysicalCountReportedItems]
    ADD CONSTRAINT [FK_PhysicalCountReportedItems_Participants]
    FOREIGN KEY ([PhysicalCountParticipantId]) REFERENCES [PhysicalCounts].[PhysicalCountParticipants]([Id]);

    CREATE INDEX [IX_PhysicalCountReportedItems_PhysicalCountId]
    ON [PhysicalCounts].[PhysicalCountReportedItems]([PhysicalCountId]);

    CREATE INDEX [IX_PhysicalCountReportedItems_ParticipantId]
    ON [PhysicalCounts].[PhysicalCountReportedItems]([PhysicalCountParticipantId]);

    CREATE INDEX [IX_PhysicalCountReportedItems_ReportType]
    ON [PhysicalCounts].[PhysicalCountReportedItems]([ReportType]);

    CREATE INDEX [IX_PhysicalCountReportedItems_ReconciliationStatus]
    ON [PhysicalCounts].[PhysicalCountReportedItems]([ReconciliationStatus]);
END;
";
        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    public sealed class CreatePhysicalCountReportedItemRequest
    {
        public string? ReportType { get; set; }
        public Guid? ToolAssetId { get; set; }
        public string? ReportedCode { get; set; }
        public string? ReportedName { get; set; }
        public string? SerialNumber { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public Guid? AssetTypeId { get; set; }
        public string? AssetTypeName { get; set; }
        public Guid? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public Guid? BranchId { get; set; }
        public string? BranchCode { get; set; }
        public Guid? LocationId { get; set; }
        public string? FoundLocation { get; set; }
        public Guid? ResponsiblePersonId { get; set; }
        public string? ResponsibleName { get; set; }
        public string? PhysicalStatus { get; set; }
        public string? OperationalStatus { get; set; }
        public string? Observation { get; set; }
        public Guid? EvidenceDocumentId { get; set; }
        public string? ReportedBy { get; set; }
        public bool RequiresUserClarification { get; set; }
    }

    public sealed class ReportedItemActionRequest
    {
        public string? ActionBy { get; set; }
        public string? Observation { get; set; }
    }

    private sealed record MinimumDataValidationResult(bool IsComplete, string? MissingData);

    [HttpPost("reported-items/{reportedItemId:guid}/create-tool-asset")]
    public async Task<IActionResult> CreateToolAssetFromReportedItem(Guid reportedItemId, [FromBody] CreateToolAssetFromReportedItemRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var item = await _context.Set<PhysicalCountReportedItem>()
            .Include(x => x.PhysicalCount)
            .Include(x => x.Participant)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == reportedItemId, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = $"No se encontró el registro reportado {reportedItemId}." });
        }

        if (item.ReportType != "ExtraNotListed")
        {
            return BadRequest(new { Message = "Solo los registros tipo herramienta no listada pueden convertirse en Inventario de AF." });
        }

        
        if (item.Rejected || item.ReconciliationStatus == "Rejected")
        {
            return BadRequest(new { Message = "El registro está rechazado y no puede crearse en Inventario de AF." });
        }

        if (item.RequiresUserClarification || item.ReconciliationStatus == "RequiresUserClarification")
        {
            return BadRequest(new { Message = "El registro requiere aclaración del usuario antes de crearse en Inventario de AF." });
        }
if (!item.ApprovedForCreation && item.ReconciliationStatus != "ApprovedForCreation")
        {
            return BadRequest(new { Message = "El registro debe estar aprobado para creación antes de crear el activo." });
        }

        if (!item.MinimumDataCompleted)
        {
            return BadRequest(new
            {
                Message = "No se puede crear el activo porque faltan datos mínimos.",
                item.MissingRequiredData
            });
        }

        if (item.CreatedToolAssetId.HasValue)
        {
            return BadRequest(new
            {
                Message = "Este registro ya fue creado en Inventario de AF.",
                item.CreatedToolAssetId
            });
        }

        var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        var branchCode = NormalizeOptional(request.BranchCode)
            ?? NormalizeOptional(item.BranchCode);

        if (string.IsNullOrWhiteSpace(branchCode))
        {
            return BadRequest(new { Message = "No se puede crear el activo porque no tiene sede/código de sede." });
        }

        branchCode = NormalizeCode(branchCode);

        var branch = await _context.Branches
            .Include(x => x.Zone)
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Code == branchCode, cancellationToken);

        if (branch is null)
        {
            return BadRequest(new { Message = $"No existe la sede {branchCode}." });
        }

        var internalCode = NormalizeOptional(request.InternalCode)
            ?? NormalizeOptional(item.ReportedCode)
            ?? $"TF-{DateTime.UtcNow:yyyyMMddHHmmss}-{item.Id.ToString("N")[..6]}";

        internalCode = NormalizeCode(internalCode);

        var existing = await _context.ToolAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.InternalCode == internalCode, cancellationToken);

        if (existing is not null)
        {
            return BadRequest(new
            {
                Message = $"Ya existe una herramienta/activo con código interno {internalCode}.",
                existing.Id,
                existing.InternalCode,
                existing.Name
            });
        }

        ToolLocation? location = null;

        var locationText = NormalizeOptional(request.LocationCode)
            ?? NormalizeOptional(item.FoundLocation);

        if (!string.IsNullOrWhiteSpace(locationText))
        {
            var locationCode = NormalizeCode(locationText);

            location = await _context.ToolLocations
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.BranchId == branch.Id &&
                    (x.Code == locationCode || x.Name == locationText),
                    cancellationToken);

            if (location is null && request.CreateMissingCatalogs)
            {
                location = new ToolLocation
                {
                    Id = Guid.NewGuid(),
                    BranchId = branch.Id,
                    Code = locationCode.Length > 80 ? locationCode[..80] : locationCode,
                    Name = locationText.Length > 180 ? locationText[..180] : locationText,
                    Description = $"Ubicación creada desde toma física {item.PhysicalCount?.CountNumber}.",
                    IsActive = true,
                    CreatedAt = now,
                    CreatedBy = actionBy
                };

                _context.ToolLocations.Add(location);
            }
        }

        ToolType? toolType = null;

        var typeName = NormalizeOptional(request.ToolTypeName)
            ?? NormalizeOptional(item.AssetTypeName)
            ?? "Herramienta";

        var typeCode = NormalizeCode(typeName);

        toolType = await _context.ToolTypes
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                (x.Code == typeCode || x.Name == typeName),
                cancellationToken);

        if (toolType is null && request.CreateMissingCatalogs)
        {
            toolType = new ToolType
            {
                Id = Guid.NewGuid(),
                Code = typeCode.Length > 80 ? typeCode[..80] : typeCode,
                Name = typeName.Length > 180 ? typeName[..180] : typeName,
                Description = "Tipo creado desde registro reportado de toma física.",
                IsActive = true,
                CreatedAt = now,
                CreatedBy = actionBy
            };

            _context.ToolTypes.Add(toolType);
        }

        ToolCategory? category = null;

        var categoryName = NormalizeOptional(request.ToolCategoryName)
            ?? NormalizeOptional(item.CategoryName)
            ?? "Sin categoría";

        var categoryCode = NormalizeCode(categoryName);

        category = await _context.ToolCategories
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                (x.Code == categoryCode || x.Name == categoryName),
                cancellationToken);

        if (category is null && request.CreateMissingCatalogs)
        {
            category = new ToolCategory
            {
                Id = Guid.NewGuid(),
                Code = categoryCode.Length > 80 ? categoryCode[..80] : categoryCode,
                Name = categoryName.Length > 180 ? categoryName[..180] : categoryName,
                Description = "Categoría creada desde registro reportado de toma física.",
                IsActive = true,
                CreatedAt = now,
                CreatedBy = actionBy
            };

            _context.ToolCategories.Add(category);
        }

        ResponsiblePerson? responsible = null;

        if (item.ResponsiblePersonId.HasValue)
        {
            responsible = await _context.ResponsiblePeople
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == item.ResponsiblePersonId.Value, cancellationToken);
        }

        if (responsible is null && item.Participant?.ResponsiblePersonId is not null)
        {
            responsible = await _context.ResponsiblePeople
                .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == item.Participant.ResponsiblePersonId.Value, cancellationToken);
        }

        var physicalStatus = MapReportedPhysicalStatus(item.PhysicalStatus);
        var operationalStatus = MapReportedOperationalStatus(item.OperationalStatus);

        var tool = new ToolAsset
        {
            Id = Guid.NewGuid(),
            InternalCode = internalCode,
            Name = NormalizeOptional(request.Name) ?? item.ReportedName,
            Description = NormalizeOptional(request.Description)
                ?? $"Creado desde toma física {item.PhysicalCount?.CountNumber}. Observación: {item.Observation}",
            Brand = NormalizeOptional(request.Brand) ?? NormalizeOptional(item.Brand),
            Model = NormalizeOptional(request.Model) ?? NormalizeOptional(item.Model),
            SerialNumber = NormalizeOptional(request.SerialNumber) ?? NormalizeOptional(item.SerialNumber),
            FixedAssetCode = NormalizeOptional(request.FixedAssetCode),
            FenixCode = NormalizeOptional(request.FenixCode),
            UnitOfMeasure = NormalizeOptional(request.UnitOfMeasure) ?? "UND",
            Quantity = request.Quantity <= 0 ? 1 : request.Quantity,
            ZoneId = branch.ZoneId,
            BranchId = branch.Id,
            LocationId = location?.Id,
            ResponsiblePersonId = null,
            ToolTypeId = toolType?.Id,
            ToolCategoryId = category?.Id,
            OperationalStatus = operationalStatus,
            PhysicalStatus = physicalStatus,
            CustodyStatus = ToolCustodyStatus.InWarehouse,
            ReconciliationStatus = ToolReconciliationStatus.Pending,
            SyncStatus = ToolSyncStatus.NotSynced,
            IsSpecialized = request.IsSpecialized,
            RequiresMaintenance = physicalStatus is ToolPhysicalStatus.Damaged,
            RequiresPreOperationalCheck = request.RequiresPreOperationalCheck,
            RequiresCertification = request.RequiresCertification,
            CreatedAt = now,
            CreatedBy = actionBy
        };

        _context.ToolAssets.Add(tool);

        item.CreatedToolAssetId = tool.Id;
        item.MatchedToolAssetId = tool.Id;
        item.ReconciliationStatus = "Reconciled";
        item.ReconciliationObservation = NormalizeOptional(request.Observation)
            ?? $"Activo creado en Inventario de AF con código {tool.InternalCode}.";
        item.ReconciledAt = now;
        item.ReconciledBy = actionBy;
        item.RequiresUserClarification = false;
        item.UpdatedAt = now;
        item.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Activo creado correctamente en Inventario de AF desde toma física.",
            ReportedItemId = item.Id,
            ToolAssetId = tool.Id,
            tool.InternalCode,
            tool.Name,
            tool.BranchId,
            BranchCode = branch.Code,
            tool.LocationId,
            LocationCode = location?.Code,
            tool.ResponsiblePersonId,
            ResponsibleName = responsible?.FullName,
            ToolTypeId = tool.ToolTypeId,
            ToolTypeName = toolType?.Name,
            ToolCategoryId = tool.ToolCategoryId,
            ToolCategoryName = category?.Name,
            item.ReconciliationStatus,
            item.ReconciledAt,
            item.ReconciledBy
        });
    }

    private static ToolPhysicalStatus MapReportedPhysicalStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ToolPhysicalStatus.Good;
        }

        if (value.Contains("dañ", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("dan", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("malo", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("poor", StringComparison.OrdinalIgnoreCase))
        {
            return ToolPhysicalStatus.Damaged;
        }

        if (value.Contains("regular", StringComparison.OrdinalIgnoreCase))
        {
            return ToolPhysicalStatus.Regular;
        }

        if (value.Contains("incompleto", StringComparison.OrdinalIgnoreCase))
        {
            return ToolPhysicalStatus.Incomplete;
        }

        return ToolPhysicalStatus.Good;
    }

    private static ToolOperationalStatus MapReportedOperationalStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ToolOperationalStatus.PendingValidation;
        }

        if (value.Contains("no operativo", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("no funciona", StringComparison.OrdinalIgnoreCase))
        {
            return ToolOperationalStatus.NotSuitable;
        }

        if (value.Contains("operativo", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("disponible", StringComparison.OrdinalIgnoreCase))
        {
            return ToolOperationalStatus.Available;
        }

        return ToolOperationalStatus.PendingValidation;
    }

    public sealed class CreateToolAssetFromReportedItemRequest
    {
        public string? InternalCode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? BranchCode { get; set; }
        public string? LocationCode { get; set; }
        public string? ToolTypeName { get; set; }
        public string? ToolCategoryName { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? FixedAssetCode { get; set; }
        public string? FenixCode { get; set; }
        public string? UnitOfMeasure { get; set; } = "UND";
        public decimal Quantity { get; set; } = 1;
        public bool IsSpecialized { get; set; }
        public bool RequiresPreOperationalCheck { get; set; }
        public bool RequiresCertification { get; set; }
        public bool CreateMissingCatalogs { get; set; } = true;
        public string? Observation { get; set; }
        public string? ActionBy { get; set; }
    }

    [HttpPost("{id:guid}/migrate-extra-items-to-reported-items")]
    public async Task<IActionResult> MigrateExtraItemsToReportedItems(Guid id, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);
        await EnsurePhysicalCountExtraItemsSchemaAsync(cancellationToken);

        var count = await _context.PhysicalCounts
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (count is null)
        {
            return NotFound(new { Message = $"No se encontró la toma física con Id {id}." });
        }

        var extras = await _context.Set<PhysicalCountExtraItem>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PhysicalCountId == id)
            .ToListAsync(cancellationToken);

        if (!extras.Any())
        {
            return Ok(new
            {
                PhysicalCountId = id,
                Migrated = 0,
                Message = "No hay herramientas no listadas antiguas para migrar."
            });
        }

        var existingReported = await _context.Set<PhysicalCountReportedItem>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.PhysicalCountId == id)
            .Select(x => new
            {
                x.PhysicalCountParticipantId,
                x.ReportedCode,
                x.ReportedName,
                x.SerialNumber
            })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var migrated = 0;
        var skipped = 0;

        foreach (var extra in extras)
        {
            var alreadyExists = existingReported.Any(x =>
                x.PhysicalCountParticipantId == extra.PhysicalCountParticipantId &&
                string.Equals(x.ReportedCode ?? string.Empty, extra.ReportedCode ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.ReportedName ?? string.Empty, extra.ReportedName ?? string.Empty, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.SerialNumber ?? string.Empty, extra.ReportedSerial ?? string.Empty, StringComparison.OrdinalIgnoreCase));

            if (alreadyExists)
            {
                skipped++;
                continue;
            }

            var minimum = new MinimumDataValidationResult(
                !string.IsNullOrWhiteSpace(extra.ReportedName) &&
                !string.IsNullOrWhiteSpace(extra.FoundLocation) &&
                !string.IsNullOrWhiteSpace(extra.PhysicalStatus) &&
                !string.IsNullOrWhiteSpace(extra.Observation),
                null);

            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(extra.ReportedName))
            {
                missing.Add("Nombre del activo/herramienta");
            }

            if (string.IsNullOrWhiteSpace(extra.FoundLocation))
            {
                missing.Add("Ubicación encontrada");
            }

            if (string.IsNullOrWhiteSpace(extra.PhysicalStatus))
            {
                missing.Add("Estado físico");
            }

            if (string.IsNullOrWhiteSpace(extra.Observation))
            {
                missing.Add("Observación");
            }

            var reported = new PhysicalCountReportedItem
            {
                Id = Guid.NewGuid(),
                PhysicalCountId = extra.PhysicalCountId,
                PhysicalCountParticipantId = extra.PhysicalCountParticipantId,
                ReportType = "ExtraNotListed",
                MatchedToolAssetId = extra.MatchedToolAssetId,
                ReportedCode = extra.ReportedCode,
                ReportedName = string.IsNullOrWhiteSpace(extra.ReportedName) ? "Herramienta no listada" : extra.ReportedName,
                SerialNumber = extra.ReportedSerial,
                Brand = extra.ReportedBrand,
                Model = extra.ReportedModel,
                AssetTypeName = "Herramienta",
                CategoryName = "Por clasificar",
                BranchId = count.BranchId,
                BranchCode = count.Branch?.Code,
                FoundLocation = extra.FoundLocation,
                PhysicalStatus = extra.PhysicalStatus,
                Observation = extra.Observation,
                ReportedBy = extra.ReportedBy,
                ReportedAt = extra.ReportedAt,
                ReconciliationStatus = extra.ReconciliationStatus == "ApprovedForCreation"
                    ? "ApprovedForCreation"
                    : extra.ReconciliationStatus,
                ReconciliationObservation = extra.ReconciliationObservation,
                RequiresUserClarification = extra.RequiresUserClarification,
                ClarificationRequestedAt = extra.ClarificationRequestedAt,
                ClarificationRequestedBy = extra.ClarificationRequestedBy,
                MinimumDataCompleted = missing.Count == 0,
                MissingRequiredData = missing.Count == 0 ? null : string.Join(", ", missing),
                ApprovedForCreation = extra.ApprovedForCreation || extra.ReconciliationStatus == "ApprovedForCreation",
                ApprovedForCreationAt = extra.ApprovedForCreationAt,
                ApprovedForCreationBy = extra.ApprovedForCreationBy,
                Rejected = extra.Rejected,
                RejectedAt = extra.RejectedAt,
                RejectedBy = extra.RejectedBy,
                RejectionReason = extra.RejectionReason,
                ReconciledAt = extra.ReconciledAt,
                ReconciledBy = extra.ReconciledBy,
                SuggestedAction = extra.SuggestedAction ?? "Validar si debe crearse en Inventario de AF",
                CreatedAt = now,
                CreatedBy = "migration"
            };

            _context.Set<PhysicalCountReportedItem>().Add(reported);
            migrated++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            PhysicalCountId = id,
            count.CountNumber,
            TotalExtraItems = extras.Count,
            Migrated = migrated,
            Skipped = skipped,
            Message = "Migración de herramientas no listadas a registros reportados finalizada."
        });
    }

    [HttpPatch("reported-items/{reportedItemId:guid}/resolve-clarification")]
    public async Task<IActionResult> ResolveReportedItemClarification(Guid reportedItemId, [FromBody] ReportedItemActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var item = await _context.Set<PhysicalCountReportedItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == reportedItemId, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = $"No se encontró el registro reportado {reportedItemId}." });
        }

        if (item.Rejected || item.ReconciliationStatus == "Rejected")
        {
            return BadRequest(new { Message = "El registro está rechazado y no puede marcarse como aclarado." });
        }

        var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        item.RequiresUserClarification = false;
        item.ReconciliationStatus = item.ReportType == "ExtraNotListed" && item.MatchedToolAssetId is null
            ? "NotFoundInSystem"
            : "PendingReview";

        item.ReconciliationObservation = NormalizeOptional(request.Observation)
            ?? "Aclaración recibida. Registro disponible nuevamente para revisión.";
        item.UpdatedAt = now;
        item.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            item.Id,
            item.RequiresUserClarification,
            item.ReconciliationStatus,
            ReconciliationStatusLabel = GetReportedItemReconciliationStatusLabel(item.ReconciliationStatus),
            item.ReconciliationObservation
        });
    }

    [HttpPatch("reported-items/{reportedItemId:guid}/void-created-tool-asset")]
    public async Task<IActionResult> VoidCreatedToolAssetFromReportedItem(Guid reportedItemId, [FromBody] ReportedItemActionRequest request, CancellationToken cancellationToken)
    {
        await EnsurePhysicalCountReportedItemsSchemaAsync(cancellationToken);

        var item = await _context.Set<PhysicalCountReportedItem>()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.Id == reportedItemId, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = $"No se encontró el registro reportado {reportedItemId}." });
        }

        if (!item.CreatedToolAssetId.HasValue || item.CreatedToolAssetId.Value == Guid.Empty)
        {
            return BadRequest(new { Message = "Este registro no tiene un activo creado en Inventario de AF para anular." });
        }

        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == item.CreatedToolAssetId.Value, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró el activo creado en Inventario de AF." });
        }

        var actionBy = NormalizeOptional(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;
        var observation = NormalizeOptional(request.Observation)
            ?? "Activo anulado desde conciliación de toma física porque el reporte no procede.";

        tool.IsDeleted = true;
        tool.UpdatedAt = now;
        tool.UpdatedBy = actionBy;

        tool.Description = string.IsNullOrWhiteSpace(tool.Description)
            ? $"ANULADO DESDE TOMA FÍSICA: {observation}"
            : $"{tool.Description} | ANULADO DESDE TOMA FÍSICA: {observation}";

        item.Rejected = true;
        item.RejectedAt = now;
        item.RejectedBy = actionBy;
        item.RejectionReason = observation;
        item.ReconciliationStatus = "Rejected";
        item.ReconciliationObservation = observation;
        item.RequiresUserClarification = false;
        item.UpdatedAt = now;
        item.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Activo creado desde toma física anulado correctamente. Se conserva trazabilidad en el registro reportado.",
            ReportedItemId = item.Id,
            ToolAssetId = tool.Id,
            tool.InternalCode,
            tool.Name,
            item.ReconciliationStatus,
            item.RejectedAt,
            item.RejectedBy,
            item.RejectionReason
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
        public bool ForceClose { get; set; }
}




















public sealed class ParticipantAssignedToolReportRequest
{
    public Guid ToolAssetId { get; set; }
    public string ReportAction { get; set; } = "Confirmed";
    public string? PhysicalStatus { get; set; }
    public string? FoundLocation { get; set; }
    public string? Observation { get; set; }
    public string? ActionBy { get; set; }
}

