using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;

namespace Navi.ToolsAssets.Domain.Entities.Loans;

public class ToolLoanItem : BaseEntity
{
    public Guid ToolLoanId { get; set; }

    public ToolLoan? ToolLoan { get; set; }

    public Guid ToolAssetId { get; set; }

    public ToolAsset? ToolAsset { get; set; }

    public decimal Quantity { get; set; } = 1;

    public string? DeliveryCondition { get; set; }

    public string? ReturnCondition { get; set; }

    public bool Returned { get; set; }
}
