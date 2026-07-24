using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Navi.ToolsAssets.Shared.Security;

namespace Navi.ToolsAssets.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user) =>
        user.GetGuidClaim(ClaimTypes.NameIdentifier) ??
        user.GetGuidClaim(JwtRegisteredClaimNames.Sub);

    public static Guid? GetSessionId(this ClaimsPrincipal user) =>
        user.GetGuidClaim(JwtRegisteredClaimNames.Sid);

    public static string? GetUserName(this ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.Name) ??
        user.FindFirstValue(JwtRegisteredClaimNames.UniqueName);

    public static string? GetRoleCode(this ClaimsPrincipal user) =>
        user.FindFirstValue(NaviClaimTypes.RoleCode) ??
        user.FindFirstValue(ClaimTypes.Role);

    public static Guid? GetCompanyId(this ClaimsPrincipal user) =>
        user.GetGuidClaim(NaviClaimTypes.CompanyId);

    public static Guid? GetBranchId(this ClaimsPrincipal user) =>
        user.GetGuidClaim(NaviClaimTypes.BranchId);

    public static Guid? GetResponsiblePersonId(this ClaimsPrincipal user) =>
        user.GetGuidClaim(NaviClaimTypes.ResponsiblePersonId);

    public static string? GetResponsiblePersonName(this ClaimsPrincipal user) =>
        user.FindFirstValue(NaviClaimTypes.ResponsiblePersonName);

    public static bool HasNaviPermission(this ClaimsPrincipal user, string permission) =>
        user.Claims
            .Where(x => x.Type == NaviClaimTypes.Permission)
            .Any(x => string.Equals(x.Value, permission, StringComparison.OrdinalIgnoreCase));

    private static Guid? GetGuidClaim(this ClaimsPrincipal user, string claimType) =>
        Guid.TryParse(user.FindFirstValue(claimType), out var value)
            ? value
            : null;
}
