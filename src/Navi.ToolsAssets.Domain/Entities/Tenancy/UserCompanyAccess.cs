using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Tenancy;

public sealed class UserCompanyAccess : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid CompanyId { get; set; }
    public Company? Company { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public string? RoleCode { get; set; }
}
