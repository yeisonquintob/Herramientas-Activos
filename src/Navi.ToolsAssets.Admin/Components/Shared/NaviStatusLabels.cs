namespace Navi.ToolsAssets.Admin.Components.Shared;

public static class NaviStatusLabels
{
    public static string ToSpanish(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "Sin estado";
        }

        return status.Trim() switch
        {
            "Available" => "Disponible",
            "Disponible" => "Disponible",

            "Assigned" => "Asignada",
            "AssignedToResponsible" => "Asignada",
            "Asignado" => "Asignada",
            "Asignada" => "Asignada",

            "Loaned" => "Prestada",
            "Prestado" => "Prestada",
            "Prestada" => "Prestada",

            "InUse" => "En uso",
            "En uso" => "En uso",

            "InMaintenance" => "Mantenimiento",
            "Maintenance" => "Mantenimiento",
            "Mantenimiento" => "Mantenimiento",
            "En mantenimiento" => "Mantenimiento",

            "Damaged" => "Dañada",
            "Dañado" => "Dañada",
            "Dañada" => "Dañada",

            "NotSuitable" => "No apta",
            "No apto" => "No apta",
            "No apta" => "No apta",

            "PendingValidation" => "Pendiente validación",
            "Pending" => "Pendiente validación",
            "Pendiente" => "Pendiente validación",
            "Pendiente validación" => "Pendiente validación",

            "NotReconciled" => "No conciliado",
            "NoConciliado" => "No conciliado",
            "PendingReconciliation" => "No conciliado",
            "Inconsistent" => "No conciliado",
            "No conciliado" => "No conciliado",

            "NotLocated" => "No localizada",
            "Lost" => "No localizada",
            "No localizado" => "No localizada",
            "No localizada" => "No localizada",

            "Disposed" => "Baja",
            "Dada de baja" => "Baja",
            "Baja" => "Baja",

            "PendingDisposal" => "Pendiente baja",
            "Pendiente baja" => "Pendiente baja",

            _ => status.Trim()
        };
    }

    public static string BadgeClass(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "navi-status-badge navi-status-muted";
        }

        return status.Trim() switch
        {
            "Available" or "Disponible"
                => "navi-status-badge navi-status-available",

            "Assigned" or "AssignedToResponsible" or "Asignado" or "Asignada"
                => "navi-status-badge navi-status-assigned",

            "Loaned" or "Prestado" or "Prestada" or "InUse" or "En uso"
                => "navi-status-badge navi-status-loaned",

            "InMaintenance" or "Maintenance" or "Mantenimiento" or "En mantenimiento"
                => "navi-status-badge navi-status-maintenance",

            "Damaged" or "Dañado" or "Dañada"
                => "navi-status-badge navi-status-damaged",

            "NotSuitable" or "No apto" or "No apta"
                => "navi-status-badge navi-status-not-suitable",

            "PendingValidation" or "Pending" or "Pendiente" or "Pendiente validación"
                => "navi-status-badge navi-status-pending-validation",

            "NotReconciled" or "NoConciliado" or "PendingReconciliation" or "Inconsistent" or "No conciliado"
                => "navi-status-badge navi-status-not-reconciled",

            "NotLocated" or "Lost" or "No localizado" or "No localizada"
                => "navi-status-badge navi-status-not-located",

            "Disposed" or "Dada de baja" or "Baja"
                => "navi-status-badge navi-status-disposed",

            "PendingDisposal" or "Pendiente baja"
                => "navi-status-badge navi-status-pending-validation",

            _ => "navi-status-badge navi-status-muted"
        };
    }
}
