using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;

namespace Navi.ToolsAssets.Domain.Entities.LifeCycles;

public class ToolLifeCycleEvent : BaseEntity
{
    public Guid ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public string EventType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? PreviousValue { get; set; }

    public string? NewValue { get; set; }

    public string? RegisteredBy { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
