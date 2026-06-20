using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/catalogs")]
public class CatalogsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public CatalogsController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet("tool-types")]
    public async Task<IActionResult> GetToolTypes()
    {
        var items = await _context.ToolTypes
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

        return Ok(items);
    }

    [HttpGet("tool-categories")]
    public async Task<IActionResult> GetToolCategories()
    {
        var items = await _context.ToolCategories
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

        return Ok(items);
    }

    [HttpGet("responsible-people")]
    public async Task<IActionResult> GetResponsiblePeople()
    {
        var items = await _context.ResponsiblePeople
            .AsNoTracking()
            .OrderBy(x => x.FullName)
            .Select(x => new
            {
                x.Id,
                x.EmployeeCode,
                x.DocumentNumber,
                x.FullName,
                x.Email,
                x.Position,
                x.Area,
                x.IsActive
            })
            .ToListAsync();

        return Ok(items);
    }
}
