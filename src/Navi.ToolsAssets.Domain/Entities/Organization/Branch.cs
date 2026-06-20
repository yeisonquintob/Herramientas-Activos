using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;

namespace Navi.ToolsAssets.Domain.Entities.Organization;

public class Branch : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? City { get; set; }

    public string? Address { get; set; }

    public Guid ZoneId { get; set; }

    public Zone? Zone { get; set; }

    public bool IsPilot { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ToolLocation> Locations { get; set; } = new List<ToolLocation>();

    public ICollection<ToolAsset> Tools { get; set; } = new List<ToolAsset>();
}
