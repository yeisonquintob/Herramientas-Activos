using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Loans;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public LoansController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetLoans()
    {
        var loans = await _context.ToolLoans
            .Include(x => x.Items)
                .ThenInclude(x => x.ToolAsset)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.LoanNumber,
                Status = x.Status.ToString(),
                x.BranchId,
                x.RequestedByPersonId,
                x.RequestedAt,
                x.ApprovedAt,
                x.DeliveredAt,
                x.ExpectedReturnAt,
                x.ReturnedAt,
                x.Notes,
                x.CreatedAt,
                x.CreatedBy,
                Items = x.Items.Select(i => new
                {
                    i.Id,
                    i.ToolAssetId,
                    ToolInternalCode = i.ToolAsset == null ? null : i.ToolAsset.InternalCode,
                    ToolName = i.ToolAsset == null ? null : i.ToolAsset.Name,
                    OperationalStatus = i.ToolAsset == null ? null : i.ToolAsset.OperationalStatus.ToString(),
                    i.Quantity,
                    i.DeliveryCondition,
                    i.ReturnCondition,
                    i.Returned
                })
            })
            .ToListAsync();

        return Ok(loans);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLoanById(Guid id)
    {
        var loan = await _context.ToolLoans
            .Include(x => x.Items)
                .ThenInclude(x => x.ToolAsset)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.LoanNumber,
                Status = x.Status.ToString(),
                x.BranchId,
                x.RequestedByPersonId,
                x.RequestedAt,
                x.ApprovedAt,
                x.DeliveredAt,
                x.ExpectedReturnAt,
                x.ReturnedAt,
                x.Notes,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy,
                Items = x.Items.Select(i => new
                {
                    i.Id,
                    i.ToolAssetId,
                    ToolInternalCode = i.ToolAsset == null ? null : i.ToolAsset.InternalCode,
                    ToolName = i.ToolAsset == null ? null : i.ToolAsset.Name,
                    OperationalStatus = i.ToolAsset == null ? null : i.ToolAsset.OperationalStatus.ToString(),
                    i.Quantity,
                    i.DeliveryCondition,
                    i.ReturnCondition,
                    i.Returned
                })
            })
            .FirstOrDefaultAsync();

        if (loan is null)
        {
            return NotFound(new { Message = $"No se encontró el préstamo con Id {id}." });
        }

        return Ok(loan);
    }

    [HttpGet("by-tool/{toolId:guid}")]
    public async Task<IActionResult> GetLoansByTool(Guid toolId)
    {
        var loans = await _context.ToolLoans
            .Include(x => x.Items)
                .ThenInclude(x => x.ToolAsset)
            .Where(x => x.Items.Any(i => i.ToolAssetId == toolId))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.LoanNumber,
                Status = x.Status.ToString(),
                x.RequestedAt,
                x.ApprovedAt,
                x.DeliveredAt,
                x.ReturnedAt,
                x.ExpectedReturnAt,
                x.Notes,
                Items = x.Items.Select(i => new
                {
                    i.Id,
                    i.ToolAssetId,
                    ToolInternalCode = i.ToolAsset == null ? null : i.ToolAsset.InternalCode,
                    ToolName = i.ToolAsset == null ? null : i.ToolAsset.Name,
                    i.Returned
                })
            })
            .ToListAsync();

        return Ok(loans);
    }

    [HttpGet("by-code/{internalCode}")]
    public async Task<IActionResult> GetLoansByToolInternalCode(string internalCode)
    {
        var loans = await _context.ToolLoans
            .Include(x => x.Items)
                .ThenInclude(x => x.ToolAsset)
            .Where(x => x.Items.Any(i => i.ToolAsset != null && i.ToolAsset.InternalCode == internalCode))
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.LoanNumber,
                Status = x.Status.ToString(),
                x.RequestedAt,
                x.ApprovedAt,
                x.DeliveredAt,
                x.ReturnedAt,
                x.ExpectedReturnAt,
                x.Notes,
                Items = x.Items.Select(i => new
                {
                    i.Id,
                    i.ToolAssetId,
                    ToolInternalCode = i.ToolAsset == null ? null : i.ToolAsset.InternalCode,
                    ToolName = i.ToolAsset == null ? null : i.ToolAsset.Name,
                    i.Returned
                })
            })
            .ToListAsync();

        return Ok(loans);
    }

    [HttpPost("request")]
    public async Task<IActionResult> RequestLoan([FromBody] RequestToolLoanRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.BranchCode))
        {
            return BadRequest(new { Message = "La sede es obligatoria." });
        }

        if (request.ToolInternalCodes is null || request.ToolInternalCodes.Count == 0)
        {
            return BadRequest(new { Message = "Debe indicar al menos una herramienta para el préstamo." });
        }

        var branchCode = request.BranchCode.Trim().ToUpperInvariant();

        var branch = await _context.Branches
            .FirstOrDefaultAsync(x => x.Code == branchCode);

        if (branch is null)
        {
            return BadRequest(new { Message = $"No existe la sede {branchCode}." });
        }

        var codes = request.ToolInternalCodes
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToUpperInvariant())
            .Distinct()
            .ToList();

        var tools = await _context.ToolAssets
            .Where(x => codes.Contains(x.InternalCode))
            .ToListAsync();

        var foundCodes = tools.Select(x => x.InternalCode).ToHashSet();
        var missingCodes = codes.Where(x => !foundCodes.Contains(x)).ToList();

        if (missingCodes.Count > 0)
        {
            return BadRequest(new
            {
                Message = "Hay herramientas que no existen.",
                MissingCodes = missingCodes
            });
        }

        var blockedTools = tools
            .Where(x => IsBlockedForLoan(x.OperationalStatus))
            .Select(x => new
            {
                x.InternalCode,
                x.Name,
                OperationalStatus = x.OperationalStatus.ToString(),
                OperationalStatusLabel = GetOperationalStatusLabel(x.OperationalStatus)
            })
            .ToList();

        if (blockedTools.Count > 0)
        {
            return BadRequest(new
            {
                Message = "Una o más herramientas no se pueden prestar por su estado actual.",
                Tools = blockedTools
            });
        }

        var createdBy = string.IsNullOrWhiteSpace(request.RequestedBy)
            ? "api"
            : request.RequestedBy.Trim();

        var loan = new ToolLoan
        {
            LoanNumber = $"PRE-{DateTime.UtcNow:yyyyMMddHHmmss}",
            BranchId = branch.Id,
            Status = GetPreferredLoanStatus(ToolLoanStatus.Draft, "Requested", "PendingApproval", "Draft"),
            RequestedAt = DateTime.UtcNow,
            ExpectedReturnAt = request.ExpectedReturnAt,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        foreach (var tool in tools)
        {
            loan.Items.Add(new ToolLoanItem
            {
                ToolAssetId = tool.Id,
                Quantity = 1,
                DeliveryCondition = request.DeliveryCondition,
                Returned = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            });

            AddToolLifeCycleEvent(
                tool.Id,
                "LoanRequested",
                "Solicitud de préstamo",
                $"Se solicitó el préstamo de la herramienta en la sede {branchCode}.",
                tool.OperationalStatus.ToString(),
                tool.OperationalStatus.ToString(),
                createdBy);
        }

        _context.ToolLoans.Add(loan);

        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetLoanById), new { id = loan.Id }, new
        {
            loan.Id,
            loan.LoanNumber,
            Status = loan.Status.ToString(),
            loan.RequestedAt,
            loan.ExpectedReturnAt
        });
    }

    [HttpPatch("{id:guid}/approve")]
    public async Task<IActionResult> ApproveLoan(Guid id, [FromBody] LoanActionRequest request)
    {
        var loan = await _context.ToolLoans
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (loan is null)
        {
            return NotFound(new { Message = $"No se encontró el préstamo con Id {id}." });
        }

        if (!LoanStatusIs(loan, "Draft", "Requested", "PendingApproval"))
        {
            return BadRequest(new { Message = $"El préstamo no se puede aprobar porque está en estado {loan.Status}." });
        }

        var changedBy = GetActionUser(request);

        var previousStatus = loan.Status.ToString();

        loan.Status = GetPreferredLoanStatus(loan.Status, "Approved");
        loan.ApprovedAt = DateTime.UtcNow;
        loan.UpdatedAt = DateTime.UtcNow;
        loan.UpdatedBy = changedBy;

        foreach (var item in loan.Items)
        {
            AddToolLifeCycleEvent(
                item.ToolAssetId,
                "LoanApproved",
                "Préstamo aprobado",
                string.IsNullOrWhiteSpace(request.Notes)
                    ? $"Se aprobó el préstamo {loan.LoanNumber}."
                    : request.Notes.Trim(),
                previousStatus,
                loan.Status.ToString(),
                changedBy);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            loan.Id,
            loan.LoanNumber,
            Status = loan.Status.ToString(),
            loan.ApprovedAt,
            loan.UpdatedBy
        });
    }

    [HttpPatch("{id:guid}/deliver")]
    public async Task<IActionResult> DeliverLoan(Guid id, [FromBody] LoanActionRequest request)
    {
        var loan = await _context.ToolLoans
            .Include(x => x.Items)
                .ThenInclude(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (loan is null)
        {
            return NotFound(new { Message = $"No se encontró el préstamo con Id {id}." });
        }

        if (!LoanStatusIs(loan, "Approved"))
        {
            return BadRequest(new { Message = $"El préstamo debe estar aprobado para poder entregarse. Estado actual: {loan.Status}." });
        }

        var blockedTools = loan.Items
            .Where(x => x.ToolAsset != null && IsBlockedForLoan(x.ToolAsset.OperationalStatus))
            .Select(x => new
            {
                x.ToolAsset!.InternalCode,
                x.ToolAsset.Name,
                OperationalStatus = x.ToolAsset.OperationalStatus.ToString(),
                OperationalStatusLabel = GetOperationalStatusLabel(x.ToolAsset.OperationalStatus)
            })
            .ToList();

        if (blockedTools.Count > 0)
        {
            return BadRequest(new
            {
                Message = "Una o más herramientas no se pueden entregar por su estado actual.",
                Tools = blockedTools
            });
        }

        var changedBy = GetActionUser(request);
        var previousLoanStatus = loan.Status.ToString();

        loan.Status = GetPreferredLoanStatus(loan.Status, "Delivered");
        loan.DeliveredAt = DateTime.UtcNow;
        loan.UpdatedAt = DateTime.UtcNow;
        loan.UpdatedBy = changedBy;

        foreach (var item in loan.Items)
        {
            if (item.ToolAsset is null)
            {
                continue;
            }

            var previousToolStatus = item.ToolAsset.OperationalStatus;

            item.ToolAsset.OperationalStatus = ToolOperationalStatus.Loaned;
            item.ToolAsset.UpdatedAt = DateTime.UtcNow;
            item.ToolAsset.UpdatedBy = changedBy;
            item.DeliveryCondition = string.IsNullOrWhiteSpace(request.Condition)
                ? item.DeliveryCondition
                : request.Condition.Trim();

            AddToolLifeCycleEvent(
                item.ToolAssetId,
                "LoanDelivered",
                "Herramienta entregada en préstamo",
                string.IsNullOrWhiteSpace(request.Notes)
                    ? $"Se entregó la herramienta en el préstamo {loan.LoanNumber}."
                    : request.Notes.Trim(),
                previousToolStatus.ToString(),
                ToolOperationalStatus.Loaned.ToString(),
                changedBy);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            loan.Id,
            loan.LoanNumber,
            PreviousStatus = previousLoanStatus,
            Status = loan.Status.ToString(),
            loan.DeliveredAt,
            loan.UpdatedBy
        });
    }

    [HttpPatch("{id:guid}/return")]
    public async Task<IActionResult> ReturnLoan(Guid id, [FromBody] LoanActionRequest request)
    {
        var loan = await _context.ToolLoans
            .Include(x => x.Items)
                .ThenInclude(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (loan is null)
        {
            return NotFound(new { Message = $"No se encontró el préstamo con Id {id}." });
        }

        if (!LoanStatusIs(loan, "Delivered"))
        {
            return BadRequest(new { Message = $"El préstamo debe estar entregado para poder devolverlo. Estado actual: {loan.Status}." });
        }

        var changedBy = GetActionUser(request);
        var previousLoanStatus = loan.Status.ToString();

        loan.Status = GetPreferredLoanStatus(loan.Status, "Returned");
        loan.ReturnedAt = DateTime.UtcNow;
        loan.UpdatedAt = DateTime.UtcNow;
        loan.UpdatedBy = changedBy;

        foreach (var item in loan.Items)
        {
            if (item.ToolAsset is null)
            {
                continue;
            }

            var previousToolStatus = item.ToolAsset.OperationalStatus;

            item.Returned = true;
            item.ReturnCondition = string.IsNullOrWhiteSpace(request.Condition)
                ? item.ReturnCondition
                : request.Condition.Trim();

            item.ToolAsset.OperationalStatus = ToolOperationalStatus.Available;
            item.ToolAsset.UpdatedAt = DateTime.UtcNow;
            item.ToolAsset.UpdatedBy = changedBy;

            AddToolLifeCycleEvent(
                item.ToolAssetId,
                "LoanReturned",
                "Herramienta devuelta",
                string.IsNullOrWhiteSpace(request.Notes)
                    ? $"Se devolvió la herramienta del préstamo {loan.LoanNumber}."
                    : request.Notes.Trim(),
                previousToolStatus.ToString(),
                ToolOperationalStatus.Available.ToString(),
                changedBy);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            loan.Id,
            loan.LoanNumber,
            PreviousStatus = previousLoanStatus,
            Status = loan.Status.ToString(),
            loan.ReturnedAt,
            loan.UpdatedBy
        });
    }

    private static bool LoanStatusIs(ToolLoan loan, params string[] statusNames)
    {
        return statusNames.Any(x =>
            string.Equals(loan.Status.ToString(), x, StringComparison.OrdinalIgnoreCase));
    }

    private static ToolLoanStatus GetPreferredLoanStatus(ToolLoanStatus fallback, params string[] preferredNames)
    {
        foreach (var status in Enum.GetValues<ToolLoanStatus>())
        {
            foreach (var preferredName in preferredNames)
            {
                if (string.Equals(status.ToString(), preferredName, StringComparison.OrdinalIgnoreCase))
                {
                    return status;
                }
            }
        }

        return fallback;
    }

    private static bool IsBlockedForLoan(ToolOperationalStatus status)
    {
        return status is ToolOperationalStatus.Damaged
            or ToolOperationalStatus.NotSuitable
            or ToolOperationalStatus.InMaintenance
            or ToolOperationalStatus.PendingDisposal
            or ToolOperationalStatus.Disposed
            or ToolOperationalStatus.NotLocated
            or ToolOperationalStatus.Inconsistent;
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

    private static string GetActionUser(LoanActionRequest request)
    {
        return string.IsNullOrWhiteSpace(request.ActionBy)
            ? "api"
            : request.ActionBy.Trim();
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

public sealed class RequestToolLoanRequest
{
    public string BranchCode { get; set; } = string.Empty;

    public List<string> ToolInternalCodes { get; set; } = new();

    public DateTime? ExpectedReturnAt { get; set; }

    public string? DeliveryCondition { get; set; }

    public string? Notes { get; set; }

    public string? RequestedBy { get; set; }
}

public sealed class LoanActionRequest
{
    public string? ActionBy { get; set; }

    public string? Notes { get; set; }

    public string? Condition { get; set; }
}

