using System.ComponentModel.DataAnnotations.Schema;
using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.PhysicalCounts;

/// <summary>
/// Evidencia física asociada a un registro reportado durante
/// una toma física.
/// </summary>
[Table(
    "PhysicalCountReportedItemEvidences",
    Schema = "PhysicalCounts")]
public class PhysicalCountReportedItemEvidence : BaseEntity
{
    public Guid PhysicalCountReportedItemId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string ObjectKey { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long SizeBytes { get; set; }

    public string? Description { get; set; }

    public string? UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
