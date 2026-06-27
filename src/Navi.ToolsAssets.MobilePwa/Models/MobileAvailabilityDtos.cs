namespace Navi.ToolsAssets.MobilePwa.Models;

public sealed class MobileAvailabilityToolDto
{
    public Guid Id { get; set; }
    public string InternalCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public string? FixedAssetCode { get; set; }
    public string? FenixCode { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }

    public Guid? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }

    public Guid? LocationId { get; set; }
    public string? LocationCode { get; set; }
    public string? LocationName { get; set; }

    public Guid? ResponsiblePersonId { get; set; }
    public string? ResponsiblePersonName { get; set; }

    public string? ToolTypeCode { get; set; }
    public string? ToolTypeName { get; set; }
    public string? ToolCategoryCode { get; set; }
    public string? ToolCategoryName { get; set; }

    public string OperationalStatus { get; set; } = string.Empty;
    public string PhysicalStatus { get; set; } = string.Empty;
}

public sealed class MobileBranchDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class MobileLocationDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class MobileAvailabilityLocationRequest
{
    public Guid ToolId { get; set; }
    public Guid BranchId { get; set; }
    public Guid? LocationId { get; set; }
    public string OperationalStatus { get; set; } = "Available";
    public string? Observation { get; set; }
    public string? ChangedBy { get; set; }
}
