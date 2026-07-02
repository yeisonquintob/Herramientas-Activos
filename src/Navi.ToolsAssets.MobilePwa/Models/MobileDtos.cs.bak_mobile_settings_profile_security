namespace Navi.ToolsAssets.MobilePwa.Models;

public sealed class MobileLoginRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
}

public sealed class MobileUser
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Position { get; set; }
    public string? Area { get; set; }
    public Guid RoleId { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public Guid? ResponsiblePersonId { get; set; }
    public string? ResponsiblePersonName { get; set; }
    public List<string> Permissions { get; set; } = new();
    public DateTime? LastLoginAt { get; set; }
}

public sealed class MobileToolDto
{
    public Guid Id { get; set; }
    public string InternalCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? FixedAssetCode { get; set; }
    public string? FenixCode { get; set; }
    public decimal Quantity { get; set; }
    public bool IsSpecialized { get; set; }
    public bool RequiresMaintenance { get; set; }
    public bool RequiresPreOperationalCheck { get; set; }
    public bool RequiresCertification { get; set; }
    public string OperationalStatus { get; set; } = string.Empty;
    public string PhysicalStatus { get; set; } = string.Empty;
    public string CustodyStatus { get; set; } = string.Empty;
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? LocationName { get; set; }
    public Guid? ResponsiblePersonId { get; set; }
    public string? ResponsiblePersonName { get; set; }
    public string? ToolTypeName { get; set; }
    public string? ToolCategoryName { get; set; }
}

public sealed class MobileDamageReportRequest
{
    public Guid? ToolAssetId { get; set; }
    public string? ToolInternalCode { get; set; }
    public string? Severity { get; set; }
    public string? Description { get; set; }
    public string? ReportedBy { get; set; }
    public bool? BlocksLoan { get; set; }
}

public sealed class MobileDamageReportResponse
{
    public Guid Id { get; set; }
    public string? ReportNumber { get; set; }
    public Guid ToolAssetId { get; set; }
    public string? ToolInternalCode { get; set; }
    public string? ToolName { get; set; }
    public string? Severity { get; set; }
    public string? SeverityLabel { get; set; }
    public string? Description { get; set; }
    public DateTime ReportedAt { get; set; }
    public string? ReportedBy { get; set; }
}
