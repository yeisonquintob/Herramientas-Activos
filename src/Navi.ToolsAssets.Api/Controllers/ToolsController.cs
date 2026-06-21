using Navi.ToolsAssets.Domain.Entities.Documents;
using Navi.ToolsAssets.Application.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;
    private readonly IDocumentStorageService _documentStorageService;

    public ToolsController(NaviToolsAssetsDbContext context, IDocumentStorageService documentStorageService)
    {
        _context = context;
        _documentStorageService = documentStorageService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTools()
    {
        var tools = await BuildToolQuery()
            .OrderBy(x => x.InternalCode)
            .ToListAsync();

        return Ok(tools);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetToolById(Guid id)
    {
        var tool = await BuildToolQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (tool is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con Id {id}."
            });
        }

        return Ok(tool);
    }

    [HttpGet("by-code/{internalCode}")]
    public async Task<IActionResult> GetToolByInternalCode(string internalCode)
    {
        var code = NormalizeCode(internalCode);

        var tool = await BuildToolQuery()
            .FirstOrDefaultAsync(x => x.InternalCode == code);

        if (tool is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con código {code}."
            });
        }

        return Ok(tool);
    }

    [HttpGet("by-branch/{branchCode}")]
    public async Task<IActionResult> GetToolsByBranch(string branchCode)
    {
        var code = NormalizeCode(branchCode);

        var tools = await BuildToolQuery()
            .Where(x => x.BranchCode == code)
            .OrderBy(x => x.InternalCode)
            .ToListAsync();

        return Ok(tools);
    }

    [HttpGet("specialized")]
    public async Task<IActionResult> GetSpecializedTools()
    {
        var tools = await BuildToolQuery()
            .Where(x => x.IsSpecialized)
            .OrderBy(x => x.InternalCode)
            .ToListAsync();

        return Ok(tools);
    }

    [HttpGet("non-specialized")]
    public async Task<IActionResult> GetNonSpecializedTools()
    {
        var tools = await BuildToolQuery()
            .Where(x => !x.IsSpecialized)
            .OrderBy(x => x.InternalCode)
            .ToListAsync();

        return Ok(tools);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTool([FromBody] CreateToolRequest request)
    {
        var validation = await ValidateCreateRequestAsync(request);

        if (validation is not null)
        {
            return validation;
        }

        var internalCode = NormalizeCode(request.InternalCode);
        var branchCode = NormalizeCode(request.BranchCode);
        var locationCode = NormalizeNullableCode(request.LocationCode);
        var responsibleCode = NormalizeNullableCode(request.ResponsibleEmployeeCode);
        var toolTypeCode = NormalizeNullableCode(request.ToolTypeCode);
        var toolCategoryCode = NormalizeNullableCode(request.ToolCategoryCode);

        var branch = await _context.Branches
            .Include(x => x.Zone)
            .FirstAsync(x => x.Code == branchCode);

        var location = locationCode is null
            ? null
            : await _context.ToolLocations.FirstOrDefaultAsync(x => x.Code == locationCode && x.BranchId == branch.Id);

        var responsible = responsibleCode is null
            ? null
            : await _context.ResponsiblePeople.FirstOrDefaultAsync(x => x.EmployeeCode == responsibleCode);

        var toolType = toolTypeCode is null
            ? null
            : await _context.ToolTypes.FirstOrDefaultAsync(x => x.Code == toolTypeCode);

        var toolCategory = toolCategoryCode is null
            ? null
            : await _context.ToolCategories.FirstOrDefaultAsync(x => x.Code == toolCategoryCode);

        var tool = new ToolAsset
        {
            InternalCode = internalCode,
            Name = (request.Name ?? string.Empty).Trim(),
            Description = request.Description,
            Brand = request.Brand,
            Model = request.Model,
            SerialNumber = NormalizeNullableCode(request.SerialNumber),
            FixedAssetCode = NormalizeNullableCode(request.FixedAssetCode),
            FenixCode = NormalizeNullableCode(request.FenixCode),
            AcquisitionDate = request.AcquisitionDate,
            UsefulLifeMonths = request.UsefulLifeMonths,
            UnitOfMeasure = string.IsNullOrWhiteSpace(request.UnitOfMeasure) ? "UND" : request.UnitOfMeasure.Trim().ToUpperInvariant(),
            Quantity = request.Quantity <= 0 ? 1 : request.Quantity,
            IsSpecialized = request.IsSpecialized,
            RequiresMaintenance = request.RequiresMaintenance,
            RequiresPreOperationalCheck = request.RequiresPreOperationalCheck,
            RequiresCertification = request.RequiresCertification,
            CertificationExpirationDate = request.CertificationExpirationDate,
            ZoneId = branch.ZoneId,
            BranchId = branch.Id,
            LocationId = location?.Id,
            ResponsiblePersonId = responsible?.Id,
            ToolTypeId = toolType?.Id,
            ToolCategoryId = toolCategory?.Id,
            OperationalStatus = ToolOperationalStatus.PendingValidation,
            PhysicalStatus = ToolPhysicalStatus.Good,
            CustodyStatus = ToolCustodyStatus.InWarehouse,
            ReconciliationStatus = ToolReconciliationStatus.Pending,
            SyncStatus = ToolSyncStatus.NotSynced,
            CreatedBy = "api"
        };

        _context.ToolAssets.Add(tool);

        await _context.SaveChangesAsync();

        var response = await BuildToolQuery()
            .FirstAsync(x => x.Id == tool.Id);

        return CreatedAtAction(nameof(GetToolById), new { id = tool.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTool(Guid id, [FromBody] UpdateToolRequest request)
    {
        var tool = await _context.ToolAssets.FirstOrDefaultAsync(x => x.Id == id);

        if (tool is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con Id {id}."
            });
        }

        var validation = await ValidateUpdateRequestAsync(id, request);

        if (validation is not null)
        {
            return validation;
        }

        var internalCode = NormalizeCode(request.InternalCode);
        var branchCode = NormalizeCode(request.BranchCode);
        var locationCode = NormalizeNullableCode(request.LocationCode);
        var responsibleCode = NormalizeNullableCode(request.ResponsibleEmployeeCode);
        var toolTypeCode = NormalizeNullableCode(request.ToolTypeCode);
        var toolCategoryCode = NormalizeNullableCode(request.ToolCategoryCode);

        var branch = await _context.Branches
            .Include(x => x.Zone)
            .FirstAsync(x => x.Code == branchCode);

        var location = locationCode is null
            ? null
            : await _context.ToolLocations.FirstOrDefaultAsync(x => x.Code == locationCode && x.BranchId == branch.Id);

        var responsible = responsibleCode is null
            ? null
            : await _context.ResponsiblePeople.FirstOrDefaultAsync(x => x.EmployeeCode == responsibleCode);

        var toolType = toolTypeCode is null
            ? null
            : await _context.ToolTypes.FirstOrDefaultAsync(x => x.Code == toolTypeCode);

        var toolCategory = toolCategoryCode is null
            ? null
            : await _context.ToolCategories.FirstOrDefaultAsync(x => x.Code == toolCategoryCode);

        tool.InternalCode = internalCode;
        tool.Name = (request.Name ?? string.Empty).Trim();
        tool.Description = request.Description;
        tool.Brand = request.Brand;
        tool.Model = request.Model;
        tool.SerialNumber = NormalizeNullableCode(request.SerialNumber);
        tool.FixedAssetCode = NormalizeNullableCode(request.FixedAssetCode);
        tool.FenixCode = NormalizeNullableCode(request.FenixCode);
        tool.AcquisitionDate = request.AcquisitionDate;
        tool.UsefulLifeMonths = request.UsefulLifeMonths;
        tool.UnitOfMeasure = string.IsNullOrWhiteSpace(request.UnitOfMeasure) ? "UND" : request.UnitOfMeasure.Trim().ToUpperInvariant();
        tool.Quantity = request.Quantity <= 0 ? 1 : request.Quantity;
        tool.IsSpecialized = request.IsSpecialized;
        tool.RequiresMaintenance = request.RequiresMaintenance;
        tool.RequiresPreOperationalCheck = request.RequiresPreOperationalCheck;
        tool.RequiresCertification = request.RequiresCertification;
        tool.CertificationExpirationDate = request.CertificationExpirationDate;
        tool.ZoneId = branch.ZoneId;
        tool.BranchId = branch.Id;
        tool.LocationId = location?.Id;
        tool.ResponsiblePersonId = responsible?.Id;
        tool.ToolTypeId = toolType?.Id;
        tool.ToolCategoryId = toolCategory?.Id;
        tool.UpdatedAt = DateTime.UtcNow;
        tool.UpdatedBy = "api";

        await _context.SaveChangesAsync();

        var response = await BuildToolQuery()
            .FirstAsync(x => x.Id == tool.Id);

        return Ok(response);
    }

    [HttpPatch("{id:guid}/classification")]
    public async Task<IActionResult> UpdateToolClassification(Guid id, [FromBody] UpdateToolClassificationRequest request)
    {
        var tool = await _context.ToolAssets.FirstOrDefaultAsync(x => x.Id == id);

        if (tool is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con Id {id}."
            });
        }

        tool.IsSpecialized = request.IsSpecialized;
        tool.UpdatedAt = DateTime.UtcNow;
        tool.UpdatedBy = "api";

        await _context.SaveChangesAsync();

        var response = await BuildToolQuery()
            .FirstAsync(x => x.Id == tool.Id);

        return Ok(response);
    }

    private async Task<IActionResult?> ValidateCreateRequestAsync(CreateToolRequest request)
    {
        var basicValidation = ValidateBasicRequest(request.InternalCode, request.Name, request.BranchCode);

        if (basicValidation is not null)
        {
            return basicValidation;
        }

        var internalCode = NormalizeCode(request.InternalCode);

        if (await _context.ToolAssets.AnyAsync(x => x.InternalCode == internalCode))
        {
            return Conflict(new
            {
                Message = $"Ya existe una herramienta con el código interno {internalCode}."
            });
        }

        return await ValidateReferencesAndDuplicatesAsync(
            id: null,
            request.BranchCode,
            request.LocationCode,
            request.ResponsibleEmployeeCode,
            request.ToolTypeCode,
            request.ToolCategoryCode,
            request.FenixCode,
            request.FixedAssetCode,
            request.SerialNumber);
    }

    private async Task<IActionResult?> ValidateUpdateRequestAsync(Guid id, UpdateToolRequest request)
    {
        var basicValidation = ValidateBasicRequest(request.InternalCode, request.Name, request.BranchCode);

        if (basicValidation is not null)
        {
            return basicValidation;
        }

        var internalCode = NormalizeCode(request.InternalCode);

        if (await _context.ToolAssets.AnyAsync(x => x.Id != id && x.InternalCode == internalCode))
        {
            return Conflict(new
            {
                Message = $"Ya existe otra herramienta con el código interno {internalCode}."
            });
        }

        return await ValidateReferencesAndDuplicatesAsync(
            id,
            request.BranchCode,
            request.LocationCode,
            request.ResponsibleEmployeeCode,
            request.ToolTypeCode,
            request.ToolCategoryCode,
            request.FenixCode,
            request.FixedAssetCode,
            request.SerialNumber);
    }

    private IActionResult? ValidateBasicRequest(string? internalCode, string? name, string? branchCode)
    {
        if (string.IsNullOrWhiteSpace(internalCode))
        {
            return BadRequest(new { Message = "El código interno es obligatorio." });
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return BadRequest(new { Message = "El nombre de la herramienta es obligatorio." });
        }

        if (string.IsNullOrWhiteSpace(branchCode))
        {
            return BadRequest(new { Message = "La sede es obligatoria." });
        }

        return null;
    }

    private async Task<IActionResult?> ValidateReferencesAndDuplicatesAsync(
        Guid? id,
        string? branchCode,
        string? locationCode,
        string? responsibleEmployeeCode,
        string? toolTypeCode,
        string? toolCategoryCode,
        string? fenixCode,
        string? fixedAssetCode,
        string? serialNumber)
    {
        var branchCodeNormalized = NormalizeCode(branchCode);

        var branch = await _context.Branches.FirstOrDefaultAsync(x => x.Code == branchCodeNormalized);

        if (branch is null)
        {
            return BadRequest(new
            {
                Message = $"No existe la sede {branchCodeNormalized}."
            });
        }

        var locationCodeNormalized = NormalizeNullableCode(locationCode);

        if (locationCodeNormalized is not null &&
            !await _context.ToolLocations.AnyAsync(x => x.Code == locationCodeNormalized && x.BranchId == branch.Id))
        {
            return BadRequest(new
            {
                Message = $"No existe la ubicación {locationCodeNormalized} para la sede {branchCodeNormalized}."
            });
        }

        var responsibleCodeNormalized = NormalizeNullableCode(responsibleEmployeeCode);

        if (responsibleCodeNormalized is not null &&
            !await _context.ResponsiblePeople.AnyAsync(x => x.EmployeeCode == responsibleCodeNormalized))
        {
            return BadRequest(new
            {
                Message = $"No existe el responsable {responsibleCodeNormalized}."
            });
        }

        var toolTypeCodeNormalized = NormalizeNullableCode(toolTypeCode);

        if (toolTypeCodeNormalized is not null &&
            !await _context.ToolTypes.AnyAsync(x => x.Code == toolTypeCodeNormalized))
        {
            return BadRequest(new
            {
                Message = $"No existe el tipo de herramienta {toolTypeCodeNormalized}."
            });
        }

        var toolCategoryCodeNormalized = NormalizeNullableCode(toolCategoryCode);

        if (toolCategoryCodeNormalized is not null &&
            !await _context.ToolCategories.AnyAsync(x => x.Code == toolCategoryCodeNormalized))
        {
            return BadRequest(new
            {
                Message = $"No existe la categoría de herramienta {toolCategoryCodeNormalized}."
            });
        }

        var fenixCodeNormalized = NormalizeNullableCode(fenixCode);

        if (fenixCodeNormalized is not null &&
            await _context.ToolAssets.AnyAsync(x => x.Id != id && x.FenixCode == fenixCodeNormalized))
        {
            return Conflict(new
            {
                Message = $"Ya existe una herramienta con código Fenix365 {fenixCodeNormalized}."
            });
        }

        var fixedAssetCodeNormalized = NormalizeNullableCode(fixedAssetCode);

        if (fixedAssetCodeNormalized is not null &&
            await _context.ToolAssets.AnyAsync(x => x.Id != id && x.FixedAssetCode == fixedAssetCodeNormalized))
        {
            return Conflict(new
            {
                Message = $"Ya existe una herramienta con activo fijo {fixedAssetCodeNormalized}."
            });
        }

        var serialNumberNormalized = NormalizeNullableCode(serialNumber);

        if (serialNumberNormalized is not null &&
            await _context.ToolAssets.AnyAsync(x => x.Id != id && x.SerialNumber == serialNumberNormalized))
        {
            return Conflict(new
            {
                Message = $"Ya existe una herramienta con serial {serialNumberNormalized}. Revisar posible duplicado."
            });
        }

        return null;
    }





    [HttpPost("{id:guid}/documents-legacy")]
    public async Task<IActionResult> UploadToolDocument(
        Guid id,
        IFormFile file,
        [FromForm] string? documentType,
        [FromForm] string? description,
        [FromForm] string? uploadedBy)
    {
        var tool = await _context.ToolAssets.FirstOrDefaultAsync(x => x.Id == id);

        if (tool is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con Id {id}."
            });
        }

        if (file is null || file.Length == 0)
        {
            return BadRequest(new
            {
                Message = "Debe adjuntar un archivo válido."
            });
        }

        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".xlsx", ".xls", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest(new
            {
                Message = "Tipo de archivo no permitido.",
                AllowedExtensions = allowedExtensions
            });
        }

        var user = string.IsNullOrWhiteSpace(uploadedBy)
            ? "api"
            : uploadedBy.Trim();

        var safeFileName = Path.GetFileName(file.FileName);
        var objectName = $"tools/{tool.InternalCode}/documents/{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension}";

        await using var stream = file.OpenReadStream();

        await _documentStorageService.UploadAsync(
            objectName,
            stream,
            string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType);

        var document = new ToolDocument
        {
            ToolAssetId = tool.Id,
            DocumentType = ParseDocumentType(documentType),
            FileName = safeFileName,
            ObjectKey = objectName,
            ContentType = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
            SizeBytes = file.Length,
            Description = description,
            UploadedBy = user,
            UploadedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user
        };

        _context.ToolDocuments.Add(document);

        _context.ToolLifeCycleEvents.Add(new()
        {
            ToolAssetId = tool.Id,
            EventType = "DocumentUploaded",
            Title = "Documento cargado",
            Description = $"Se cargó el documento {safeFileName}.",
            PreviousValue = null,
            NewValue = objectName,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = user
        });

        await _context.SaveChangesAsync();

        return Ok(new
        {
            document.Id,
            document.ToolAssetId,
            document.DocumentType,
            document.FileName,
            StoragePath = document.ObjectKey,
            document.ContentType,
            SizeInBytes = document.SizeBytes,
            document.Description,
            document.CreatedAt,
            document.CreatedBy
        });
    }

    [HttpGet("{id:guid}/documents-legacy")]
    public async Task<IActionResult> GetToolDocuments(Guid id)
    {
        var exists = await _context.ToolAssets.AnyAsync(x => x.Id == id);

        if (!exists)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con Id {id}."
            });
        }

        var documents = await _context.ToolDocuments
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.DocumentType,
                x.FileName,
                StoragePath = x.ObjectKey,
                x.ContentType,
                SizeInBytes = x.SizeBytes,
                x.Description,
                x.CreatedAt,
                x.CreatedBy
            })
            .ToListAsync();

        return Ok(documents);
    }

    [HttpGet("documents/{documentId:guid}/download-legacy")]
    public async Task<IActionResult> DownloadToolDocument(Guid documentId)
    {
        var document = await _context.ToolDocuments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentId);

        if (document is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró el documento con Id {documentId}."
            });
        }

        var exists = await _documentStorageService.ExistsAsync(document.ObjectKey);

        if (!exists)
        {
            return NotFound(new
            {
                Message = "El archivo no existe en MinIO.",
                document.ObjectKey
            });
        }

        var stream = await _documentStorageService.DownloadAsync(document.ObjectKey);

        return File(
            stream,
            string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType,
            document.FileName);
    }
    [HttpGet("{id:guid}/life-record")]
    public async Task<IActionResult> GetToolLifeRecord(Guid id)
    {
        var tool = await BuildToolQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (tool is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con Id {id}."
            });
        }

        var lifeCycleEvents = await _context.ToolLifeCycleEvents
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.EventType,
                x.Title,
                x.Description,
                x.PreviousValue,
                x.NewValue,
                EventDate = x.CreatedAt,
                PerformedBy = x.CreatedBy,
                Source = "API",
                x.CreatedAt,
                x.CreatedBy
            })
            .ToListAsync();

        var documents = await _context.ToolDocuments
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var loanItems = await _context.ToolLoanItems
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var damageReports = await _context.DamageReports
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var maintenanceRecords = await _context.MaintenanceRecords
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var physicalCountItems = await _context.PhysicalCountItems
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var reconciliationRecords = await _context.FenixReconciliationRecords
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var response = new
        {
            Tool = tool,
            CurrentState = new
            {
                tool.OperationalStatus,
                tool.PhysicalStatus,
                tool.CustodyStatus,
                tool.ReconciliationStatus,
                tool.SyncStatus,
                tool.IsSpecialized,
                tool.RequiresMaintenance,
                tool.RequiresPreOperationalCheck,
                tool.RequiresCertification
            },
            Location = new
            {
                tool.ZoneId,
                tool.ZoneCode,
                tool.ZoneName,
                tool.BranchId,
                tool.BranchCode,
                tool.BranchName,
                tool.LocationId,
                tool.LocationCode,
                tool.LocationName,
                tool.ResponsiblePersonId,
                tool.ResponsiblePersonName
            },
            TechnicalData = new
            {
                tool.Brand,
                tool.Model,
                tool.SerialNumber,
                tool.FixedAssetCode,
                tool.FenixCode,
                tool.UnitOfMeasure,
                tool.Quantity,
                tool.ToolTypeId,
                tool.ToolTypeName,
                tool.ToolCategoryId,
                tool.ToolCategoryName
            },
            LifeCycle = new
            {
                Events = lifeCycleEvents,
                Documents = documents,
                LoanItems = loanItems,
                DamageReports = damageReports,
                MaintenanceRecords = maintenanceRecords,
                PhysicalCountItems = physicalCountItems,
                ReconciliationRecords = reconciliationRecords
            },
            Summary = new
            {
                EventsCount = lifeCycleEvents.Count,
                DocumentsCount = documents.Count,
                LoanItemsCount = loanItems.Count,
                DamageReportsCount = damageReports.Count,
                MaintenanceRecordsCount = maintenanceRecords.Count,
                PhysicalCountItemsCount = physicalCountItems.Count,
                ReconciliationRecordsCount = reconciliationRecords.Count
            }
        };

        return Ok(response);
    }

    [HttpGet("by-code/{internalCode}/life-record")]
    public async Task<IActionResult> GetToolLifeRecordByInternalCode(string internalCode)
    {
        var code = NormalizeCode(internalCode);

        var toolId = await _context.ToolAssets
            .AsNoTracking()
            .Where(x => x.InternalCode == code)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (toolId == Guid.Empty)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con código interno {code}."
            });
        }

        return await GetToolLifeRecord(toolId);
    }
    [HttpGet("{id:guid}/life-cycle-events")]
    public async Task<IActionResult> GetToolLifeCycleEvents(Guid id)
    {
        var exists = await _context.ToolAssets.AnyAsync(x => x.Id == id);

        if (!exists)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con Id {id}."
            });
        }

        var events = await _context.ToolLifeCycleEvents
            .Where(x => x.ToolAssetId == id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.EventType,
                x.Title,
                x.Description,
                x.PreviousValue,
                x.NewValue,
                EventDate = x.CreatedAt,
                PerformedBy = x.CreatedBy,
                Source = "API",
                x.CreatedAt,
                x.CreatedBy
            })
            .ToListAsync();

        return Ok(events);
    }
    [HttpGet("operational-statuses")]
    public IActionResult GetOperationalStatuses()
    {
        var statuses = Enum.GetNames<ToolOperationalStatus>()
            .Select(x => new
            {
                Value = x,
                Label = GetOperationalStatusLabel(x)
            })
            .ToList();

        return Ok(statuses);
    }

    [HttpPatch("{id:guid}/operational-status")]
    public async Task<IActionResult> UpdateOperationalStatus(Guid id, [FromBody] UpdateToolOperationalStatusRequest request)
    {
        var tool = await _context.ToolAssets.FirstOrDefaultAsync(x => x.Id == id);

        if (tool is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con Id {id}."
            });
        }

        var normalizedStatus = NormalizeOperationalStatusName(request.OperationalStatus);

        if (normalizedStatus is null ||
            !Enum.TryParse<ToolOperationalStatus>(normalizedStatus, ignoreCase: true, out var newStatus))
        {
            return BadRequest(new
            {
                Message = "El estado operativo enviado no es válido.",
                AllowedValues = Enum.GetNames<ToolOperationalStatus>()
            });
        }

        var validation = ValidateOperationalStatusChange(tool, newStatus, request.Reason);

        if (validation is not null)
        {
            return validation;
        }

        var previousStatus = tool.OperationalStatus;
        var changedBy = string.IsNullOrWhiteSpace(request.ChangedBy)
            ? "api"
            : request.ChangedBy.Trim();

        tool.OperationalStatus = newStatus;
        tool.UpdatedAt = DateTime.UtcNow;
        tool.UpdatedBy = changedBy;

        RegisterOperationalStatusLifeCycleEvent(tool, previousStatus, newStatus, request, changedBy);

        await _context.SaveChangesAsync();

        var response = await BuildToolQuery()
            .FirstAsync(x => x.Id == tool.Id);

        return Ok(response);
    }



    private static ToolDocumentType ParseDocumentType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ToolDocumentType.Other;
        }

        var text = value.Trim();

        if (Enum.TryParse<ToolDocumentType>(text, ignoreCase: true, out var parsed))
        {
            return parsed;
        }

        return text.ToUpperInvariant() switch
        {
            "EVIDENCE" => ToolDocumentType.Other,
            "EVIDENCIA" => ToolDocumentType.Other,
            "DOCUMENTO" => ToolDocumentType.Other,
            "OTHER" => ToolDocumentType.Other,
            "OTRO" => ToolDocumentType.Other,
            _ => ToolDocumentType.Other
        };
    }
    private void RegisterOperationalStatusLifeCycleEvent(
        ToolAsset tool,
        ToolOperationalStatus previousStatus,
        ToolOperationalStatus newStatus,
        UpdateToolOperationalStatusRequest request,
        string changedBy)
    {
        if (previousStatus == newStatus)
        {
            return;
        }

        var previousStatusName = previousStatus.ToString();
        var newStatusName = newStatus.ToString();

        var reason = string.IsNullOrWhiteSpace(request.Reason)
            ? "Sin motivo registrado."
            : request.Reason.Trim();

        _context.ToolLifeCycleEvents.Add(new()
        {
            ToolAssetId = tool.Id,
            EventType = "OperationalStatusChanged",
            Title = "Cambio de estado operativo",
            Description = $"La herramienta cambió de {GetOperationalStatusLabel(previousStatusName)} a {GetOperationalStatusLabel(newStatusName)}. Motivo: {reason}",
            PreviousValue = previousStatusName,
            NewValue = newStatusName,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = changedBy
        });
    }
    private IActionResult? ValidateOperationalStatusChange(
        ToolAsset tool,
        ToolOperationalStatus newStatus,
        string? reason)
    {
        var currentStatus = tool.OperationalStatus.ToString();
        var targetStatus = newStatus.ToString();

        if (currentStatus.Equals(targetStatus, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (IsDisposedStatus(currentStatus))
        {
            return Conflict(new
            {
                Message = "La herramienta ya está dada de baja y no puede modificarse operativamente."
            });
        }

        if (IsLoanedStatus(targetStatus) && IsBlockedForLoan(currentStatus))
        {
            return Conflict(new
            {
                Message = $"La herramienta no puede pasar a prestada porque su estado actual es {GetOperationalStatusLabel(currentStatus)}."
            });
        }

        if (RequiresReasonForStatusChange(currentStatus, targetStatus) &&
            string.IsNullOrWhiteSpace(reason))
        {
            return BadRequest(new
            {
                Message = $"Para cambiar la herramienta de {GetOperationalStatusLabel(currentStatus)} a {GetOperationalStatusLabel(targetStatus)} se debe indicar una razón."
            });
        }

        return null;
    }

    private static bool IsDisposedStatus(string status)
    {
        return status.Equals("Disposed", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLoanedStatus(string status)
    {
        return status.Equals("Loaned", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Borrowed", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Prestada", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsBlockedForLoan(string status)
    {
        return status.Equals("Damaged", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("NotSuitable", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("InMaintenance", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("PendingDisposal", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Disposed", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("NotLocated", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Inconsistent", StringComparison.OrdinalIgnoreCase);
    }

    private static bool RequiresReasonForStatusChange(string currentStatus, string targetStatus)
    {
        if (targetStatus.Equals("Damaged", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (targetStatus.Equals("NotSuitable", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (targetStatus.Equals("InMaintenance", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (targetStatus.Equals("PendingDisposal", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (targetStatus.Equals("Disposed", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (targetStatus.Equals("Available", StringComparison.OrdinalIgnoreCase) &&
            IsBlockedForDirectAvailability(currentStatus))
        {
            return true;
        }

        return false;
    }

    private static bool IsBlockedForDirectAvailability(string status)
    {
        return status.Equals("Damaged", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("NotSuitable", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("InMaintenance", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("PendingDisposal", StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeOperationalStatusName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var text = value.Trim();

        return text.ToUpperInvariant() switch
        {
            "DISPONIBLE" => "Available",
            "ASIGNADA" => "Assigned",
            "PRESTADA" => "Loaned",
            "EN MANTENIMIENTO" => "InMaintenance",
            "MANTENIMIENTO" => "InMaintenance",
            "DAÑADA" => "Damaged",
            "DANADA" => "Damaged",
            "NO APTA" => "NotSuitable",
            "PENDIENTE DE VALIDACION" => "PendingValidation",
            "PENDIENTE DE VALIDACIÓN" => "PendingValidation",
            "INCONSISTENTE" => "Inconsistent",
            "NO LOCALIZADA" => "NotLocated",
            "PENDIENTE DE BAJA" => "PendingDisposal",
            "DADA DE BAJA" => "Disposed",
            _ => text
        };
    }

    private static string GetOperationalStatusLabel(string status)
    {
        return status switch
        {
            "Available" => "Disponible",
            "Assigned" => "Asignada",
            "Loaned" => "Prestada",
            "Borrowed" => "Prestada",
            "InMaintenance" => "En mantenimiento",
            "Damaged" => "Dañada",
            "NotSuitable" => "No apta",
            "PendingValidation" => "Pendiente de validación",
            "Inconsistent" => "Inconsistente",
            "NotLocated" => "No localizada",
            "PendingDisposal" => "Pendiente de baja",
            "Disposed" => "Dada de baja",
            _ => status
        };
    }

    private IQueryable<ToolAssetResponse> BuildToolQuery()
    {
        return _context.ToolAssets
            .AsNoTracking()
            .Include(x => x.Zone)
            .Include(x => x.Branch)
            .Include(x => x.Location)
            .Include(x => x.ResponsiblePerson)
            .Include(x => x.ToolType)
            .Include(x => x.ToolCategory)
            .Select(x => new ToolAssetResponse
            {
                Id = x.Id,
                InternalCode = x.InternalCode,
                Name = x.Name,
                Description = x.Description,
                Brand = x.Brand,
                Model = x.Model,
                SerialNumber = x.SerialNumber,
                FixedAssetCode = x.FixedAssetCode,
                FenixCode = x.FenixCode,
                UnitOfMeasure = x.UnitOfMeasure,
                Quantity = x.Quantity,
                IsSpecialized = x.IsSpecialized,
                RequiresMaintenance = x.RequiresMaintenance,
                RequiresPreOperationalCheck = x.RequiresPreOperationalCheck,
                RequiresCertification = x.RequiresCertification,
                OperationalStatus = x.OperationalStatus.ToString(),
                PhysicalStatus = x.PhysicalStatus.ToString(),
                CustodyStatus = x.CustodyStatus.ToString(),
                ReconciliationStatus = x.ReconciliationStatus.ToString(),
                SyncStatus = x.SyncStatus.ToString(),

                ZoneId = x.ZoneId,
                ZoneCode = x.Zone == null ? null : x.Zone.Code,
                ZoneName = x.Zone == null ? null : x.Zone.Name,

                BranchId = x.BranchId,
                BranchCode = x.Branch == null ? null : x.Branch.Code,
                BranchName = x.Branch == null ? null : x.Branch.Name,

                LocationId = x.LocationId,
                LocationCode = x.Location == null ? null : x.Location.Code,
                LocationName = x.Location == null ? null : x.Location.Name,

                ResponsiblePersonId = x.ResponsiblePersonId,
                ResponsiblePersonName = x.ResponsiblePerson == null ? null : x.ResponsiblePerson.FullName,

                ToolTypeId = x.ToolTypeId,
                ToolTypeName = x.ToolType == null ? null : x.ToolType.Name,

                ToolCategoryId = x.ToolCategoryId,
                ToolCategoryName = x.ToolCategory == null ? null : x.ToolCategory.Name
            });
    }

    private static string NormalizeCode(string? value)
    {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string? NormalizeNullableCode(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToUpperInvariant();
    }

    public class CreateToolRequest
    {
        public string? InternalCode { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? FixedAssetCode { get; set; }
        public string? FenixCode { get; set; }
        public DateTime? AcquisitionDate { get; set; }
        public int? UsefulLifeMonths { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal Quantity { get; set; } = 1;
        public bool IsSpecialized { get; set; }
        public bool RequiresMaintenance { get; set; }
        public bool RequiresPreOperationalCheck { get; set; }
        public bool RequiresCertification { get; set; }
        public DateTime? CertificationExpirationDate { get; set; }
        public string? BranchCode { get; set; }
        public string? LocationCode { get; set; }
        public string? ResponsibleEmployeeCode { get; set; }
        public string? ToolTypeCode { get; set; }
        public string? ToolCategoryCode { get; set; }
    }

    public sealed class UpdateToolRequest : CreateToolRequest
    {
    }

    public sealed class UpdateToolClassificationRequest
    {
        public bool IsSpecialized { get; set; }
    }


    public sealed class UpdateToolOperationalStatusRequest
    {
        public string? OperationalStatus { get; set; }
        public string? Reason { get; set; }
        public string? ChangedBy { get; set; }
    }
    private sealed class ToolAssetResponse
    {
        public Guid Id { get; set; }
        public string InternalCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? FixedAssetCode { get; set; }
        public string? FenixCode { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public bool IsSpecialized { get; set; }
        public bool RequiresMaintenance { get; set; }
        public bool RequiresPreOperationalCheck { get; set; }
        public bool RequiresCertification { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
        public string PhysicalStatus { get; set; } = string.Empty;
        public string CustodyStatus { get; set; } = string.Empty;
        public string ReconciliationStatus { get; set; } = string.Empty;
        public string SyncStatus { get; set; } = string.Empty;

        public Guid ZoneId { get; set; }
        public string? ZoneCode { get; set; }
        public string? ZoneName { get; set; }

        public Guid BranchId { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }

        public Guid? LocationId { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }

        public Guid? ResponsiblePersonId { get; set; }
        public string? ResponsiblePersonName { get; set; }

        public Guid? ToolTypeId { get; set; }
        public string? ToolTypeName { get; set; }

        public Guid? ToolCategoryId { get; set; }
        public string? ToolCategoryName { get; set; }
    }
}
























