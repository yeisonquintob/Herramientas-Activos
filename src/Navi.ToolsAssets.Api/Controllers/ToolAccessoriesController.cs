using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolAccessoriesController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public ToolAccessoriesController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:guid}/accessories")]
    public async Task<IActionResult> GetByToolId(Guid id, CancellationToken cancellationToken)
    {
        var toolExists = await _context.ToolAssets
            .AnyAsync(x => x.Id == id, cancellationToken);

        if (!toolExists)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        var accessories = await _context.ToolAccessories
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.Name,
                x.Quantity,
                x.RequiresMaintenance,
                x.IsMeasurementEquipment,
                x.Observation,
                x.IsActive,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return Ok(accessories);
    }

    [HttpGet("by-code/{internalCode}/accessories")]
    public async Task<IActionResult> GetByToolCode(string internalCode, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        var accessories = await _context.ToolAccessories
            .AsNoTracking()
            .Where(x => x.ToolAssetId == tool.Id && x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.Name,
                x.Quantity,
                x.RequiresMaintenance,
                x.IsMeasurementEquipment,
                x.Observation,
                x.IsActive,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return Ok(accessories);
    }

    [HttpPost("{id:guid}/accessories")]
    public async Task<IActionResult> Create(Guid id, [FromBody] CreateToolAccessoryRequest request, CancellationToken cancellationToken)
    {
        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await CreateAccessoryAsync(tool, request, cancellationToken);
    }

    [HttpPost("by-code/{internalCode}/accessories")]
    public async Task<IActionResult> CreateByCode(string internalCode, [FromBody] CreateToolAccessoryRequest request, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await CreateAccessoryAsync(tool, request, cancellationToken);
    }

    [HttpPut("accessories/{accessoryId:guid}")]
    public async Task<IActionResult> Update(Guid accessoryId, [FromBody] UpdateToolAccessoryRequest request, CancellationToken cancellationToken)
    {
        var accessory = await _context.ToolAccessories
            .Include(x => x.ToolAsset)
            .FirstOrDefaultAsync(x => x.Id == accessoryId, cancellationToken);

        if (accessory is null)
        {
            return NotFound(new { Message = "No se encontró el accesorio." });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "El nombre del accesorio es obligatorio." });
        }

        var user = string.IsNullOrWhiteSpace(request.UpdatedBy) ? "yquinto" : request.UpdatedBy.Trim();

        var previousValue = $"{accessory.Name} | Cantidad: {accessory.Quantity}";

        accessory.Name = request.Name.Trim();
        accessory.Quantity = request.Quantity <= 0 ? 1 : request.Quantity;
        accessory.RequiresMaintenance = request.RequiresMaintenance;
        accessory.IsMeasurementEquipment = request.IsMeasurementEquipment;
        accessory.Observation = string.IsNullOrWhiteSpace(request.Observation) ? null : request.Observation.Trim();
        accessory.UpdatedAt = DateTime.UtcNow;
        accessory.UpdatedBy = user;

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = accessory.ToolAssetId,
            EventType = "ToolAccessoryUpdated",
            Title = "Accesorio actualizado",
            Description = $"Se actualizó el accesorio: {accessory.Name}.",
            PreviousValue = previousValue,
            NewValue = $"{accessory.Name} | Cantidad: {accessory.Quantity}",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Accesorio actualizado correctamente.",
            accessory.Id,
            accessory.ToolAssetId,
            accessory.Name,
            accessory.Quantity,
            accessory.RequiresMaintenance,
            accessory.IsMeasurementEquipment,
            accessory.Observation
        });
    }

    [HttpDelete("accessories/{accessoryId:guid}")]
    public async Task<IActionResult> Delete(Guid accessoryId, [FromQuery] string? deletedBy, CancellationToken cancellationToken)
    {
        var accessory = await _context.ToolAccessories
            .FirstOrDefaultAsync(x => x.Id == accessoryId, cancellationToken);

        if (accessory is null)
        {
            return NotFound(new { Message = "No se encontró el accesorio." });
        }

        var user = string.IsNullOrWhiteSpace(deletedBy) ? "yquinto" : deletedBy.Trim();

        accessory.IsActive = false;
        accessory.IsDeleted = true;
        accessory.UpdatedAt = DateTime.UtcNow;
        accessory.UpdatedBy = user;

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = accessory.ToolAssetId,
            EventType = "ToolAccessoryDeleted",
            Title = "Accesorio desactivado",
            Description = $"Se desactivó el accesorio: {accessory.Name}.",
            PreviousValue = accessory.Name,
            NewValue = "Desactivado",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Accesorio desactivado correctamente.",
            accessory.Id,
            accessory.ToolAssetId,
            accessory.Name
        });
    }

    private async Task<IActionResult> CreateAccessoryAsync(
        ToolAsset tool,
        CreateToolAccessoryRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "El nombre del accesorio es obligatorio." });
        }

        var user = string.IsNullOrWhiteSpace(request.CreatedBy) ? "yquinto" : request.CreatedBy.Trim();
        var normalizedName = request.Name.Trim().ToUpperInvariant();

        var exists = await _context.ToolAccessories
            .AnyAsync(x =>
                x.ToolAssetId == tool.Id &&
                !x.IsDeleted &&
                x.Name.ToUpper() == normalizedName,
                cancellationToken);

        if (exists)
        {
            return BadRequest(new { Message = "Ya existe un accesorio activo con el mismo nombre para esta herramienta." });
        }

        var accessory = new ToolAccessory
        {
            ToolAssetId = tool.Id,
            Name = request.Name.Trim(),
            Quantity = request.Quantity <= 0 ? 1 : request.Quantity,
            RequiresMaintenance = request.RequiresMaintenance,
            IsMeasurementEquipment = request.IsMeasurementEquipment,
            Observation = string.IsNullOrWhiteSpace(request.Observation) ? null : request.Observation.Trim(),
            CreatedBy = user
        };

        _context.ToolAccessories.Add(accessory);

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = tool.Id,
            EventType = "ToolAccessoryCreated",
            Title = "Accesorio registrado",
            Description = $"Se registró el accesorio: {accessory.Name}.",
            PreviousValue = null,
            NewValue = $"{accessory.Name} | Cantidad: {accessory.Quantity}",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Accesorio creado correctamente.",
            accessory.Id,
            accessory.ToolAssetId,
            accessory.Name,
            accessory.Quantity,
            accessory.RequiresMaintenance,
            accessory.IsMeasurementEquipment,
            accessory.Observation
        });
    }
}

public sealed class CreateToolAccessoryRequest
{
    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public bool RequiresMaintenance { get; set; }

    public bool IsMeasurementEquipment { get; set; }

    public string? Observation { get; set; }

    public string? CreatedBy { get; set; }
}

public sealed class UpdateToolAccessoryRequest
{
    public string Name { get; set; } = string.Empty;

    public int Quantity { get; set; } = 1;

    public bool RequiresMaintenance { get; set; }

    public bool IsMeasurementEquipment { get; set; }

    public string? Observation { get; set; }

    public string? UpdatedBy { get; set; }
}
