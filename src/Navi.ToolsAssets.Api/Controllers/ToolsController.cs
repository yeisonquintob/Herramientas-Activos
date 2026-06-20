using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public ToolsController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetTools()
    {
        var tools = await BuildToolQuery()
            .OrderBy(x => x.InternalCode)
            .ToListAsync();

        return Ok(tools);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetToolById(Guid id)
    {
        var tool = await BuildToolQuery()
            .FirstOrDefaultAsync(x => x.Id == id);

        if (tool is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con Id {id}."
            });
        }

        return Ok(tool);
    }

    [HttpGet("by-code/{internalCode}")]
    public async Task<IActionResult> GetToolByInternalCode(string internalCode)
    {
        var tool = await BuildToolQuery()
            .FirstOrDefaultAsync(x => x.InternalCode == internalCode);

        if (tool is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la herramienta con código {internalCode}."
            });
        }

        return Ok(tool);
    }

    [HttpGet("by-branch/{branchCode}")]
    public async Task<IActionResult> GetToolsByBranch(string branchCode)
    {
        var tools = await BuildToolQuery()
            .Where(x => x.BranchCode == branchCode)
            .OrderBy(x => x.InternalCode)
            .ToListAsync();

        return Ok(tools);
    }

    [HttpGet("specialized")]
    public async Task<IActionResult> GetSpecializedTools()
    {
        var tools = await BuildToolQuery()
            .Where(x => x.IsSpecialized)
            .OrderBy(x => x.InternalCode)
            .ToListAsync();

        return Ok(tools);
    }

    [HttpGet("non-specialized")]
    public async Task<IActionResult> GetNonSpecializedTools()
    {
        var tools = await BuildToolQuery()
            .Where(x => !x.IsSpecialized)
            .OrderBy(x => x.InternalCode)
            .ToListAsync();

        return Ok(tools);
    }

    private IQueryable<ToolAssetResponse> BuildToolQuery()
    {
        return _context.ToolAssets
            .AsNoTracking()
            .Include(x => x.Zone)
            .Include(x => x.Branch)
            .Include(x => x.Location)
            .Include(x => x.ResponsiblePerson)
            .Include(x => x.ToolType)
            .Include(x => x.ToolCategory)
            .Select(x => new ToolAssetResponse
            {
                Id = x.Id,
                InternalCode = x.InternalCode,
                Name = x.Name,
                Description = x.Description,
                Brand = x.Brand,
                Model = x.Model,
                SerialNumber = x.SerialNumber,
                FixedAssetCode = x.FixedAssetCode,
                FenixCode = x.FenixCode,
                UnitOfMeasure = x.UnitOfMeasure,
                Quantity = x.Quantity,
                IsSpecialized = x.IsSpecialized,
                RequiresMaintenance = x.RequiresMaintenance,
                RequiresPreOperationalCheck = x.RequiresPreOperationalCheck,
                RequiresCertification = x.RequiresCertification,
                OperationalStatus = x.OperationalStatus.ToString(),
                PhysicalStatus = x.PhysicalStatus.ToString(),
                CustodyStatus = x.CustodyStatus.ToString(),
                ReconciliationStatus = x.ReconciliationStatus.ToString(),
                SyncStatus = x.SyncStatus.ToString(),

                ZoneId = x.ZoneId,
                ZoneCode = x.Zone == null ? null : x.Zone.Code,
                ZoneName = x.Zone == null ? null : x.Zone.Name,

                BranchId = x.BranchId,
                BranchCode = x.Branch == null ? null : x.Branch.Code,
                BranchName = x.Branch == null ? null : x.Branch.Name,

                LocationId = x.LocationId,
                LocationCode = x.Location == null ? null : x.Location.Code,
                LocationName = x.Location == null ? null : x.Location.Name,

                ResponsiblePersonId = x.ResponsiblePersonId,
                ResponsiblePersonName = x.ResponsiblePerson == null ? null : x.ResponsiblePerson.FullName,

                ToolTypeId = x.ToolTypeId,
                ToolTypeName = x.ToolType == null ? null : x.ToolType.Name,

                ToolCategoryId = x.ToolCategoryId,
                ToolCategoryName = x.ToolCategory == null ? null : x.ToolCategory.Name
            });
    }

    private sealed class ToolAssetResponse
    {
        public Guid Id { get; set; }
        public string InternalCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? SerialNumber { get; set; }
        public string? FixedAssetCode { get; set; }
        public string? FenixCode { get; set; }
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public bool IsSpecialized { get; set; }
        public bool RequiresMaintenance { get; set; }
        public bool RequiresPreOperationalCheck { get; set; }
        public bool RequiresCertification { get; set; }
        public string OperationalStatus { get; set; } = string.Empty;
        public string PhysicalStatus { get; set; } = string.Empty;
        public string CustodyStatus { get; set; } = string.Empty;
        public string ReconciliationStatus { get; set; } = string.Empty;
        public string SyncStatus { get; set; } = string.Empty;

        public Guid ZoneId { get; set; }
        public string? ZoneCode { get; set; }
        public string? ZoneName { get; set; }

        public Guid BranchId { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }

        public Guid? LocationId { get; set; }
        public string? LocationCode { get; set; }
        public string? LocationName { get; set; }

        public Guid? ResponsiblePersonId { get; set; }
        public string? ResponsiblePersonName { get; set; }

        public Guid? ToolTypeId { get; set; }
        public string? ToolTypeName { get; set; }

        public Guid? ToolCategoryId { get; set; }
        public string? ToolCategoryName { get; set; }
    }
}
