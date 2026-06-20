namespace Navi.ToolsAssets.Domain.Enums;

public enum ToolOperationalStatus
{
    Available = 1,
    Assigned = 2,
    Loaned = 3,
    InMaintenance = 4,
    Damaged = 5,
    NotSuitable = 6,
    PendingValidation = 7,
    Inconsistent = 8,
    NotLocated = 9,
    PendingDisposal = 10,
    Disposed = 11
}

public enum ToolPhysicalStatus
{
    Good = 1,
    Regular = 2,
    Damaged = 3,
    Lost = 4,
    Incomplete = 5
}

public enum ToolCustodyStatus
{
    InWarehouse = 1,
    AssignedToResponsible = 2,
    LoanedToTechnician = 3,
    InExternalService = 4,
    NotLocated = 5
}

public enum ToolReconciliationStatus
{
    Pending = 1,
    Validated = 2,
    Inconsistent = 3,
    PossibleDuplicate = 4,
    Duplicate = 5,
    NotFoundInFenix365 = 6,
    ExistsInFenix365WithoutLifeCycle = 7,
    LifeCycleWithoutFenix365Asset = 8
}

public enum ToolSyncStatus
{
    NotSynced = 1,
    PendingSync = 2,
    Synced = 3,
    SyncError = 4
}

public enum ToolDocumentType
{
    LifeCycleExcel = 1,
    PhotoEvidence = 2,
    DeliveryAct = 3,
    ReturnAct = 4,
    MaintenanceSupport = 5,
    DisposalSupport = 6,
    ImportFile = 7,
    TechnicalDocument = 8,
    PhysicalCountEvidence = 9,
    Other = 99
}

public enum ToolLoanStatus
{
    Draft = 1,
    Requested = 2,
    Approved = 3,
    Delivered = 4,
    PartiallyReturned = 5,
    Returned = 6,
    Cancelled = 7,
    Rejected = 8
}

public enum ToolDamageSeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum ToolMaintenanceType
{
    Preventive = 1,
    Corrective = 2,
    Calibration = 3,
    Inspection = 4
}

public enum ToolMaintenanceStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Failed = 5
}

public enum PhysicalCountStatus
{
    Draft = 1,
    InProgress = 2,
    Completed = 3,
    Reconciled = 4,
    Cancelled = 5
}
