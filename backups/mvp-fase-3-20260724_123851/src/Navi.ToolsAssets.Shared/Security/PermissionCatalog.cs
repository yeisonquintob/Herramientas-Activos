namespace Navi.ToolsAssets.Shared.Security;

/// <summary>
/// Catálogo funcional único de permisos NAVI.
/// </summary>
public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionDefinition> All { get; } = Build();

    public static bool TryGet(string? code, out PermissionDefinition? definition)
    {
        definition = All.FirstOrDefault(x =>
            string.Equals(x.Code, code, StringComparison.OrdinalIgnoreCase));

        return definition is not null;
    }

    private static IReadOnlyList<PermissionDefinition> Build()
    {
        var branch = PermissionScope.Branch;
        var company = PermissionScope.Company;

        return
        [
            Define(PermissionCodes.DashboardView, "Ver dashboard", "Dashboard", "Consulta", "Ver dashboard ejecutivo.", company),

            Define(PermissionCodes.ToolsView, "Ver inventario", "Inventario AF", "Consulta", "Ver inventario de herramientas y activos.", branch),
            Define(PermissionCodes.ToolsCreate, "Crear activo", "Inventario AF", "Gestión", "Crear herramientas o activos.", branch),
            Define(PermissionCodes.ToolsEdit, "Editar activo", "Inventario AF", "Gestión", "Editar herramientas o activos.", branch),
            Define(PermissionCodes.ToolsDelete, "Eliminar activo", "Inventario AF", "Crítica", "Desactivar o eliminar herramientas o activos.", branch, admin: true),
            Define(PermissionCodes.ToolsStatusChange, "Cambiar estado", "Inventario AF", "Gestión", "Cambiar el estado operativo.", branch),
            Define(PermissionCodes.ToolsSpecializedMark, "Marcar especializado", "Inventario AF", "Gestión", "Marcar activos especializados.", branch),

            Define(PermissionCodes.AssetAvailabilityView, "Ver disponibilidad", "Disponible y ubicación", "Consulta", "Ver disponibilidad y ubicación.", branch),
            Define(PermissionCodes.AssetAvailabilityEdit, "Gestionar ubicación", "Disponible y ubicación", "Gestión", "Cambiar disponibilidad, sede o ubicación.", branch),

            Define(PermissionCodes.AssetAssignmentView, "Ver asignaciones", "Asignaciones", "Consulta", "Ver el módulo de asignaciones.", branch),
            Define(PermissionCodes.AssetAssignmentRequest, "Solicitar asignación", "Asignaciones", "Solicitud", "Crear solicitudes de asignación.", branch),
            Define(PermissionCodes.AssetAssignmentApprove, "Aprobar asignación", "Asignaciones", "Aprobación", "Aprobar solicitudes de asignación.", branch),
            Define(PermissionCodes.AssetAssignmentDeny, "Denegar asignación", "Asignaciones", "Aprobación", "Denegar solicitudes de asignación.", branch),
            Define(PermissionCodes.AssetAssignmentAssign, "Asignar activo", "Asignaciones", "Gestión", "Asignar activos a responsables.", branch),
            Define(PermissionCodes.AssetAssignmentReturn, "Regresar activo", "Asignaciones", "Gestión", "Regresar activos a almacén o taller.", branch),
            Define(PermissionCodes.AssetAssignmentHistory, "Ver historial", "Asignaciones", "Consulta", "Ver historial de asignaciones.", branch),

            Define(PermissionCodes.TechnicalLifeRecordView, "Ver hoja de vida", "Hoja de vida", "Consulta", "Ver hoja de vida técnica.", branch),
            Define(PermissionCodes.TechnicalLifeRecordEdit, "Editar hoja de vida", "Hoja de vida", "Gestión", "Editar datos técnicos.", branch),
            Define(PermissionCodes.TechnicalLifeRecordExport, "Exportar hoja de vida", "Hoja de vida", "Exportación", "Exportar hoja de vida.", branch),
            Define(PermissionCodes.TechnicalLifeRecordAccessories, "Gestionar accesorios", "Hoja de vida", "Gestión", "Gestionar accesorios.", branch),
            Define(PermissionCodes.TechnicalLifeRecordDocuments, "Gestionar documentos", "Hoja de vida", "Gestión", "Gestionar documentos.", branch),
            Define(PermissionCodes.TechnicalLifeRecordMaintenance, "Gestionar mantenimiento", "Hoja de vida", "Gestión", "Gestionar mantenimiento.", branch),
            Define(PermissionCodes.TechnicalLifeRecordSafePractices, "Gestionar prácticas seguras", "Hoja de vida", "Gestión", "Gestionar prácticas seguras.", branch),

            Define(PermissionCodes.DocumentsView, "Ver documentos", "Documentos", "Consulta", "Ver documentos y evidencias.", branch),
            Define(PermissionCodes.DocumentsUpload, "Cargar documentos", "Documentos", "Gestión", "Cargar documentos y evidencias.", branch),
            Define(PermissionCodes.DocumentsDownload, "Descargar documentos", "Documentos", "Exportación", "Descargar documentos.", branch),
            Define(PermissionCodes.DocumentsDelete, "Eliminar documentos", "Documentos", "Crítica", "Eliminar documentos.", branch),

            Define(PermissionCodes.MaintenanceView, "Ver mantenimiento", "Mantenimiento", "Consulta", "Ver mantenimientos.", branch),
            Define(PermissionCodes.MaintenanceRequest, "Solicitar mantenimiento", "Mantenimiento", "Solicitud", "Solicitar mantenimiento.", branch),
            Define(PermissionCodes.MaintenanceQuote, "Cotizar mantenimiento", "Mantenimiento", "Gestión", "Registrar cotizaciones.", branch),
            Define(PermissionCodes.MaintenanceGenerate, "Generar mantenimiento", "Mantenimiento", "Gestión", "Generar solicitud final.", branch),
            Define(PermissionCodes.MaintenanceExecute, "Ejecutar mantenimiento", "Mantenimiento", "Gestión", "Registrar ejecución.", branch),
            Define(PermissionCodes.MaintenanceClose, "Cerrar mantenimiento", "Mantenimiento", "Aprobación", "Cerrar mantenimiento.", branch),
            Define(PermissionCodes.MaintenanceReject, "Rechazar mantenimiento", "Mantenimiento", "Aprobación", "Rechazar mantenimiento.", branch),
            Define(PermissionCodes.MaintenancePlansView, "Ver planes", "Mantenimiento", "Consulta", "Ver planes de mantenimiento.", branch),
            Define(PermissionCodes.MaintenancePlansManage, "Gestionar planes", "Mantenimiento", "Gestión", "Gestionar planes de mantenimiento.", branch),

            Define(PermissionCodes.PurchasesView, "Ver compras", "Compras AF", "Consulta", "Ver solicitudes de compra.", branch),
            Define(PermissionCodes.PurchasesRequest, "Solicitar compra", "Compras AF", "Solicitud", "Solicitar compra.", branch),
            Define(PermissionCodes.PurchasesQuote, "Cotizar compra", "Compras AF", "Gestión", "Registrar cotizaciones.", branch),
            Define(PermissionCodes.PurchasesGenerate, "Generar compra", "Compras AF", "Gestión", "Generar solicitud final.", branch),
            Define(PermissionCodes.PurchasesApprove, "Aprobar compra", "Compras AF", "Aprobación", "Aprobar compra.", branch),
            Define(PermissionCodes.PurchasesReject, "Rechazar compra", "Compras AF", "Aprobación", "Rechazar compra.", branch),

            Define(PermissionCodes.PhysicalCountsView, "Ver tomas", "Tomas físicas", "Consulta", "Ver tomas físicas.", branch),
            Define(PermissionCodes.PhysicalCountsCreate, "Crear toma", "Tomas físicas", "Gestión", "Crear toma física.", branch),
            Define(PermissionCodes.PhysicalCountsClose, "Cerrar toma", "Tomas físicas", "Crítica", "Cerrar toma física.", branch),
            Define(PermissionCodes.PhysicalCountsReport, "Reportar toma", "Tomas físicas", "Móvil", "Reportar resultado de toma.", branch, mobile: true),
            Define(PermissionCodes.PhysicalCountsEvidenceUpload, "Cargar evidencia", "Tomas físicas", "Móvil", "Cargar evidencia de toma.", branch, mobile: true),

            Define(PermissionCodes.SafePracticesView, "Ver prácticas seguras", "Prácticas seguras", "Consulta", "Ver prácticas seguras.", branch),
            Define(PermissionCodes.SafePracticesManage, "Gestionar prácticas seguras", "Prácticas seguras", "Gestión", "Administrar prácticas seguras.", branch),

            Define(PermissionCodes.ReportsView, "Ver reportes", "Reportes", "Consulta", "Ver reportes.", company),
            Define(PermissionCodes.ReportsExport, "Exportar reportes", "Reportes", "Exportación", "Exportar reportes.", company),

            Define(PermissionCodes.ReconciliationView, "Ver conciliación", "Conciliación", "Consulta", "Ver conciliación.", branch),
            Define(PermissionCodes.ReconciliationManage, "Gestionar conciliación", "Conciliación", "Gestión", "Gestionar decisiones.", branch),
            Define(PermissionCodes.ReconciliationClarify, "Solicitar aclaración", "Conciliación", "Gestión", "Solicitar aclaración.", branch),
            Define(PermissionCodes.ReconciliationApproveCreation, "Aprobar creación", "Conciliación", "Aprobación", "Aprobar creación de activos.", branch),

            Define(PermissionCodes.SettingsView, "Ver configuración", "Configuración", "Consulta", "Ver configuración.", company, admin: true),
            Define(PermissionCodes.SettingsManage, "Administrar configuración", "Configuración", "Administración", "Administrar configuración.", company, admin: true),
            Define(PermissionCodes.SecurityUsers, "Administrar usuarios", "Seguridad", "Administración", "Administrar usuarios.", company, admin: true),
            Define(PermissionCodes.SecurityRoles, "Administrar roles", "Seguridad", "Administración", "Administrar roles y permisos.", company, admin: true),

            Define(PermissionCodes.MobileAccess, "Acceder a Mobile", "Mobile", "Acceso", "Acceder a Mobile PWA.", company, mobile: true),
            Define(PermissionCodes.MobileToolsView, "Ver activos en Mobile", "Mobile", "Consulta", "Ver activos en Mobile.", branch, mobile: true),
            Define(PermissionCodes.MobileToolsReview, "Revisar activos en Mobile", "Mobile", "Gestión", "Revisar activos en Mobile.", branch, mobile: true),
            Define(PermissionCodes.MobilePreOperationalReport, "Reportar preoperacional", "Mobile", "Gestión", "Reportar preoperacional.", branch, mobile: true),
            Define(PermissionCodes.MobileDamageReport, "Reportar daño", "Mobile", "Gestión", "Reportar daños.", branch, mobile: true),
            Define(PermissionCodes.MobileLoansRequest, "Solicitar préstamo", "Mobile", "Solicitud", "Solicitar préstamos.", branch, mobile: true)
        ];
    }

    private static PermissionDefinition Define(
        string code,
        string displayName,
        string module,
        string category,
        string description,
        PermissionScope scope,
        bool admin = false,
        bool mobile = false) =>
        new(
            code,
            displayName,
            module,
            category,
            description,
            scope,
            admin,
            mobile,
            RequiresCompany: scope is PermissionScope.Company or PermissionScope.Branch,
            RequiresBranch: scope is PermissionScope.Branch);
}
