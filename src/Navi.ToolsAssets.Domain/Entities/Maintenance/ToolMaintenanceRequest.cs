using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.Organization;

namespace Navi.ToolsAssets.Domain.Entities.Maintenance;

public class ToolMaintenanceRequest : BaseEntity
{
    public string RequestNumber { get; set; } = string.Empty;

    public Guid? ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public Guid? BranchId { get; set; }

    public Branch? Branch { get; set; }

    public string RequestType { get; set; } = "Correctivo";

    public string Priority { get; set; } = "Media";

    public string Status { get; set; } = "Draft";

    public string Title { get; set; } = string.Empty;

    public string ProblemDescription { get; set; } = string.Empty;

    public string? WorkDescription { get; set; }

    public string? FailureCause { get; set; }

    public string? RequestedByUserName { get; set; }

    public Guid? RequestedByUserId { get; set; }

    public Guid? RequestedByResponsiblePersonId { get; set; }

    public string? RequestedByResponsiblePersonName { get; set; }

    public string PreparedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    public string? SubmittedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public string? ApprovalComment { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? RejectedBy { get; set; }

    public string? RejectionReason { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public string? ScheduledBy { get; set; }

    public string? AssignedTechnician { get; set; }

    public DateTime? ExecutionStartedAt { get; set; }

    public string? ExecutionStartedBy { get; set; }

    public DateTime? ExecutionFinishedAt { get; set; }

    public string? ExecutionFinishedBy { get; set; }

    public DateTime? ClosedAt { get; set; }

    public string? ClosedBy { get; set; }

    public string? ClosingComment { get; set; }

    public DateTime? CanceledAt { get; set; }

    public string? CanceledBy { get; set; }

    public string? CancellationReason { get; set; }

    public bool RequiresStop { get; set; }

    public bool IsSafetyRisk { get; set; }

    public string? EstimatedCostText { get; set; }

    public string? VendorSuggestion { get; set; }

    public string? Notes { get; set; }
}
