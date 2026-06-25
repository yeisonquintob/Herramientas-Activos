using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Domain.Entities.PhysicalCounts;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public sealed class ToolAssetControlController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public ToolAssetControlController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }


    [HttpGet("{id:guid}/reported-source")]
    public async Task<IActionResult> GetReportedSource(Guid id, CancellationToken cancellationToken)
    {
        var toolExists = await _context.ToolAssets
            .AsNoTracking()
            .AnyAsync(x => !x.IsDeleted && x.Id == id, cancellationToken);

        if (!toolExists)
        {
            return NotFound(new { Message = $"No se encontró el activo {id}." });
        }

        var source = await _context.Set<PhysicalCountReportedItem>()
            .AsNoTracking()
            .Include(x => x.PhysicalCount)
            .Include(x => x.Participant)
            .Where(x => !x.IsDeleted && x.CreatedToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                HasSource = true,
                ReportedItemId = x.Id,
                x.PhysicalCountId,
                PhysicalCountNumber = x.PhysicalCount == null ? null : x.PhysicalCount.CountNumber,
                PhysicalCountStatus = x.PhysicalCount == null ? null : x.PhysicalCount.Status.ToString(),
                ParticipantId = x.PhysicalCountParticipantId,
                ParticipantName = x.Participant == null ? null : x.Participant.DisplayName,
                ParticipantUserName = x.Participant == null ? null : x.Participant.UserName,
                x.ReportType,
                x.ReportedCode,
                x.ReportedName,
                x.FoundLocation,
                x.PhysicalStatus,
                x.Observation,
                x.ReportedBy,
                x.ReportedAt,
                x.ReconciliationStatus,
                x.ReconciliationObservation,
                x.ReconciledAt,
                x.ReconciledBy,
                x.ApprovedForCreation,
                x.ApprovedForCreationAt,
                x.ApprovedForCreationBy
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (source is null)
        {
            return Ok(new
            {
                HasSource = false,
                Message = "El activo no registra origen desde toma física."
            });
        }

        return Ok(source);
    }
    [HttpPatch("{id:guid}/master-data")]
    public async Task<IActionResult> UpdateMasterData(Guid id, [FromBody] UpdateToolAssetMasterDataRequest request, CancellationToken cancellationToken)
    {
        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = $"No se encontró el activo {id}." });
        }

        var actionBy = Normalize(request.UpdatedBy) ?? "admin";
        var now = DateTime.UtcNow;
        var previousOperationalStatusForTrace = tool.OperationalStatus.ToString();
        var previousPhysicalStatusForTrace = tool.PhysicalStatus.ToString();
        var previousCustodyStatusForTrace = tool.CustodyStatus.ToString();
        var previousResponsibleForTrace = tool.ResponsiblePersonId?.ToString();


        var internalCode = Normalize(request.InternalCode);

        if (!string.IsNullOrWhiteSpace(internalCode) &&
            !string.Equals(tool.InternalCode, internalCode, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.ToolAssets
                .AsNoTracking()
                .AnyAsync(x => !x.IsDeleted && x.Id != id && x.InternalCode == internalCode, cancellationToken);

            if (exists)
            {
                return BadRequest(new { Message = $"Ya existe otro activo con el código {internalCode}." });
            }

            tool.InternalCode = internalCode;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            tool.Name = request.Name.Trim();
        }

        tool.Description = Normalize(request.Description);
        tool.SerialNumber = Normalize(request.SerialNumber);
        tool.Brand = Normalize(request.Brand);
        tool.Model = Normalize(request.Model);
        tool.FixedAssetCode = Normalize(request.FixedAssetCode);
        tool.FenixCode = Normalize(request.FenixCode);

        if (TryParseEnum<ToolOperationalStatus>(request.OperationalStatus, out var operationalStatus))
        {
            tool.OperationalStatus = operationalStatus;
        }

        if (TryParseEnum<ToolPhysicalStatus>(request.PhysicalStatus, out var physicalStatus))
        {
            tool.PhysicalStatus = physicalStatus;
        }

        if (request.IsSpecialized.HasValue)
        {
            tool.IsSpecialized = request.IsSpecialized.Value;
        }

        if (request.RequiresMaintenance.HasValue)
        {
            tool.RequiresMaintenance = request.RequiresMaintenance.Value;
        }

        if (request.RequiresPreOperationalCheck.HasValue)
        {
            tool.RequiresPreOperationalCheck = request.RequiresPreOperationalCheck.Value;
        }

        if (request.RequiresCertification.HasValue)
        {
            tool.RequiresCertification = request.RequiresCertification.Value;
        }

        if (request.CertificationExpirationDate.HasValue)
        {
            tool.CertificationExpirationDate = request.CertificationExpirationDate.Value;
        }
        else if (request.ClearCertificationExpirationDate)
        {
            tool.CertificationExpirationDate = null;
        }

        if (request.MaintenancePeriodMonths.HasValue)
        {
            tool.MaintenancePeriodMonths = request.MaintenancePeriodMonths.Value <= 0
                ? null
                : request.MaintenancePeriodMonths.Value;
        }
        if (request.ValidateAndAssignReporter)
        {
            var reportedSource = await _context.Set<PhysicalCountReportedItem>()
                .Include(x => x.Participant)
                .FirstOrDefaultAsync(x =>
                    !x.IsDeleted &&
                    x.CreatedToolAssetId == tool.Id,
                    cancellationToken);

            if (reportedSource is not null)
            {
                var reporterResponsibleId = reportedSource.ResponsiblePersonId
                    ?? reportedSource.Participant?.ResponsiblePersonId;

                if (!reporterResponsibleId.HasValue)
                {
                    return BadRequest(new
                    {
                        Message = "El activo fue creado desde toma física, pero el reporte no tiene responsable asociado para asignarlo."
                    });
                }

                var reporterExists = await _context.ResponsiblePeople
                    .AsNoTracking()
                    .AnyAsync(x =>
                        !x.IsDeleted &&
                        x.Id == reporterResponsibleId.Value,
                        cancellationToken);

                if (!reporterExists)
                {
                    return BadRequest(new
                    {
                        Message = "El responsable que reportó el activo ya no existe o está eliminado."
                    });
                }

                tool.ResponsiblePersonId = reporterResponsibleId.Value;
                tool.CustodyStatus = ToolCustodyStatus.AssignedToResponsible;
                tool.OperationalStatus = ToolOperationalStatus.Assigned;
                tool.ReconciliationStatus = ToolReconciliationStatus.Validated;

                tool.Description = string.IsNullOrWhiteSpace(tool.Description)
                    ? "Validado y asignado al reportante desde toma física."
                    : $"{tool.Description} | Validado y asignado al reportante desde toma física.";

                reportedSource.ReconciliationStatus = "Reconciled";
                reportedSource.ReconciliationObservation = "Activo validado, creado en Inventario de AF y asignado al responsable que lo reportó.";
                reportedSource.ReconciledAt = now;
                reportedSource.ReconciledBy = actionBy;
                reportedSource.RequiresUserClarification = false;
                reportedSource.UpdatedAt = now;
                reportedSource.UpdatedBy = actionBy;
            }
        }
        tool.UpdatedAt = now;
        tool.UpdatedBy = actionBy;


        AddToolLifeCycleEvent(
            tool.Id,
            "MasterDataUpdatedFromDetail",
            "Datos del activo actualizados",
            "Se actualizaron datos maestros, estado físico, estado operativo o reglas operativas desde el detalle del activo.",
            $"Operativo: {previousOperationalStatusForTrace} | Físico: {previousPhysicalStatusForTrace} | Custodia: {previousCustodyStatusForTrace} | Responsable: {previousResponsibleForTrace}",
            $"Operativo: {tool.OperationalStatus} | Físico: {tool.PhysicalStatus} | Custodia: {tool.CustodyStatus} | Responsable: {tool.ResponsiblePersonId}",
            actionBy);

        if (request.ValidateAndAssignReporter && tool.CustodyStatus == ToolCustodyStatus.AssignedToResponsible)
        {
            AddToolLifeCycleEvent(
                tool.Id,
                "ReportedToolValidatedAndAssigned",
                "Activo validado y asignado al reportante",
                "El activo creado desde toma física fue validado y asignado al responsable que lo reportó.",
                previousCustodyStatusForTrace,
                tool.CustodyStatus.ToString(),
                actionBy);
        }
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Activo actualizado correctamente.",
            tool.Id,
            tool.InternalCode,
            tool.Name,
            tool.SerialNumber,
            tool.Brand,
            tool.Model,
            OperationalStatus = tool.OperationalStatus.ToString(),
            PhysicalStatus = tool.PhysicalStatus.ToString(),
            tool.IsSpecialized,
            tool.UpdatedAt,
            tool.UpdatedBy
        });
    }

    [HttpPatch("{id:guid}/void")]
    public async Task<IActionResult> VoidToolAsset(Guid id, [FromBody] VoidToolAssetRequest request, CancellationToken cancellationToken)
    {
        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = $"No se encontró el activo {id}." });
        }

        var actionBy = Normalize(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;
        var reason = Normalize(request.Reason) ?? "Activo anulado desde detalle de Inventario de AF.";

        tool.IsDeleted = true;        tool.UpdatedAt = now;
        tool.UpdatedBy = actionBy;

        tool.Description = string.IsNullOrWhiteSpace(tool.Description)
            ? $"ANULADO: {reason}"
            : $"{tool.Description} | ANULADO: {reason}";


        AddToolLifeCycleEvent(
            tool.Id,
            "ToolVoidedFromDetail",
            "Activo anulado",
            reason,
            tool.OperationalStatus.ToString(),
            "Anulado",
            actionBy);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Activo anulado correctamente. No se elimina la trazabilidad histórica.",
            tool.Id,
            tool.InternalCode,
            tool.Name,
            Reason = reason,
            tool.UpdatedAt,
            tool.UpdatedBy
        });
    }


    [HttpPatch("{id:guid}/reset-custody-to-warehouse")]
    public async Task<IActionResult> ResetCustodyToWarehouse(Guid id, [FromBody] VoidToolAssetRequest request, CancellationToken cancellationToken)
    {
        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = $"No se encontró el activo {id}." });
        }

        var actionBy = Normalize(request.ActionBy) ?? "admin";
        var now = DateTime.UtcNow;

        tool.ResponsiblePersonId = null;
        tool.CustodyStatus = ToolCustodyStatus.InWarehouse;
        tool.OperationalStatus = ToolOperationalStatus.PendingValidation;
        tool.ReconciliationStatus = ToolReconciliationStatus.Pending;        tool.UpdatedAt = now;
        tool.UpdatedBy = actionBy;

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Activo devuelto a custodia de almacén y pendiente de validación.",
            tool.Id,
            tool.InternalCode,
            tool.Name,
            CustodyStatus = tool.CustodyStatus.ToString(),
            OperationalStatus = tool.OperationalStatus.ToString(),
            ReconciliationStatus = tool.ReconciliationStatus.ToString()
        });
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
    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum result)
        where TEnum : struct, Enum
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value.Trim(), ignoreCase: true, out result);
    }

    public sealed class UpdateToolAssetMasterDataRequest
    {
        public string? InternalCode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? SerialNumber { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? FixedAssetCode { get; set; }
        public string? FenixCode { get; set; }
        public string? OperationalStatus { get; set; }
        public string? PhysicalStatus { get; set; }
        public bool? IsSpecialized { get; set; }
        public bool? RequiresMaintenance { get; set; }
        public bool? RequiresPreOperationalCheck { get; set; }
        public bool? RequiresCertification { get; set; }
        public DateTime? CertificationExpirationDate { get; set; }
        public bool ClearCertificationExpirationDate { get; set; }
        public int? MaintenancePeriodMonths { get; set; }
        public bool ValidateAndAssignReporter { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public sealed class VoidToolAssetRequest
    {
        public string? ActionBy { get; set; }
        public string? Reason { get; set; }
    }
}





