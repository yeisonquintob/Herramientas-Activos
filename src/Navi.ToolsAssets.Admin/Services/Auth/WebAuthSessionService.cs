namespace Navi.ToolsAssets.Admin.Services.Auth;

public sealed class WebAuthSessionService
{
    public bool IsAuthenticated { get; private set; }

    public string UserName { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string Role { get; private set; } = string.Empty;

    public string BranchCode { get; private set; } = "TODAS";

    public event Action? OnChange;

    public void Login(string userName, string displayName, string role, string branchCode)
    {
        IsAuthenticated = true;
        UserName = userName;
        DisplayName = displayName;
        Role = role;
        BranchCode = branchCode;

        NotifyStateChanged();
    }

    public void Logout()
    {
        IsAuthenticated = false;
        UserName = string.Empty;
        DisplayName = string.Empty;
        Role = string.Empty;
        BranchCode = "TODAS";

        NotifyStateChanged();
    }

    public bool HasRole(params string[] roles)
    {
        if (!IsAuthenticated)
        {
            return false;
        }

        return roles.Any(x => string.Equals(x, Role, StringComparison.OrdinalIgnoreCase));
    }

    private void NotifyStateChanged()
    {
        OnChange?.Invoke();
    }
}
