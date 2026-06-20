using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Inventory;

public class ToolCategory : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ToolAsset> Tools { get; set; } = new List<ToolAsset>();
}
