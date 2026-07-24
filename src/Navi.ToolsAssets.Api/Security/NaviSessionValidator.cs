using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Security;

public sealed class NaviSessionValidator
{
    private readonly NaviToolsAssetsDbContext _context;
    private readonly JwtOptions _options;

    public NaviSessionValidator(
        NaviToolsAssetsDbContext context,
        IOptions<JwtOptions> options)
    {
        _context = context;
        _options = options.Value;
    }

    public async Task<bool> ValidateAsync(
        System.Security.Claims.ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var sessionId = principal.GetSessionId();
        var userId = principal.GetUserId();

        if (!sessionId.HasValue || !userId.HasValue)
        {
            return false;
        }

        var session = await _context.UserSessions
            .Include(x => x.AppUser)
            .FirstOrDefaultAsync(
                x => x.Id == sessionId.Value && x.AppUserId == userId.Value,
                cancellationToken);

        var now = DateTime.UtcNow;
        var inactivityLimit = now.AddMinutes(-Math.Clamp(_options.InactivityMinutes, 5, 240));

        if (session is null ||
            session.IsRevoked ||
            session.AbsoluteExpiresAtUtc <= now ||
            session.LastActivityAtUtc < inactivityLimit ||
            session.AppUser is null ||
            !session.AppUser.IsActive ||
            session.AppUser.IsDeleted ||
            session.SecurityStamp != session.AppUser.SecurityStamp)
        {
            return false;
        }

        if (session.LastActivityAtUtc < now.AddMinutes(-2))
        {
            session.LastActivityAtUtc = now;
            session.UpdatedAt = now;
            session.UpdatedBy = "session-validation";
            await _context.SaveChangesAsync(cancellationToken);
        }

        return true;
    }
}
