using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Application.Documents;
using Navi.ToolsAssets.Domain.Entities.Purchases;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/purchase-requests")]
public sealed class PurchaseRequestsController : ControllerBase
{
    private const long MaxEvidenceFileBytes = 5L * 1024L * 1024L;

    private readonly NaviToolsAssetsDbContext _context;
    private readonly IDocumentStorageService _storageService;

    public PurchaseRequestsController(
        NaviToolsAssetsDbContext context,
        IDocumentStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }
    [RequirePermission("Purchases.View")]
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? status,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        var query = _context.PurchaseRequests
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
                x.ItemCode.ToLower().Contains(value) ||
                x.ItemName.ToLower().Contains(value) ||
                x.RequestedByUserName.ToLower().Contains(value) ||
                x.PreparedBy.ToLower().Contains(value));
        }

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Take(300)
            .Select(x => new PurchaseRequestDto
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                ToolAssetId = x.ToolAssetId,
                ToolInternalCode = x.ToolAsset != null ? x.ToolAsset.InternalCode : null,
                ToolName = x.ToolAsset != null ? x.ToolAsset.Name : null,
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                ItemDescription = x.ItemDescription,
                Quantity = x.Quantity,
                Unit = x.Unit,
                PurchasePurpose = x.PurchasePurpose,
                Justification = x.Justification,
                Priority = x.Priority,
                Status = x.Status,
                BranchId = x.BranchId,
                BranchCode = x.Branch != null ? x.Branch.Code : null,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                RequestedByUserName = x.RequestedByUserName,
                RequestedByResponsiblePersonName = x.RequestedByResponsiblePersonName,
                PreparedBy = x.PreparedBy,
                RequestedAt = x.RequestedAt,
                SubmittedAt = x.SubmittedAt,
                SubmittedBy = x.SubmittedBy,
                RequiredAt = x.RequiredAt,
                ProjectId = x.ProjectId,
                VendorSuggestion = x.VendorSuggestion,
                EstimatedCostText = x.EstimatedCostText,
                ApprovalComment = x.ApprovalComment,
                ApprovedBy = x.ApprovedBy,
                ApprovedAt = x.ApprovedAt,
                RejectedBy = x.RejectedBy,
                RejectedAt = x.RejectedAt,
                RejectionReason = x.RejectionReason,
                ClosedBy = x.ClosedBy,
                ClosedAt = x.ClosedAt,
                SentToDynamics = x.SentToDynamics,
                DynamicsPurchaseRequisitionNumber = x.DynamicsPurchaseRequisitionNumber,
                DynamicsStatus = x.DynamicsStatus,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Ok(items);
    }
    [RequirePermission("Purchases.View")]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        var item = await _context.PurchaseRequests
            .AsNoTracking()
            .Include(x => x.ToolAsset)
            .Include(x => x.Branch)
            .Where(x => !x.IsDeleted && x.Id == id)
            .Select(x => new PurchaseRequestDto
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                ToolAssetId = x.ToolAssetId,
                ToolInternalCode = x.ToolAsset != null ? x.ToolAsset.InternalCode : null,
                ToolName = x.ToolAsset != null ? x.ToolAsset.Name : null,
                ItemCode = x.ItemCode,
                ItemName = x.ItemName,
                ItemDescription = x.ItemDescription,
                Quantity = x.Quantity,
                Unit = x.Unit,
                PurchasePurpose = x.PurchasePurpose,
                Justification = x.Justification,
                Priority = x.Priority,
                Status = x.Status,
                BranchId = x.BranchId,
                BranchCode = x.Branch != null ? x.Branch.Code : null,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                RequestedByUserName = x.RequestedByUserName,
                RequestedByResponsiblePersonName = x.RequestedByResponsiblePersonName,
                PreparedBy = x.PreparedBy,
                RequestedAt = x.RequestedAt,
                SubmittedAt = x.SubmittedAt,
                SubmittedBy = x.SubmittedBy,
                RequiredAt = x.RequiredAt,
                ProjectId = x.ProjectId,
                VendorSuggestion = x.VendorSuggestion,
                EstimatedCostText = x.EstimatedCostText,
                ApprovalComment = x.ApprovalComment,
                ApprovedBy = x.ApprovedBy,
                ApprovedAt = x.ApprovedAt,
                RejectedBy = x.RejectedBy,
                RejectedAt = x.RejectedAt,
                RejectionReason = x.RejectionReason,
                ClosedBy = x.ClosedBy,
                ClosedAt = x.ClosedAt,
                SentToDynamics = x.SentToDynamics,
                DynamicsPurchaseRequisitionNumber = x.DynamicsPurchaseRequisitionNumber,
                DynamicsStatus = x.DynamicsStatus,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de compra." });
        }

        var evidences = await _context.PurchaseRequestEvidences
            .AsNoTracking()
            .Where(x =>
                !x.IsDeleted &&
                x.PurchaseRequestId == id)
            .OrderBy(x => x.EvidenceType)
            .ThenBy(x => x.ReferenceName)
            .ThenBy(x => x.UploadedAt)
            .ToListAsync(cancellationToken);

        item.Evidences = evidences
            .Select(ToEvidenceDto)
            .ToList();

        return Ok(item);
    }

    [RequirePermission("Purchases.Request", "Purchases.Quote")]
    [HttpPost("workspaces/{workspaceId:guid}/evidences")]
    [RequestSizeLimit(MaxEvidenceFileBytes + 1024L * 1024L)]
    public async Task<IActionResult> UploadWorkspaceEvidence(
        Guid workspaceId,
        [FromForm] IFormFile file,
        [FromForm] Guid referenceId,
        [FromForm] string evidenceType,
        [FromForm] string? referenceName,
        CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        if (workspaceId == Guid.Empty || referenceId == Guid.Empty)
        {
            return BadRequest(new
            {
                Message = "La referencia de la solicitud o de la evidencia no es válida."
            });
        }

        if (file is null || file.Length <= 0)
        {
            return BadRequest(new { Message = "Debes seleccionar una imagen válida." });
        }

        if (file.Length > MaxEvidenceFileBytes)
        {
            return BadRequest(new { Message = "La imagen no puede superar 5 MB." });
        }

        var normalizedType = NormalizeEvidenceType(evidenceType);

        if (normalizedType is null)
        {
            return BadRequest(new
            {
                Message = "El tipo de evidencia debe ser ToolImage o QuotationImage."
            });
        }

        var normalizedContentType = NormalizeImageContentType(
            file.ContentType,
            file.FileName);

        if (normalizedContentType is null)
        {
            return BadRequest(new
            {
                Message = "Solo se permiten imágenes JPG, PNG o WEBP."
            });
        }

        var currentUser = GetUserName();
        var extension = GetImageExtension(normalizedContentType);
        var objectKey =
            $"purchase-requests/workspaces/{workspaceId:N}/" +
            $"{normalizedType.ToLowerInvariant()}/{referenceId:N}/" +
            $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}{extension}";

        var existing = await _context.PurchaseRequestEvidences
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x =>
                x.WorkspaceId == workspaceId &&
                x.ReferenceId == referenceId &&
                x.EvidenceType == normalizedType,
                cancellationToken);

        if (existing?.PurchaseRequestId is not null)
        {
            return BadRequest(new
            {
                Message = "La evidencia ya pertenece a una solicitud generada y no puede reemplazarse."
            });
        }

        var previousObjectKey = existing?.ObjectKey;

        await using var inputStream = file.OpenReadStream();
        using var uploadStream = new MemoryStream();
        await inputStream.CopyToAsync(uploadStream, cancellationToken);
        uploadStream.Position = 0;

        await _storageService.UploadAsync(
            objectKey,
            uploadStream,
            normalizedContentType,
            cancellationToken);

        try
        {
            if (existing is null)
            {
                existing = new PurchaseRequestEvidence
                {
                    Id = Guid.NewGuid(),
                    WorkspaceId = workspaceId,
                    ReferenceId = referenceId,
                    EvidenceType = normalizedType,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUser
                };

                _context.PurchaseRequestEvidences.Add(existing);
            }

            existing.IsDeleted = false;
            existing.ReferenceName = string.IsNullOrWhiteSpace(referenceName)
                ? null
                : referenceName.Trim();
            existing.FileName = Path.GetFileName(file.FileName);
            existing.ContentType = normalizedContentType;
            existing.ObjectKey = objectKey;
            existing.FileSize = file.Length;
            existing.UploadedBy = currentUser;
            existing.UploadedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = currentUser;

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            try
            {
                await _storageService.DeleteAsync(objectKey, cancellationToken);
            }
            catch
            {
                // No se oculta el error principal de persistencia.
            }

            throw;
        }

        if (!string.IsNullOrWhiteSpace(previousObjectKey) &&
            !string.Equals(previousObjectKey, objectKey, StringComparison.Ordinal))
        {
            try
            {
                await _storageService.DeleteAsync(
                    previousObjectKey,
                    cancellationToken);
            }
            catch
            {
                // El nuevo archivo ya está persistido.
            }
        }

        return Ok(ToEvidenceDto(existing));
    }

    [RequirePermission("Purchases.View", "Purchases.Request", "Purchases.Quote")]
    [HttpGet("evidences/{evidenceId:guid}/download")]
    public async Task<IActionResult> DownloadEvidence(
        Guid evidenceId,
        CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        var evidence = await _context.PurchaseRequestEvidences
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                x.Id == evidenceId,
                cancellationToken);

        if (evidence is null)
        {
            return NotFound(new { Message = "No se encontró la evidencia." });
        }

        if (!await _storageService.ExistsAsync(
                evidence.ObjectKey,
                cancellationToken))
        {
            return NotFound(new
            {
                Message = "El archivo físico de la evidencia no está disponible."
            });
        }

        var stream = await _storageService.DownloadAsync(
            evidence.ObjectKey,
            cancellationToken);

        return File(
            stream,
            evidence.ContentType,
            evidence.FileName,
            enableRangeProcessing: true);
    }

    [RequirePermission("Purchases.Generate")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseRequestRequest request, CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(request.ItemName))
        {
            return BadRequest(new { Message = "Debe ingresar el nombre del activo o herramienta solicitada." });
        }

        if (string.IsNullOrWhiteSpace(request.Justification))
        {
            return BadRequest(new { Message = "Debe ingresar la justificación de la compra." });
        }

        var currentUser = GetUserName();
        var number = await GenerateRequestNumberAsync(cancellationToken);

        var item = new PurchaseRequest
        {
            Id = Guid.NewGuid(),
            RequestNumber = number,
            ToolAssetId = request.ToolAssetId,
            ItemCode = Normalize(request.ItemCode),
            ItemName = request.ItemName.Trim(),
            ItemDescription = request.ItemDescription?.Trim(),
            Quantity = request.Quantity <= 0 ? 1 : request.Quantity,
            Unit = string.IsNullOrWhiteSpace(request.Unit) ? "Und" : request.Unit.Trim(),
            PurchasePurpose = string.IsNullOrWhiteSpace(request.PurchasePurpose) ? "Consumo" : request.PurchasePurpose.Trim(),
            Justification = request.Justification.Trim(),
            Priority = string.IsNullOrWhiteSpace(request.Priority) ? "Media" : request.Priority.Trim(),
            Status = request.SendToReview ? "InReview" : "Draft",
            BranchId = request.BranchId ?? GetBranchId(),
            RequestedByUserId = GetUserId(),
            RequestedByUserName = currentUser,
            RequestedByResponsiblePersonId = GetResponsiblePersonId(),
            RequestedByResponsiblePersonName = GetResponsiblePersonName(),
            PreparedBy = currentUser,
            RequestedAt = DateTime.UtcNow,
            SubmittedAt = request.SendToReview ? DateTime.UtcNow : null,
            SubmittedBy = request.SendToReview ? currentUser : null,
            RequiredAt = request.RequiredAt,
            ProjectId = request.ProjectId?.Trim(),
            VendorSuggestion = request.VendorSuggestion?.Trim(),
            EstimatedCostText = request.EstimatedCostText?.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUser
        };

        if (string.IsNullOrWhiteSpace(item.ItemCode))
        {
            item.ItemCode = item.ToolAssetId.HasValue
                ? "ACTIVO-ASOCIADO"
                : "SIN-CODIGO";
        }

        _context.PurchaseRequests.Add(item);

        if (request.WorkspaceId.HasValue &&
            request.EvidenceIds is { Count: > 0 })
        {
            var evidenceIds = request.EvidenceIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            var evidences = await _context.PurchaseRequestEvidences
                .Where(x =>
                    x.WorkspaceId == request.WorkspaceId.Value &&
                    x.PurchaseRequestId == null &&
                    evidenceIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            if (evidences.Count != evidenceIds.Count)
            {
                return BadRequest(new
                {
                    Message = "Una o más evidencias no pertenecen a la solicitud activa."
                });
            }

            foreach (var evidence in evidences)
            {
                evidence.PurchaseRequestId = item.Id;
                evidence.UpdatedAt = DateTime.UtcNow;
                evidence.UpdatedBy = currentUser;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = request.SendToReview
                ? "Solicitud de compra creada y enviada a revisión."
                : "Solicitud de compra creada en borrador.",
            item.Id,
            item.RequestNumber,
            item.Status
        });
    }
    [RequirePermission("Purchases.Reject")]
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> Submit(Guid id, CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        var item = await _context.PurchaseRequests.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de compra." });
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
    [RequirePermission("Purchases.Approve")]
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApprovalRequest request, CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        var item = await _context.PurchaseRequests.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de compra." });
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

        return Ok(new { Message = "Solicitud aprobada.", item.Id, item.RequestNumber, item.Status });
    }
    [RequirePermission("Purchases.Reject")]
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] ApprovalRequest request, CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        var item = await _context.PurchaseRequests.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de compra." });
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
    [RequirePermission("Purchases.Approve")]
    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] ApprovalRequest request, CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        var item = await _context.PurchaseRequests.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de compra." });
        }

        if (item.Status != "Approved")
        {
            return BadRequest(new { Message = "Solo se pueden cerrar solicitudes aprobadas." });
        }

        item.Status = "Closed";
        item.ClosedAt = DateTime.UtcNow;
        item.ClosedBy = GetUserName();
        item.Notes = string.IsNullOrWhiteSpace(request.Comment)
            ? item.Notes
            : request.Comment.Trim();
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = GetUserName();

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Solicitud cerrada.", item.Id, item.RequestNumber, item.Status });
    }
    [RequirePermission("Purchases.Request")]
    [HttpPost("{id:guid}/mark-sent-dynamics")]
    public async Task<IActionResult> MarkSentToDynamics(Guid id, [FromBody] DynamicsMarkRequest request, CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        var item = await _context.PurchaseRequests.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (item is null)
        {
            return NotFound(new { Message = "No se encontró la solicitud de compra." });
        }

        item.SentToDynamics = true;
        item.SentToDynamicsAt = DateTime.UtcNow;
        item.DynamicsPurchaseRequisitionNumber = string.IsNullOrWhiteSpace(request.DynamicsPurchaseRequisitionNumber)
            ? item.DynamicsPurchaseRequisitionNumber
            : request.DynamicsPurchaseRequisitionNumber.Trim();
        item.DynamicsStatus = string.IsNullOrWhiteSpace(request.DynamicsStatus)
            ? "Pendiente integración"
            : request.DynamicsStatus.Trim();
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = GetUserName();

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Solicitud marcada como enviada a Dynamics.", item.Id, item.RequestNumber });
    }
    [RequirePermission("Purchases.Request")]
    [HttpPost("seed-demo")]
    public async Task<IActionResult> SeedDemo(CancellationToken cancellationToken)
    {
        await EnsurePurchaseSchemaAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var currentUser = GetUserName();

        var existingDemo = await _context.PurchaseRequests
            .IgnoreQueryFilters()
            .Where(x => x.RequestNumber.StartsWith("SPC-NAVI-DEMO-"))
            .ToListAsync(cancellationToken);

        if (existingDemo.Any())
        {
            _context.PurchaseRequests.RemoveRange(existingDemo);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var demoRequests = new List<PurchaseRequest>
        {
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "SPC-NAVI-DEMO-0001",
                ItemCode = "TALADRO-PERCUTOR",
                ItemName = "Taladro percutor industrial",
                ItemDescription = "Herramienta requerida para trabajos de taller y mantenimiento correctivo.",
                Quantity = 2,
                Unit = "Und",
                PurchasePurpose = "Activo fijo",
                Justification = "Reposición de herramienta por desgaste operativo y aumento de demanda en taller.",
                Priority = "Alta",
                Status = "Draft",
                RequestedByUserName = currentUser,
                PreparedBy = currentUser,
                RequestedAt = now.AddDays(-8),
                RequiredAt = now.AddDays(15),
                VendorSuggestion = "Proveedor local ferretería industrial",
                EstimatedCostText = "$1.800.000",
                Notes = "Solicitud demo en estado borrador.",
                CreatedAt = now.AddDays(-8),
                CreatedBy = currentUser
            },
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "SPC-NAVI-DEMO-0002",
                ItemCode = "GATO-HIDRAULICO-20T",
                ItemName = "Gato hidráulico 20 toneladas",
                ItemDescription = "Equipo especializado para soporte de vehículos pesados.",
                Quantity = 1,
                Unit = "Und",
                PurchasePurpose = "Activo fijo",
                Justification = "Se requiere para operación segura en intervención de vehículos de carga.",
                Priority = "Crítica",
                Status = "InReview",
                RequestedByUserName = currentUser,
                PreparedBy = currentUser,
                RequestedAt = now.AddDays(-6),
                SubmittedAt = now.AddDays(-5),
                SubmittedBy = currentUser,
                RequiredAt = now.AddDays(10),
                VendorSuggestion = "Proveedor equipos hidráulicos",
                EstimatedCostText = "$4.500.000",
                Notes = "Solicitud demo pendiente por aprobación.",
                CreatedAt = now.AddDays(-6),
                CreatedBy = currentUser
            },
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "SPC-NAVI-DEMO-0003",
                ItemCode = "ESCANER-DIAGNOSTICO",
                ItemName = "Escáner de diagnóstico multimarca",
                ItemDescription = "Equipo electrónico para diagnóstico de motores y sistemas eléctricos.",
                Quantity = 1,
                Unit = "Und",
                PurchasePurpose = "Activo fijo",
                Justification = "Permite reducir tiempos de diagnóstico y mejorar trazabilidad técnica.",
                Priority = "Alta",
                Status = "Approved",
                RequestedByUserName = "herramientero",
                PreparedBy = "herramientero",
                RequestedAt = now.AddDays(-12),
                SubmittedAt = now.AddDays(-11),
                SubmittedBy = "herramientero",
                ApprovedAt = now.AddDays(-10),
                ApprovedBy = "ing_servicios",
                ApprovalComment = "Aprobado por necesidad operativa del taller.",
                RequiredAt = now.AddDays(5),
                VendorSuggestion = "Proveedor tecnología automotriz",
                EstimatedCostText = "$9.800.000",
                Notes = "Solicitud demo aprobada, pendiente de enviar a Dynamics.",
                CreatedAt = now.AddDays(-12),
                CreatedBy = "herramientero"
            },
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "SPC-NAVI-DEMO-0004",
                ItemCode = "KIT-LLAVES-TORQUE",
                ItemName = "Kit de llaves de torque",
                ItemDescription = "Herramientas manuales de precisión para torque controlado.",
                Quantity = 3,
                Unit = "Kit",
                PurchasePurpose = "Reposición",
                Justification = "Se solicita reposición por pérdida de precisión y desgaste.",
                Priority = "Media",
                Status = "Rejected",
                RequestedByUserName = "tecnico",
                PreparedBy = "tecnico",
                RequestedAt = now.AddDays(-15),
                SubmittedAt = now.AddDays(-14),
                SubmittedBy = "tecnico",
                RejectedAt = now.AddDays(-13),
                RejectedBy = "coordinador_taller",
                RejectionReason = "Debe validarse inventario disponible antes de comprar.",
                RequiredAt = now.AddDays(20),
                VendorSuggestion = "Proveedor herramientas manuales",
                EstimatedCostText = "$2.100.000",
                Notes = "Solicitud demo rechazada.",
                CreatedAt = now.AddDays(-15),
                CreatedBy = "tecnico"
            },
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "SPC-NAVI-DEMO-0005",
                ItemCode = "COMPRESOR-AIRE-50L",
                ItemName = "Compresor de aire 50 litros",
                ItemDescription = "Equipo para operación neumática de herramientas.",
                Quantity = 1,
                Unit = "Und",
                PurchasePurpose = "Mantenimiento",
                Justification = "Equipo requerido para mantener continuidad operativa en herramientas neumáticas.",
                Priority = "Media",
                Status = "Closed",
                RequestedByUserName = "coordinador_taller",
                PreparedBy = "coordinador_taller",
                RequestedAt = now.AddDays(-25),
                SubmittedAt = now.AddDays(-24),
                SubmittedBy = "coordinador_taller",
                ApprovedAt = now.AddDays(-23),
                ApprovedBy = "ing_servicios",
                ApprovalComment = "Aprobado y gestionado.",
                ClosedAt = now.AddDays(-20),
                ClosedBy = "admin",
                RequiredAt = now.AddDays(-5),
                VendorSuggestion = "Proveedor industrial",
                EstimatedCostText = "$3.200.000",
                SentToDynamics = true,
                SentToDynamicsAt = now.AddDays(-22),
                DynamicsPurchaseRequisitionNumber = "SPC000210-DEMO",
                DynamicsStatus = "Cerrado en ERP",
                Notes = "Solicitud demo cerrada y enviada a Dynamics.",
                CreatedAt = now.AddDays(-25),
                CreatedBy = "coordinador_taller"
            },
            new PurchaseRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = "SPC-NAVI-DEMO-0006",
                ItemCode = "EPP-GUANTES-INDUSTRIALES",
                ItemName = "Guantes industriales para operación de taller",
                ItemDescription = "Elemento requerido para manipulación segura de herramientas.",
                Quantity = 20,
                Unit = "Par",
                PurchasePurpose = "Consumo",
                Justification = "Requerimiento de seguridad para actividades operativas del taller.",
                Priority = "Baja",
                Status = "InReview",
                RequestedByUserName = "herramientero",
                PreparedBy = "herramientero",
                RequestedAt = now.AddDays(-2),
                SubmittedAt = now.AddDays(-1),
                SubmittedBy = "herramientero",
                RequiredAt = now.AddDays(7),
                VendorSuggestion = "Proveedor EPP",
                EstimatedCostText = "$600.000",
                Notes = "Solicitud demo de consumo pendiente por aprobación.",
                CreatedAt = now.AddDays(-2),
                CreatedBy = "herramientero"
            }
        };

        _context.PurchaseRequests.AddRange(demoRequests);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Solicitudes demo insertadas correctamente.",
            Total = demoRequests.Count,
            Estados = demoRequests
                .GroupBy(x => x.Status)
                .Select(x => new
                {
                    Estado = x.Key,
                    Cantidad = x.Count()
                })
                .ToList()
        });
    }

    private static PurchaseRequestEvidenceDto ToEvidenceDto(
        PurchaseRequestEvidence evidence)
    {
        return new PurchaseRequestEvidenceDto
        {
            Id = evidence.Id,
            WorkspaceId = evidence.WorkspaceId,
            PurchaseRequestId = evidence.PurchaseRequestId,
            ReferenceId = evidence.ReferenceId,
            EvidenceType = evidence.EvidenceType,
            ReferenceName = evidence.ReferenceName,
            FileName = evidence.FileName,
            ContentType = evidence.ContentType,
            FileSize = evidence.FileSize,
            UploadedBy = evidence.UploadedBy,
            UploadedAt = evidence.UploadedAt,
            DownloadUrl =
                $"api/purchase-requests/evidences/{evidence.Id}/download"
        };
    }

    private static string? NormalizeEvidenceType(string? value)
    {
        if (string.Equals(
                value,
                "ToolImage",
                StringComparison.OrdinalIgnoreCase))
        {
            return "ToolImage";
        }

        if (string.Equals(
                value,
                "QuotationImage",
                StringComparison.OrdinalIgnoreCase))
        {
            return "QuotationImage";
        }

        return null;
    }

    private static string? NormalizeImageContentType(
        string? contentType,
        string? fileName)
    {
        var normalized = contentType?.Trim().ToLowerInvariant();

        if (normalized is "image/jpeg" or "image/png" or "image/webp")
        {
            return normalized;
        }

        return (Path.GetExtension(fileName) ?? string.Empty).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => null
        };
    }

    private static string GetImageExtension(string contentType)
    {
        return contentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }

    private async Task<string> GenerateRequestNumberAsync(CancellationToken cancellationToken)
    {
        var prefix = $"SPC-NAVI-{DateTime.UtcNow:yyyyMM}";

        var count = await _context.PurchaseRequests
            .IgnoreQueryFilters()
            .CountAsync(x => x.RequestNumber.StartsWith(prefix), cancellationToken);

        return $"{prefix}-{count + 1:0000}";
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }

    private string GetUserName()
    {
        return Request.Headers.TryGetValue("X-Navi-User", out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : "admin-web";
    }

    private Guid? GetUserId()
    {
        return TryGetGuidHeader("X-Navi-UserId");
    }

    private Guid? GetBranchId()
    {
        return TryGetGuidHeader("X-Navi-BranchId");
    }

    private Guid? GetResponsiblePersonId()
    {
        return TryGetGuidHeader("X-Navi-ResponsiblePersonId");
    }

    private string? GetResponsiblePersonName()
    {
        if (Request.Headers.TryGetValue("X-Navi-ResponsiblePersonName-B64", out var encodedValue) &&
            !string.IsNullOrWhiteSpace(encodedValue))
        {
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(encodedValue.ToString()));
            }
            catch (FormatException)
            {
                return null;
            }
        }

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

        return Guid.TryParse(value.ToString(), out var id)
            ? id
            : null;
    }

    private async Task EnsurePurchaseSchemaAsync(CancellationToken cancellationToken)
    {
        var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Purchases')
BEGIN
    EXEC('CREATE SCHEMA [Purchases]')
END

IF OBJECT_ID('[Purchases].[PurchaseRequests]', 'U') IS NULL
BEGIN
    CREATE TABLE [Purchases].[PurchaseRequests](
        [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_PurchaseRequests] PRIMARY KEY,
        [RequestNumber] NVARCHAR(60) NOT NULL,
        [ToolAssetId] UNIQUEIDENTIFIER NULL,
        [ItemCode] NVARCHAR(100) NOT NULL,
        [ItemName] NVARCHAR(300) NOT NULL,
        [ItemDescription] NVARCHAR(1000) NULL,
        [Quantity] INT NOT NULL,
        [Unit] NVARCHAR(40) NOT NULL,
        [PurchasePurpose] NVARCHAR(80) NOT NULL,
        [Justification] NVARCHAR(2000) NOT NULL,
        [Priority] NVARCHAR(40) NOT NULL,
        [Status] NVARCHAR(40) NOT NULL,
        [BranchId] UNIQUEIDENTIFIER NULL,
        [RequestedByUserId] UNIQUEIDENTIFIER NULL,
        [RequestedByUserName] NVARCHAR(150) NOT NULL,
        [RequestedByResponsiblePersonId] UNIQUEIDENTIFIER NULL,
        [RequestedByResponsiblePersonName] NVARCHAR(250) NULL,
        [PreparedBy] NVARCHAR(150) NOT NULL,
        [RequestedAt] DATETIME2 NOT NULL,
        [SubmittedAt] DATETIME2 NULL,
        [SubmittedBy] NVARCHAR(150) NULL,
        [RequiredAt] DATETIME2 NULL,
        [ProjectId] NVARCHAR(120) NULL,
        [VendorSuggestion] NVARCHAR(300) NULL,
        [EstimatedCostText] NVARCHAR(120) NULL,
        [ApprovalComment] NVARCHAR(1000) NULL,
        [ApprovedBy] NVARCHAR(150) NULL,
        [ApprovedAt] DATETIME2 NULL,
        [RejectedBy] NVARCHAR(150) NULL,
        [RejectedAt] DATETIME2 NULL,
        [RejectionReason] NVARCHAR(1000) NULL,
        [ClosedBy] NVARCHAR(150) NULL,
        [ClosedAt] DATETIME2 NULL,
        [SentToDynamics] BIT NOT NULL CONSTRAINT [DF_PurchaseRequests_SentToDynamics] DEFAULT(0),
        [DynamicsPurchaseRequisitionNumber] NVARCHAR(120) NULL,
        [SentToDynamicsAt] DATETIME2 NULL,
        [DynamicsStatus] NVARCHAR(120) NULL,
        [Notes] NVARCHAR(2000) NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [CreatedBy] NVARCHAR(150) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(150) NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_PurchaseRequests_IsDeleted] DEFAULT(0)
    );

    CREATE UNIQUE INDEX [IX_PurchaseRequests_RequestNumber]
        ON [Purchases].[PurchaseRequests]([RequestNumber]);
