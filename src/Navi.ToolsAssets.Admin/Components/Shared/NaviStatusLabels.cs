namespace Navi.ToolsAssets.Admin.Components.Shared;

public static class NaviStatusLabels
{
    public static string ToSpanish(string? status)
    {
        var normalized = Normalize(status);

        if (normalized.Length == 0)
        {
            return "Sin estado";
        }

        return normalized switch
        {
            "available" or
            "disponible"
                => "Disponible",

            "assigned" or
            "assignedtoresponsible" or
            "asignado" or
            "asignada"
                => "Asignada",

            "loaned" or
            "prestado" or
            "prestada"
                => "Prestada",

            "inuse" or
            "en uso"
                => "En uso",

            "inmaintenance" or
            "maintenance" or
            "mantenimiento" or
            "en mantenimiento"
                => "Mantenimiento",

            "damaged" or
            "dañado" or
            "dañada"
                => "Dañada",

            "notsuitable" or
            "no apto" or
            "no apta"
                => "No apta",

            "pendingvalidation" or
            "pending" or
            "pendiente" or
            "pendiente validación" or
            "pendiente de validación"
                => "Pendiente de validación",

            "notreconciled" or
            "noconciliado" or
            "pendingreconciliation" or
            "inconsistent" or
            "no conciliado"
                => "No conciliado",

            "notlocated" or
            "lost" or
            "no localizado" or
            "no localizada"
                => "No localizada",

            "disposed" or
            "dada de baja" or
            "baja"
                => "Baja",

            "pendingdisposal" or
            "pendiente baja" or
            "pendiente de baja"
                => "Pendiente de baja",

            "validated" or
            "validado" or
            "validada"
                => "Validado",

            "synced" or
            "synchronized" or
            "sincronizado" or
            "sincronizada"
                => "Sincronizado",

            "notsynced" or
            "not synchronized" or
            "no sincronizado" or
            "no sincronizada"
                => "No sincronizado",

            "draft" or
            "borrador"
                => "Borrador",

            "inreview" or
            "en revisión"
                => "En revisión",

            "pendingapproval" or
            "pendiente aprobación" or
            "pendiente de aprobación"
                => "Pendiente de aprobación",

            "approved" or
            "aprobado" or
            "aprobada"
                => "Aprobado",

            "accepted" or
            "aceptado" or
            "aceptada"
                => "Aceptado",

            "confirmed" or
            "confirmado" or
            "confirmada"
                => "Confirmado",

            "rejected" or
            "rechazado" or
            "rechazada"
                => "Rechazado",

            "denied" or
            "denegado" or
            "denegada"
                => "Denegado",

            "canceled" or
            "cancelled" or
            "cancelado" or
            "cancelada"
                => "Cancelado",

            "active" or
            "activo" or
            "activa"
                => "Activo",

            "inactive" or
            "inactivo" or
            "inactiva"
                => "Inactivo",

            "completed" or
            "completado" or
            "completada"
                => "Completado",

            "open" or
            "abierto" or
            "abierta"
                => "Abierto",

            "closed" or
            "cerrado" or
            "cerrada"
                => "Cerrado",

            "requested" or
            "solicitado" or
            "solicitada"
                => "Solicitado",

            "returned" or
            "devuelto" or
            "devuelta"
                => "Devuelto",

            "partial" or
            "parcial"
                => "Parcial",

            "overdue" or
            "vencido" or
            "vencida"
                => "Vencido",

            "failed" or
            "error" or
            "fallido" or
            "fallida"
                => "Fallido",

            _ => status!.Trim()
        };
    }

    public static string BadgeClass(string? status)
    {
        return Normalize(status) switch
        {
            "available" or
            "disponible"
                => "navi-status-badge navi-status-available",

            "assigned" or
            "assignedtoresponsible" or
            "asignado" or
            "asignada"
                => "navi-status-badge navi-status-assigned",

            "loaned" or
            "prestado" or
            "prestada" or
            "inuse" or
            "en uso"
                => "navi-status-badge navi-status-loaned",

            "inmaintenance" or
            "maintenance" or
            "mantenimiento" or
            "en mantenimiento"
                => "navi-status-badge navi-status-maintenance",

            "damaged" or
            "dañado" or
            "dañada"
                => "navi-status-badge navi-status-damaged",

            "notsuitable" or
            "no apto" or
            "no apta"
                => "navi-status-badge navi-status-not-suitable",

            "pendingvalidation" or
            "pending" or
            "pendiente" or
            "pendiente validación" or
            "pendiente de validación" or
            "pendingdisposal" or
            "pendiente baja" or
            "pendiente de baja"
                => "navi-status-badge navi-status-pending-validation",

            "notreconciled" or
            "noconciliado" or
            "pendingreconciliation" or
            "inconsistent" or
            "no conciliado"
                => "navi-status-badge navi-status-not-reconciled",

            "notlocated" or
            "lost" or
            "no localizado" or
            "no localizada"
                => "navi-status-badge navi-status-not-located",

            "disposed" or
            "dada de baja" or
            "baja"
                => "navi-status-badge navi-status-disposed",

            "validated" or
            "validado" or
            "validada" or
            "synced" or
            "synchronized" or
            "sincronizado" or
            "sincronizada" or
            "approved" or
            "aprobado" or
            "aprobada" or
            "accepted" or
            "aceptado" or
            "aceptada" or
            "confirmed" or
            "confirmado" or
            "confirmada" or
            "active" or
            "activo" or
            "activa" or
            "completed" or
            "completado" or
            "completada" or
            "returned" or
            "devuelto" or
            "devuelta"
                => "navi-status-badge navi-status-success",

            "draft" or
            "borrador" or
            "open" or
            "abierto" or
            "abierta" or
            "requested" or
            "solicitado" or
            "solicitada"
                => "navi-status-badge navi-status-process",

            "inreview" or
            "en revisión" or
            "pendingapproval" or
            "pendiente aprobación" or
            "pendiente de aprobación" or
            "notsynced" or
            "not synchronized" or
            "no sincronizado" or
            "no sincronizada" or
            "partial" or
            "parcial"
                => "navi-status-badge navi-status-warning",

            "rejected" or
            "rechazado" or
            "rechazada" or
            "denied" or
            "denegado" or
            "denegada" or
            "canceled" or
            "cancelled" or
            "cancelado" or
            "cancelada" or
            "overdue" or
            "vencido" or
            "vencida" or
            "failed" or
            "error" or
            "fallido" or
            "fallida"
                => "navi-status-badge navi-status-danger",

            "inactive" or
            "inactivo" or
            "inactiva" or
            "closed" or
            "cerrado" or
            "cerrada"
                => "navi-status-badge navi-status-inactive",

            _ => "navi-status-badge navi-status-muted"
        };
    }

    private static string Normalize(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? string.Empty
            : status.Trim().ToLowerInvariant();
    }
}
