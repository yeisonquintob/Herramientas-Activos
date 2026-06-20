using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Enums;

namespace Navi.ToolsAssets.Domain.Entities.Documents;

public class ToolDocument : BaseEntity
{
    public Guid ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public ToolDocumentType DocumentType { get; set; } = ToolDocumentType.Other;

    public string FileName { get; set; } = string.Empty;

    public string ObjectKey { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long SizeBytes { get; set; }

    public string? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public string? Description { get; set; }
}
