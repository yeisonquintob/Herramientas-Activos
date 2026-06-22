using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Configuration;

public class SettingCatalogItem : BaseEntity
{
    public string CatalogType { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
