using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Navi.ToolsAssets.Api.Security;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Maintenance;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;
using Navi.ToolsAssets.Domain.Enums;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/maintenance-requests")]
public sealed class MaintenanceRequestsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public MaintenanceRequestsController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }
    [RequirePermission("Maintenance.View")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var query = _context.MaintenanceRequests
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Include(x => x.Branch)
            .Where(x => !x.IsDeleted);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim().ToLower();

            query = query.Where(x =>
                x.RequestNumber.ToLower().Contains(value) ||
                x.Title.ToLower().Contains(value) ||
                x.ProblemDescription.ToLower().Contains(value) ||
                x.PreparedBy.ToLower().Contains(value));
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(300)
            .Select(x => new MaintenanceRequestDto
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                ToolAssetId = x.ToolAssetId,
                ToolInternalCode = x.ToolAsset != null ? x.ToolAsset.InternalCode : null,
                ToolName = x.ToolAsset != null ? x.ToolAsset.Name : null,
                BranchId = x.BranchId,
                BranchCode = x.Branch != null ? x.Branch.Code : null,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                RequestType = x.RequestType,
                Priority = x.Priority,
                Status = x.Status,
                Title = x.Title,
                ProblemDescription = x.ProblemDescription,
                WorkDescription = x.WorkDescription,
                FailureCause = x.FailureCause,
                RequestedByUserName = x.RequestedByUserName,
                RequestedByResponsiblePersonName = x.RequestedByResponsiblePersonName,
                PreparedBy = x.PreparedBy,
                RequestedAt = x.RequestedAt,
                SubmittedAt = x.SubmittedAt,
                SubmittedBy = x.SubmittedBy,
                ApprovedAt = x.ApprovedAt,
                ApprovedBy = x.ApprovedBy,
                ApprovalComment = x.ApprovalComment,
                RejectedAt = x.RejectedAt,
                RejectedBy = x.RejectedBy,
                RejectionReason = x.RejectionReason,
                ScheduledAt = x.ScheduledAt,
                ScheduledBy = x.ScheduledBy,
                AssignedTechnician = x.AssignedTechnician,
                ExecutionStartedAt = x.ExecutionStartedAt,
                ExecutionStartedBy = x.ExecutionStartedBy,
                ExecutionFinishedAt = x.ExecutionFinishedAt,
                ExecutionFinishedBy = x.ExecutionFinishedBy,
                ClosedAt = x.ClosedAt,
                ClosedBy = x.ClosedBy,
                ClosingComment = x.ClosingComment,
                CanceledAt = x.CanceledAt,
                CanceledBy = x.CanceledBy,
                CancellationReason = x.CancellationReason,
                RequiresStop = x.RequiresStop,
                IsSafetyRisk = x.IsSafetyRisk,
                EstimatedCostText = x.EstimatedCostText,
                VendorSuggestion = x.VendorSuggestion,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }
    [RequirePermission("Maintenance.View")]
    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetMaintenanceDashboardSummary(CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var requests = await _context.MaintenanceRequests
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .ToListAsync(cancellationToken);

        var records = await _context.MaintenanceRecords
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => new
            {
                x.ToolAssetId,
                x.MaintenanceNumber,
                x.Status,
                x.ScheduledAt,
                x.FinishedAt,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var tools = await _context.ToolAssets
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.InternalCode,
                x.Name,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        const int defaultPeriodMonths = 6;

        var overduePlans = 0;
        var dueSoonPlans = 0;
        var onTimePlans = 0;

        foreach (var tool in tools)
        {
            var toolRecords = records
                .Where(x => x.ToolAssetId == tool.Id)
                .ToList();

            var lastDate = tool.CreatedAt;

            foreach (var record in toolRecords)
            {
                var recordDate = record.FinishedAt.HasValue
                    ? record.FinishedAt.Value
                    : record.ScheduledAt;

                if (recordDate > lastDate)
                {
                    lastDate = recordDate;
                }
            }

            var nextDate = lastDate.AddMonths(defaultPeriodMonths);
            var daysRemaining = (nextDate.Date - now.Date).Days;

            if (daysRemaining < 0)
            {
                overduePlans++;
            }
            else if (daysRemaining <= 30)
            {
                dueSoonPlans++;
            }
            else
            {
                onTimePlans++;
            }
        }

        var openStatuses = new[] { "Draft", "InReview", "Approved", "Scheduled", "InExecution" };

        var summary = new
        {
            TotalRequests = requests.Count,
            DraftRequests = requests.Count(x => x.Status == "Draft"),
            InReviewRequests = requests.Count(x => x.Status == "InReview"),
            ApprovedRequests = requests.Count(x => x.Status == "Approved"),
            ScheduledRequests = requests.Count(x => x.Status == "Scheduled"),
            InExecutionRequests = requests.Count(x => x.Status == "InExecution"),
            ClosedRequests = requests.Count(x => x.Status == "Closed"),
            RejectedRequests = requests.Count(x => x.Status == "Rejected"),
            CanceledRequests = requests.Count(x => x.Status == "Canceled"),
            OpenRequests = requests.Count(x => openStatuses.Contains(x.Status)),
            CriticalRequests = requests.Count(x => x.Priority == "Crítica" || x.IsSafetyRisk),
            LinkedToAssetRequests = requests.Count(x => x.ToolAssetId.HasValue),
            ClosedLinkedToSchedule = requests.Count(x => x.Status == "Closed" && x.ToolAssetId.HasValue),
            TotalMaintenanceRecords = records.Count,
            OverduePlans = overduePlans,
            DueSoonPlans = dueSoonPlans,
            OnTimePlans = onTimePlans,
            GeneratedAt = now
        };

        return Ok(summary);
    }
    [RequirePermission("Maintenance.View")]
    [HttpGet("plans-board")]
    public async Task<IActionResult> GetPlansBoard(CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var tools = await _context.ToolAssets
            .AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Location)
            .Include(x => x.ResponsiblePerson)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.InternalCode)
            .Select(x => new
            {
                x.Id,
                x.InternalCode,
                x.Name,
                BranchCode = x.Branch != null ? x.Branch.Code : null,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                LocationName = x.Location != null ? x.Location.Name : null,
                ResponsibleName = x.ResponsiblePerson != null ? x.ResponsiblePerson.FullName : null,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var maintenanceRecords = await _context.Set<MaintenanceRecord>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.MaintenanceNumber,
                x.Type,
                x.Status,
                x.ScheduledAt,
                x.StartedAt,
                x.FinishedAt,
                x.Technician,
                x.Provider,
                x.Result,
                x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var requests = await _context.MaintenanceRequests
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.ToolAssetId.HasValue)
            .Select(x => new
            {
                x.Id,
                ToolAssetId = x.ToolAssetId!.Value,
                x.RequestNumber,
                x.Status,
                x.Priority,
                x.RequestType,
                x.Title,
                x.CreatedAt,
                x.ScheduledAt,
                x.ClosedAt
            })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        const int defaultPeriodMonths = 6;

        var result = tools.Select(tool =>
        {
            var toolMaintenances = maintenanceRecords
                .Where(x => x.ToolAssetId == tool.Id)
                .OrderByDescending(x => x.FinishedAt ?? x.ScheduledAt)
                .ToList();

            var toolRequests = requests
                .Where(x => x.ToolAssetId == tool.Id)
                .OrderByDescending(x => x.CreatedAt)
                .ToList();

            var lastMaintenance = toolMaintenances
                .Where(x => x.Status == ToolMaintenanceStatus.Completed || x.FinishedAt.HasValue)
                .OrderByDescending(x => x.FinishedAt ?? x.CreatedAt)
                .FirstOrDefault();

            var pendingRequests = toolRequests.Count(x =>
                x.Status is "Draft" or "InReview" or "Approved" or "Scheduled" or "InExecution");

            var completedCount = toolMaintenances.Count(x =>
                x.Status == ToolMaintenanceStatus.Completed || x.FinishedAt.HasValue);

            var scheduledCount = toolMaintenances.Count(x =>
                x.Status == ToolMaintenanceStatus.Scheduled);

            var lastDate = lastMaintenance?.FinishedAt ?? lastMaintenance?.ScheduledAt;
            var baseDate = lastDate ?? tool.CreatedAt;
            var nextDate = baseDate.AddMonths(defaultPeriodMonths);
            var daysRemaining = (nextDate.Date - now.Date).Days;

            var status = daysRemaining < 0
                ? "Overdue"
                : daysRemaining <= 30
                    ? "DueSoon"
                    : "OnTime";

            return new
            {
                ToolAssetId = tool.Id,
                tool.InternalCode,
                tool.Name,
                tool.BranchCode,
                tool.BranchName,
                tool.LocationName,
                tool.ResponsibleName,
                PeriodMonths = defaultPeriodMonths,
                LastMaintenanceAt = lastDate,
                NextMaintenanceAt = nextDate,
                DaysRemaining = daysRemaining,
                PlanStatus = status,
                TotalMaintenances = toolMaintenances.Count(),
                CompletedMaintenances = completedCount,
                ScheduledMaintenances = scheduledCount,
                PendingRequests = pendingRequests,
                LastMaintenanceNumber = lastMaintenance?.MaintenanceNumber,
                LastTechnician = lastMaintenance?.Technician,
                LastProvider = lastMaintenance?.Provider
            };
        })
        .OrderBy(x => x.DaysRemaining)
        .ToList();

        return Ok(result);
    }
    [RequirePermission("Maintenance.Generate")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaintenanceRequestRequest request, CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return BadRequest(new { Message = "Debe ingresar el título de la solicitud." });
        }

        if (string.IsNullOrWhiteSpace(request.ProblemDescription))
        {
            return BadRequest(new { Message = "Debe ingresar la descripción del problema o necesidad." });
        }

        var currentUser = GetUserName();
        var number = await GenerateRequestNumberAsync(cancellationToken);

        var item = new ToolMaintenanceRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = number,
            ToolAssetId = request.ToolAssetId,
            BranchId = request.BranchId ?? GetBranchId(),
            RequestType = string.IsNullOrWhiteSpace(request.RequestType) ? "Correctivo" : request.RequestType.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Media" : request.Priority.Trim(),
            Status = request.SendToReview ? "InReview" : "Draft",
            Title = request.Title.Trim(),
            ProblemDescription = request.ProblemDescription.Trim(),
            WorkDescription = request.WorkDescription?.Trim(),
            FailureCause = request.FailureCause?.Trim(),
            RequestedByUserId = GetUserId(),
            RequestedByUserName = currentUser,
            RequestedByResponsiblePersonId = GetResponsiblePersonId(),
            RequestedByResponsiblePersonName = GetResponsiblePersonName(),
            PreparedBy = currentUser,
            RequestedAt = DateTime.UtcNow,
            SubmittedAt = request.SendToReview ? DateTime.UtcNow : null,
            SubmittedBy = request.SendToReview ? currentUser : null,
            RequiresStop = request.RequiresStop,
            IsSafetyRisk = request.IsSafetyRisk,
            EstimatedCostText = request.EstimatedCostText?.Trim(),
            VendorSuggestion = request.VendorSuggestion?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser
        };

        _context.MaintenanceRequests.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = request.SendToReview
                ? "Solicitud de mantenimiento creada y enviada a revisión."
                : "Solicitud de mantenimiento creada en borrador.",
            item.Id,
            item.RequestNumber,
            item.Status
        });
    }
    [RequirePermission("Maintenance.Reject")]
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var item = await FindAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de mantenimiento." });
        }

        if (item.Status is not "Draft" and not "Rejected")
        {
            return BadRequest(new { Message = "Solo se pueden enviar solicitudes en borrador o rechazadas." });
        }

        item.Status = "InReview";
        item.SubmittedAt = DateTime.UtcNow;
        item.SubmittedBy = GetUserName();
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = GetUserName();

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Solicitud enviada a revisión.", item.Id, item.RequestNumber, item.Status });
    }
    [RequirePermission("Maintenance.Close")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] MaintenanceRequestActionCommand request, CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var item = await FindAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de mantenimiento." });
        }

        if (item.Status != "InReview")
        {
            return BadRequest(new { Message = "Solo se pueden aprobar solicitudes en revisión." });
        }

        item.Status = "Approved";
        item.ApprovedAt = DateTime.UtcNow;
        item.ApprovedBy = GetUserName();
        item.ApprovalComment = request.Comment?.Trim();
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = GetUserName();

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Solicitud de mantenimiento aprobada.", item.Id, item.RequestNumber, item.Status });
    }
    [RequirePermission("Maintenance.Reject")]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] MaintenanceRequestActionCommand request, CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var item = await FindAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de mantenimiento." });
        }

        if (item.Status != "InReview")
        {
            return BadRequest(new { Message = "Solo se pueden rechazar solicitudes en revisión." });
        }

        item.Status = "Rejected";
        item.RejectedAt = DateTime.UtcNow;
        item.RejectedBy = GetUserName();
        item.RejectionReason = string.IsNullOrWhiteSpace(request.Comment)
            ? "Solicitud rechazada."
            : request.Comment.Trim();
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = GetUserName();

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Solicitud rechazada.", item.Id, item.RequestNumber, item.Status });
    }
    [RequirePermission("Maintenance.Close")]
    [HttpPost("{id:guid}/schedule")]
    public async Task<IActionResult> Schedule(Guid id, [FromBody] MaintenanceRequestActionCommand request, CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var item = await FindAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de mantenimiento." });
        }

        if (item.Status != "Approved")
        {
            return BadRequest(new { Message = "Solo se pueden programar solicitudes aprobadas." });
        }

        item.Status = "Scheduled";
        item.ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow;
        item.ScheduledBy = GetUserName();
        item.AssignedTechnician = request.AssignedTechnician?.Trim();
        item.Notes = string.IsNullOrWhiteSpace(request.Comment) ? item.Notes : request.Comment.Trim();
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = GetUserName();

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Mantenimiento programado.", item.Id, item.RequestNumber, item.Status });
    }
    [RequirePermission("Maintenance.Execute")]
    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> StartExecution(Guid id, [FromBody] MaintenanceRequestActionCommand request, CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var item = await FindAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de mantenimiento." });
        }

        if (item.Status is not "Approved" and not "Scheduled")
        {
            return BadRequest(new { Message = "Solo se puede iniciar ejecución de solicitudes aprobadas o programadas." });
        }

        item.Status = "InExecution";
        item.ExecutionStartedAt = DateTime.UtcNow;
        item.ExecutionStartedBy = GetUserName();
        item.WorkDescription = string.IsNullOrWhiteSpace(request.Comment) ? item.WorkDescription : request.Comment.Trim();
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = GetUserName();

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Ejecución de mantenimiento iniciada.", item.Id, item.RequestNumber, item.Status });
    }
    [RequirePermission("Maintenance.Execute")]
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] MaintenanceRequestActionCommand request, CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var item = await _context.MaintenanceRequests
            .Include(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de mantenimiento." });
        }

        if (item.Status is not "Approved" and not "Scheduled" and not "InExecution")
        {
            return BadRequest(new { Message = "Solo se pueden cerrar solicitudes aprobadas, programadas o en ejecución." });
        }

        var user = GetUserName();
        var now = DateTime.UtcNow;

        item.Status = "Closed";
        item.ExecutionFinishedAt = now;
        item.ExecutionFinishedBy = user;
        item.ClosedAt = now;
        item.ClosedBy = user;
        item.ClosingComment = string.IsNullOrWhiteSpace(request.Comment)
            ? "Mantenimiento cerrado desde NAVI."
            : request.Comment.Trim();
        item.UpdatedAt = now;
        item.UpdatedBy = user;

        MaintenanceRecord? maintenanceRecord = null;
        var scheduleCreated = false;

        if (item.ToolAssetId.HasValue)
        {
            var maintenanceNumber = $"MTTO-{item.RequestNumber}";

            maintenanceRecord = await _context.Set<MaintenanceRecord>()
                .FirstOrDefaultAsync(x =>
                    x.ToolAssetId == item.ToolAssetId.Value &&
                    x.MaintenanceNumber == maintenanceNumber &&
                    !x.IsDeleted,
                    cancellationToken);

            if (maintenanceRecord is null)
            {
                maintenanceRecord = new MaintenanceRecord
                {
                    Id = Guid.NewGuid(),
                    ToolAssetId = item.ToolAssetId.Value,
                    MaintenanceNumber = maintenanceNumber,
                    Type = MapMaintenanceType(item.RequestType),
                    Status = ToolMaintenanceStatus.Completed,
                    ScheduledAt = item.ScheduledAt
                        ?? item.ApprovedAt
                        ?? item.SubmittedAt
                        ?? item.RequestedAt,
                    StartedAt = item.ExecutionStartedAt
                        ?? item.ScheduledAt
                        ?? item.ApprovedAt
                        ?? now,
                    FinishedAt = now,
                    Provider = NormalizeText(item.VendorSuggestion),
                    Technician = NormalizeText(item.AssignedTechnician ?? item.ExecutionStartedBy ?? user),
                    Description = NormalizeText(item.ProblemDescription),
                    MaintenanceActivities = NormalizeText(item.WorkDescription ?? item.Title),
                    ExecutionNotes = NormalizeText(item.ClosingComment ?? item.Notes),
                    InvoiceNumber = $"SOL-{item.RequestNumber}",
                    ResponsibleName = NormalizeText(item.RequestedByResponsiblePersonName ?? item.RequestedByUserName ?? item.PreparedBy),
                    ResponsiblePosition = "Responsable de mantenimiento",
                    IsToolOperational = true,
                    EvidenceDocumentId = null,
                    Cost = ParseCost(item.EstimatedCostText),
                    Result = NormalizeText(item.ClosingComment),
                    CreatedAt = now,
                    CreatedBy = user
                };

                _context.Set<MaintenanceRecord>().Add(maintenanceRecord);
                scheduleCreated = true;
            }
            else
            {
                maintenanceRecord.Status = ToolMaintenanceStatus.Completed;
                maintenanceRecord.FinishedAt = now;
                maintenanceRecord.Technician = NormalizeText(item.AssignedTechnician ?? item.ExecutionStartedBy ?? user);
                maintenanceRecord.ExecutionNotes = NormalizeText(item.ClosingComment ?? item.Notes);
                maintenanceRecord.Result = NormalizeText(item.ClosingComment);
                maintenanceRecord.UpdatedAt = now;
                maintenanceRecord.UpdatedBy = user;
            }

            _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
            {
                ToolAssetId = item.ToolAssetId.Value,
                EventType = "MaintenanceRequestClosedAndLinked",
                Title = "Solicitud de mantenimiento cerrada",
                Description = $"Se cerró la solicitud {item.RequestNumber} y se vinculó al cronograma del activo.",
                PreviousValue = item.RequestNumber,
                NewValue = maintenanceRecord.MaintenanceNumber,
                RegisteredBy = user,
                CreatedBy = user
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = item.ToolAssetId.HasValue
                ? "Solicitud cerrada y vinculada al cronograma del activo."
                : "Solicitud cerrada. No se creó cronograma porque no tenía activo relacionado.",
            item.Id,
            item.RequestNumber,
            item.Status,
            ToolAssetId = item.ToolAssetId,
            MaintenanceRecordId = maintenanceRecord?.Id,
            MaintenanceNumber = maintenanceRecord?.MaintenanceNumber,
            ScheduleCreated = scheduleCreated
        });
    }
    [RequirePermission("Maintenance.Close")]
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] MaintenanceRequestActionCommand request, CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var item = await FindAsync(id, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de mantenimiento." });
        }

        if (item.Status == "Closed")
        {
            return BadRequest(new { Message = "No se puede cancelar una solicitud cerrada." });
        }

        item.Status = "Canceled";
        item.CanceledAt = DateTime.UtcNow;
        item.CanceledBy = GetUserName();
        item.CancellationReason = string.IsNullOrWhiteSpace(request.Comment)
            ? "Solicitud cancelada."
            : request.Comment.Trim();
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = GetUserName();

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Solicitud cancelada.", item.Id, item.RequestNumber, item.Status });
    }
    [RequirePermission("Maintenance.Execute")]
    [HttpPost("seed-demo")]
    public async Task<IActionResult> SeedDemo(CancellationToken cancellationToken)
    {
        await EnsureMaintenanceSchemaAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var currentUser = GetUserName();

        var existingDemo = await _context.MaintenanceRequests
            .IgnoreQueryFilters()
            .Where(x => x.RequestNumber.StartsWith("MNT-NAVI-DEMO-"))
            .ToListAsync(cancellationToken);

        if (existingDemo.Any())
        {
            _context.MaintenanceRequests.RemoveRange(existingDemo);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var tools = await _context.ToolAssets
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.InternalCode)
            .Take(5)
            .Select(x => new { x.Id, x.BranchId })
            .ToListAsync(cancellationToken);

        var firstTool = tools.ElementAtOrDefault(0);
        var secondTool = tools.ElementAtOrDefault(1);
        var thirdTool = tools.ElementAtOrDefault(2);
        var fourthTool = tools.ElementAtOrDefault(3);
        var fifthTool = tools.ElementAtOrDefault(4);

        var demo = new List<ToolMaintenanceRequest>
        {
            CreateDemo("MNT-NAVI-DEMO-0001", "Correctivo", "Alta", "Draft", "Ruido anormal en gato hidráulico", "Se reporta ruido durante operación.", currentUser, now.AddDays(-8), firstTool?.Id, firstTool?.BranchId),
            CreateDemo("MNT-NAVI-DEMO-0002", "Preventivo", "Media", "InReview", "Mantenimiento preventivo escáner", "Revisión periódica de equipo diagnóstico.", "herramientero", now.AddDays(-6), secondTool?.Id, secondTool?.BranchId),
            CreateDemo("MNT-NAVI-DEMO-0003", "Correctivo", "Crítica", "Approved", "Falla en compresor de aire", "El equipo pierde presión durante operación.", "herramientero", now.AddDays(-12), thirdTool?.Id, thirdTool?.BranchId),
            CreateDemo("MNT-NAVI-DEMO-0004", "Calibración", "Media", "Scheduled", "Calibración llaves de torque", "Requiere calibración por vencimiento de control metrológico.", "coordinador_taller", now.AddDays(-10), fourthTool?.Id, fourthTool?.BranchId),
            CreateDemo("MNT-NAVI-DEMO-0005", "Correctivo", "Alta", "InExecution", "Cambio de cableado en equipo eléctrico", "Se encuentra cable deteriorado.", "tecnico", now.AddDays(-4), fifthTool?.Id, fifthTool?.BranchId),
            CreateDemo("MNT-NAVI-DEMO-0006", "Garantía", "Baja", "Closed", "Garantía cerrada herramienta nueva", "Proceso de garantía finalizado.", "ing_servicios", now.AddDays(-20), firstTool?.Id, firstTool?.BranchId),
            CreateDemo("MNT-NAVI-DEMO-0007", "Correctivo", "Media", "Rejected", "Solicitud sin evidencia suficiente", "No se adjuntó evidencia del daño reportado.", "tecnico", now.AddDays(-15), secondTool?.Id, secondTool?.BranchId)
        };

        foreach (var item in demo)
        {
            if (item.Status != "Draft")
            {
                item.SubmittedAt = item.CreatedAt.AddDays(1);
                item.SubmittedBy = item.PreparedBy;
            }

            if (item.Status is "Approved" or "Scheduled" or "InExecution" or "Closed")
            {
                item.ApprovedAt = item.CreatedAt.AddDays(2);
                item.ApprovedBy = "ing_servicios";
                item.ApprovalComment = "Aprobado para intervención.";
            }

            if (item.Status is "Scheduled" or "InExecution" or "Closed")
            {
                item.ScheduledAt = item.CreatedAt.AddDays(3);
                item.ScheduledBy = "coordinador_taller";
                item.AssignedTechnician = "tecnico";
            }

            if (item.Status is "InExecution" or "Closed")
            {
                item.ExecutionStartedAt = item.CreatedAt.AddDays(4);
                item.ExecutionStartedBy = "tecnico";
            }

            if (item.Status == "Closed")
            {
                item.ExecutionFinishedAt = item.CreatedAt.AddDays(5);
                item.ExecutionFinishedBy = "tecnico";
                item.ClosedAt = item.CreatedAt.AddDays(5);
                item.ClosedBy = "coordinador_taller";
                item.ClosingComment = "Mantenimiento finalizado correctamente.";
            }

            if (item.Status == "Rejected")
            {
                item.RejectedAt = item.CreatedAt.AddDays(2);
                item.RejectedBy = "coordinador_taller";
                item.RejectionReason = "Se requiere mayor detalle y evidencia.";
            }
        }

        _context.MaintenanceRequests.AddRange(demo);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Solicitudes demo de mantenimiento insertadas correctamente.",
            Total = demo.Count,
            Estados = demo.GroupBy(x => x.Status).Select(x => new { Estado = x.Key, Cantidad = x.Count() })
        });
    }

    private static ToolMaintenanceRequest CreateDemo(
        string number,
        string type,
        string priority,
        string status,
        string title,
        string description,
        string user,
        DateTime createdAt,
        Guid? toolAssetId = null,
        Guid? branchId = null)
    {
        return new ToolMaintenanceRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = number,
            ToolAssetId = toolAssetId,
            BranchId = branchId,
            RequestType = type,
            Priority = priority,
            Status = status,
            Title = title,
            ProblemDescription = description,
            PreparedBy = user,
            RequestedByUserName = user,
            RequestedAt = createdAt,
            RequiresStop = priority is "Alta" or "Crítica",
            IsSafetyRisk = priority == "Crítica",
            EstimatedCostText = "$0",
            Notes = "Registro demo generado para pruebas.",
            CreatedAt = createdAt,
            CreatedBy = user
        };
    }


    private static ToolMaintenanceType MapMaintenanceType(string? requestType)
    {
        if (string.IsNullOrWhiteSpace(requestType))
        {
            return ToolMaintenanceType.Preventive;
        }

        var value = requestType.Trim().ToLowerInvariant();

        if (value.Contains("correct"))
        {
            return ToolMaintenanceType.Corrective;
        }

        if (value.Contains("calibr"))
        {
            return ToolMaintenanceType.Calibration;
        }

        if (value.Contains("inspe") || value.Contains("ssta") || value.Contains("seguridad"))
        {
            return ToolMaintenanceType.Inspection;
        }

        return ToolMaintenanceType.Preventive;
    }

    private static decimal? ParseCost(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var cleaned = value
            .Replace("$", string.Empty)
            .Replace("COP", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace(" ", string.Empty)
            .Trim();

        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return null;
        }

        if (cleaned.Contains(',') && cleaned.Contains('.'))
        {
            cleaned = cleaned.Replace(".", string.Empty).Replace(",", ".");
        }
        else if (cleaned.Contains(','))
        {
            cleaned = cleaned.Replace(",", ".");
        }

        return decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private static string? NormalizeText(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private async Task<ToolMaintenanceRequest?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.MaintenanceRequests
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
    }

    private async Task<string> GenerateRequestNumberAsync(CancellationToken cancellationToken)
    {
        var prefix = $"MNT-NAVI-{DateTime.UtcNow:yyyyMM}";

        var count = await _context.MaintenanceRequests
            .IgnoreQueryFilters()
            .CountAsync(x => x.RequestNumber.StartsWith(prefix), cancellationToken);

        return $"{prefix}-{count + 1:0000}";
    }

    private string GetUserName()
    {
        return Request.Headers.TryGetValue("X-Navi-User", out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : "admin-web";
    }

    private Guid? GetUserId() => TryGetGuidHeader("X-Navi-UserId");

    private Guid? GetBranchId() => TryGetGuidHeader("X-Navi-BranchId");

    private Guid? GetResponsiblePersonId() => TryGetGuidHeader("X-Navi-ResponsiblePersonId");

    private string? GetResponsiblePersonName()
    {
        return Request.Headers.TryGetValue("X-Navi-ResponsiblePersonName", out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : null;
    }

    private Guid? TryGetGuidHeader(string headerName)
    {
        if (!Request.Headers.TryGetValue(headerName, out var value))
        {
            return null;
        }

        return Guid.TryParse(value.ToString(), out var id) ? id : null;
    }

    private async Task EnsureMaintenanceSchemaAsync(CancellationToken cancellationToken)
    {
        var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Maintenance')
BEGIN
    EXEC('CREATE SCHEMA [Maintenance]')
END

IF OBJECT_ID('[Maintenance].[MaintenanceRequests]', 'U') IS NULL
BEGIN
    CREATE TABLE [Maintenance].[MaintenanceRequests](
        [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_MaintenanceRequests] PRIMARY KEY,
        [RequestNumber] NVARCHAR(60) NOT NULL,
        [ToolAssetId] UNIQUEIDENTIFIER NULL,
        [BranchId] UNIQUEIDENTIFIER NULL,
        [RequestType] NVARCHAR(80) NOT NULL,
        [Priority] NVARCHAR(40) NOT NULL,
        [Status] NVARCHAR(40) NOT NULL,
        [Title] NVARCHAR(300) NOT NULL,
        [ProblemDescription] NVARCHAR(2000) NOT NULL,
        [WorkDescription] NVARCHAR(2000) NULL,
        [FailureCause] NVARCHAR(500) NULL,
        [RequestedByUserName] NVARCHAR(150) NULL,
        [RequestedByUserId] UNIQUEIDENTIFIER NULL,
        [RequestedByResponsiblePersonId] UNIQUEIDENTIFIER NULL,
        [RequestedByResponsiblePersonName] NVARCHAR(250) NULL,
        [PreparedBy] NVARCHAR(150) NOT NULL,
        [RequestedAt] DATETIME2 NOT NULL,
        [SubmittedAt] DATETIME2 NULL,
        [SubmittedBy] NVARCHAR(150) NULL,
        [ApprovedAt] DATETIME2 NULL,
        [ApprovedBy] NVARCHAR(150) NULL,
        [ApprovalComment] NVARCHAR(1000) NULL,
        [RejectedAt] DATETIME2 NULL,
        [RejectedBy] NVARCHAR(150) NULL,
        [RejectionReason] NVARCHAR(1000) NULL,
        [ScheduledAt] DATETIME2 NULL,
        [ScheduledBy] NVARCHAR(150) NULL,
        [AssignedTechnician] NVARCHAR(150) NULL,
        [ExecutionStartedAt] DATETIME2 NULL,
        [ExecutionStartedBy] NVARCHAR(150) NULL,
        [ExecutionFinishedAt] DATETIME2 NULL,
        [ExecutionFinishedBy] NVARCHAR(150) NULL,
        [ClosedAt] DATETIME2 NULL,
        [ClosedBy] NVARCHAR(150) NULL,
        [ClosingComment] NVARCHAR(1000) NULL,
        [CanceledAt] DATETIME2 NULL,
        [CanceledBy] NVARCHAR(150) NULL,
        [CancellationReason] NVARCHAR(1000) NULL,
        [RequiresStop] BIT NOT NULL CONSTRAINT [DF_MaintenanceRequests_RequiresStop] DEFAULT(0),
        [IsSafetyRisk] BIT NOT NULL CONSTRAINT [DF_MaintenanceRequests_IsSafetyRisk] DEFAULT(0),
        [EstimatedCostText] NVARCHAR(120) NULL,
        [VendorSuggestion] NVARCHAR(300) NULL,
        [Notes] NVARCHAR(2000) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [CreatedBy] NVARCHAR(150) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(150) NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_MaintenanceRequests_IsDeleted] DEFAULT(0)
    );

    CREATE UNIQUE INDEX [IX_MaintenanceRequests_RequestNumber]
        ON [Maintenance].[MaintenanceRequests]([RequestNumber]);
END
";
        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}

public sealed class CreateMaintenanceRequestRequest
{
    public Guid? ToolAssetId { get; set; }
    public Guid? BranchId { get; set; }
    public string? RequestType { get; set; }
    public string? Priority { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string? WorkDescription { get; set; }
    public string? FailureCause { get; set; }
    public bool RequiresStop { get; set; }
    public bool IsSafetyRisk { get; set; }
    public string? EstimatedCostText { get; set; }
    public string? VendorSuggestion { get; set; }
    public string? RequestChannel { get; set; }
    public string? MaintenanceClassification { get; set; }
    public string? ServiceType { get; set; }
    public DateTime? RequiredAt { get; set; }
    public string? SerialNumber { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? EquipmentReference { get; set; }
    public string? ImageEvidenceDescription { get; set; }
    public string? EvidenceReference { get; set; }
    public string? ExcelReference { get; set; }
    public DateTime? FailureDate { get; set; }
    public string? NeedDescription { get; set; }
    public string? FailureDetail { get; set; }
    public string? MaintenanceLocation { get; set; }
    public decimal? EstimatedDowntimeHours { get; set; }
    public bool WarrantyApplies { get; set; }
    public string? WarrantyProvider { get; set; }
    public string? ServiceProvider { get; set; }
    public bool RequiresQuotation { get; set; }
    public int? QuotationCount { get; set; }
    public string? SelectedVendor { get; set; }
    public string? QuotationReferences { get; set; }
    public string? VendorSelectionReason { get; set; }
    public bool RequiresPurchaseOrder { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? PurchaseOrderStatus { get; set; }
    public string? MroCodeOrAccount { get; set; }
    public string? AccountingConcept { get; set; }
    public string? AccountingAccount { get; set; }
    public string? ProviderActivationCriteria { get; set; }
    public bool RequiresAccountingValidation { get; set; }
    public string? AccountingValidationStatus { get; set; }
    public string? AccountingValidationComment { get; set; }
    public string? Notes { get; set; }
    public bool SendToReview { get; set; }
}

public sealed class MaintenanceRequestActionCommand
{
    public string? Comment { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? AssignedTechnician { get; set; }
}

public sealed class MaintenanceRequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public Guid? ToolAssetId { get; set; }
    public string? ToolInternalCode { get; set; }
    public string? ToolName { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ProblemDescription { get; set; } = string.Empty;
    public string? WorkDescription { get; set; }
    public string? FailureCause { get; set; }
    public string? RequestedByUserName { get; set; }
    public string? RequestedByResponsiblePersonName { get; set; }
    public string PreparedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? ApprovalComment { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public string? ScheduledBy { get; set; }
    public string? AssignedTechnician { get; set; }
    public DateTime? ExecutionStartedAt { get; set; }
    public string? ExecutionStartedBy { get; set; }
    public DateTime? ExecutionFinishedAt { get; set; }
    public string? ExecutionFinishedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosedBy { get; set; }
    public string? ClosingComment { get; set; }
    public DateTime? CanceledAt { get; set; }
    public string? CanceledBy { get; set; }
    public string? CancellationReason { get; set; }
    public bool RequiresStop { get; set; }
    public bool IsSafetyRisk { get; set; }
    public string? EstimatedCostText { get; set; }
    public string? VendorSuggestion { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}






