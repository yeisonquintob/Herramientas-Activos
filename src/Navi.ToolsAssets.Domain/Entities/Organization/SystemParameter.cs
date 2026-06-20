using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Organization;

public class SystemParameter : BaseEntity
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Value { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
