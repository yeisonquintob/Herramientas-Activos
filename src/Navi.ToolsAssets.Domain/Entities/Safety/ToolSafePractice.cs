using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;

namespace Navi.ToolsAssets.Domain.Entities.Safety;

public class ToolSafePractice : BaseEntity
{
    public Guid ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public string PracticeName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
