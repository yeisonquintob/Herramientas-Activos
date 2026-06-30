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

    public string? PurchaseType { get; set; }

    public string? RequestChannel { get; set; }

    public string? InventoryClassification { get; set; }

    public string? GenericCode { get; set; }

    public string? ItemVariant { get; set; }

    public string? VariantDetail { get; set; }

    public bool CodeExists { get; set; }

    public bool RequiresCodeCreation { get; set; }

    public bool RequiresVariantCreation { get; set; }

    public string? PlanningRequestReference { get; set; }

    public string? TechnicalSpecifications { get; set; }

    public string? Capacity { get; set; }

    public string? Dimensions { get; set; }

    public string? RequiredUse { get; set; }

    public string? SerialReference { get; set; }

    public string? FailureDetail { get; set; }

    public string? MaintenanceTypeIfApplies { get; set; }

    public bool HasPhotoSupport { get; set; }

    public string? PhotoSupportDescription { get; set; }

    public string? DocumentSupportReference { get; set; }

    public string? CostCenter { get; set; }

    public string? AccountingConcept { get; set; }

    public string? AccountingAccount { get; set; }

    public bool RequiresAccountingValidation { get; set; }

    public string? AccountingValidationStatus { get; set; }

    public string? AccountingValidationComment { get; set; }

    public string? FixedAssetReason { get; set; }

    public string? WarehouseCode { get; set; }

    public string? LocationCode { get; set; }

    public string? DeliveryWarehouse { get; set; }

    public string? AmountRange { get; set; }

    public bool IsLocalLowAmountPurchase { get; set; }

    public bool RequiresMroManagement { get; set; }

    public string? SelectedVendor { get; set; }

    public int? QuotationCount { get; set; }

    public string? QuotationReferences { get; set; }

    public string? VendorSelectionCriteria { get; set; }

    public string? MroBuyer { get; set; }

    public string? MroValidationStatus { get; set; }

    public string? PurchaseOrderNumber { get; set; }

    public string? PurchaseOrderStatus { get; set; }

    public DateTime? PurchaseOrderDate { get; set; }

    public string? InvoiceReference { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public string? ReceivedBy { get; set; }

    public string? Notes { get; set; }
}
