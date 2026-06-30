namespace Navi.ToolsAssets.Admin.Services.Auth;

public static class NaviPermissionRouteService
{
    private static readonly (string Permission, string Route, string Label)[] OrderedRoutes =
    {
        ("Dashboard.View", "/dashboard", "Volver al dashboard"),
        ("Tools.View", "/tools", "Ir a inventario"),
        ("AssetAvailability.View", "/asset-availability", "Ir a disponibilidad"),
        ("AssetAssignment.View", "/asset-assignment", "Ir a asignaciones"),
        ("AssetAssignment.History", "/asset-assignment-history", "Ir al historial"),
        ("TechnicalLifeRecord.View", "/technical-life-records", "Ir a hoja de vida"),
        ("Maintenance.Request", "/maintenance-request", "Ir a solicitud de mantenimiento"),
        ("Maintenance.View", "/maintenance", "Ir a consulta de mantenimiento"),
        ("Purchases.Request", "/mro-purchases", "Ir a solicitud de compra"),
        ("Purchases.View", "/mro-purchases", "Ir a compras"),
        ("MaintenancePlans.View", "/maintenance-plans", "Ir a planes"),
        ("PhysicalCounts.View", "/physical-counts", "Ir a tomas físicas"),
        ("Reconciliation.View", "/reconciliation", "Ir a conciliación"),
        ("Documents.View", "/documents", "Ir a documentos"),
        ("Reports.View", "/reports", "Ir a reportes"),
        ("Settings.View", "/settings", "Ir a configuración"),
        ("Security.Users", "/settings/users", "Ir a usuarios"),
        ("Security.Roles", "/settings/roles", "Ir a roles")
    };

    public static string GetStartRoute(WebAuthSessionService auth)
    {
        if (!auth.IsAuthenticated)
        {
            return "/login";
        }

        foreach (var item in OrderedRoutes)
        {
            if (auth.HasPermission(item.Permission))
            {
                return item.Route;
            }
        }

        return "/access-denied";
    }

    public static string GetStartLabel(WebAuthSessionService auth)
    {
        var route = GetStartRoute(auth);

        foreach (var item in OrderedRoutes)
        {
            if (string.Equals(item.Route, route, StringComparison.OrdinalIgnoreCase))
            {
                return item.Label;
            }
        }

        return "Ir al módulo permitido";
    }
}
