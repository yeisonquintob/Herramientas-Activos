namespace Navi.ToolsAssets.MobilePwa.Services;

public sealed class NaviMobileNotificationState
{
    public event Action? OnRefreshRequested;

    public void RequestRefresh()
    {
        OnRefreshRequested?.Invoke();
    }
}
