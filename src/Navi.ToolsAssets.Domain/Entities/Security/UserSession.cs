using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Security;

public sealed class UserSession : BaseEntity
{
    public Guid AppUserId { get; set; }

    public AppUser? AppUser { get; set; }

    public Guid SecurityStamp { get; set; }

    public Guid? CompanyId { get; set; }

    public DateTime LastActivityAtUtc { get; set; }

    public DateTime AbsoluteExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? RevokedReason { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public bool IsRevoked => RevokedAtUtc.HasValue;
}
