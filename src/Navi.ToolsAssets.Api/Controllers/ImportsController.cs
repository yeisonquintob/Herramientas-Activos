using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using Navi.ToolsAssets.Domain.Entities.Imports;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Enums;
using System.Text.Json;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/imports")]
public class ImportsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;
    private readonly IConfiguration _configuration;

    public ImportsController(NaviToolsAssetsDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetImports()
    {
        var imports = await _context.ImportBatches
            .AsNoTracking()
            .Include(x => x.Rows)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new
            {
                x.Id,
                x.ImportNumber,
                x.SourceType,
                x.FileName,
                x.ObjectKey,
                x.Status,
                x.TotalRows,
                x.ValidRows,
                x.ErrorRows,
                x.CreatedTools,
                x.UpdatedTools,
                x.DuplicateRows,
                x.Summary,
                x.ProcessedAt,
                x.ProcessedBy,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync();

        return Ok(imports);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetImportById(Guid id)
    {
        var import = await _context.ImportBatches
            .AsNoTracking()
            .Include(x => x.Rows)
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ImportNumber,
                x.SourceType,
                x.FileName,
                x.ObjectKey,
                x.Status,
                x.TotalRows,
                x.ValidRows,
                x.ErrorRows,
                x.CreatedTools,
                x.UpdatedTools,
                x.DuplicateRows,
                x.Summary,
                x.ProcessedAt,
                x.ProcessedBy,
                Rows = x.Rows
                    .OrderBy(r => r.RowNumber)
                    .Select(r => new
                    {
                        r.Id,
                        r.RowNumber,
                        r.InternalCode,
                        r.FenixCode,
                        r.FixedAssetCode,
                        r.SerialNumber,
                        r.ToolName,
                        r.BranchCode,
                        r.ResponsibleName,
                        r.OperationalStatus,
                        r.ResultStatus,
                        r.Message,
                        r.RawDataJson
                    }),
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .FirstOrDefaultAsync();

        if (import is null)
        {
            return NotFound(new { Message = $"No se encontró la importación con Id {id}." });
        }

        return Ok(import);
    }

    [HttpGet("{id:guid}/rows")]
    public async Task<IActionResult> GetImportRows(Guid id)
    {
        var exists = await _context.ImportBatches.AnyAsync(x => x.Id == id);

        if (!exists)
        {
            return NotFound(new { Message = $"No se encontró la importación con Id {id}." });
        }

        var rows = await _context.ImportRows
            .AsNoTracking()
            .Where(x => x.ImportBatchId == id)
            .OrderBy(x => x.RowNumber)
            .Select(x => new
            {
                x.Id,
                x.ImportBatchId,
                x.RowNumber,
                x.InternalCode,
                x.FenixCode,
                x.FixedAssetCode,
                x.SerialNumber,
                x.ToolName,
                x.BranchCode,
                x.ResponsibleName,
                x.OperationalStatus,
                x.ResultStatus,
                x.Message,
                x.RawDataJson
            })
            .ToListAsync();

        return Ok(rows);
    }

    [HttpPost("excel")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadExcel([FromForm] ImportExcelRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { Message = "Debe adjuntar un archivo Excel." });
        }

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

        if (extension is not ".xlsx" and not ".xlsm" and not ".xls")
        {
            return BadRequest(new { Message = "El archivo debe ser Excel: .xlsx, .xlsm o .xls." });
        }

        var processedBy = string.IsNullOrWhiteSpace(request.ProcessedBy)
            ? "api"
            : request.ProcessedBy.Trim();

        var sourceType = string.IsNullOrWhiteSpace(request.SourceType)
            ? "Unknown"
            : request.SourceType.Trim();

        await using var excelStream = new MemoryStream();
        await request.File.CopyToAsync(excelStream, cancellationToken);
        excelStream.Position = 0;

        var objectKey = await UploadToMinioAsync(request.File, sourceType, cancellationToken);

        var importBatch = new ImportBatch
        {
            ImportNumber = $"IMP-{DateTime.UtcNow:yyyyMMddHHmmss}",
            SourceType = sourceType,
            FileName = Path.GetFileName(request.File.FileName),
            ObjectKey = objectKey,
            Status = "Processing",
            ProcessedAt = DateTime.UtcNow,
            ProcessedBy = processedBy,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = processedBy
        };

        _context.ImportBatches.Add(importBatch);

        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheets.FirstOrDefault();

        if (worksheet is null)
        {
            importBatch.Status = "Failed";
            importBatch.Summary = "El archivo no contiene hojas.";
            await _context.SaveChangesAsync(cancellationToken);

            return BadRequest(new { Message = importBatch.Summary });
        }

        var headerRow = worksheet.FirstRowUsed();

        if (headerRow is null)
        {
            importBatch.Status = "Failed";
            importBatch.Summary = "El archivo no contiene encabezados.";
            await _context.SaveChangesAsync(cancellationToken);

            return BadRequest(new { Message = importBatch.Summary });
        }

        var headers = headerRow.CellsUsed()
            .Select(cell => new ExcelHeader(
                ColumnNumber: cell.Address.ColumnNumber,
                Name: cell.GetString().Trim(),
                NormalizedName: NormalizeHeader(cell.GetString())))
            .Where(x => !string.IsNullOrWhiteSpace(x.Name))
            .ToList();

        var dataRows = worksheet.RowsUsed()
            .Where(row => row.RowNumber() > headerRow.RowNumber())
            .ToList();

        foreach (var row in dataRows)
        {
            var internalCode = NormalizeCodeOrNull(GetCellValue(row, headers, "codigo navi", "codigo interno", "internal code", "código interno", "codigo herramienta", "code"));
            var fenixCode = NormalizeCodeOrNull(GetCellValue(row, headers, "codigo fenix", "fenix code", "codigo fenix365", "código fenix365"));
            var fixedAssetCode = NormalizeCodeOrNull(GetCellValue(row, headers, "activo fijo", "fixed asset", "placa", "placa activo", "asset code"));
            var serialNumber = NormalizeCodeOrNull(GetCellValue(row, headers, "serial", "serie", "serial number"));
            var toolName = GetCellValue(row, headers, "nombre", "herramienta", "tool name", "descripcion", "descripción");
            var branchCode = NormalizeCodeOrNull(GetCellValue(row, headers, "sede", "branch", "branch code", "centro", "ubicacion sede"));
            var responsibleName = GetCellValue(row, headers, "responsable", "responsible", "custodio", "asignado a");
            var operationalStatus = GetCellValue(row, headers, "estado", "status", "estado operativo");

            var rawData = BuildRawData(row, headers);

            var importRow = new ImportRow
            {
                ImportBatch = importBatch,
                RowNumber = row.RowNumber(),
                InternalCode = internalCode,
                FenixCode = fenixCode,
                FixedAssetCode = fixedAssetCode,
                SerialNumber = serialNumber,
                ToolName = toolName,
                BranchCode = branchCode,
                ResponsibleName = responsibleName,
                OperationalStatus = operationalStatus,
                RawDataJson = JsonSerializer.Serialize(rawData),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = processedBy
            };

            await AnalyzeImportRowAsync(importRow, cancellationToken);

            importBatch.Rows.Add(importRow);
        }

        importBatch.TotalRows = importBatch.Rows.Count;
        importBatch.ErrorRows = importBatch.Rows.Count(x => x.ResultStatus == "Error");
        importBatch.ValidRows = importBatch.TotalRows - importBatch.ErrorRows;
        importBatch.DuplicateRows = importBatch.Rows.Count(x => x.ResultStatus is "Existing" or "PossibleDuplicate");
        importBatch.CreatedTools = 0;
        importBatch.UpdatedTools = 0;
        importBatch.Status = importBatch.ErrorRows > 0 ? "CompletedWithErrors" : "Completed";
        importBatch.Summary = $"Filas: {importBatch.TotalRows}. Válidas: {importBatch.ValidRows}. Errores: {importBatch.ErrorRows}. Duplicados/Existentes: {importBatch.DuplicateRows}.";
        importBatch.ProcessedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetImportById), new { id = importBatch.Id }, new
        {
            importBatch.Id,
            importBatch.ImportNumber,
            importBatch.SourceType,
            importBatch.FileName,
            importBatch.ObjectKey,
            importBatch.Status,
            importBatch.TotalRows,
            importBatch.ValidRows,
            importBatch.ErrorRows,
            importBatch.DuplicateRows,
            importBatch.CreatedTools,
            importBatch.UpdatedTools,
            importBatch.Summary,
            importBatch.ProcessedAt,
            importBatch.ProcessedBy
        });
    }


    [HttpPost("{id:guid}/apply-new-candidates")]
    public async Task<IActionResult> ApplyNewCandidates(Guid id, [FromBody] ApplyImportCandidatesRequest request, CancellationToken cancellationToken)
    {
        var importBatch = await _context.ImportBatches
            .Include(x => x.Rows)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (importBatch is null)
        {
            return NotFound(new { Message = $"No se encontró la importación con Id {id}." });
        }

        var candidateRows = importBatch.Rows
            .Where(x => string.Equals(x.ResultStatus, "NewCandidate", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.RowNumber)
            .ToList();

        if (candidateRows.Count == 0)
        {
            return BadRequest(new { Message = "La importación no tiene filas NewCandidate para crear herramientas." });
        }

        var processedBy = string.IsNullOrWhiteSpace(request.ProcessedBy)
            ? "api"
            : request.ProcessedBy.Trim();

        var defaultToolTypeCode = string.IsNullOrWhiteSpace(request.DefaultToolTypeCode)
            ? "HERR"
            : NormalizeCode(request.DefaultToolTypeCode);

        var defaultToolCategoryCode = string.IsNullOrWhiteSpace(request.DefaultToolCategoryCode)
            ? "OTRO"
            : NormalizeCode(request.DefaultToolCategoryCode);

        var defaultLocationSuffix = string.IsNullOrWhiteSpace(request.DefaultLocationSuffix)
            ? "BOD-HERR"
            : request.DefaultLocationSuffix.Trim().ToUpperInvariant();

        var createdTools = new List<object>();
        var errorRows = new List<object>();

        foreach (var row in candidateRows)
        {
            if (string.IsNullOrWhiteSpace(row.InternalCode))
            {
                row.ResultStatus = "Error";
                row.Message = "No se puede crear la herramienta porque la fila no tiene código interno.";
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.ToolName))
            {
                row.ResultStatus = "Error";
                row.Message = "No se puede crear la herramienta porque la fila no tiene nombre.";
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            var branchCode = !string.IsNullOrWhiteSpace(row.BranchCode)
                ? NormalizeCode(row.BranchCode)
                : NormalizeCode(request.DefaultBranchCode ?? "");

            if (string.IsNullOrWhiteSpace(branchCode))
            {
                row.ResultStatus = "Error";
                row.Message = "No se puede crear la herramienta porque la fila no tiene sede y no se indicó sede por defecto.";
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            var alreadyExists = await FindExistingToolAsync(
                row.InternalCode,
                row.FenixCode,
                row.FixedAssetCode,
                row.SerialNumber,
                cancellationToken);

            if (alreadyExists is not null)
            {
                row.ResultStatus = "Existing";
                row.Message = $"No se creó porque ya existe una herramienta relacionada en NAVI: {alreadyExists.InternalCode}.";
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                continue;
            }

            var branch = await _context.Branches
                .FirstOrDefaultAsync(x => x.Code == branchCode, cancellationToken);

            if (branch is null)
            {
                row.ResultStatus = "Error";
                row.Message = $"No se puede crear la herramienta porque no existe la sede {branchCode}.";
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            var locationCode = $"{branch.Code}-{defaultLocationSuffix}";

            var location = await _context.ToolLocations
                .FirstOrDefaultAsync(x => x.BranchId == branch.Id && x.Code == locationCode, cancellationToken);

            location ??= await _context.ToolLocations
                .FirstOrDefaultAsync(x => x.BranchId == branch.Id, cancellationToken);

            if (location is null)
            {
                row.ResultStatus = "Error";
                row.Message = $"No se puede crear la herramienta porque la sede {branch.Code} no tiene ubicación configurada.";
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            var toolType = await _context.ToolTypes
                .FirstOrDefaultAsync(x => x.Code == defaultToolTypeCode, cancellationToken);

            toolType ??= await _context.ToolTypes
                .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

            if (toolType is null)
            {
                row.ResultStatus = "Error";
                row.Message = "No se puede crear la herramienta porque no existe tipo de herramienta configurado.";
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            var toolCategory = await _context.ToolCategories
                .FirstOrDefaultAsync(x => x.Code == defaultToolCategoryCode, cancellationToken);

            toolCategory ??= await _context.ToolCategories
                .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

            if (toolCategory is null)
            {
                row.ResultStatus = "Error";
                row.Message = "No se puede crear la herramienta porque no existe categoría configurada.";
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            var tool = new ToolAsset
            {
                InternalCode = NormalizeCode(row.InternalCode),
                Name = row.ToolName.Trim(),
                Description = $"Herramienta creada desde importación {importBatch.ImportNumber}.",
                SerialNumber = row.SerialNumber,
                FixedAssetCode = row.FixedAssetCode,
                FenixCode = row.FenixCode,
                UnitOfMeasure = "UND",
                Quantity = 1,
                IsSpecialized = request.IsSpecialized ?? false,
                RequiresMaintenance = request.RequiresMaintenance ?? false,
                RequiresPreOperationalCheck = request.RequiresPreOperationalCheck ?? false,
                RequiresCertification = request.RequiresCertification ?? false,
                ZoneId = branch.ZoneId,
                BranchId = branch.Id,
                LocationId = location.Id,
                ToolTypeId = toolType.Id,
                ToolCategoryId = toolCategory.Id,
                OperationalStatus = ToolOperationalStatus.PendingValidation,
                PhysicalStatus = ToolPhysicalStatus.Good,
                CustodyStatus = ToolCustodyStatus.InWarehouse,
                ReconciliationStatus = ToolReconciliationStatus.Pending,
                SyncStatus = ToolSyncStatus.NotSynced,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = processedBy
            };

            _context.ToolAssets.Add(tool);

            AddToolLifeCycleEvent(
                tool.Id,
                "ToolCreatedFromImport",
                "Herramienta creada desde importación",
                $"Herramienta creada desde la importación {importBatch.ImportNumber}, fila {row.RowNumber}.",
                null,
                tool.InternalCode,
                processedBy);

            row.ResultStatus = "Created";
            row.Message = $"Herramienta creada en NAVI con código {tool.InternalCode}.";
            row.UpdatedAt = DateTime.UtcNow;
            row.UpdatedBy = processedBy;

            importBatch.CreatedTools++;

            createdTools.Add(new
            {
                tool.Id,
                tool.InternalCode,
                tool.Name,
                BranchCode = branch.Code,
                LocationCode = location.Code,
                ToolTypeCode = toolType.Code,
                ToolCategoryCode = toolCategory.Code
            });
        }

        importBatch.UpdatedAt = DateTime.UtcNow;
        importBatch.UpdatedBy = processedBy;
        importBatch.Status = errorRows.Count > 0 ? "AppliedWithErrors" : "Applied";
        importBatch.ErrorRows = importBatch.Rows.Count(x => x.ResultStatus == "Error");
        importBatch.ValidRows = importBatch.Rows.Count(x => x.ResultStatus != "Error");
        importBatch.Summary = $"Aplicación de candidatos finalizada. Creadas: {createdTools.Count}. Errores: {errorRows.Count}.";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            importBatch.Id,
            importBatch.ImportNumber,
            importBatch.Status,
            importBatch.TotalRows,
            importBatch.ValidRows,
            importBatch.ErrorRows,
            TotalCreatedTools = importBatch.CreatedTools,
            importBatch.UpdatedTools,
            importBatch.DuplicateRows,
            importBatch.Summary,
            CreatedCount = createdTools.Count,
            ErrorCount = errorRows.Count,
            CreatedToolDetails = createdTools,
            ErrorRowDetails = errorRows
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

    [HttpGet("{id:guid}/summary")]
    public async Task<IActionResult> GetImportSummary(Guid id)
    {
        var importBatch = await _context.ImportBatches
            .AsNoTracking()
            .Include(x => x.Rows)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (importBatch is null)
        {
            return NotFound(new { Message = $"No se encontró la importación con Id {id}." });
        }

        var statusSummary = importBatch.Rows
            .GroupBy(x => x.ResultStatus)
            .Select(x => new
            {
                Status = x.Key,
                Count = x.Count()
            })
            .OrderBy(x => x.Status)
            .ToList();

        return Ok(new
        {
            importBatch.Id,
            importBatch.ImportNumber,
            importBatch.SourceType,
            importBatch.FileName,
            importBatch.Status,
            importBatch.TotalRows,
            importBatch.ValidRows,
            importBatch.ErrorRows,
            importBatch.CreatedTools,
            importBatch.UpdatedTools,
            importBatch.DuplicateRows,
            importBatch.Summary,
            importBatch.ProcessedAt,
            importBatch.ProcessedBy,
            StatusSummary = statusSummary
        });
    }

    [HttpGet("{id:guid}/rows/by-status/{status}")]
    public async Task<IActionResult> GetImportRowsByStatus(Guid id, string status)
    {
        var exists = await _context.ImportBatches.AnyAsync(x => x.Id == id);

        if (!exists)
        {
            return NotFound(new { Message = $"No se encontró la importación con Id {id}." });
        }

        var normalizedStatus = status.Trim();

        var rows = await _context.ImportRows
            .AsNoTracking()
            .Where(x => x.ImportBatchId == id && x.ResultStatus == normalizedStatus)
            .OrderBy(x => x.RowNumber)
            .Select(x => new
            {
                x.Id,
                x.ImportBatchId,
                x.RowNumber,
                x.InternalCode,
                x.FenixCode,
                x.FixedAssetCode,
                x.SerialNumber,
                x.ToolName,
                x.BranchCode,
                x.ResponsibleName,
                x.OperationalStatus,
                x.ResultStatus,
                x.Message,
                x.RawDataJson,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync();

        return Ok(rows);
    }

    [HttpPatch("rows/{rowId:guid}/mark-reviewed")]
    public async Task<IActionResult> MarkImportRowReviewed(Guid rowId, [FromBody] ImportRowActionRequest request, CancellationToken cancellationToken)
    {
        var row = await _context.ImportRows
            .Include(x => x.ImportBatch)
            .FirstOrDefaultAsync(x => x.Id == rowId, cancellationToken);

        if (row is null)
        {
            return NotFound(new { Message = $"No se encontró la fila de importación con Id {rowId}." });
        }

        var changedBy = GetImportRowActionUser(request);
        var previousStatus = row.ResultStatus;

        row.ResultStatus = "Reviewed";
        row.Message = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Fila revisada manualmente. Estado anterior: {previousStatus}."
            : request.Notes.Trim();

        row.UpdatedAt = DateTime.UtcNow;
        row.UpdatedBy = changedBy;

        await UpdateImportBatchCountersAsync(row.ImportBatchId, changedBy, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            row.Id,
            row.ImportBatchId,
            row.RowNumber,
            row.InternalCode,
            PreviousStatus = previousStatus,
            row.ResultStatus,
            row.Message,
            row.UpdatedBy
        });
    }

    [HttpPatch("rows/{rowId:guid}/mark-ignored")]
    public async Task<IActionResult> MarkImportRowIgnored(Guid rowId, [FromBody] ImportRowActionRequest request, CancellationToken cancellationToken)
    {
        var row = await _context.ImportRows
            .Include(x => x.ImportBatch)
            .FirstOrDefaultAsync(x => x.Id == rowId, cancellationToken);

        if (row is null)
        {
            return NotFound(new { Message = $"No se encontró la fila de importación con Id {rowId}." });
        }

        var changedBy = GetImportRowActionUser(request);
        var previousStatus = row.ResultStatus;

        row.ResultStatus = "Ignored";
        row.Message = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Fila ignorada manualmente. Estado anterior: {previousStatus}."
            : request.Notes.Trim();

        row.UpdatedAt = DateTime.UtcNow;
        row.UpdatedBy = changedBy;

        await UpdateImportBatchCountersAsync(row.ImportBatchId, changedBy, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            row.Id,
            row.ImportBatchId,
            row.RowNumber,
            row.InternalCode,
            PreviousStatus = previousStatus,
            row.ResultStatus,
            row.Message,
            row.UpdatedBy
        });
    }

    [HttpPatch("rows/{rowId:guid}/link-to-tool/{toolId:guid}")]
    public async Task<IActionResult> LinkImportRowToTool(Guid rowId, Guid toolId, [FromBody] ImportRowActionRequest request, CancellationToken cancellationToken)
    {
        var row = await _context.ImportRows
            .Include(x => x.ImportBatch)
            .FirstOrDefaultAsync(x => x.Id == rowId, cancellationToken);

        if (row is null)
        {
            return NotFound(new { Message = $"No se encontró la fila de importación con Id {rowId}." });
        }

        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == toolId, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = $"No se encontró la herramienta con Id {toolId}." });
        }

        var changedBy = GetImportRowActionUser(request);
        var previousStatus = row.ResultStatus;

        row.ResultStatus = "Linked";
        row.Message = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Fila asociada manualmente a la herramienta NAVI {tool.InternalCode}. Estado anterior: {previousStatus}."
            : request.Notes.Trim();

        row.UpdatedAt = DateTime.UtcNow;
        row.UpdatedBy = changedBy;

        AddToolLifeCycleEvent(
            tool.Id,
            "ImportRowLinkedToTool",
            "Fila de importación asociada a herramienta",
            $"Fila {row.RowNumber} de la importación {row.ImportBatch?.ImportNumber} asociada manualmente a esta herramienta.",
            previousStatus,
            row.ResultStatus,
            changedBy);

        await UpdateImportBatchCountersAsync(row.ImportBatchId, changedBy, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            row.Id,
            row.ImportBatchId,
            row.RowNumber,
            row.InternalCode,
            PreviousStatus = previousStatus,
            row.ResultStatus,
            row.Message,
            LinkedTool = new
            {
                tool.Id,
                tool.InternalCode,
                tool.Name
            },
            row.UpdatedBy
        });
    }

    private static string GetImportRowActionUser(ImportRowActionRequest request)
    {
        return string.IsNullOrWhiteSpace(request.ActionBy)
            ? "api"
            : request.ActionBy.Trim();
    }

    private async Task UpdateImportBatchCountersAsync(Guid importBatchId, string changedBy, CancellationToken cancellationToken)
    {
        var importBatch = await _context.ImportBatches
            .Include(x => x.Rows)
            .FirstOrDefaultAsync(x => x.Id == importBatchId, cancellationToken);

        if (importBatch is null)
        {
            return;
        }

        importBatch.TotalRows = importBatch.Rows.Count;
        importBatch.ErrorRows = importBatch.Rows.Count(x => x.ResultStatus == "Error");
        importBatch.ValidRows = importBatch.Rows.Count(x => x.ResultStatus != "Error");
        importBatch.CreatedTools = importBatch.Rows.Count(x => x.ResultStatus == "Created");
        importBatch.DuplicateRows = importBatch.Rows.Count(x => x.ResultStatus is "Existing" or "PossibleDuplicate" or "Linked");

        importBatch.Status = importBatch.ErrorRows > 0
            ? "ReviewedWithErrors"
            : "Reviewed";

        importBatch.Summary = $"Revisión de importación actualizada. Filas: {importBatch.TotalRows}. Creadas: {importBatch.CreatedTools}. Errores: {importBatch.ErrorRows}. Existentes/Duplicadas/Asociadas: {importBatch.DuplicateRows}.";
        importBatch.UpdatedAt = DateTime.UtcNow;
        importBatch.UpdatedBy = changedBy;
    }
    private async Task AnalyzeImportRowAsync(ImportRow row, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(row.InternalCode)
            && string.IsNullOrWhiteSpace(row.FenixCode)
            && string.IsNullOrWhiteSpace(row.FixedAssetCode)
            && string.IsNullOrWhiteSpace(row.SerialNumber))
        {
            row.ResultStatus = "Error";
            row.Message = "La fila no tiene código interno, código Fenix365, activo fijo ni serial.";
            return;
        }

        var existingTool = await FindExistingToolAsync(
            row.InternalCode,
            row.FenixCode,
            row.FixedAssetCode,
            row.SerialNumber,
            cancellationToken);

        if (existingTool is null)
        {
            row.ResultStatus = "NewCandidate";
            row.Message = "Herramienta candidata para creación. No existe coincidencia en NAVI.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(row.InternalCode)
            && string.Equals(existingTool.InternalCode, row.InternalCode, StringComparison.OrdinalIgnoreCase))
        {
            var differences = new List<string>();

            if (!string.IsNullOrWhiteSpace(row.BranchCode)
                && existingTool.Branch != null
                && !string.Equals(existingTool.Branch.Code, row.BranchCode, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add($"Sede diferente. NAVI='{existingTool.Branch.Code}' / Archivo='{row.BranchCode}'");
            }

            if (!string.IsNullOrWhiteSpace(row.ToolName)
                && !string.Equals(existingTool.Name, row.ToolName, StringComparison.OrdinalIgnoreCase))
            {
                differences.Add($"Nombre diferente. NAVI='{existingTool.Name}' / Archivo='{row.ToolName}'");
            }

            row.ResultStatus = differences.Count == 0 ? "Existing" : "Inconsistent";
            row.Message = differences.Count == 0
                ? $"La herramienta ya existe en NAVI con código {existingTool.InternalCode}."
                : string.Join(" | ", differences);

            return;
        }

        row.ResultStatus = "PossibleDuplicate";
        row.Message = $"Posible duplicado con herramienta NAVI {existingTool.InternalCode}. Coincidencia por Fenix, activo fijo o serial.";
    }

    private async Task<ToolAsset?> FindExistingToolAsync(
        string? internalCode,
        string? fenixCode,
        string? fixedAssetCode,
        string? serialNumber,
        CancellationToken cancellationToken)
    {
        return await _context.ToolAssets
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x =>
                (!string.IsNullOrWhiteSpace(internalCode) && x.InternalCode == internalCode)
                || (!string.IsNullOrWhiteSpace(fenixCode) && x.FenixCode == fenixCode)
                || (!string.IsNullOrWhiteSpace(fixedAssetCode) && x.FixedAssetCode == fixedAssetCode)
                || (!string.IsNullOrWhiteSpace(serialNumber) && x.SerialNumber == serialNumber),
                cancellationToken);
    }

    private async Task<string> UploadToMinioAsync(IFormFile file, string sourceType, CancellationToken cancellationToken)
    {
        var endpoint = _configuration["Minio:Endpoint"] ?? "localhost:9100";
        var accessKey = _configuration["Minio:AccessKey"] ?? "naviadmin";
        var secretKey = _configuration["Minio:SecretKey"] ?? "Navitrans_2026*Minio!";
        var bucketName = _configuration["Minio:BucketName"] ?? "navi-tools-documents";

        var useSsl = bool.TryParse(_configuration["Minio:UseSsl"], out var parsedUseSsl) && parsedUseSsl;

        var minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .WithSSL(useSsl)
            .Build();

        var bucketExists = await minioClient.BucketExistsAsync(
            new BucketExistsArgs().WithBucket(bucketName),
            cancellationToken);

        if (!bucketExists)
        {
            await minioClient.MakeBucketAsync(
                new MakeBucketArgs().WithBucket(bucketName),
                cancellationToken);
        }

        var safeSourceType = NormalizeFilePart(sourceType);
        var safeFileName = NormalizeFilePart(Path.GetFileName(file.FileName));
        var objectKey = $"imports/{safeSourceType}/{DateTime.UtcNow:yyyyMMddHHmmssfff}_{safeFileName}";

        await using var stream = file.OpenReadStream();

        await minioClient.PutObjectAsync(
            new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectKey)
                .WithStreamData(stream)
                .WithObjectSize(file.Length)
                .WithContentType(string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType),
            cancellationToken);

        return objectKey;
    }

    private static Dictionary<string, string?> BuildRawData(IXLRow row, List<ExcelHeader> headers)
    {
        var raw = new Dictionary<string, string?>();

        foreach (var header in headers)
        {
            raw[header.Name] = row.Cell(header.ColumnNumber).GetFormattedString().Trim();
        }

        return raw;
    }

    private static string? GetCellValue(IXLRow row, List<ExcelHeader> headers, params string[] possibleHeaders)
    {
        var normalizedHeaders = possibleHeaders
            .Select(NormalizeHeader)
            .ToHashSet();

        var header = headers.FirstOrDefault(x => normalizedHeaders.Contains(x.NormalizedName));

        if (header is null)
        {
            return null;
        }

        var value = row.Cell(header.ColumnNumber).GetFormattedString().Trim();

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static string NormalizeHeader(string value)
    {
        return value
            .Trim()
            .ToLowerInvariant()
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ñ", "n")
            .Replace("_", " ")
            .Replace("-", " ");
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string? NormalizeCodeOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().ToUpperInvariant();
    }

    private static string NormalizeFilePart(string value)
    {
        var invalidChars = Path.GetInvalidFileNameChars();

        var cleaned = new string(value
            .Select(ch => invalidChars.Contains(ch) ? '_' : ch)
            .ToArray());

        return cleaned
            .Trim()
            .Replace(" ", "_")
            .Replace("__", "_");
    }

    private sealed record ExcelHeader(int ColumnNumber, string Name, string NormalizedName);
}

public sealed class ImportRowActionRequest
{
    public string? ActionBy { get; set; }

    public string? Notes { get; set; }
}

public sealed class ApplyImportCandidatesRequest
{
    public string? DefaultBranchCode { get; set; }

    public string? DefaultToolTypeCode { get; set; }

    public string? DefaultToolCategoryCode { get; set; }

    public string? DefaultLocationSuffix { get; set; }

    public bool? IsSpecialized { get; set; }

    public bool? RequiresMaintenance { get; set; }

    public bool? RequiresPreOperationalCheck { get; set; }

    public bool? RequiresCertification { get; set; }

    public string? ProcessedBy { get; set; }
}

public sealed class ImportExcelRequest
{
    public IFormFile? File { get; set; }

    public string? SourceType { get; set; }

    public string? ProcessedBy { get; set; }
}



