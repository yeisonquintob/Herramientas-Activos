using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Inventory;

public class ToolAccessory : BaseEntity
{
    public Guid ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public bool RequiresMaintenance { get; set; }

    public bool IsMeasurementEquipment { get; set; }

    public string? Observation { get; set; }

    public bool IsActive { get; set; } = true;
}
