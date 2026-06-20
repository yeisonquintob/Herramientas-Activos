using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Enums;

namespace Navi.ToolsAssets.Domain.Entities.Sync;

public class FenixReconciliationRecord : BaseEntity
{
    public Guid ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public string SourceSystem { get; set; } = "Fenix365";

    public string? FenixCode { get; set; }

    public string? FixedAssetCode { get; set; }

    public string? FenixStatus { get; set; }

    public string? FenixBranch { get; set; }

    public string? FenixResponsible { get; set; }

    public ToolReconciliationStatus ResultStatus { get; set; } = ToolReconciliationStatus.Pending;

    public string? Differences { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}
