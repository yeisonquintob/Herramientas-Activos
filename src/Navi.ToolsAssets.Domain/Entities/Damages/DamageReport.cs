using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Enums;

namespace Navi.ToolsAssets.Domain.Entities.Damages;

public class DamageReport : BaseEntity
{
    public string ReportNumber { get; set; } = string.Empty;

    public Guid ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public ToolDamageSeverity Severity { get; set; } = ToolDamageSeverity.Low;

    public string Description { get; set; } = string.Empty;

    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    public string? ReportedBy { get; set; }

    public string? ActionTaken { get; set; }

    public bool BlocksLoan { get; set; } = true;
}
