using System.ComponentModel.DataAnnotations.Schema;
using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.Organization;

namespace Navi.ToolsAssets.Domain.Entities.PhysicalCounts;

[Table("PhysicalCountReportedItems", Schema = "PhysicalCounts")]
public class PhysicalCountReportedItem : BaseEntity
{
    public Guid PhysicalCountId { get; set; }
    public PhysicalCount? PhysicalCount { get; set; }

    public Guid? PhysicalCountParticipantId { get; set; }
    public PhysicalCountParticipant? Participant { get; set; }

    public string ReportType { get; set; } = "ExtraNotListed";

    public Guid? ToolAssetId { get; set; }
    public ToolAsset? ToolAsset { get; set; }

    public Guid? MatchedToolAssetId { get; set; }
    public ToolAsset? MatchedToolAsset { get; set; }

    public Guid? CreatedToolAssetId { get; set; }

    public string? ReportedCode { get; set; }
    public string ReportedName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }

    public Guid? AssetTypeId { get; set; }
    public string? AssetTypeName { get; set; }

    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    public Guid? BranchId { get; set; }
    public Branch? Branch { get; set; }
    public string? BranchCode { get; set; }

    public Guid? LocationId { get; set; }
    public ToolLocation? Location { get; set; }
    public string? FoundLocation { get; set; }

    public Guid? ResponsiblePersonId { get; set; }
    public ResponsiblePerson? ResponsiblePerson { get; set; }
    public string? ResponsibleName { get; set; }

    public string? PhysicalStatus { get; set; }
    public string? OperationalStatus { get; set; }

    public string? Observation { get; set; }
    public Guid? EvidenceDocumentId { get; set; }

    public string? ReportedBy { get; set; }
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;

    public string ReconciliationStatus { get; set; } = "PendingReview";
    public string? ReconciliationObservation { get; set; }

    public bool RequiresUserClarification { get; set; }
    public DateTime? ClarificationRequestedAt { get; set; }
    public string? ClarificationRequestedBy { get; set; }

    public bool MinimumDataCompleted { get; set; }
    public string? MissingRequiredData { get; set; }

    public bool ApprovedForCreation { get; set; }
    public DateTime? ApprovedForCreationAt { get; set; }
    public string? ApprovedForCreationBy { get; set; }

    public bool Rejected { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime? ReconciledAt { get; set; }
    public string? ReconciledBy { get; set; }

    public string? SuggestedAction { get; set; }
}
