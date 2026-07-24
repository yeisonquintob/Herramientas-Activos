using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Tenancy;

public sealed class CompanySchemaVersion : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public DateTime? AppliedAtUtc { get; set; }
    public string? ErrorCode { get; set; }
}
