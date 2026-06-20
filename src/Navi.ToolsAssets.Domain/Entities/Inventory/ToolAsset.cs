using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Damages;
using Navi.ToolsAssets.Domain.Entities.Documents;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Loans;
using Navi.ToolsAssets.Domain.Entities.Maintenance;
using Navi.ToolsAssets.Domain.Entities.Organization;
using Navi.ToolsAssets.Domain.Entities.PhysicalCounts;
using Navi.ToolsAssets.Domain.Entities.Sync;
using Navi.ToolsAssets.Domain.Enums;

namespace Navi.ToolsAssets.Domain.Entities.Inventory;

public class ToolAsset : BaseEntity
{
    public string InternalCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? FixedAssetCode { get; set; }

    public string? FenixCode { get; set; }

    public DateTime? AcquisitionDate { get; set; }

    public int? UsefulLifeMonths { get; set; }

    public string UnitOfMeasure { get; set; } = "UND";

    public decimal Quantity { get; set; } = 1;

    public bool RequiresMaintenance { get; set; }

    public bool RequiresPreOperationalCheck { get; set; }

    public bool RequiresCertification { get; set; }

    public DateTime? CertificationExpirationDate { get; set; }

    public Guid ZoneId { get; set; }

    public Zone? Zone { get; set; }

    public Guid BranchId { get; set; }

    public Branch? Branch { get; set; }

    public Guid? LocationId { get; set; }

    public ToolLocation? Location { get; set; }

    public Guid? ResponsiblePersonId { get; set; }

    public ResponsiblePerson? ResponsiblePerson { get; set; }

    public Guid? ToolTypeId { get; set; }

    public ToolType? ToolType { get; set; }

    public Guid? ToolCategoryId { get; set; }

    public ToolCategory? ToolCategory { get; set; }

    public ToolOperationalStatus OperationalStatus { get; set; } = ToolOperationalStatus.PendingValidation;

    public ToolPhysicalStatus PhysicalStatus { get; set; } = ToolPhysicalStatus.Good;

    public ToolCustodyStatus CustodyStatus { get; set; } = ToolCustodyStatus.InWarehouse;

    public ToolReconciliationStatus ReconciliationStatus { get; set; } = ToolReconciliationStatus.Pending;

    public ToolSyncStatus SyncStatus { get; set; } = ToolSyncStatus.NotSynced;

    public string? FenixName { get; set; }

    public string? FenixStatus { get; set; }

    public string? FenixBranch { get; set; }

    public string? FenixResponsible { get; set; }

    public DateTime? LastSyncAt { get; set; }

    public ICollection<ToolDocument> Documents { get; set; } = new List<ToolDocument>();

    public ICollection<ToolLifeCycleEvent> LifeCycleEvents { get; set; } = new List<ToolLifeCycleEvent>();

    public ICollection<ToolLoanItem> LoanItems { get; set; } = new List<ToolLoanItem>();

    public ICollection<DamageReport> DamageReports { get; set; } = new List<DamageReport>();

    public ICollection<MaintenanceRecord> MaintenanceRecords { get; set; } = new List<MaintenanceRecord>();

    public ICollection<PhysicalCountItem> PhysicalCountItems { get; set; } = new List<PhysicalCountItem>();

    public ICollection<FenixReconciliationRecord> ReconciliationRecords { get; set; } = new List<FenixReconciliationRecord>();
}
