using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Organization;
using Navi.ToolsAssets.Domain.Entities.Security;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsManagementController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public SettingsManagementController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // ROLES
    // =========================================================

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles(CancellationToken cancellationToken)
    {
        var roles = await _context.AppRoles
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.Permissions,
                x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(roles);
    }

    [HttpPost("roles")]
    public async Task<IActionResult> CreateRole([FromBody] SaveRoleRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeCode(request.Code);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Código y nombre son obligatorios." });
        }

        var exists = await _context.AppRoles
            .AnyAsync(x => x.Code == code && !x.IsDeleted, cancellationToken);

        if (exists)
        {
            return Conflict(new { Message = $"Ya existe un rol con código {code}." });
        }

        var role = new AppRole
        {
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            Permissions = request.Permissions?.Trim(),
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ChangedBy ?? "settings"
        };

        _context.AppRoles.Add(role);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { role.Id, Message = "Rol creado correctamente." });
    }

    [HttpPut("roles/{id:guid}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] SaveRoleRequest request, CancellationToken cancellationToken)
    {
        var role = await _context.AppRoles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (role is null)
        {
            return NotFound(new { Message = "No se encontró el rol." });
        }

        var code = NormalizeCode(request.Code);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Código y nombre son obligatorios." });
        }

        var duplicated = await _context.AppRoles
            .AnyAsync(x => x.Id != id && x.Code == code && !x.IsDeleted, cancellationToken);

        if (duplicated)
        {
            return Conflict(new { Message = $"Ya existe otro rol con código {code}." });
        }

        role.Code = code;
        role.Name = request.Name.Trim();
        role.Description = request.Description?.Trim();
        role.Permissions = request.Permissions?.Trim();
        role.IsActive = request.IsActive ?? role.IsActive;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = request.ChangedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Rol actualizado correctamente." });
    }

    [HttpDelete("roles/{id:guid}")]
    public async Task<IActionResult> DeleteRole(Guid id, [FromQuery] string? changedBy, CancellationToken cancellationToken)
    {
        var role = await _context.AppRoles.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (role is null)
        {
            return NotFound(new { Message = "No se encontró el rol." });
        }

        var hasUsers = await _context.AppUsers.AnyAsync(x => x.AppRoleId == id && !x.IsDeleted, cancellationToken);

        if (hasUsers)
        {
            return BadRequest(new { Message = "No se puede eliminar el rol porque tiene usuarios asociados." });
        }

        role.IsDeleted = true;
        role.IsActive = false;
        role.UpdatedAt = DateTime.UtcNow;
        role.UpdatedBy = changedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Rol eliminado correctamente." });
    }

    // =========================================================
    // USUARIOS
    // =========================================================

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await _context.AppUsers
            .AsNoTracking()
            .Include(x => x.AppRole)
            .Include(x => x.Branch)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.UserName)
            .Select(x => new
            {
                x.Id,
                x.UserName,
                x.DisplayName,
                x.Email,
                x.Position,
                x.Area,
                x.IsActive,
                Role = x.AppRole == null ? null : new
                {
                    x.AppRole.Id,
                    x.AppRole.Code,
                    x.AppRole.Name
                },
                Branch = x.Branch == null ? null : new
                {
                    x.Branch.Id,
                    x.Branch.Code,
                    x.Branch.Name
                }
            })
            .ToListAsync(cancellationToken);

        return Ok(users);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] SaveUserRequest request, CancellationToken cancellationToken)
    {
        var userName = NormalizeUserName(request.UserName);

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest(new { Message = "Usuario y nombre son obligatorios." });
        }

        var roleExists = await _context.AppRoles.AnyAsync(x => x.Id == request.AppRoleId && !x.IsDeleted, cancellationToken);

        if (!roleExists)
        {
            return BadRequest(new { Message = "El rol seleccionado no existe." });
        }

        var exists = await _context.AppUsers.AnyAsync(x => x.UserName == userName && !x.IsDeleted, cancellationToken);

        if (exists)
        {
            return Conflict(new { Message = $"Ya existe un usuario con nombre {userName}." });
        }

        var user = new AppUser
        {
            UserName = userName,
            DisplayName = request.DisplayName.Trim(),
            Email = request.Email?.Trim(),
            Position = request.Position?.Trim(),
            Area = request.Area?.Trim(),
            AppRoleId = request.AppRoleId,
            BranchId = request.BranchId,
            ResponsiblePersonId = request.ResponsiblePersonId,
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ChangedBy ?? "settings"
        };

        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { user.Id, Message = "Usuario creado correctamente." });
    }

    [HttpPut("users/{id:guid}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] SaveUserRequest request, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            return NotFound(new { Message = "No se encontró el usuario." });
        }

        var userName = NormalizeUserName(request.UserName);

        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest(new { Message = "Usuario y nombre son obligatorios." });
        }

        var roleExists = await _context.AppRoles.AnyAsync(x => x.Id == request.AppRoleId && !x.IsDeleted, cancellationToken);

        if (!roleExists)
        {
            return BadRequest(new { Message = "El rol seleccionado no existe." });
        }

        var duplicated = await _context.AppUsers
            .AnyAsync(x => x.Id != id && x.UserName == userName && !x.IsDeleted, cancellationToken);

        if (duplicated)
        {
            return Conflict(new { Message = $"Ya existe otro usuario con nombre {userName}." });
        }

        user.UserName = userName;
        user.DisplayName = request.DisplayName.Trim();
        user.Email = request.Email?.Trim();
        user.Position = request.Position?.Trim();
        user.Area = request.Area?.Trim();
        user.AppRoleId = request.AppRoleId;
        user.BranchId = request.BranchId;
        user.ResponsiblePersonId = request.ResponsiblePersonId;
        user.IsActive = request.IsActive ?? user.IsActive;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = request.ChangedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Usuario actualizado correctamente." });
    }

    [HttpDelete("users/{id:guid}")]
    public async Task<IActionResult> DeleteUser(Guid id, [FromQuery] string? changedBy, CancellationToken cancellationToken)
    {
        var user = await _context.AppUsers.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (user is null)
        {
            return NotFound(new { Message = "No se encontró el usuario." });
        }

        user.IsDeleted = true;
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = changedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Usuario eliminado correctamente." });
    }

    // =========================================================
    // SEDES
    // =========================================================

    [HttpPost("branches")]
    public async Task<IActionResult> CreateBranch([FromBody] SaveBranchRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeCode(request.Code);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Código y nombre son obligatorios." });
        }

        var zoneExists = await _context.Zones.AnyAsync(x => x.Id == request.ZoneId && !x.IsDeleted, cancellationToken);

        if (!zoneExists)
        {
            return BadRequest(new { Message = "La zona seleccionada no existe." });
        }

        var exists = await _context.Branches.AnyAsync(x => x.Code == code && !x.IsDeleted, cancellationToken);

        if (exists)
        {
            return Conflict(new { Message = $"Ya existe una sede con código {code}." });
        }

        var branch = new Branch
        {
            Code = code,
            Name = request.Name.Trim(),
            City = request.City?.Trim(),
            Address = request.Address?.Trim(),
            ZoneId = request.ZoneId,
            IsPilot = request.IsPilot ?? false,
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ChangedBy ?? "settings"
        };

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { branch.Id, Message = "Sede creada correctamente." });
    }

    [HttpPut("branches/{id:guid}")]
    public async Task<IActionResult> UpdateBranch(Guid id, [FromBody] SaveBranchRequest request, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (branch is null)
        {
            return NotFound(new { Message = "No se encontró la sede." });
        }

        var code = NormalizeCode(request.Code);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Código y nombre son obligatorios." });
        }

        branch.Code = code;
        branch.Name = request.Name.Trim();
        branch.City = request.City?.Trim();
        branch.Address = request.Address?.Trim();
        branch.ZoneId = request.ZoneId;
        branch.IsPilot = request.IsPilot ?? branch.IsPilot;
        branch.IsActive = request.IsActive ?? branch.IsActive;
        branch.UpdatedAt = DateTime.UtcNow;
        branch.UpdatedBy = request.ChangedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Sede actualizada correctamente." });
    }

    [HttpDelete("branches/{id:guid}")]
    public async Task<IActionResult> DeleteBranch(Guid id, [FromQuery] string? changedBy, CancellationToken cancellationToken)
    {
        var branch = await _context.Branches.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (branch is null)
        {
            return NotFound(new { Message = "No se encontró la sede." });
        }

        var hasTools = await _context.ToolAssets.AnyAsync(x => x.BranchId == id && !x.IsDeleted, cancellationToken);

        if (hasTools)
        {
            return BadRequest(new { Message = "No se puede eliminar la sede porque tiene activos asociados." });
        }

        branch.IsDeleted = true;
        branch.IsActive = false;
        branch.UpdatedAt = DateTime.UtcNow;
        branch.UpdatedBy = changedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Sede eliminada correctamente." });
    }

    // =========================================================
    // ALMACENES / TALLERES / UBICACIONES
    // =========================================================

    [HttpPost("warehouses")]
    public async Task<IActionResult> CreateWarehouse([FromBody] SaveWarehouseRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeCode(request.Code);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Código y nombre son obligatorios." });
        }

        var branchExists = await _context.Branches.AnyAsync(x => x.Id == request.BranchId && !x.IsDeleted, cancellationToken);

        if (!branchExists)
        {
            return BadRequest(new { Message = "La sede seleccionada no existe." });
        }

        var exists = await _context.ToolLocations.AnyAsync(x => x.Code == code && !x.IsDeleted, cancellationToken);

        if (exists)
        {
            return Conflict(new { Message = $"Ya existe un almacén, taller o ubicación con código {code}." });
        }

        var location = new ToolLocation
        {
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            BranchId = request.BranchId,
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ChangedBy ?? "settings"
        };

        _context.ToolLocations.Add(location);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { location.Id, Message = "Almacén / taller creado correctamente." });
    }

    [HttpPut("warehouses/{id:guid}")]
    public async Task<IActionResult> UpdateWarehouse(Guid id, [FromBody] SaveWarehouseRequest request, CancellationToken cancellationToken)
    {
        var location = await _context.ToolLocations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (location is null)
        {
            return NotFound(new { Message = "No se encontró el almacén, taller o ubicación." });
        }

        var code = NormalizeCode(request.Code);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Código y nombre son obligatorios." });
        }

        location.Code = code;
        location.Name = request.Name.Trim();
        location.Description = request.Description?.Trim();
        location.BranchId = request.BranchId;
        location.IsActive = request.IsActive ?? location.IsActive;
        location.UpdatedAt = DateTime.UtcNow;
        location.UpdatedBy = request.ChangedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Almacén / taller actualizado correctamente." });
    }

    [HttpDelete("warehouses/{id:guid}")]
    public async Task<IActionResult> DeleteWarehouse(Guid id, [FromQuery] string? changedBy, CancellationToken cancellationToken)
    {
        var location = await _context.ToolLocations.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (location is null)
        {
            return NotFound(new { Message = "No se encontró el almacén, taller o ubicación." });
        }

        var hasTools = await _context.ToolAssets.AnyAsync(x => x.LocationId == id && !x.IsDeleted, cancellationToken);

        if (hasTools)
        {
            return BadRequest(new { Message = "No se puede eliminar porque tiene activos asociados." });
        }

        location.IsDeleted = true;
        location.IsActive = false;
        location.UpdatedAt = DateTime.UtcNow;
        location.UpdatedBy = changedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Almacén / taller eliminado correctamente." });
    }


    // =========================================================
    // ZONAS
    // =========================================================

    [HttpGet("zones")]
    public async Task<IActionResult> GetZones(CancellationToken cancellationToken)
    {
        var zones = await _context.Zones
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(zones);
    }

    [HttpPost("zones")]
    public async Task<IActionResult> CreateZone([FromBody] SaveZoneRequest request, CancellationToken cancellationToken)
    {
        var code = NormalizeCode(request.Code);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Código y nombre son obligatorios." });
        }

        var exists = await _context.Zones
            .AnyAsync(x => x.Code == code && !x.IsDeleted, cancellationToken);

        if (exists)
        {
            return Conflict(new { Message = $"Ya existe una zona con código {code}." });
        }

        var zone = new Zone
        {
            Code = code,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            IsActive = request.IsActive ?? true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = request.ChangedBy ?? "settings"
        };

        _context.Zones.Add(zone);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { zone.Id, Message = "Zona creada correctamente." });
    }

    [HttpPut("zones/{id:guid}")]
    public async Task<IActionResult> UpdateZone(Guid id, [FromBody] SaveZoneRequest request, CancellationToken cancellationToken)
    {
        var zone = await _context.Zones
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (zone is null)
        {
            return NotFound(new { Message = "No se encontró la zona." });
        }

        var code = NormalizeCode(request.Code);

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { Message = "Código y nombre son obligatorios." });
        }

        var duplicated = await _context.Zones
            .AnyAsync(x => x.Id != id && x.Code == code && !x.IsDeleted, cancellationToken);

        if (duplicated)
        {
            return Conflict(new { Message = $"Ya existe otra zona con código {code}." });
        }

        zone.Code = code;
        zone.Name = request.Name.Trim();
        zone.Description = request.Description?.Trim();
        zone.IsActive = request.IsActive ?? zone.IsActive;
        zone.UpdatedAt = DateTime.UtcNow;
        zone.UpdatedBy = request.ChangedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Zona actualizada correctamente." });
    }

    [HttpDelete("zones/{id:guid}")]
    public async Task<IActionResult> DeleteZone(Guid id, [FromQuery] string? changedBy, CancellationToken cancellationToken)
    {
        var zone = await _context.Zones
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (zone is null)
        {
            return NotFound(new { Message = "No se encontró la zona." });
        }

        var hasBranches = await _context.Branches
            .AnyAsync(x => x.ZoneId == id && !x.IsDeleted, cancellationToken);

        if (hasBranches)
        {
            return BadRequest(new { Message = "No se puede eliminar la zona porque tiene sedes asociadas." });
        }

        zone.IsDeleted = true;
        zone.IsActive = false;
        zone.UpdatedAt = DateTime.UtcNow;
        zone.UpdatedBy = changedBy ?? "settings";

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new { Message = "Zona eliminada correctamente." });
    }

    // =========================================================
    // CONSULTAS DE SEDES Y ALMACENES
    // =========================================================

    [HttpGet("branches")]
    public async Task<IActionResult> GetBranches(CancellationToken cancellationToken)
    {
        var branches = await _context.Branches
            .AsNoTracking()
            .Include(x => x.Zone)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.City,
                x.Address,
                x.ZoneId,
                Zone = x.Zone == null ? null : new
                {
                    x.Zone.Id,
                    x.Zone.Code,
                    x.Zone.Name
                },
                x.IsPilot,
                x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(branches);
    }

    [HttpGet("warehouses")]
    public async Task<IActionResult> GetWarehouses(CancellationToken cancellationToken)
    {
        var warehouses = await _context.ToolLocations
            .AsNoTracking()
            .Include(x => x.Branch)
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Branch == null ? "" : x.Branch.Code)
            .ThenBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.BranchId,
                Branch = x.Branch == null ? null : new
                {
                    x.Branch.Id,
                    x.Branch.Code,
                    x.Branch.Name
                },
                x.IsActive
            })
            .ToListAsync(cancellationToken);

        return Ok(warehouses);
    }
    private static string NormalizeCode(string? code)
    {
        return (code ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static string NormalizeUserName(string? userName)
    {
        return (userName ?? string.Empty).Trim().ToLowerInvariant();
    }
}


public sealed class SaveZoneRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool? IsActive { get; set; }
    public string? ChangedBy { get; set; }
}
public sealed class SaveRoleRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Permissions { get; set; }
    public bool? IsActive { get; set; }
    public string? ChangedBy { get; set; }
}

public sealed class SaveUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Position { get; set; }
    public string? Area { get; set; }
    public Guid AppRoleId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ResponsiblePersonId { get; set; }
    public bool? IsActive { get; set; }
    public string? ChangedBy { get; set; }
}

public sealed class SaveBranchRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Address { get; set; }
    public Guid ZoneId { get; set; }
    public bool? IsPilot { get; set; }
    public bool? IsActive { get; set; }
    public string? ChangedBy { get; set; }
}

public sealed class SaveWarehouseRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid BranchId { get; set; }
    public bool? IsActive { get; set; }
    public string? ChangedBy { get; set; }
}

