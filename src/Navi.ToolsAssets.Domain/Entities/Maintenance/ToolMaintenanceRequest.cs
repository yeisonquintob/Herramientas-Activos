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

    public string? RequestChannel { get; set; }

    public string? MaintenanceClassification { get; set; }

    public string? ServiceType { get; set; }

    public DateTime? RequiredAt { get; set; }

    public string? SerialNumber { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? EquipmentReference { get; set; }

    public string? ImageEvidenceDescription { get; set; }

    public string? EvidenceReference { get; set; }

    public string? ExcelReference { get; set; }

    public DateTime? FailureDate { get; set; }

    public string? NeedDescription { get; set; }

    public string? FailureDetail { get; set; }

    public string? MaintenanceLocation { get; set; }

    public decimal? EstimatedDowntimeHours { get; set; }

    public bool WarrantyApplies { get; set; }

    public string? WarrantyProvider { get; set; }

    public string? ServiceProvider { get; set; }

    public bool RequiresQuotation { get; set; }

    public int? QuotationCount { get; set; }

    public string? SelectedVendor { get; set; }

    public string? QuotationReferences { get; set; }

    public string? VendorSelectionReason { get; set; }

    public bool RequiresPurchaseOrder { get; set; }

    public string? PurchaseOrderNumber { get; set; }

    public string? PurchaseOrderStatus { get; set; }

    public string? MroCodeOrAccount { get; set; }

    public string? AccountingConcept { get; set; }

    public string? AccountingAccount { get; set; }

    public string? ProviderActivationCriteria { get; set; }

    public bool RequiresAccountingValidation { get; set; }

    public string? AccountingValidationStatus { get; set; }

    public string? AccountingValidationComment { get; set; }

    public string? Notes { get; set; }
}
