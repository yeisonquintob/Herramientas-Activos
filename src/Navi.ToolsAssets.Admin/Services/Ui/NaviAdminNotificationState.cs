namespace Navi.ToolsAssets.Admin.Services.Ui;

public sealed class NaviAdminNotificationState
{
    public event Action? OnRefreshRequested;

    public void RequestRefresh()
    {
        OnRefreshRequested?.Invoke();
    }
}
