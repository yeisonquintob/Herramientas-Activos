using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Tenancy;

public sealed class CompanyDatabase : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionKey { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string? SchemaVersion { get; set; }
    public DateTime? LastConnectionTestAtUtc { get; set; }
    public bool? LastConnectionTestSucceeded { get; set; }
}
