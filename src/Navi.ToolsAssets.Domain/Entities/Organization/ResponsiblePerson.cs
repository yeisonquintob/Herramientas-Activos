using Navi.ToolsAssets.Domain.Common.Base;
using Navi.ToolsAssets.Domain.Entities.Inventory;

namespace Navi.ToolsAssets.Domain.Entities.Organization;

public class ResponsiblePerson : BaseEntity
{
    public string? EmployeeCode { get; set; }

    public string? DocumentNumber { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? Position { get; set; }

    public string? Area { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<ToolAsset> ToolsAsResponsible { get; set; } = new List<ToolAsset>();
}
