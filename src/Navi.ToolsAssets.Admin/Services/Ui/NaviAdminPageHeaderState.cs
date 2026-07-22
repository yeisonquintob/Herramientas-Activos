namespace Navi.ToolsAssets.Admin.Services.Ui;

public sealed class NaviAdminPageHeaderState
{
    private static readonly string DefaultTitle =
        "NAVI Herramientas y Activos";

    private static readonly string DefaultDescription =
        "Consola de administración";

    private Guid? currentOwnerId;

    public event Action? OnChange;

    public string Title { get; private set; } =
        DefaultTitle;

    public string Description { get; private set; } =
        DefaultDescription;

    public void Set(
        Guid ownerId,
        string? title,
        string? description)
    {
        var normalizedTitle =
            string.IsNullOrWhiteSpace(title)
                ? DefaultTitle
                : title.Trim();

        var normalizedDescription =
            string.IsNullOrWhiteSpace(description)
                ? DefaultDescription
                : description.Trim();

        var changed =
            currentOwnerId != ownerId ||
            !string.Equals(
                Title,
                normalizedTitle,
                StringComparison.Ordinal) ||
            !string.Equals(
                Description,
                normalizedDescription,
                StringComparison.Ordinal);

        currentOwnerId = ownerId;
        Title = normalizedTitle;
        Description = normalizedDescription;

        if (changed)
        {
            OnChange?.Invoke();
        }
    }

    public void Clear(Guid ownerId)
    {
        if (currentOwnerId != ownerId)
        {
            return;
        }

        currentOwnerId = null;
        Title = DefaultTitle;
        Description = DefaultDescription;

        OnChange?.Invoke();
    }
}
