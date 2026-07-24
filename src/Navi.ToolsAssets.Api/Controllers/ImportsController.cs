using System.Globalization;
using System.Text;
using System.Text.Json;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Domain.Entities.Imports;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;
using Navi.ToolsAssets.Shared.Security;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/imports")]
[RequirePermission(PermissionCodes.ImportsView)]
public class ImportsController : ControllerBase
{
    private const long DefaultMaximumFileSizeBytes = 10 * 1024 * 1024;
    private const int DefaultMaximumRows = 10_000;

    private static readonly HashSet<string> AllowedExcelContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/octet-stream",
        "application/zip"
    };

    private readonly NaviToolsAssetsDbContext _context;
    private readonly IConfiguration _configuration;

    public ImportsController(NaviToolsAssetsDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetImports(CancellationToken cancellationToken)
    {
        var imports = await _context.ImportBatches
            .AsNoTracking()
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
                x.WarningRows,
                x.IgnoredRows,
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
            .ToListAsync(cancellationToken);

        return Ok(imports);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetImportById(Guid id, CancellationToken cancellationToken)
    {
        var import = await _context.ImportBatches
            .AsNoTracking()
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
                x.WarningRows,
                x.IgnoredRows,
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
                        r.RawDataJson,
                        r.NormalizedDataJson,
                        r.ErrorsJson,
                        r.WarningsJson,
                        r.MatchKey,
                        r.TargetToolAssetId,
                        r.ImportedAt,
                        r.Decision,
                        r.DecisionAt,
                        r.DecisionBy
                    }),
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (import is null)
        {
            return NotFound(new { Message = $"No se encontr� la importaci�n con Id {id}." });
        }

        return Ok(import);
    }

    [HttpGet("{id:guid}/rows")]
    public async Task<IActionResult> GetImportRows(Guid id, CancellationToken cancellationToken)
    {
        var exists = await _context.ImportBatches.AnyAsync(x => x.Id == id, cancellationToken);

        if (!exists)
        {
            return NotFound(new { Message = $"No se encontr� la importaci�n con Id {id}." });
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
                x.RawDataJson,
                x.NormalizedDataJson,
                x.ErrorsJson,
                x.WarningsJson,
                x.MatchKey,
                x.TargetToolAssetId,
                x.ImportedAt,
                x.Decision,
                x.DecisionAt,
                x.DecisionBy
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpPost("excel")]
    [Consumes("multipart/form-data")]
    [RequirePermission(PermissionCodes.ImportsUpload)]
    public async Task<IActionResult> UploadExcel([FromForm] ImportExcelRequest request, CancellationToken cancellationToken)
    {
        if (request.File is null || request.File.Length == 0)
        {
            return BadRequest(new { Message = "Debe adjuntar un archivo Excel." });
        }

        var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();

        if (extension != ".xlsx")
        {
            return BadRequest(new
            {
                Message = "Solo se permiten archivos .xlsx sin macros. Los formatos .xls y .xlsm se rechazan por seguridad."
            });
        }

        var maximumFileSize = Math.Clamp(
            _configuration.GetValue<long?>("Imports:MaximumFileSizeBytes") ?? DefaultMaximumFileSizeBytes,
            1,
            25 * 1024 * 1024);

        if (request.File.Length > maximumFileSize)
        {
            return BadRequest(new
            {
                Message = $"El archivo supera el tamaño máximo permitido de {maximumFileSize / 1024 / 1024} MB."
            });
        }

        if (!string.IsNullOrWhiteSpace(request.File.ContentType)
            && !AllowedExcelContentTypes.Contains(request.File.ContentType))
        {
            return BadRequest(new { Message = "El tipo de contenido del archivo no corresponde a un libro .xlsx." });
        }

        var processedBy = User.GetUserName() ?? "authenticated-user";

        var sourceType = string.IsNullOrWhiteSpace(request.SourceType)
            ? "Unknown"
            : request.SourceType.Trim()[..Math.Min(request.SourceType.Trim().Length, 100)];

        await using var excelStream = new MemoryStream();
        await request.File.CopyToAsync(excelStream, cancellationToken);
        excelStream.Position = 0;

        if (!IsOpenXmlPackage(excelStream))
        {
            return BadRequest(new { Message = "El contenido no corresponde a un archivo .xlsx válido." });
        }

        XLWorkbook workbook;

        try
        {
            workbook = new XLWorkbook(excelStream);
        }
        catch (Exception exception) when (exception is InvalidDataException or ArgumentException)
        {
            return BadRequest(new { Message = "No fue posible leer el archivo .xlsx. Verifique que no esté dañado o protegido." });
        }

        using (workbook)
        {
            var worksheet = workbook.Worksheets.FirstOrDefault();

            if (worksheet is null)
            {
                return BadRequest(new { Message = "El archivo no contiene hojas." });
            }

            var headerRow = worksheet.FirstRowUsed();

            if (headerRow is null)
            {
                return BadRequest(new { Message = "El archivo no contiene encabezados." });
            }

            var headers = headerRow.CellsUsed()
                .Select(cell => new ExcelHeader(
                    ColumnNumber: cell.Address.ColumnNumber,
                    Name: cell.GetString().Trim(),
                    NormalizedName: NormalizeHeader(cell.GetString())))
                .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                .ToList();

            if (!HasSupportedIdentifierHeader(headers))
            {
                return BadRequest(new
                {
                    Message = "El archivo debe incluir al menos un identificador: código NAVI/interno, Fenix365, activo fijo o serial."
                });
            }

            var maximumRows = Math.Clamp(
                _configuration.GetValue<int?>("Imports:MaximumRows") ?? DefaultMaximumRows,
                1,
                50_000);

            var dataRows = worksheet.RowsUsed()
                .Where(row => row.RowNumber() > headerRow.RowNumber())
                .Take(maximumRows + 1)
                .ToList();

            if (dataRows.Count > maximumRows)
            {
                return BadRequest(new { Message = $"El archivo supera el máximo permitido de {maximumRows:N0} filas." });
            }

            var objectKey = await UploadToMinioAsync(request.File, sourceType, cancellationToken);

            var importBatch = new ImportBatch
            {
                ImportNumber = $"IMP-{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}"[..43],
                SourceType = sourceType,
                FileName = Path.GetFileName(request.File.FileName)[..Math.Min(Path.GetFileName(request.File.FileName).Length, 300)],
                ObjectKey = objectKey,
                Status = "Processing",
                ProcessedAt = DateTime.UtcNow,
                ProcessedBy = processedBy,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = processedBy
            };

            _context.ImportBatches.Add(importBatch);

            foreach (var row in dataRows)
            {
            var internalCode = NormalizeCodeOrNull(GetCellValue(row, headers, "codigo navi", "codigo interno", "internal code", "código interno", "codigo herramienta", "code"), 100);
            var fenixCode = NormalizeCodeOrNull(GetCellValue(row, headers, "codigo fenix", "fenix code", "codigo fenix365", "código fenix365"), 100);
            var fixedAssetCode = NormalizeCodeOrNull(GetCellValue(row, headers, "activo fijo", "fixed asset", "placa", "placa activo", "asset code"), 100);
            var serialNumber = NormalizeCodeOrNull(GetCellValue(row, headers, "serial", "serie", "serial number"), 150);
            var toolName = NormalizeOptionalText(GetCellValue(row, headers, "nombre", "herramienta", "tool name", "descripcion", "descripción"), 300);
            var branchCode = NormalizeCodeOrNull(GetCellValue(row, headers, "sede", "branch", "branch code", "centro", "ubicacion sede"), 50);
            var responsibleName = NormalizeOptionalText(GetCellValue(row, headers, "responsable", "responsible", "custodio", "asignado a"), 300);
            var operationalStatus = NormalizeOptionalText(GetCellValue(row, headers, "estado", "status", "estado operativo"), 100);
            var locationCode = NormalizeCodeOrNull(GetCellValue(row, headers, "ubicacion", "location", "almacen", "taller"), 80);
            var shelfLocation = NormalizeOptionalText(GetCellValue(row, headers, "estanteria", "shelf", "ubicacion exacta"), 150);
            var toolTypeCode = NormalizeCodeOrNull(GetCellValue(row, headers, "tipo", "tipo activo", "tool type"), 80);
            var categoryCode = NormalizeCodeOrNull(GetCellValue(row, headers, "categoria", "category"), 80);
            var brand = NormalizeOptionalText(GetCellValue(row, headers, "marca", "brand"), 150);
            var model = NormalizeOptionalText(GetCellValue(row, headers, "modelo", "model"), 150);
            var voltage = NormalizeOptionalText(GetCellValue(row, headers, "voltaje", "voltage"), 100);
            var loadCapacity = NormalizeOptionalText(GetCellValue(row, headers, "capacidad de carga", "load capacity", "capacidad"), 100);
            var provider = NormalizeOptionalText(GetCellValue(row, headers, "proveedor", "provider"), 200);

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
                    NormalizedDataJson = JsonSerializer.Serialize(new
                    {
                        InternalCode = internalCode,
                        FenixCode = fenixCode,
                        FixedAssetCode = fixedAssetCode,
                        SerialNumber = serialNumber,
                        ToolName = NormalizeOptionalText(toolName),
                        BranchCode = branchCode,
                        ResponsibleName = NormalizeOptionalText(responsibleName),
                        OperationalStatus = NormalizeOptionalText(operationalStatus),
                        LocationCode = locationCode,
                        ShelfLocation = shelfLocation,
                        ToolTypeCode = toolTypeCode,
                        CategoryCode = categoryCode,
                        Brand = brand,
                        Model = model,
                        Voltage = voltage,
                        LoadCapacity = loadCapacity,
                        Provider = provider
                    }),
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = processedBy
                };

                await AnalyzeImportRowAsync(importRow, cancellationToken);

                importBatch.Rows.Add(importRow);
            }

            RecalculateImportBatch(importBatch);
            importBatch.CreatedTools = 0;
            importBatch.UpdatedTools = 0;
            importBatch.Status = importBatch.ErrorRows > 0 ? "CompletedWithErrors" : "Completed";
            importBatch.Summary = BuildImportSummary(importBatch);
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
                importBatch.WarningRows,
                importBatch.IgnoredRows,
                importBatch.DuplicateRows,
                importBatch.CreatedTools,
                importBatch.UpdatedTools,
                importBatch.Summary,
                importBatch.ProcessedAt,
                importBatch.ProcessedBy
            });
        }
    }


    [HttpPost("{id:guid}/apply-new-candidates")]
    [RequirePermission(PermissionCodes.ImportsApply)]
    public async Task<IActionResult> ApplyNewCandidates(Guid id, [FromBody] ApplyImportCandidatesRequest request, CancellationToken cancellationToken)
    {
        var importBatch = await _context.ImportBatches
            .Include(x => x.Rows)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (importBatch is null)
        {
            return NotFound(new { Message = $"No se encontr� la importaci�n con Id {id}." });
        }

        var batchSize = Math.Clamp(
            request.BatchSize
                ?? _configuration.GetValue<int?>("Imports:ApplyBatchSize")
                ?? 250,
            1,
            1_000);

        var candidateRows = importBatch.Rows
            .Where(x => string.Equals(x.ResultStatus, "NewCandidate", StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.RowNumber)
            .Take(batchSize)
            .ToList();

        if (candidateRows.Count == 0)
        {
            return BadRequest(new { Message = "La importaci�n no tiene filas NewCandidate para crear herramientas." });
        }

        var processedBy = User.GetUserName() ?? "authenticated-user";

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
        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        foreach (var row in candidateRows)
        {
            var normalizedData = DeserializeNormalizedData(row.NormalizedDataJson);

            if (string.IsNullOrWhiteSpace(row.InternalCode))
            {
                row.ResultStatus = "Error";
                row.Message = "No se puede crear la herramienta porque la fila no tiene c�digo interno.";
                row.ErrorsJson = SerializeMessages(row.Message);
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            if (string.IsNullOrWhiteSpace(row.ToolName))
            {
                row.ResultStatus = "Error";
                row.Message = "No se puede crear la herramienta porque la fila no tiene nombre.";
                row.ErrorsJson = SerializeMessages(row.Message);
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
                row.Message = "No se puede crear la herramienta porque la fila no tiene sede y no se indic� sede por defecto.";
                row.ErrorsJson = SerializeMessages(row.Message);
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
                row.Message = $"No se cre� porque ya existe una herramienta relacionada en NAVI: {alreadyExists.InternalCode}.";
                row.MatchKey = BuildMatchKey(row, alreadyExists);
                row.TargetToolAssetId = alreadyExists.Id;
                row.Decision = "Matched";
                row.DecisionAt = DateTime.UtcNow;
                row.DecisionBy = processedBy;
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
                row.ErrorsJson = SerializeMessages(row.Message);
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            var locationCode = !string.IsNullOrWhiteSpace(normalizedData?.LocationCode)
                ? normalizedData.LocationCode
                : $"{branch.Code}-{defaultLocationSuffix}";

            var location = await _context.ToolLocations
                .FirstOrDefaultAsync(x => x.BranchId == branch.Id && x.Code == locationCode, cancellationToken);

            location ??= await _context.ToolLocations
                .FirstOrDefaultAsync(x => x.BranchId == branch.Id, cancellationToken);

            if (location is null)
            {
                row.ResultStatus = "Error";
                row.Message = $"No se puede crear la herramienta porque la sede {branch.Code} no tiene ubicaci�n configurada.";
                row.ErrorsJson = SerializeMessages(row.Message);
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            var selectedToolTypeCode = normalizedData?.ToolTypeCode ?? defaultToolTypeCode;
            var toolType = await _context.ToolTypes
                .FirstOrDefaultAsync(x => x.Code == selectedToolTypeCode, cancellationToken);

            toolType ??= await _context.ToolTypes
                .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

            if (toolType is null)
            {
                row.ResultStatus = "Error";
                row.Message = "No se puede crear la herramienta porque no existe tipo de herramienta configurado.";
                row.ErrorsJson = SerializeMessages(row.Message);
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            var selectedToolCategoryCode = normalizedData?.CategoryCode ?? defaultToolCategoryCode;
            var toolCategory = await _context.ToolCategories
                .FirstOrDefaultAsync(x => x.Code == selectedToolCategoryCode, cancellationToken);

            toolCategory ??= await _context.ToolCategories
                .FirstOrDefaultAsync(x => x.IsActive, cancellationToken);

            if (toolCategory is null)
            {
                row.ResultStatus = "Error";
                row.Message = "No se puede crear la herramienta porque no existe categor�a configurada.";
                row.ErrorsJson = SerializeMessages(row.Message);
                row.UpdatedAt = DateTime.UtcNow;
                row.UpdatedBy = processedBy;

                errorRows.Add(new { row.RowNumber, row.Message });
                continue;
            }

            Guid? responsiblePersonId = null;
            if (!string.IsNullOrWhiteSpace(row.ResponsibleName))
            {
                var responsibleMatches = await _context.ResponsiblePeople
                    .AsNoTracking()
                    .Where(person => person.IsActive && person.FullName == row.ResponsibleName)
                    .Take(2)
                    .Select(person => person.Id)
                    .ToListAsync(cancellationToken);

                if (responsibleMatches.Count == 1)
                {
                    responsiblePersonId = responsibleMatches[0];
                }
                else
                {
                    var warning = responsibleMatches.Count == 0
                        ? $"No se encontró el responsable activo '{row.ResponsibleName}'."
                        : $"El responsable '{row.ResponsibleName}' es ambiguo.";
                    row.WarningsJson = AppendQualityIssue(
                        row.WarningsJson,
                        QualityIssue(
                            "ResponsibleName",
                            row.ResponsibleName,
                            "UniqueResponsible",
                            warning,
                            "Warning",
                            "Asocie el responsable manualmente después de crear el activo."));
                }
            }

            var tool = new ToolAsset
            {
                InternalCode = NormalizeCode(row.InternalCode),
                Name = row.ToolName.Trim(),
                Description = $"Herramienta creada desde importaci�n {importBatch.ImportNumber}.",
                SerialNumber = row.SerialNumber,
                FixedAssetCode = row.FixedAssetCode,
                FenixCode = row.FenixCode,
                Brand = normalizedData?.Brand,
                Model = normalizedData?.Model,
                Voltage = normalizedData?.Voltage,
                LoadCapacity = normalizedData?.LoadCapacity,
                Provider = normalizedData?.Provider,
                UnitOfMeasure = "UND",
                Quantity = 1,
                IsSpecialized = request.IsSpecialized ?? false,
                RequiresMaintenance = request.RequiresMaintenance ?? false,
                RequiresPreOperationalCheck = request.RequiresPreOperationalCheck ?? false,
                RequiresCertification = request.RequiresCertification ?? false,
                ZoneId = branch.ZoneId,
                BranchId = branch.Id,
                LocationId = location.Id,
                ShelfLocation = normalizedData?.ShelfLocation,
                ResponsiblePersonId = responsiblePersonId,
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
                "Herramienta creada desde importaci�n",
                $"Herramienta creada desde la importaci�n {importBatch.ImportNumber}, fila {row.RowNumber}.",
                null,
                tool.InternalCode,
                processedBy);

            row.ResultStatus = "Created";
            row.Message = $"Herramienta creada en NAVI con c�digo {tool.InternalCode}.";
            row.TargetToolAssetId = tool.Id;
            row.Decision = "Created";
            row.DecisionAt = DateTime.UtcNow;
            row.DecisionBy = processedBy;
            row.ImportedAt = DateTime.UtcNow;
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
        var remainingCandidates = importBatch.Rows.Count(row => row.ResultStatus == "NewCandidate");
        importBatch.Status = remainingCandidates > 0
            ? "PartiallyApplied"
            : errorRows.Count > 0
                ? "AppliedWithErrors"
                : "Applied";
        RecalculateImportBatch(importBatch);
        importBatch.Summary = $"Aplicaci�n de candidatos finalizada. Creadas: {createdTools.Count}. Errores: {errorRows.Count}.";

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new
        {
            importBatch.Id,
            importBatch.ImportNumber,
            importBatch.Status,
            importBatch.TotalRows,
            importBatch.ValidRows,
            importBatch.ErrorRows,
            importBatch.WarningRows,
            importBatch.IgnoredRows,
            TotalCreatedTools = importBatch.CreatedTools,
            importBatch.UpdatedTools,
            importBatch.DuplicateRows,
            importBatch.Summary,
            CreatedCount = createdTools.Count,
            ErrorCount = errorRows.Count,
            RemainingCandidateCount = remainingCandidates,
            BatchSize = batchSize,
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
    public async Task<IActionResult> GetImportSummary(Guid id, CancellationToken cancellationToken)
    {
        var importBatch = await _context.ImportBatches
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (importBatch is null)
        {
            return NotFound(new { Message = $"No se encontr� la importaci�n con Id {id}." });
        }

        var statusSummary = await _context.ImportRows
            .AsNoTracking()
            .Where(row => row.ImportBatchId == id)
            .GroupBy(row => row.ResultStatus)
            .Select(group => new
            {
                Status = group.Key,
                Count = group.Count()
            })
            .OrderBy(x => x.Status)
            .ToListAsync(cancellationToken);

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
            importBatch.WarningRows,
            importBatch.IgnoredRows,
            importBatch.CreatedTools,
            importBatch.UpdatedTools,
            importBatch.DuplicateRows,
            importBatch.Summary,
            importBatch.ProcessedAt,
            importBatch.ProcessedBy,
            StatusSummary = statusSummary
        });
    }

    [HttpGet("{id:guid}/quality-report")]
    public async Task<IActionResult> GetQualityReport(Guid id, CancellationToken cancellationToken)
    {
        var batch = await _context.ImportBatches
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new
            {
                x.Id,
                x.ImportNumber,
                x.SourceType,
                x.FileName,
                x.Status,
                x.TotalRows,
                x.ValidRows,
                x.ErrorRows,
                x.WarningRows,
                x.IgnoredRows,
                x.DuplicateRows,
                x.CreatedTools,
                x.UpdatedTools,
                x.ProcessedAt,
                x.ProcessedBy
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (batch is null)
        {
            return NotFound(new { Message = $"No se encontró la importación con Id {id}." });
        }

        var statusSummary = await _context.ImportRows
            .AsNoTracking()
            .Where(x => x.ImportBatchId == id)
            .GroupBy(x => x.ResultStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync(cancellationToken);

        var issues = await _context.ImportRows
            .AsNoTracking()
            .Where(x => x.ImportBatchId == id
                && (x.ErrorsJson != null || x.WarningsJson != null || x.ResultStatus == "Error"))
            .OrderBy(x => x.RowNumber)
            .Take(500)
            .Select(x => new
            {
                x.Id,
                x.RowNumber,
                x.InternalCode,
                x.ToolName,
                x.ResultStatus,
                x.Message,
                x.ErrorsJson,
                x.WarningsJson,
                x.MatchKey,
                x.Decision
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            Batch = batch,
            QualityScore = batch.TotalRows == 0
                ? 0m
                : Math.Round((decimal)batch.ValidRows / batch.TotalRows * 100m, 2),
            StatusSummary = statusSummary,
            Issues = issues,
            IssuesLimitedTo = 500
        });
    }

    [HttpGet("{id:guid}/quality-report.csv")]
    [RequirePermission(PermissionCodes.ReportsExport)]
    public async Task<IActionResult> ExportQualityReport(Guid id, CancellationToken cancellationToken)
    {
        var batch = await _context.ImportBatches
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new { x.ImportNumber })
            .FirstOrDefaultAsync(cancellationToken);

        if (batch is null)
        {
            return NotFound(new { Message = $"No se encontró la importación con Id {id}." });
        }

        var rows = await _context.ImportRows
            .AsNoTracking()
            .Where(x => x.ImportBatchId == id)
            .OrderBy(x => x.RowNumber)
            .Select(x => new
            {
                x.RowNumber,
                x.InternalCode,
                x.FenixCode,
                x.FixedAssetCode,
                x.SerialNumber,
                x.ToolName,
                x.BranchCode,
                x.ResultStatus,
                x.Message,
                x.ErrorsJson,
                x.WarningsJson,
                x.Decision
            })
            .ToListAsync(cancellationToken);

        var csv = new StringBuilder("Fila,CodigoNavi,CodigoFenix,ActivoFijo,Serial,Nombre,Sede,Resultado,Mensaje,Errores,Advertencias,Decision\r\n");

        foreach (var row in rows)
        {
            csv.AppendJoin(',',
                Csv(row.RowNumber.ToString(CultureInfo.InvariantCulture)),
                Csv(row.InternalCode),
                Csv(row.FenixCode),
                Csv(row.FixedAssetCode),
                Csv(row.SerialNumber),
                Csv(row.ToolName),
                Csv(row.BranchCode),
                Csv(row.ResultStatus),
                Csv(row.Message),
                Csv(row.ErrorsJson),
                Csv(row.WarningsJson),
                Csv(row.Decision));
            csv.Append("\r\n");
        }

        var bytes = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true).GetBytes(csv.ToString());
        return File(bytes, "text/csv; charset=utf-8", $"{batch.ImportNumber}-calidad.csv");
    }

    [HttpGet("{id:guid}/rows/by-status/{status}")]
    public async Task<IActionResult> GetImportRowsByStatus(Guid id, string status, CancellationToken cancellationToken)
    {
        var exists = await _context.ImportBatches.AnyAsync(x => x.Id == id, cancellationToken);

        if (!exists)
        {
            return NotFound(new { Message = $"No se encontr� la importaci�n con Id {id}." });
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
                x.NormalizedDataJson,
                x.ErrorsJson,
                x.WarningsJson,
                x.MatchKey,
                x.TargetToolAssetId,
                x.ImportedAt,
                x.Decision,
                x.DecisionAt,
                x.DecisionBy,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpPatch("rows/{rowId:guid}/mark-reviewed")]
    [RequirePermission(PermissionCodes.ImportsReconcile)]
    public async Task<IActionResult> MarkImportRowReviewed(Guid rowId, [FromBody] ImportRowActionRequest request, CancellationToken cancellationToken)
    {
        var row = await _context.ImportRows
            .Include(x => x.ImportBatch)
            .FirstOrDefaultAsync(x => x.Id == rowId, cancellationToken);

        if (row is null)
        {
            return NotFound(new { Message = $"No se encontr� la fila de importaci�n con Id {rowId}." });
        }

        var changedBy = GetImportRowActionUser(request);
        var previousStatus = row.ResultStatus;

        row.ResultStatus = "Reviewed";
        row.Message = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Fila revisada manualmente. Estado anterior: {previousStatus}."
            : request.Notes.Trim();
        row.Decision = "Reviewed";
        row.DecisionAt = DateTime.UtcNow;
        row.DecisionBy = changedBy;

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

    [HttpPatch("rows/{rowId:guid}/send-to-review")]
    [RequirePermission(PermissionCodes.ImportsReconcile)]
    public async Task<IActionResult> SendImportRowToReview(
        Guid rowId,
        [FromBody] ImportRowActionRequest request,
        CancellationToken cancellationToken)
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

        row.ResultStatus = "PendingReview";
        row.Message = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Fila enviada a revisión manual. Estado anterior: {previousStatus}."
            : request.Notes.Trim();
        row.Decision = "PendingReview";
        row.DecisionAt = DateTime.UtcNow;
        row.DecisionBy = changedBy;
        row.UpdatedAt = DateTime.UtcNow;
        row.UpdatedBy = changedBy;

        await UpdateImportBatchCountersAsync(row.ImportBatchId, changedBy, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            row.Id,
            row.RowNumber,
            PreviousStatus = previousStatus,
            row.ResultStatus,
            row.Message,
            row.DecisionBy
        });
    }

    [HttpPatch("rows/{rowId:guid}/apply-update/{toolId:guid}")]
    [RequirePermission(PermissionCodes.ImportsApply)]
    public async Task<IActionResult> ApplyImportRowUpdate(
        Guid rowId,
        Guid toolId,
        [FromBody] ApplyImportUpdateRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.Confirm)
        {
            return BadRequest(new { Message = "La actualización requiere confirmación explícita." });
        }

        var row = await _context.ImportRows
            .Include(x => x.ImportBatch)
            .FirstOrDefaultAsync(x => x.Id == rowId, cancellationToken);

        if (row is null)
        {
            return NotFound(new { Message = $"No se encontró la fila de importación con Id {rowId}." });
        }

        var tool = await _context.ToolAssets
            .Include(x => x.Branch)
            .FirstOrDefaultAsync(x => x.Id == toolId, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = $"No se encontró la herramienta con Id {toolId}." });
        }

        var changedBy = User.GetUserName() ?? "authenticated-user";
        var before = JsonSerializer.Serialize(new
        {
            tool.Name,
            tool.FenixCode,
            tool.FixedAssetCode,
            tool.SerialNumber,
            tool.BranchId,
            tool.LocationId,
            tool.ResponsiblePersonId
        });

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        if (request.ApplyName && !string.IsNullOrWhiteSpace(row.ToolName))
        {
            tool.Name = row.ToolName;
        }

        if (request.FillMissingIdentifiers)
        {
            tool.FenixCode ??= row.FenixCode;
            tool.FixedAssetCode ??= row.FixedAssetCode;
            tool.SerialNumber ??= row.SerialNumber;
        }

        if (request.ApplyBranch && !string.IsNullOrWhiteSpace(row.BranchCode))
        {
            var branch = await _context.Branches
                .FirstOrDefaultAsync(x => x.Code == row.BranchCode, cancellationToken);

            if (branch is null)
            {
                return BadRequest(new { Message = $"No existe la sede {row.BranchCode}; no se aplicó ningún cambio." });
            }

            var location = await _context.ToolLocations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.BranchId == branch.Id && x.IsActive, cancellationToken);

            tool.ZoneId = branch.ZoneId;
            tool.BranchId = branch.Id;
            tool.LocationId = location?.Id;
        }

        if (request.ApplyResponsible && !string.IsNullOrWhiteSpace(row.ResponsibleName))
        {
            var normalizedResponsibleName = row.ResponsibleName.Trim();
            var responsibleMatches = await _context.ResponsiblePeople
                .Where(person => person.IsActive && person.FullName == normalizedResponsibleName)
                .Take(2)
                .ToListAsync(cancellationToken);

            if (responsibleMatches.Count != 1)
            {
                return Conflict(new
                {
                    Message = responsibleMatches.Count == 0
                        ? $"No existe un responsable activo con nombre exacto '{normalizedResponsibleName}'."
                        : $"El responsable '{normalizedResponsibleName}' es ambiguo; debe asociarse manualmente."
                });
            }

            tool.ResponsiblePersonId = responsibleMatches[0].Id;
        }

        tool.UpdatedAt = DateTime.UtcNow;
        tool.UpdatedBy = changedBy;

        row.ResultStatus = "Updated";
        row.TargetToolAssetId = tool.Id;
        row.MatchKey = BuildMatchKey(row, tool);
        row.Decision = "Updated";
        row.DecisionAt = DateTime.UtcNow;
        row.DecisionBy = changedBy;
        row.ImportedAt = DateTime.UtcNow;
        row.Message = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Datos confirmados aplicados a la herramienta {tool.InternalCode}."
            : request.Notes.Trim();
        row.UpdatedAt = DateTime.UtcNow;
        row.UpdatedBy = changedBy;

        AddToolLifeCycleEvent(
            tool.Id,
            "ToolUpdatedFromImport",
            "Herramienta actualizada desde importación",
            $"Actualización confirmada desde la importación {row.ImportBatch?.ImportNumber}, fila {row.RowNumber}.",
            before,
            JsonSerializer.Serialize(new
            {
                tool.Name,
                tool.FenixCode,
                tool.FixedAssetCode,
                tool.SerialNumber,
                tool.BranchId,
                tool.LocationId,
                tool.ResponsiblePersonId
            }),
            changedBy);

        if (row.ImportBatch is not null)
        {
            row.ImportBatch.UpdatedTools++;
        }

        await UpdateImportBatchCountersAsync(row.ImportBatchId, changedBy, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Ok(new
        {
            row.Id,
            row.RowNumber,
            row.ResultStatus,
            row.Decision,
            Tool = new { tool.Id, tool.InternalCode, tool.Name }
        });
    }

    [HttpPatch("rows/{rowId:guid}/mark-ignored")]
    [RequirePermission(PermissionCodes.ImportsReconcile)]
    public async Task<IActionResult> MarkImportRowIgnored(Guid rowId, [FromBody] ImportRowActionRequest request, CancellationToken cancellationToken)
    {
        var row = await _context.ImportRows
            .Include(x => x.ImportBatch)
            .FirstOrDefaultAsync(x => x.Id == rowId, cancellationToken);

        if (row is null)
        {
            return NotFound(new { Message = $"No se encontr� la fila de importaci�n con Id {rowId}." });
        }

        var changedBy = GetImportRowActionUser(request);
        var previousStatus = row.ResultStatus;

        row.ResultStatus = "Ignored";
        row.Message = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Fila ignorada manualmente. Estado anterior: {previousStatus}."
            : request.Notes.Trim();
        row.Decision = "Ignored";
        row.DecisionAt = DateTime.UtcNow;
        row.DecisionBy = changedBy;

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
    [RequirePermission(PermissionCodes.ImportsReconcile)]
    public async Task<IActionResult> LinkImportRowToTool(Guid rowId, Guid toolId, [FromBody] ImportRowActionRequest request, CancellationToken cancellationToken)
    {
        var row = await _context.ImportRows
            .Include(x => x.ImportBatch)
            .FirstOrDefaultAsync(x => x.Id == rowId, cancellationToken);

        if (row is null)
        {
            return NotFound(new { Message = $"No se encontr� la fila de importaci�n con Id {rowId}." });
        }

        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == toolId, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = $"No se encontr� la herramienta con Id {toolId}." });
        }

        var changedBy = GetImportRowActionUser(request);
        var previousStatus = row.ResultStatus;

        row.ResultStatus = "Linked";
        row.Message = string.IsNullOrWhiteSpace(request.Notes)
            ? $"Fila asociada manualmente a la herramienta NAVI {tool.InternalCode}. Estado anterior: {previousStatus}."
            : request.Notes.Trim();
        row.MatchKey = BuildMatchKey(row, tool);
        row.TargetToolAssetId = tool.Id;
        row.Decision = "Linked";
        row.DecisionAt = DateTime.UtcNow;
        row.DecisionBy = changedBy;

        row.UpdatedAt = DateTime.UtcNow;
        row.UpdatedBy = changedBy;

        AddToolLifeCycleEvent(
            tool.Id,
            "ImportRowLinkedToTool",
            "Fila de importaci�n asociada a herramienta",
            $"Fila {row.RowNumber} de la importaci�n {row.ImportBatch?.ImportNumber} asociada manualmente a esta herramienta.",
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

    private string GetImportRowActionUser(ImportRowActionRequest request)
    {
        return User.GetUserName() ?? "authenticated-user";
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

        RecalculateImportBatch(importBatch);

        importBatch.Status = importBatch.ErrorRows > 0
            ? "ReviewedWithErrors"
            : "Reviewed";

        importBatch.Summary = $"Revisi�n de importaci�n actualizada. Filas: {importBatch.TotalRows}. Creadas: {importBatch.CreatedTools}. Errores: {importBatch.ErrorRows}. Existentes/Duplicadas/Asociadas: {importBatch.DuplicateRows}.";
        importBatch.UpdatedAt = DateTime.UtcNow;
        importBatch.UpdatedBy = changedBy;
    }
    private async Task AnalyzeImportRowAsync(ImportRow row, CancellationToken cancellationToken)
    {
        var warnings = new List<ImportDataIssue>();

        if (string.IsNullOrWhiteSpace(row.InternalCode)
            && string.IsNullOrWhiteSpace(row.FenixCode)
            && string.IsNullOrWhiteSpace(row.FixedAssetCode)
            && string.IsNullOrWhiteSpace(row.SerialNumber))
        {
            row.ResultStatus = "Error";
            row.Message = "La fila no tiene código interno, código Fenix365, activo fijo ni serial.";
            row.ErrorsJson = SerializeIssue(
                "Identifier",
                null,
                "RequiredIdentifier",
                row.Message,
                "Error",
                "Complete al menos uno de los identificadores admitidos.");
            return;
        }

        if (string.IsNullOrWhiteSpace(row.ToolName))
        {
            warnings.Add(QualityIssue(
                "ToolName",
                row.ToolName,
                "Recommended",
                "La fila no tiene nombre de herramienta.",
                "Warning",
                "Complete el nombre antes de crear el activo."));
        }

        if (string.IsNullOrWhiteSpace(row.BranchCode))
        {
            warnings.Add(QualityIssue(
                "BranchCode",
                row.BranchCode,
                "Recommended",
                "La fila no tiene código de sede.",
                "Warning",
                "Seleccione una sede por defecto o corrija el archivo."));
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
            row.WarningsJson = warnings.Count == 0 ? null : JsonSerializer.Serialize(warnings);
            return;
        }

        row.TargetToolAssetId = existingTool.Id;
        row.MatchKey = BuildMatchKey(row, existingTool);

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
            warnings.AddRange(differences.Select(difference => QualityIssue(
                "Match",
                row.MatchKey,
                "Consistency",
                difference,
                "Warning",
                "Revise la diferencia antes de actualizar el activo.")));
            row.WarningsJson = warnings.Count == 0 ? null : JsonSerializer.Serialize(warnings);

            return;
        }

        row.ResultStatus = "PossibleDuplicate";
        row.Message = $"Posible duplicado con herramienta NAVI {existingTool.InternalCode}. Coincidencia por Fenix, activo fijo o serial.";
        warnings.Add(QualityIssue(
            "Identifier",
            row.MatchKey,
            "UniqueMatch",
            row.Message,
            "Warning",
            "Asocie manualmente el activo correcto; no se fusionará automáticamente."));
        row.WarningsJson = JsonSerializer.Serialize(warnings);
    }

    private async Task<ToolAsset?> FindExistingToolAsync(
        string? internalCode,
        string? fenixCode,
        string? fixedAssetCode,
        string? serialNumber,
        CancellationToken cancellationToken)
    {
        return await _context.ToolAssets
            .AsNoTracking()
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
        var accessKey = _configuration["Minio:AccessKey"];
        var secretKey = _configuration["Minio:SecretKey"];
        var bucketName = _configuration["Minio:BucketName"] ?? "navi-tools-documents";

        if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException(
                "Configure Minio:AccessKey y Minio:SecretKey mediante secretos externos.");
        }

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
        var decomposed = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var withoutDiacritics = new string(decomposed
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .ToArray())
            .Normalize(NormalizationForm.FormC)
            .Replace("_", " ")
            .Replace("-", " ");

        return string.Join(' ', withoutDiacritics.Split(
            ' ',
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static string NormalizeCode(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string? NormalizeCodeOrNull(string? value, int maximumLength = 100)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim().ToUpperInvariant();
        return normalized[..Math.Min(normalized.Length, maximumLength)];
    }

    private static string? NormalizeOptionalText(string? value, int maximumLength = 300)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized[..Math.Min(normalized.Length, maximumLength)];
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

    private static bool IsOpenXmlPackage(Stream stream)
    {
        if (!stream.CanSeek || stream.Length < 4)
        {
            return false;
        }

        var originalPosition = stream.Position;
        Span<byte> signature = stackalloc byte[4];
        stream.Position = 0;
        var bytesRead = stream.Read(signature);
        stream.Position = originalPosition;

        return bytesRead == signature.Length
            && signature[0] == 0x50
            && signature[1] == 0x4B
            && signature[2] is 0x03 or 0x05 or 0x07
            && signature[3] is 0x04 or 0x06 or 0x08;
    }

    private static bool HasSupportedIdentifierHeader(IEnumerable<ExcelHeader> headers)
    {
        var supported = new HashSet<string>
        {
            "codigo navi",
            "codigo interno",
            "internal code",
            "codigo herramienta",
            "code",
            "codigo fenix",
            "fenix code",
            "codigo fenix365",
            "activo fijo",
            "fixed asset",
            "placa",
            "placa activo",
            "asset code",
            "serial",
            "serie",
            "serial number"
        };

        return headers.Any(header => supported.Contains(header.NormalizedName));
    }

    private static string BuildMatchKey(ImportRow row, ToolAsset tool)
    {
        if (!string.IsNullOrWhiteSpace(row.InternalCode)
            && string.Equals(row.InternalCode, tool.InternalCode, StringComparison.OrdinalIgnoreCase))
        {
            return $"InternalCode:{row.InternalCode}";
        }

        if (!string.IsNullOrWhiteSpace(row.FenixCode)
            && string.Equals(row.FenixCode, tool.FenixCode, StringComparison.OrdinalIgnoreCase))
        {
            return $"FenixCode:{row.FenixCode}";
        }

        if (!string.IsNullOrWhiteSpace(row.FixedAssetCode)
            && string.Equals(row.FixedAssetCode, tool.FixedAssetCode, StringComparison.OrdinalIgnoreCase))
        {
            return $"FixedAssetCode:{row.FixedAssetCode}";
        }

        return $"SerialNumber:{row.SerialNumber}";
    }

    private static string SerializeMessages(params string[] messages) =>
        JsonSerializer.Serialize(messages
            .Where(message => !string.IsNullOrWhiteSpace(message))
            .Select(message => QualityIssue(
                null,
                null,
                "BusinessValidation",
                message,
                "Error",
                "Corrija la fila o envíela a revisión manual.")));

    private static string SerializeIssue(
        string? field,
        string? value,
        string rule,
        string message,
        string severity,
        string suggestion) =>
        JsonSerializer.Serialize(new[]
        {
            QualityIssue(field, value, rule, message, severity, suggestion)
        });

    private static ImportDataIssue QualityIssue(
        string? field,
        string? value,
        string rule,
        string message,
        string severity,
        string suggestion) =>
        new(field, value, rule, message, severity, suggestion);

    private static string AppendQualityIssue(string? existingJson, ImportDataIssue issue)
    {
        var issues = string.IsNullOrWhiteSpace(existingJson)
            ? new List<ImportDataIssue>()
            : JsonSerializer.Deserialize<List<ImportDataIssue>>(existingJson) ?? new List<ImportDataIssue>();
        issues.Add(issue);
        return JsonSerializer.Serialize(issues);
    }

    private static ImportToolNormalizedData? DeserializeNormalizedData(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ImportToolNormalizedData>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static void RecalculateImportBatch(ImportBatch importBatch)
    {
        importBatch.TotalRows = importBatch.Rows.Count;
        importBatch.ErrorRows = importBatch.Rows.Count(row => row.ResultStatus == "Error");
        importBatch.WarningRows = importBatch.Rows.Count(row => !string.IsNullOrWhiteSpace(row.WarningsJson));
        importBatch.IgnoredRows = importBatch.Rows.Count(row => row.ResultStatus == "Ignored");
        importBatch.ValidRows = importBatch.TotalRows - importBatch.ErrorRows - importBatch.IgnoredRows;
        importBatch.CreatedTools = importBatch.Rows.Count(row => row.ResultStatus == "Created");
        importBatch.DuplicateRows = importBatch.Rows.Count(row =>
            row.ResultStatus is "Existing" or "PossibleDuplicate" or "Linked");
    }

    private static string BuildImportSummary(ImportBatch importBatch) =>
        $"Filas: {importBatch.TotalRows}. Válidas: {importBatch.ValidRows}. "
        + $"Errores: {importBatch.ErrorRows}. Advertencias: {importBatch.WarningRows}. "
        + $"Duplicados/Existentes: {importBatch.DuplicateRows}.";

    private static string Csv(string? value)
    {
        var safeValue = value ?? string.Empty;
        if (safeValue.Length > 0 && safeValue[0] is '=' or '+' or '-' or '@')
        {
            safeValue = $"'{safeValue}";
        }

        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }

    private sealed record ExcelHeader(int ColumnNumber, string Name, string NormalizedName);

    private sealed record ImportDataIssue(
        string? Field,
        string? Value,
        string Rule,
        string Message,
        string Severity,
        string Suggestion);

    private sealed record ImportToolNormalizedData(
        string? InternalCode,
        string? FenixCode,
        string? FixedAssetCode,
        string? SerialNumber,
        string? ToolName,
        string? BranchCode,
        string? ResponsibleName,
        string? OperationalStatus,
        string? LocationCode,
        string? ShelfLocation,
        string? ToolTypeCode,
        string? CategoryCode,
        string? Brand,
        string? Model,
        string? Voltage,
        string? LoadCapacity,
        string? Provider);
}

public sealed class ImportRowActionRequest
{
    public string? ActionBy { get; set; }

    public string? Notes { get; set; }
}

public sealed class ApplyImportUpdateRequest
{
    public bool Confirm { get; set; }

    public bool ApplyName { get; set; } = true;

    public bool FillMissingIdentifiers { get; set; } = true;

    public bool ApplyBranch { get; set; }

    public bool ApplyResponsible { get; set; }

    public string? Notes { get; set; }
}

public sealed class ApplyImportCandidatesRequest
{
    public int? BatchSize { get; set; }

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
