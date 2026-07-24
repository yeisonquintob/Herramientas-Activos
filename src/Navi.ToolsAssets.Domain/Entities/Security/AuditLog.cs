using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Security;

public sealed class AuditLog : BaseEntity
{
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public Guid? UserId { get; set; }

    public string? UserName { get; set; }

    public Guid? CompanyId { get; set; }

    public Guid? BranchId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Module { get; set; } = string.Empty;

    public string? EntityType { get; set; }

    public string? EntityId { get; set; }

    public string Result { get; set; } = string.Empty;

    public string CorrelationId { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public string? BeforeJson { get; set; }

    public string? AfterJson { get; set; }

    public string? MetadataJson { get; set; }

    public string? ErrorCode { get; set; }

    public string? Reason { get; set; }
}
