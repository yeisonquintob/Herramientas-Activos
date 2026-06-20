using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Organization;
using Navi.ToolsAssets.Domain.Enums;

namespace Navi.ToolsAssets.Domain.Entities.PhysicalCounts;

public class PhysicalCount : BaseEntity
{
    public string CountNumber { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public Branch? Branch { get; set; }

    public PhysicalCountStatus Status { get; set; } = PhysicalCountStatus.Draft;

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FinishedAt { get; set; }

    public string? ResponsibleBy { get; set; }

    public string? Notes { get; set; }

    public ICollection<PhysicalCountItem> Items { get; set; } = new List<PhysicalCountItem>();
}
