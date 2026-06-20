using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/organization")]
public class OrganizationController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public OrganizationController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet("zones")]
    public async Task<IActionResult> GetZones()
    {
        var zones = await _context.Zones
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.IsActive
            })
            .ToListAsync();

        return Ok(zones);
    }

    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches()
    {
        var branches = await _context.Branches
            .AsNoTracking()
            .Include(x => x.Zone)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.City,
                x.Address,
                x.IsPilot,
                x.IsActive,
                Zone = x.Zone == null ? null : new
                {
                    x.Zone.Id,
                    x.Zone.Code,
                    x.Zone.Name
                }
            })
            .ToListAsync();

        return Ok(branches);
    }

    [HttpGet("branches/{code}")]
    public async Task<IActionResult> GetBranchByCode(string code)
    {
        var branch = await _context.Branches
            .AsNoTracking()
            .Include(x => x.Zone)
            .Where(x => x.Code == code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.City,
                x.Address,
                x.IsPilot,
                x.IsActive,
                Zone = x.Zone == null ? null : new
                {
                    x.Zone.Id,
                    x.Zone.Code,
                    x.Zone.Name
                }
            })
            .FirstOrDefaultAsync();

        if (branch is null)
        {
            return NotFound(new
            {
                Message = $"No se encontró la sede con código {code}."
            });
        }

        return Ok(branch);
    }

    [HttpGet("branches/by-zone/{zoneCode}")]
    public async Task<IActionResult> GetBranchesByZone(string zoneCode)
    {
        var branches = await _context.Branches
            .AsNoTracking()
            .Include(x => x.Zone)
            .Where(x => x.Zone != null && x.Zone.Code == zoneCode)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.City,
                x.Address,
                x.IsPilot,
                x.IsActive,
                Zone = x.Zone == null ? null : new
                {
                    x.Zone.Id,
                    x.Zone.Code,
                    x.Zone.Name
                }
            })
            .ToListAsync();

        return Ok(branches);
    }

    [HttpGet("locations")]
    public async Task<IActionResult> GetLocations()
    {
        var locations = await _context.ToolLocations
            .AsNoTracking()
            .Include(x => x.Branch)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.IsActive,
                Branch = x.Branch == null ? null : new
                {
                    x.Branch.Id,
                    x.Branch.Code,
                    x.Branch.Name
                }
            })
            .ToListAsync();

        return Ok(locations);
    }

    [HttpGet("locations/by-branch/{branchCode}")]
    public async Task<IActionResult> GetLocationsByBranch(string branchCode)
    {
        var locations = await _context.ToolLocations
            .AsNoTracking()
            .Include(x => x.Branch)
            .Where(x => x.Branch != null && x.Branch.Code == branchCode)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.IsActive,
                Branch = x.Branch == null ? null : new
                {
                    x.Branch.Id,
                    x.Branch.Code,
                    x.Branch.Name
                }
            })
            .ToListAsync();

        return Ok(locations);
    }
}
