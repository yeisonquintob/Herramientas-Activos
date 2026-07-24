using Hangfire.Dashboard;

namespace Navi.ToolsAssets.Api.Security;

public sealed class NaviHangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context.GetHttpContext().User;

        return user.Identity?.IsAuthenticated == true &&
               (string.Equals(user.GetRoleCode(), "ADMIN", StringComparison.OrdinalIgnoreCase) ||
                user.HasNaviPermission("Settings.Manage"));
    }
}
