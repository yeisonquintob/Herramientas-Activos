using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Tenancy;

public sealed class Company : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = CompanyStatuses.Provisioning;
    public string? TaxIdentifier { get; set; }
    public string? PlanCode { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ActivatedAtUtc { get; set; }
    public ICollection<CompanyDatabase> Databases { get; set; } = new List<CompanyDatabase>();
    public ICollection<UserCompanyAccess> UserAccesses { get; set; } = new List<UserCompanyAccess>();
    public ICollection<CompanySchemaVersion> SchemaVersions { get; set; } = new List<CompanySchemaVersion>();
    public ICollection<CompanyBackup> Backups { get; set; } = new List<CompanyBackup>();
    public ICollection<CompanyImportJob> ImportJobs { get; set; } = new List<CompanyImportJob>();
}

public static class CompanyStatuses
{
    public const string Provisioning = "Provisioning";
    public const string Active = "Active";
    public const string Suspended = "Suspended";
    public const string Failed = "Failed";
}
