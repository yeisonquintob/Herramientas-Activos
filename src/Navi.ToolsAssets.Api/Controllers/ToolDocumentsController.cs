using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Application.Documents;
using Navi.ToolsAssets.Domain.Entities.Documents;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Maintenance;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolDocumentsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;
    private readonly IDocumentStorageService _storageService;

    public ToolDocumentsController(
        NaviToolsAssetsDbContext context,
        IDocumentStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    [HttpGet("{id:guid}/documents")]
    public async Task<IActionResult> GetByToolId(Guid id, CancellationToken cancellationToken)
    {
        var toolExists = await _context.ToolAssets
            .AnyAsync(x => x.Id == id, cancellationToken);

        if (!toolExists)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        var documents = await _context.Set<ToolDocument>()
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id && !x.IsDeleted)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync(cancellationToken);

        return Ok(documents.Select(ToDocumentResponse));
    }

    [HttpGet("by-code/{internalCode}/documents")]
    public async Task<IActionResult> GetByToolCode(string internalCode, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await GetByToolId(tool.Id, cancellationToken);
    }

    [HttpPost("{id:guid}/documents")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadByToolId(
        Guid id,
        IFormFile file,
        [FromForm] string? documentType,
        [FromForm] string? description,
        [FromForm] string? uploadedBy,
        CancellationToken cancellationToken)
    {
        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await UploadInternalAsync(
            tool.Id,
            tool.InternalCode,
            file,
            documentType,
            description,
            uploadedBy,
            cancellationToken);
    }

    [HttpPost("by-code/{internalCode}/documents")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> UploadByToolCode(
        string internalCode,
        IFormFile file,
        [FromForm] string? documentType,
        [FromForm] string? description,
        [FromForm] string? uploadedBy,
        CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await UploadInternalAsync(
            tool.Id,
            tool.InternalCode,
            file,
            documentType,
            description,
            uploadedBy,
            cancellationToken);
    }

    [HttpGet("documents/{documentId:guid}/download")]
    public async Task<IActionResult> Download(Guid documentId, CancellationToken cancellationToken)
    {
        var document = await _context.Set<ToolDocument>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == documentId && !x.IsDeleted, cancellationToken);

        if (document is null)
        {
            return NotFound(new { Message = "No se encontró el documento." });
        }

        var exists = await _storageService.ExistsAsync(document.ObjectKey, cancellationToken);

        if (!exists)
        {
            return NotFound(new
            {
                Message = "El archivo no existe en MinIO.",
                document.ObjectKey
            });
        }

        await using var stream = await _storageService.DownloadAsync(document.ObjectKey, cancellationToken);

        using var memoryStream = new MemoryStream();

        await stream.CopyToAsync(memoryStream, cancellationToken);

        var fileBytes = memoryStream.ToArray();

        if (fileBytes.Length == 0)
        {
            return NotFound(new
            {
                Message = "El documento existe en MinIO, pero se descargó vacío.",
                document.ObjectKey
            });
        }

        return File(
            fileBytes,
            string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType,
            document.FileName);
    }

    [HttpDelete("documents/{documentId:guid}")]
    public async Task<IActionResult> Delete(Guid documentId, [FromQuery] string? deletedBy, CancellationToken cancellationToken)
    {
        var document = await _context.Set<ToolDocument>()
            .FirstOrDefaultAsync(x => x.Id == documentId && !x.IsDeleted, cancellationToken);

        if (document is null)
        {
            return NotFound(new { Message = "No se encontró el documento." });
        }

        var user = string.IsNullOrWhiteSpace(deletedBy)
            ? "yquinto"
            : deletedBy.Trim();

        document.IsDeleted = true;
        document.UpdatedAt = DateTime.UtcNow;
        document.UpdatedBy = user;

        var affectedMaintenances = await _context.Set<MaintenanceRecord>()
            .Where(x => x.EvidenceDocumentId == documentId && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var maintenance in affectedMaintenances)
        {
            maintenance.EvidenceDocumentId = null;
            maintenance.UpdatedAt = DateTime.UtcNow;
            maintenance.UpdatedBy = user;
        }

        if (affectedMaintenances.Count > 0)
        {
            _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
            {
                ToolAssetId = document.ToolAssetId,
                EventType = "MaintenanceEvidenceDocumentCleared",
                Title = "Evidencia de mantenimiento limpiada",
                Description = $"Se limpió la evidencia documental en {affectedMaintenances.Count} mantenimiento(s), debido a la eliminación del documento {document.FileName}.",
                PreviousValue = document.FileName,
                NewValue = "EvidenceDocumentId limpiado",
                RegisteredBy = user,
                CreatedBy = user
            });
        }

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = document.ToolAssetId,
            EventType = "DocumentDeleted",
            Title = "Documento desactivado",
            Description = $"Se desactivó el documento {document.FileName}.",
            PreviousValue = document.FileName,
            NewValue = "Desactivado",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Documento desactivado correctamente.",
            document.Id,
            document.ToolAssetId,
            document.FileName
        });
    }

    private async Task<IActionResult> UploadInternalAsync(
        Guid toolId,
        string internalCode,
        IFormFile file,
        string? documentType,
        string? description,
        string? uploadedBy,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { Message = "Debe adjuntar un archivo válido." });
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
            ? "yquinto"
            : uploadedBy.Trim();

        var safeFileName = Path.GetFileName(file.FileName);
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;

        var objectKey = $"tools/{internalCode}/documents/{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension}";

        await using var stream = file.OpenReadStream();

        await _storageService.UploadAsync(
            objectKey,
            stream,
            contentType,
            cancellationToken);

        var parsedDocumentType = ParseDocumentType(documentType);

        var document = new ToolDocument
        {
            ToolAssetId = toolId,
            DocumentType = parsedDocumentType,
            FileName = safeFileName,
            ObjectKey = objectKey,
            ContentType = contentType,
            SizeBytes = file.Length,
            UploadedBy = user,
            UploadedAt = DateTime.UtcNow,
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            CreatedBy = user
        };

        _context.Set<ToolDocument>().Add(document);

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = toolId,
            EventType = "DocumentUploaded",
            Title = "Documento cargado",
            Description = $"Se cargó el documento {safeFileName}.",
            PreviousValue = null,
            NewValue = $"{parsedDocumentType} | {safeFileName}",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Documento cargado correctamente.",
            Document = ToDocumentResponse(document)
        });
    }

    private static ToolDocumentType ParseDocumentType(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return ToolDocumentType.Other;
        }

        return Enum.TryParse<ToolDocumentType>(value.Trim(), true, out var parsed)
            ? parsed
            : ToolDocumentType.Other;
    }

    private static object ToDocumentResponse(ToolDocument document)
    {
        return new
        {
            document.Id,
            document.ToolAssetId,
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


