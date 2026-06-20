using Navi.ToolsAssets.Domain.Common.Base;

namespace Navi.ToolsAssets.Domain.Entities.Imports;

public class ImportRow : BaseEntity
{
    public Guid ImportBatchId { get; set; }

    public ImportBatch? ImportBatch { get; set; }

    public int RowNumber { get; set; }

    public string? InternalCode { get; set; }

    public string? FenixCode { get; set; }

    public string? FixedAssetCode { get; set; }

    public string? SerialNumber { get; set; }

    public string? ToolName { get; set; }

    public string? BranchCode { get; set; }

    public string? ResponsibleName { get; set; }

    public string? OperationalStatus { get; set; }

    public string ResultStatus { get; set; } = "Pending";

    public string? Message { get; set; }

    public string? RawDataJson { get; set; }
}
