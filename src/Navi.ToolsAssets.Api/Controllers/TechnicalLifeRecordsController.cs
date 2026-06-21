using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class TechnicalLifeRecordsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public TechnicalLifeRecordsController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:guid}/technical-life-record")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var tool = await BuildBaseQuery()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return BuildResponse(tool);
    }

    [HttpGet("by-code/{internalCode}/technical-life-record")]
    public async Task<IActionResult> GetByCode(string internalCode, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await BuildBaseQuery()
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        return BuildResponse(tool);
    }

    private IQueryable<ToolAsset> BuildBaseQuery()
    {
        return _context.ToolAssets
            .AsNoTracking()
            .Include(x => x.Zone)
            .Include(x => x.Branch)
            .Include(x => x.Location)
            .Include(x => x.ResponsiblePerson)
            .Include(x => x.ToolType)
            .Include(x => x.ToolCategory)
            .Include(x => x.Accessories)
            .Include(x => x.SafePractices)
            .Include(x => x.Documents)
            .Include(x => x.MaintenanceRecords)
            .Include(x => x.LifeCycleEvents);
    }

    private IActionResult BuildResponse(ToolAsset? tool)
    {
        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta solicitada." });
        }

        var documents = tool.Documents
            .OrderByDescending(x => x.UploadedAt)
            .ToList();

        var maintenanceRecords = tool.MaintenanceRecords
            .OrderByDescending(x => x.ScheduledAt)
            .ToList();

        var lastCompletedMaintenance = maintenanceRecords
            .Where(x => x.Status == ToolMaintenanceStatus.Completed || x.FinishedAt.HasValue)
            .OrderByDescending(x => x.FinishedAt ?? x.StartedAt ?? x.ScheduledAt)
            .FirstOrDefault();

        DateTime? lastMaintenanceDate = tool.LastMaintenanceDate;

        if (!lastMaintenanceDate.HasValue && lastCompletedMaintenance is not null)
        {
            lastMaintenanceDate = lastCompletedMaintenance.FinishedAt
                ?? lastCompletedMaintenance.StartedAt
                ?? lastCompletedMaintenance.ScheduledAt;
        }

        DateTime? nextMaintenanceDate = tool.NextMaintenanceDate;

        if (!nextMaintenanceDate.HasValue
            && lastMaintenanceDate.HasValue
            && tool.MaintenancePeriodMonths.HasValue
            && tool.MaintenancePeriodMonths.Value > 0)
        {
            nextMaintenanceDate = lastMaintenanceDate.Value.AddMonths(tool.MaintenancePeriodMonths.Value);
        }

        var usefulLifeStartDate = tool.UsefulLifeStartDate ?? tool.AcquisitionDate;

        var usefulLifeDays = tool.UsefulLifeDays;

        if (!usefulLifeDays.HasValue && tool.UsefulLifeMonths.HasValue)
        {
            usefulLifeDays = tool.UsefulLifeMonths.Value * 30;
        }

        DateTime? usefulLifeEndDate = null;
        int? elapsedDays = null;
        int? remainingDays = null;
        int? usedPercentage = null;

        if (usefulLifeStartDate.HasValue && usefulLifeDays.HasValue && usefulLifeDays.Value > 0)
        {
            usefulLifeEndDate = usefulLifeStartDate.Value.Date.AddDays(usefulLifeDays.Value);
            elapsedDays = Math.Max(0, (DateTime.UtcNow.Date - usefulLifeStartDate.Value.Date).Days);
            remainingDays = Math.Max(0, (usefulLifeEndDate.Value.Date - DateTime.UtcNow.Date).Days);
            usedPercentage = Math.Clamp((int)Math.Round((decimal)elapsedDays.Value / usefulLifeDays.Value * 100), 0, 100);
        }

        var response = new
        {
            Summary = new
            {
                tool.Id,
                tool.InternalCode,
                tool.Name,
                tool.Description,
                OperationalStatus = tool.OperationalStatus.ToString(),
                PhysicalStatus = tool.PhysicalStatus.ToString(),
                CustodyStatus = tool.CustodyStatus.ToString(),
                ReconciliationStatus = tool.ReconciliationStatus.ToString(),
                SyncStatus = tool.SyncStatus.ToString(),
                tool.IsSpecialized,
                tool.RequiresMaintenance,
                tool.RequiresPreOperationalCheck,
                tool.RequiresCertification,
                tool.CreatedAt,
                tool.CreatedBy,
                tool.UpdatedAt,
                tool.UpdatedBy
            },

            EquipmentInformation = new
            {
                tool.InternalCode,
                tool.Name,
                tool.Description,
                tool.Brand,
                tool.Model,
                tool.SerialNumber,
                tool.FixedAssetCode,
                tool.FenixCode,
                tool.AcquisitionDate,
                tool.UnitOfMeasure,
                tool.Quantity,
                ZoneCode = tool.Zone == null ? null : tool.Zone.Code,
                ZoneName = tool.Zone == null ? null : tool.Zone.Name,
                BranchCode = tool.Branch == null ? null : tool.Branch.Code,
                BranchName = tool.Branch == null ? null : tool.Branch.Name,
                LocationCode = tool.Location == null ? null : tool.Location.Code,
                LocationName = tool.Location == null ? null : tool.Location.Name,
                ResponsibleName = tool.ResponsiblePerson == null ? null : tool.ResponsiblePerson.FullName,
                ToolTypeCode = tool.ToolType == null ? null : tool.ToolType.Code,
                ToolTypeName = tool.ToolType == null ? null : tool.ToolType.Name,
                ToolCategoryCode = tool.ToolCategory == null ? null : tool.ToolCategory.Code,
                ToolCategoryName = tool.ToolCategory == null ? null : tool.ToolCategory.Name
            },

            TechnicalSpecifications = new
            {
                tool.Voltage,
                tool.LoadCapacity,
                tool.Provider,
                tool.RequiresMaintenance,
                tool.RequiresPreOperationalCheck,
                tool.RequiresCertification,
                tool.CertificationExpirationDate
            },

            Warranty = new
            {
                tool.HasWarranty,
                tool.WarrantyType
            },

            UsefulLife = new
            {
                StartDate = usefulLifeStartDate,
                EndDate = usefulLifeEndDate,
                UsefulLifeMonths = tool.UsefulLifeMonths,
                UsefulLifeDays = usefulLifeDays,
                ElapsedDays = elapsedDays,
                RemainingDays = remainingDays,
                UsedPercentage = usedPercentage
            },

            MaintenancePlan = new
            {
                LastMaintenanceDate = lastMaintenanceDate,
                NextMaintenanceDate = nextMaintenanceDate,
                tool.MaintenancePeriodMonths,
                RequiresMaintenance = tool.RequiresMaintenance,
                TotalMaintenanceRecords = maintenanceRecords.Count,
                CompletedMaintenanceRecords = maintenanceRecords.Count(x => x.Status == ToolMaintenanceStatus.Completed),
                PendingMaintenanceRecords = maintenanceRecords.Count(x => x.Status is ToolMaintenanceStatus.Scheduled or ToolMaintenanceStatus.InProgress)
            },

            Accessories = tool.Accessories
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.Quantity,
                    x.RequiresMaintenance,
                    x.IsMeasurementEquipment,
                    x.Observation,
                    x.CreatedAt,
                    x.CreatedBy
                })
                .ToList(),

            SafePractices = tool.SafePractices
                .Where(x => x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.PracticeName)
                .Select(x => new
                {
                    x.Id,
                    x.PracticeName,
                    x.Description,
                    x.SortOrder,
                    x.CreatedAt,
                    x.CreatedBy
                })
                .ToList(),

            Documents = new
            {
                PhotoRecords = documents
                    .Where(x => x.DocumentType is ToolDocumentType.PhotoEvidence or ToolDocumentType.PhysicalCountEvidence)
                    .Select(ToDocumentResponse)
                    .ToList(),

                TechnicalDocuments = documents
                    .Where(x => x.DocumentType is ToolDocumentType.TechnicalDocument or ToolDocumentType.LifeCycleExcel)
                    .Select(ToDocumentResponse)
                    .ToList(),

                MaintenanceEvidences = documents
                    .Where(x => x.DocumentType == ToolDocumentType.MaintenanceSupport)
                    .Select(ToDocumentResponse)
                    .ToList(),

                AllDocuments = documents
                    .Select(ToDocumentResponse)
                    .ToList()
            },

            MaintenanceSchedule = maintenanceRecords
                .Select(x =>
                {
                    var evidence = x.EvidenceDocumentId.HasValue
                        ? documents.FirstOrDefault(d => d.Id == x.EvidenceDocumentId.Value)
                        : null;

                    return new
                    {
                        x.Id,
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
                        x.Cost,
                        x.Result,
                        x.EvidenceDocumentId,
                        EvidenceFileName = evidence == null ? null : evidence.FileName,
                        EvidenceDownloadUrl = evidence == null ? null : $"/api/tools/documents/{evidence.Id}/download"
                    };
                })
                .ToList(),

            LifeCycleEvents = tool.LifeCycleEvents
                .OrderByDescending(x => x.RegisteredAt)
                .ThenByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.EventType,
                    x.Title,
                    x.Description,
                    x.PreviousValue,
                    x.NewValue,
                    PerformedBy = string.IsNullOrWhiteSpace(x.RegisteredBy) ? x.CreatedBy : x.RegisteredBy,
                    EventDate = x.RegisteredAt == default ? x.CreatedAt : x.RegisteredAt,
                    x.CreatedAt,
                    x.CreatedBy
                })
                .ToList()
        };

        return Ok(response);
    }

    private static object ToDocumentResponse(Navi.ToolsAssets.Domain.Entities.Documents.ToolDocument document)
    {
        return new
        {
            document.Id,
            DocumentType = document.DocumentType.ToString(),
            document.FileName,
            document.ObjectKey,
            document.ContentType,
            document.SizeBytes,
            document.UploadedBy,
            document.UploadedAt,
            document.Description,
            DownloadUrl = $"/api/tools/documents/{document.Id}/download"
        };
    }
}
