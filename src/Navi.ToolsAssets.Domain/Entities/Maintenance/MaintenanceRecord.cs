using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Enums;

namespace Navi.ToolsAssets.Domain.Entities.Maintenance;

public class MaintenanceRecord : BaseEntity
{
    public string MaintenanceNumber { get; set; } = string.Empty;

    public Guid ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public ToolMaintenanceType Type { get; set; } = ToolMaintenanceType.Preventive;

    public ToolMaintenanceStatus Status { get; set; } = ToolMaintenanceStatus.Scheduled;

    public DateTime ScheduledAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? Provider { get; set; }

    public string? Technician { get; set; }

    public string? Description { get; set; }

    public string? MaintenanceActivities { get; set; }

    public string? ExecutionNotes { get; set; }

    public string? InvoiceNumber { get; set; }

    public string? ResponsibleName { get; set; }

    public string? ResponsiblePosition { get; set; }

    public bool? IsToolOperational { get; set; }

    public Guid? EvidenceDocumentId { get; set; }

    public decimal? Cost { get; set; }

    public string? Result { get; set; }
}
