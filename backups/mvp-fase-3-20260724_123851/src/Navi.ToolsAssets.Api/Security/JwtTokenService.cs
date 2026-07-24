using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Navi.ToolsAssets.Domain.Entities.Security;
using Navi.ToolsAssets.Shared.Security;

namespace Navi.ToolsAssets.Api.Security;

public sealed class JwtTokenService
{
    private readonly JwtOptions _options;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;

        if (Encoding.UTF8.GetByteCount(_options.SigningKey) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:SigningKey debe contener al menos 32 bytes y provenir de una fuente segura.");
        }

        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);
    }

    public IssuedAccessToken CreateAccessToken(
        AppUser user,
        UserSession session,
        IReadOnlyCollection<string> permissions)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(Math.Clamp(_options.AccessTokenMinutes, 5, 240));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sid, session.Id.ToString()),
            new(ClaimTypes.Role, user.AppRole?.Code ?? string.Empty),
            new(NaviClaimTypes.RoleCode, user.AppRole?.Code ?? string.Empty),
            new(NaviClaimTypes.RoleName, user.AppRole?.Name ?? string.Empty),
            new(NaviClaimTypes.SecurityStamp, user.SecurityStamp.ToString())
        };

        AddOptionalGuidClaim(claims, NaviClaimTypes.BranchId, user.BranchId);
        AddOptionalGuidClaim(claims, NaviClaimTypes.ResponsiblePersonId, user.ResponsiblePersonId);

        if (!string.IsNullOrWhiteSpace(user.ResponsiblePerson?.FullName))
        {
            claims.Add(new Claim(NaviClaimTypes.ResponsiblePersonName, user.ResponsiblePerson.FullName));
        }

        foreach (var permission in permissions
                     .Where(x => !string.IsNullOrWhiteSpace(x))
                     .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            claims.Add(new Claim(NaviClaimTypes.Permission, permission));
        }

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        return new IssuedAccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc,
            session.Id);
    }

    private static void AddOptionalGuidClaim(
        ICollection<Claim> claims,
        string claimType,
        Guid? value)
    {
        if (value.HasValue)
        {
            claims.Add(new Claim(claimType, value.Value.ToString()));
        }
    }
}

public sealed record IssuedAccessToken(
    string AccessToken,
    DateTime ExpiresAtUtc,
    Guid SessionId);