END

IF OBJECT_ID('[Purchases].[PurchaseRequestEvidences]', 'U') IS NULL
BEGIN
    CREATE TABLE [Purchases].[PurchaseRequestEvidences](
        [Id] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [PK_PurchaseRequestEvidences] PRIMARY KEY,
        [WorkspaceId] UNIQUEIDENTIFIER NOT NULL,
        [PurchaseRequestId] UNIQUEIDENTIFIER NULL,
        [ReferenceId] UNIQUEIDENTIFIER NOT NULL,
        [EvidenceType] NVARCHAR(40) NOT NULL,
        [ReferenceName] NVARCHAR(300) NULL,
        [FileName] NVARCHAR(260) NOT NULL,
        [ContentType] NVARCHAR(100) NOT NULL,
        [ObjectKey] NVARCHAR(700) NOT NULL,
        [FileSize] BIGINT NOT NULL,
        [UploadedBy] NVARCHAR(150) NOT NULL,
        [UploadedAt] DATETIME2 NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [CreatedBy] NVARCHAR(150) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(150) NULL,
        [IsDeleted] BIT NOT NULL CONSTRAINT [DF_PurchaseRequestEvidences_IsDeleted] DEFAULT(0)
    );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PurchaseRequestEvidences_WorkspaceId'
      AND object_id = OBJECT_ID('[Purchases].[PurchaseRequestEvidences]')
)
BEGIN
    CREATE INDEX [IX_PurchaseRequestEvidences_WorkspaceId]
        ON [Purchases].[PurchaseRequestEvidences]([WorkspaceId]);
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PurchaseRequestEvidences_PurchaseRequestId'
      AND object_id = OBJECT_ID('[Purchases].[PurchaseRequestEvidences]')
)
BEGIN
    CREATE INDEX [IX_PurchaseRequestEvidences_PurchaseRequestId]
        ON [Purchases].[PurchaseRequestEvidences]([PurchaseRequestId]);
