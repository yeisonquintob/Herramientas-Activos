using System.ComponentModel.DataAnnotations.Schema;
using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;

namespace Navi.ToolsAssets.Domain.Entities.PhysicalCounts;

[Table("PhysicalCountExtraItems", Schema = "PhysicalCounts")]
public class PhysicalCountExtraItem : BaseEntity
{
    public Guid PhysicalCountId { get; set; }

    public PhysicalCount? PhysicalCount { get; set; }

    public Guid? PhysicalCountParticipantId { get; set; }

    public PhysicalCountParticipant? Participant { get; set; }

    public Guid? MatchedToolAssetId { get; set; }

    public ToolAsset? MatchedToolAsset { get; set; }

    public string? ReportedCode { get; set; }

    public string ReportedName { get; set; } = string.Empty;

    public string? ReportedSerial { get; set; }

    public string? ReportedBrand { get; set; }

    public string? ReportedModel { get; set; }

    public string? FoundLocation { get; set; }

    public string? PhysicalStatus { get; set; }

    public string? Observation { get; set; }

    public string? ReportedBy { get; set; }

    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    public string ReconciliationStatus { get; set; } = "PendingReview";

    public string? ReconciliationObservation { get; set; }

    public bool RequiresUserClarification { get; set; }

    public DateTime? ClarificationRequestedAt { get; set; }

    public string? ClarificationRequestedBy { get; set; }

    public DateTime? ReconciledAt { get; set; }

    public string? ReconciledBy { get; set; }

    public string? SuggestedAction { get; set; }

    public bool ApprovedForCreation { get; set; }

    public DateTime? ApprovedForCreationAt { get; set; }

    public string? ApprovedForCreationBy { get; set; }

    public bool Rejected { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? RejectedBy { get; set; }

    public string? RejectionReason { get; set; }
}
