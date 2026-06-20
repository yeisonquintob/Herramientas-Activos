using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Organization;
using Navi.ToolsAssets.Domain.Enums;

namespace Navi.ToolsAssets.Domain.Entities.Loans;

public class ToolLoan : BaseEntity
{
    public string LoanNumber { get; set; } = string.Empty;

    public Guid BranchId { get; set; }

    public Branch? Branch { get; set; }

    public Guid? RequestedByPersonId { get; set; }

    public ResponsiblePerson? RequestedByPerson { get; set; }

    public ToolLoanStatus Status { get; set; } = ToolLoanStatus.Draft;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ApprovedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ExpectedReturnAt { get; set; }

    public DateTime? ReturnedAt { get; set; }

    public string? Notes { get; set; }

    public ICollection<ToolLoanItem> Items { get; set; } = new List<ToolLoanItem>();
}