END

IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_PurchaseRequestEvidences_Workspace_Type_Reference'
      AND object_id = OBJECT_ID('[Purchases].[PurchaseRequestEvidences]')
)
BEGIN
    CREATE INDEX [IX_PurchaseRequestEvidences_Workspace_Type_Reference]
        ON [Purchases].[PurchaseRequestEvidences](
            [WorkspaceId],
            [EvidenceType],
            [ReferenceId]
        );
END

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = 'FK_PurchaseRequestEvidences_PurchaseRequests_PurchaseRequestId'
)
BEGIN
    ALTER TABLE [Purchases].[PurchaseRequestEvidences]
        ADD CONSTRAINT [FK_PurchaseRequestEvidences_PurchaseRequests_PurchaseRequestId]
        FOREIGN KEY ([PurchaseRequestId])
        REFERENCES [Purchases].[PurchaseRequests]([Id]);
END
";
        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }
}

public sealed class CreatePurchaseRequestRequest
{
    public Guid? ToolAssetId { get; set; }
    public string? ItemCode { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public int Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public string? PurchasePurpose { get; set; }
    public string Justification { get; set; } = string.Empty;
    public string? Priority { get; set; }
    public Guid? BranchId { get; set; }
    public DateTime? RequiredAt { get; set; }
    public string? ProjectId { get; set; }
    public string? VendorSuggestion { get; set; }
    public string? EstimatedCostText { get; set; }
    public string? PurchaseType { get; set; }
    public string? RequestChannel { get; set; }
    public string? InventoryClassification { get; set; }
    public string? GenericCode { get; set; }
    public string? ItemVariant { get; set; }
    public string? VariantDetail { get; set; }
    public bool CodeExists { get; set; }
    public bool RequiresCodeCreation { get; set; }
    public bool RequiresVariantCreation { get; set; }
    public string? PlanningRequestReference { get; set; }
    public string? TechnicalSpecifications { get; set; }
    public string? Capacity { get; set; }
    public string? Dimensions { get; set; }
    public string? RequiredUse { get; set; }
    public string? SerialReference { get; set; }
    public string? FailureDetail { get; set; }
    public string? MaintenanceTypeIfApplies { get; set; }
    public bool HasPhotoSupport { get; set; }
    public string? PhotoSupportDescription { get; set; }
    public string? DocumentSupportReference { get; set; }
    public string? CostCenter { get; set; }
    public string? AccountingConcept { get; set; }
    public string? AccountingAccount { get; set; }
    public bool RequiresAccountingValidation { get; set; }
    public string? AccountingValidationStatus { get; set; }
    public string? AccountingValidationComment { get; set; }
    public string? FixedAssetReason { get; set; }
    public string? WarehouseCode { get; set; }
    public string? LocationCode { get; set; }
    public string? DeliveryWarehouse { get; set; }
    public string? AmountRange { get; set; }
    public bool IsLocalLowAmountPurchase { get; set; }
    public bool RequiresMroManagement { get; set; }
    public string? SelectedVendor { get; set; }
    public int? QuotationCount { get; set; }
    public string? QuotationReferences { get; set; }
    public string? VendorSelectionCriteria { get; set; }
    public string? MroBuyer { get; set; }
    public string? MroValidationStatus { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    public string? PurchaseOrderStatus { get; set; }
    public DateTime? PurchaseOrderDate { get; set; }
    public string? InvoiceReference { get; set; }
    public DateTime? ReceivedAt { get; set; }
    public string? ReceivedBy { get; set; }
    public string? Notes { get; set; }
    public Guid? WorkspaceId { get; set; }
    public List<Guid> EvidenceIds { get; set; } = new();
    public bool SendToReview { get; set; }
}

public sealed class ApprovalRequest
{
    public string? Comment { get; set; }
}

public sealed class DynamicsMarkRequest
{
    public string? DynamicsPurchaseRequisitionNumber { get; set; }
    public string? DynamicsStatus { get; set; }
}

public sealed class PurchaseRequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public Guid? ToolAssetId { get; set; }
    public string? ToolInternalCode { get; set; }
    public string? ToolName { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string PurchasePurpose { get; set; } = string.Empty;
    public string Justification { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string RequestedByUserName { get; set; } = string.Empty;
    public string? RequestedByResponsiblePersonName { get; set; }
    public string PreparedBy { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? SubmittedBy { get; set; }
    public DateTime? RequiredAt { get; set; }
    public string? ProjectId { get; set; }
    public string? VendorSuggestion { get; set; }
    public string? EstimatedCostText { get; set; }
    public string? ApprovalComment { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? ClosedBy { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool SentToDynamics { get; set; }
    public string? DynamicsPurchaseRequisitionNumber { get; set; }
    public string? DynamicsStatus { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PurchaseRequestEvidenceDto> Evidences { get; set; } = new();
}

public sealed class PurchaseRequestEvidenceDto
{
    public Guid Id { get; set; }
    public Guid WorkspaceId { get; set; }
    public Guid? PurchaseRequestId { get; set; }
    public Guid ReferenceId { get; set; }
    public string EvidenceType { get; set; } = string.Empty;
    public string? ReferenceName { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;
}
