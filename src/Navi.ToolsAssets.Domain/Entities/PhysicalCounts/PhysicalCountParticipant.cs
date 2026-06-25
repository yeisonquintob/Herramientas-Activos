using System.ComponentModel.DataAnnotations.Schema;
using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Organization;

namespace Navi.ToolsAssets.Domain.Entities.PhysicalCounts;

[Table("PhysicalCountParticipants", Schema = "PhysicalCounts")]
public class PhysicalCountParticipant : BaseEntity
{
    public Guid PhysicalCountId { get; set; }

    public PhysicalCount? PhysicalCount { get; set; }

    public Guid? ResponsiblePersonId { get; set; }

    public ResponsiblePerson? ResponsiblePerson { get; set; }

    public Guid? BranchId { get; set; }

    public Branch? Branch { get; set; }

    public Guid? ZoneId { get; set; }

    public Zone? Zone { get; set; }

    public Guid? LocationId { get; set; }

    public ToolLocation? Location { get; set; }

    public string? UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Area { get; set; }

    public string? Position { get; set; }

    public string Status { get; set; } = "NotStarted";

    public int ExpectedItems { get; set; }

    public int CountedItems { get; set; }

    public int PendingItems { get; set; }

    public int FoundItems { get; set; }

    public int MissingItems { get; set; }

    public int DifferentItems { get; set; }

    public int DamagedItems { get; set; }

    public int ExtraItems { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public DateTime? LastActivityAt { get; set; }

    public string? Observation { get; set; }
}
