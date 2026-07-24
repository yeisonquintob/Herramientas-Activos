using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Tenancy;

public sealed class CompanyImportJob : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid ImportJobId { get; set; }
    public string Status { get; set; } = "Uploaded";
    public string FileName { get; set; } = string.Empty;
    public string? Sha256Checksum { get; set; }
    public Guid RequestedByUserId { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}
