using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Imports;

public class ImportBatch : BaseEntity
{
    public string ImportNumber { get; set; } = string.Empty;

    public string SourceType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string? ObjectKey { get; set; }

    public string Status { get; set; } = "Pending";

    public int TotalRows { get; set; }

    public int ValidRows { get; set; }

    public int ErrorRows { get; set; }

    public int CreatedTools { get; set; }

    public int UpdatedTools { get; set; }

    public int DuplicateRows { get; set; }

    public string? Summary { get; set; }

    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    public string? ProcessedBy { get; set; }

    public ICollection<ImportRow> Rows { get; set; } = new List<ImportRow>();
}
