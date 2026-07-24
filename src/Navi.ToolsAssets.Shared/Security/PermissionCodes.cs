namespace Navi.ToolsAssets.Shared.Security;

/// <summary>
/// Fuente canónica de códigos de permisos. Los literales históricos continúan
/// siendo compatibles mientras los consumidores migran progresivamente.
/// </summary>
public static class PermissionCodes
{
    public const string DashboardView = "Dashboard.View";

    public const string ToolsView = "Tools.View";
    public const string ToolsCreate = "Tools.Create";
    public const string ToolsEdit = "Tools.Edit";
    public const string ToolsDelete = "Tools.Delete";
    public const string ToolsStatusChange = "Tools.Status.Change";
    public const string ToolsSpecializedMark = "Tools.Specialized.Mark";

    public const string AssetAvailabilityView = "AssetAvailability.View";
    public const string AssetAvailabilityEdit = "AssetAvailability.Edit";

    public const string AssetAssignmentView = "AssetAssignment.View";
    public const string AssetAssignmentRequest = "AssetAssignment.Request";
    public const string AssetAssignmentApprove = "AssetAssignment.Approve";
    public const string AssetAssignmentDeny = "AssetAssignment.Deny";
    public const string AssetAssignmentAssign = "AssetAssignment.Assign";
    public const string AssetAssignmentReturn = "AssetAssignment.Return";
    public const string AssetAssignmentHistory = "AssetAssignment.History";

    public const string TechnicalLifeRecordView = "TechnicalLifeRecord.View";
    public const string TechnicalLifeRecordEdit = "TechnicalLifeRecord.Edit";
    public const string TechnicalLifeRecordExport = "TechnicalLifeRecord.Export";
    public const string TechnicalLifeRecordAccessories = "TechnicalLifeRecord.Accessories";
    public const string TechnicalLifeRecordDocuments = "TechnicalLifeRecord.Documents";
    public const string TechnicalLifeRecordMaintenance = "TechnicalLifeRecord.Maintenance";
    public const string TechnicalLifeRecordSafePractices = "TechnicalLifeRecord.SafePractices";

    public const string DocumentsView = "Documents.View";
    public const string DocumentsUpload = "Documents.Upload";
    public const string DocumentsDownload = "Documents.Download";
    public const string DocumentsDelete = "Documents.Delete";

    public const string MaintenanceView = "Maintenance.View";
    public const string MaintenanceRequest = "Maintenance.Request";
    public const string MaintenanceQuote = "Maintenance.Quote";
    public const string MaintenanceGenerate = "Maintenance.Generate";
    public const string MaintenanceExecute = "Maintenance.Execute";
    public const string MaintenanceClose = "Maintenance.Close";
    public const string MaintenanceReject = "Maintenance.Reject";
    public const string MaintenancePlansView = "Maintenance.Plans.View";
    public const string MaintenancePlansManage = "Maintenance.Plans.Manage";

    public const string PurchasesView = "Purchases.View";
    public const string PurchasesRequest = "Purchases.Request";
    public const string PurchasesQuote = "Purchases.Quote";
    public const string PurchasesGenerate = "Purchases.Generate";
    public const string PurchasesApprove = "Purchases.Approve";
    public const string PurchasesReject = "Purchases.Reject";

    public const string PhysicalCountsView = "PhysicalCounts.View";
    public const string PhysicalCountsCreate = "PhysicalCounts.Create";
    public const string PhysicalCountsClose = "PhysicalCounts.Close";
    public const string PhysicalCountsReport = "PhysicalCounts.Report";
    public const string PhysicalCountsEvidenceUpload = "PhysicalCounts.Evidence.Upload";

    public const string SafePracticesView = "SafePractices.View";
    public const string SafePracticesManage = "SafePractices.Manage";

    public const string ReportsView = "Reports.View";
    public const string ReportsExport = "Reports.Export";

    public const string ReconciliationView = "Reconciliation.View";
    public const string ReconciliationManage = "Reconciliation.Manage";
    public const string ReconciliationClarify = "Reconciliation.Clarify";
    public const string ReconciliationApproveCreation = "Reconciliation.ApproveCreation";

    public const string SettingsView = "Settings.View";
    public const string SettingsManage = "Settings.Manage";
    public const string SecurityUsers = "Security.Users";
    public const string SecurityRoles = "Security.Roles";

    public const string MobileAccess = "Mobile.Access";
    public const string MobileToolsView = "Mobile.Tools.View";
    public const string MobileToolsReview = "Mobile.Tools.Review";
    public const string MobilePreOperationalReport = "Mobile.PreOperational.Report";
    public const string MobileDamageReport = "Mobile.Damage.Report";
    public const string MobileLoansRequest = "Mobile.Loans.Request";
}
