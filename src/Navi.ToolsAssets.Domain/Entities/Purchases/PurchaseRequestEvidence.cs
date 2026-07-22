using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Purchases;

public sealed class PurchaseRequestEvidence : BaseEntity
{
    public Guid WorkspaceId { get; set; }

    public Guid? PurchaseRequestId { get; set; }

    public PurchaseRequest? PurchaseRequest { get; set; }

    public Guid ReferenceId { get; set; }

    public string EvidenceType { get; set; } = string.Empty;

    public string? ReferenceName { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string ObjectKey { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string UploadedBy { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
