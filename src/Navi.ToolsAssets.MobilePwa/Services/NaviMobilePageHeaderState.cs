namespace Navi.ToolsAssets.MobilePwa.Services;

public sealed class NaviMobilePageHeaderState
{
    public event Action? OnChange;

    public string Title { get; private set; } =
        "NAVI Herramientas y Activos";

    public string Description { get; private set; } =
        "Gestión móvil de herramientas y activos.";

    public bool ShowBackButton { get; private set; }

    public string? BackUrl { get; private set; }

    public string BackFallbackUrl { get; private set; } =
        "/";

    public bool UseBrowserHistoryWhenNoBackUrl
    {
        get;
        private set;
    } = true;

    public void Set(
        string? title,
        string? description,
        bool showBackButton,
        string? backUrl,
        string? backFallbackUrl,
        bool useBrowserHistoryWhenNoBackUrl)
    {
        var normalizedTitle =
            string.IsNullOrWhiteSpace(title)
                ? "NAVI Herramientas y Activos"
                : title.Trim();

        var normalizedDescription =
            string.IsNullOrWhiteSpace(description)
                ? "Gestión móvil de herramientas y activos."
                : description.Trim();

        var normalizedBackUrl =
            string.IsNullOrWhiteSpace(backUrl)
                ? null
                : backUrl.Trim();

        var normalizedFallback =
            string.IsNullOrWhiteSpace(backFallbackUrl)
                ? "/"
                : backFallbackUrl.Trim();

        var changed =
            !string.Equals(
                Title,
                normalizedTitle,
                StringComparison.Ordinal) ||
            !string.Equals(
                Description,
                normalizedDescription,
                StringComparison.Ordinal) ||
            ShowBackButton != showBackButton ||
            !string.Equals(
                BackUrl,
                normalizedBackUrl,
                StringComparison.Ordinal) ||
            !string.Equals(
                BackFallbackUrl,
                normalizedFallback,
                StringComparison.Ordinal) ||
            UseBrowserHistoryWhenNoBackUrl !=
                useBrowserHistoryWhenNoBackUrl;

        if (!changed)
        {
            return;
        }

        Title = normalizedTitle;
        Description = normalizedDescription;
        ShowBackButton = showBackButton;
        BackUrl = normalizedBackUrl;
        BackFallbackUrl = normalizedFallback;
        UseBrowserHistoryWhenNoBackUrl =
            useBrowserHistoryWhenNoBackUrl;

        OnChange?.Invoke();
    }

    public void Reset(
        string title,
        string description)
    {
        Set(
            title,
            description,
            false,
            null,
            "/",
            true);
    }
}
