namespace Navi.ToolsAssets.MobilePwa.Components.Shared;

public static class NaviMobileStatusLabels
{
    public static string ToSpanish(string? status)
    {
        var key = NormalizeKey(status);

        if (key.Length == 0)
        {
            return "Sin estado";
        }

        return key switch
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
            "enuso"
                => "En uso",

            "inmaintenance" or
            "maintenance" or
            "mantenimiento" or
            "enmantenimiento"
                => "Mantenimiento",

            "damaged" or
            "dañado" or
            "dañada"
                => "Dañada",

            "notsuitable" or
            "noapto" or
            "noapta"
                => "No apta",

            "pendingvalidation" or
            "pending" or
            "pendiente" or
            "pendientevalidación" or
            "pendientedevalidación"
                => "Pendiente de validación",

            "notreconciled" or
            "noconciliado" or
            "pendingreconciliation" or
            "inconsistent"
                => "No conciliado",

            "notlocated" or
            "lost" or
            "nolocalizado" or
            "nolocalizada"
                => "No localizada",

            "disposed" or
            "dadadebaja" or
            "baja"
                => "Baja",

            "pendingdisposal" or
            "pendientebaja" or
            "pendientedebaja"
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
            "notsynchronized" or
            "nosincronizado" or
            "nosincronizada"
                => "No sincronizado",

            "draft" or
            "borrador"
                => "Borrador",

            "inreview" or
            "enrevisión"
                => "En revisión",

            "pendingapproval" or
            "pendienteaprobación" or
            "pendientedeaprobación"
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

    public static string Tone(string? status)
    {
        return NormalizeKey(status) switch
        {
            "available" or
            "disponible"
                => "available",

            "assigned" or
            "assignedtoresponsible" or
            "asignado" or
            "asignada"
                => "assigned",

            "loaned" or
            "prestado" or
            "prestada" or
            "inuse" or
            "enuso"
                => "loaned",

            "inmaintenance" or
            "maintenance" or
            "mantenimiento" or
            "enmantenimiento"
                => "maintenance",

            "damaged" or
            "dañado" or
            "dañada"
                => "damaged",

            "notsuitable" or
            "noapto" or
            "noapta"
                => "not-suitable",

            "pendingvalidation" or
            "pending" or
            "pendiente" or
            "pendientevalidación" or
            "pendientedevalidación" or
            "pendingdisposal" or
            "pendientebaja" or
            "pendientedebaja"
                => "pending",

            "notreconciled" or
            "noconciliado" or
            "pendingreconciliation" or
            "inconsistent"
                => "not-reconciled",

            "notlocated" or
            "lost" or
            "nolocalizado" or
            "nolocalizada"
                => "not-located",

            "disposed" or
            "dadadebaja" or
            "baja"
                => "disposed",

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
                => "success",

            "draft" or
            "borrador" or
            "open" or
            "abierto" or
            "abierta" or
            "requested" or
            "solicitado" or
            "solicitada"
                => "process",

            "inreview" or
            "enrevisión" or
            "pendingapproval" or
            "pendienteaprobación" or
            "pendientedeaprobación" or
            "notsynced" or
            "notsynchronized" or
            "nosincronizado" or
            "nosincronizada" or
            "partial" or
            "parcial"
                => "warning",

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
                => "danger",

            "inactive" or
            "inactivo" or
            "inactiva" or
            "closed" or
            "cerrado" or
            "cerrada"
                => "inactive",

            _ => "muted"
        };
    }

    public static string BadgeClass(string? status)
    {
        return
            $"navi-mobile-status-badge " +
            $"navi-mobile-status-{Tone(status)}";
    }

    private static string NormalizeKey(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return string.Empty;
        }

        return status
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(".", string.Empty)
            .Replace("/", string.Empty);
    }
}
