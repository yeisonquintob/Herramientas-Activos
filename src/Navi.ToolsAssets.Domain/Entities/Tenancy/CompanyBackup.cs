using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Tenancy;

public sealed class CompanyBackup : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public string BackupNumber { get; set; } = string.Empty;
    public string Status { get; set; } = "Requested";
    public string StorageObjectKey { get; set; } = string.Empty;
    public string? SchemaVersion { get; set; }
    public string? Sha256Checksum { get; set; }
    public long? SizeBytes { get; set; }
    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid RequestedByUserId { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public string? ErrorCode { get; set; }
}
