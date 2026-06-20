using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;

namespace Navi.ToolsAssets.Domain.Entities.PhysicalCounts;

public class PhysicalCountItem : BaseEntity
{
    public Guid PhysicalCountId { get; set; }

    public PhysicalCount? PhysicalCount { get; set; }

    public Guid ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public bool WasFound { get; set; }

    public string? ExpectedLocation { get; set; }

    public string? FoundLocation { get; set; }

    public string? Observation { get; set; }

    public DateTime CountedAt { get; set; } = DateTime.UtcNow;
}
