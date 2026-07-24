namespace Navi.ToolsAssets.Shared.Security;

public enum PermissionScope
{
    Global = 0,
    Company = 1,
    Branch = 2
}

/// <summary>
/// Metadatos funcionales de un permiso NAVI.
/// </summary>
public sealed record PermissionDefinition(
    string Code,
    string DisplayName,
    string Module,
    string Category,
    string Description,
    PermissionScope Scope,
    bool IsAdministrative = false,
    bool IsMobile = false,
    bool RequiresCompany = false,
    bool RequiresBranch = false)
{
    // Alias de compatibilidad para consumidores que todavía esperan
    // el campo histórico "Action" en api/auth/permissions.
    public string Action => Category;
}
