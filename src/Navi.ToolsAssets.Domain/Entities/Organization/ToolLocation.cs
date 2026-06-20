using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;

namespace Navi.ToolsAssets.Domain.Entities.Organization;

public class ToolLocation : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid BranchId { get; set; }

    public Branch? Branch { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ToolAsset> Tools { get; set; } = new List<ToolAsset>();
}
