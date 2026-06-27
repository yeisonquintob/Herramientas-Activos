namespace Navi.ToolsAssets.MobilePwa.Models;

public sealed class MobileExecutiveDashboard
{
    public DateTime GeneratedAt { get; set; }
    public MobileDashboardSummary Summary { get; set; } = new();
    public List<MobileStatusItem> ToolsByStatus { get; set; } = new();
    public List<MobileBranchItem> ToolsByBranch { get; set; } = new();
    public List<MobileMaintenancePeriodItem> MaintenanceByMonth { get; set; } = new();
    public List<MobileMaintenancePeriodItem> MaintenanceByDay { get; set; } = new();
    public MobileLifeRecordCoverage LifeRecordCoverage { get; set; } = new();
    public MobileMaintenanceSummary MaintenanceSummary { get; set; } = new();
}

public sealed class MobileDashboardSummary
{
    public int TotalTools { get; set; }
    public int AvailableTools { get; set; }
    public int PendingValidationTools { get; set; }
    public int AssignedTools { get; set; }
    public int LoanedTools { get; set; }
    public int InMaintenanceTools { get; set; }
    public int DamagedTools { get; set; }
    public int NotLocatedTools { get; set; }
    public int DisposedTools { get; set; }
    public int FixedAssets { get; set; }
    public int ToolsWithoutFixedAssetCode { get; set; }
    public int ToolsOnly { get; set; }
    public int NonToolAssets { get; set; }
    public int SpecializedTools { get; set; }
    public int RequiresMaintenance { get; set; }
    public int ActiveResponsibles { get; set; }
    public int InactiveResponsibles { get; set; }
    public int ActiveBranches { get; set; }
    public int InactiveBranches { get; set; }
    public int WithLifeRecord { get; set; }
    public int WithoutLifeRecord { get; set; }
}

public sealed class MobileStatusItem
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class MobileBranchItem
{
    public string BranchCode { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public int Total { get; set; }
}

public sealed class MobileMaintenancePeriodItem
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public sealed class MobileLifeRecordCoverage
{
    public int MissingTechnicalData { get; set; }
}

public sealed class MobileMaintenanceSummary
{
    public int Total { get; set; }
    public int ThisMonth { get; set; }
    public int Today { get; set; }
}
