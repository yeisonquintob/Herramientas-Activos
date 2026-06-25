namespace Navi.ToolsAssets.Admin.Services.Auth;

public sealed class NaviPermissionHttpMessageHandler : DelegatingHandler
{
    private readonly WebAuthSessionService _authSession;

    public NaviPermissionHttpMessageHandler(WebAuthSessionService authSession)
    {
        _authSession = authSession;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        request.Headers.Remove("X-Navi-User");
        request.Headers.Remove("X-Navi-Role");
        request.Headers.Remove("X-Navi-Role-Code");
        request.Headers.Remove("X-Navi-Permissions");
        request.Headers.Remove("X-Navi-BranchId");
        request.Headers.Remove("X-Navi-ResponsiblePersonId");
        request.Headers.Remove("X-Navi-ResponsiblePersonName");

        var isAuthenticated = _authSession.IsAuthenticated;
        var roleCode = string.IsNullOrWhiteSpace(_authSession.RoleCode)
            ? "ADMIN"
            : _authSession.RoleCode.Trim();

        request.Headers.Add("X-Navi-User", string.IsNullOrWhiteSpace(_authSession.UserName) ? "admin" : _authSession.UserName);
        request.Headers.Add("X-Navi-Role", string.IsNullOrWhiteSpace(_authSession.RoleName) ? "Administrador NAVI" : _authSession.RoleName);
        request.Headers.Add("X-Navi-Role-Code", roleCode);

        var permissions = (_authSession.Permissions ?? new List<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!permissions.Any() || roleCode.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            permissions = new List<string>
            {
                "ALL",
                "Dashboard.View",
                "Tools.View",
                "Tools.Create",
                "Tools.Edit",
                "Tools.Delete",
                "AssetAvailability.View",
                "AssetAvailability.Edit",
                "AssetAssignment.View",
                "AssetAssignment.Assign",
                "AssetAssignment.Return",
                "AssetAssignment.History",
                "TechnicalLifeRecord.View",
                "TechnicalLifeRecord.Edit",
                "TechnicalLifeRecord.Export",
                "Documents.View",
                "Documents.Upload",
                "Documents.Download",
                "Documents.Delete",
                "Maintenance.View",
                "Maintenance.Request",
                "Maintenance.Execute",
                "Maintenance.Close",
                "Purchases.View",
                "Purchases.Request",
                "Purchases.Approve",
                "Purchases.Reject",
                "PhysicalCounts.View",
                "PhysicalCounts.Create",
                "PhysicalCounts.Close",
                "Reports.View",
                "Settings.View",
                "Settings.Manage",
                "Security.Users",
                "Security.Roles",
                "Mobile.Access",
                "Mobile.Tools.View",
                "Mobile.Tools.Review",
                "Mobile.PreOperational.Report",
                "Mobile.Damage.Report",
                "Mobile.Loans.Request"
            };
        }

        request.Headers.Add("X-Navi-Permissions", string.Join(";", permissions));

        if (_authSession.BranchId.HasValue)
        {
            request.Headers.Add("X-Navi-BranchId", _authSession.BranchId.Value.ToString());
        }

        if (_authSession.ResponsiblePersonId.HasValue)
        {
            request.Headers.Add("X-Navi-ResponsiblePersonId", _authSession.ResponsiblePersonId.Value.ToString());
        }

        if (!string.IsNullOrWhiteSpace(_authSession.ResponsiblePersonName))
        {
            request.Headers.Add("X-Navi-ResponsiblePersonName", _authSession.ResponsiblePersonName);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
