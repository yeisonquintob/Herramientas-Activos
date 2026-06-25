using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.Organization;

namespace Navi.ToolsAssets.Domain.Entities.Purchases;

public class PurchaseRequest : BaseEntity
{
    public string RequestNumber { get; set; } = string.Empty;

    public Guid? ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public string ItemCode { get; set; } = string.Empty;

    public string ItemName { get; set; } = string.Empty;

    public string? ItemDescription { get; set; }

    public int Quantity { get; set; } = 1;

    public string Unit { get; set; } = "Und";

    public string PurchasePurpose { get; set; } = "Consumo";

    public string Justification { get; set; } = string.Empty;

    public string Priority { get; set; } = "Media";

    public string Status { get; set; } = "Draft";

    public Guid? BranchId { get; set; }

    public Branch? Branch { get; set; }

    public Guid? RequestedByUserId { get; set; }

    public string RequestedByUserName { get; set; } = string.Empty;

    public Guid? RequestedByResponsiblePersonId { get; set; }

    public string? RequestedByResponsiblePersonName { get; set; }

    public string PreparedBy { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    public string? SubmittedBy { get; set; }

    public DateTime? RequiredAt { get; set; }

    public string? ProjectId { get; set; }

    public string? VendorSuggestion { get; set; }

    public string? EstimatedCostText { get; set; }

    public string? ApprovalComment { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? RejectedBy { get; set; }

    public DateTime? RejectedAt { get; set; }

    public string? RejectionReason { get; set; }

    public string? ClosedBy { get; set; }

    public DateTime? ClosedAt { get; set; }

    public bool SentToDynamics { get; set; }

    public string? DynamicsPurchaseRequisitionNumber { get; set; }

    public DateTime? SentToDynamicsAt { get; set; }

    public string? DynamicsStatus { get; set; }

    public string? Notes { get; set; }
}
