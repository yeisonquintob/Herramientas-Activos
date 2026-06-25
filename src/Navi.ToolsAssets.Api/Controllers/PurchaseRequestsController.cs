using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Domain.Entities.Purchases;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/purchase-requests")]
public sealed class PurchaseRequestsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public PurchaseRequestsController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }
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

        return Ok(item);
    }
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
    public string? Notes { get; set; }
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
}



