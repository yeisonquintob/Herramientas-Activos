using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Minio;
using Minio.DataModel.Args;
using Navi.ToolsAssets.Domain.Entities.Imports;
using Navi.ToolsAssets.Domain.Entities.Inventory;
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

public sealed class ImportExcelRequest
{
    public IFormFile? File { get; set; }

    public string? SourceType { get; set; }

    public string? ProcessedBy { get; set; }
}
