using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Api.Tenancy;
using Navi.ToolsAssets.Domain.Entities.Tenancy;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;
using Navi.ToolsAssets.Infrastructure.Tenancy;
using Navi.ToolsAssets.Shared.Security;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/companies")]
public sealed class CompaniesController : ControllerBase
{
    private static readonly Regex CompanyCodePattern =
        new("^[A-Z0-9]{2,20}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly NaviMasterDbContext _masterContext;
    private readonly NaviToolsAssetsDbContext _securityContext;
    private readonly JwtTokenService _tokenService;
    private readonly JwtOptions _jwtOptions;
    private readonly IConfiguration _configuration;

    public CompaniesController(
        NaviMasterDbContext masterContext,
        NaviToolsAssetsDbContext securityContext,
        JwtTokenService tokenService,
        IOptions<JwtOptions> jwtOptions,
        IConfiguration configuration)
    {
        _masterContext = masterContext;
        _securityContext = securityContext;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
        _configuration = configuration;
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var companies = await _masterContext.UserCompanyAccesses
            .AsNoTracking()
            .Where(x => x.UserId == userId.Value && x.IsActive)
            .Select(x => new CompanySummaryResponse(
                x.CompanyId,
                x.Company!.Code,
                x.Company.Name,
                x.Company.Status,
                x.Company.IsActive,
                x.IsDefault,
                x.Company.Databases
                    .Where(database => !database.IsDeleted)
                    .Select(database => database.Status)
                    .FirstOrDefault(),
                x.Company.Databases
                    .Where(database => !database.IsDeleted)
                    .Select(database => database.SchemaVersion)
                    .FirstOrDefault()))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(companies);
    }

    [HttpGet]
    [RequirePermission(PermissionCodes.CompaniesView)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var companies = await _masterContext.Companies
            .AsNoTracking()
            .Select(x => new CompanySummaryResponse(
                x.Id,
                x.Code,
                x.Name,
                x.Status,
                x.IsActive,
                false,
                x.Databases
                    .Where(database => !database.IsDeleted)
                    .Select(database => database.Status)
                    .FirstOrDefault(),
                x.Databases
                    .Where(database => !database.IsDeleted)
                    .Select(database => database.SchemaVersion)
                    .FirstOrDefault()))
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return Ok(companies);
    }

    [HttpPost]
    [RequirePermission(PermissionCodes.CompaniesManage)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCompanyRequest request,
        CancellationToken cancellationToken)
    {
        var code = request.Code?.Trim().ToUpperInvariant();
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(code) || !CompanyCodePattern.IsMatch(code))
        {
            return UnprocessableEntity(new
            {
                Message = "El código debe contener de 2 a 20 caracteres A-Z o 0-9."
            });
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            return UnprocessableEntity(new
            {
                Message = "El nombre es obligatorio y no puede superar 200 caracteres."
            });
        }

        if (await _masterContext.Companies.AnyAsync(x => x.Code == code, cancellationToken))
        {
            return Conflict(new { Message = "Ya existe una compañía con ese código." });
        }

        var actor = User.GetUserName() ?? "system";
        var company = new Company
        {
            Code = code,
            Name = name,
            TaxIdentifier = Limit(request.TaxIdentifier, 80),
            PlanCode = Limit(request.PlanCode, 80),
            Status = CompanyStatuses.Provisioning,
            IsActive = false,
            CreatedBy = actor
        };

        var database = new CompanyDatabase
        {
            CompanyId = company.Id,
            DatabaseName = $"NaviTenant_{code}",
            ConnectionKey = $"Company_{code}",
            Status = "Pending",
            CreatedBy = actor
        };

        company.Databases.Add(database);
        _masterContext.Companies.Add(company);

        var actorId = User.GetUserId();
        if (actorId.HasValue)
        {
            company.UserAccesses.Add(new UserCompanyAccess
            {
                UserId = actorId.Value,
                CompanyId = company.Id,
                IsActive = true,
                IsDefault = false,
                RoleCode = "ADMIN",
                CreatedBy = actor
            });
        }

        await _masterContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetAll),
            new { id = company.Id },
            new CompanySummaryResponse(
                company.Id,
                company.Code,
                company.Name,
                company.Status,
                company.IsActive,
                false,
                database.Status,
                database.SchemaVersion));
    }

    [HttpPost("{companyId:guid}/access")]
    [RequirePermission(PermissionCodes.CompaniesManage)]
    public async Task<IActionResult> GrantAccess(
        Guid companyId,
        [FromBody] GrantCompanyAccessRequest request,
        CancellationToken cancellationToken)
    {
        if (!await _masterContext.Companies.AnyAsync(x => x.Id == companyId, cancellationToken))
        {
            return NotFound(new { Message = "La compañía no existe." });
        }

        var access = await _masterContext.UserCompanyAccesses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.UserId == request.UserId && x.CompanyId == companyId,
                cancellationToken);

        if (access is null)
        {
            access = new UserCompanyAccess
            {
                UserId = request.UserId,
                CompanyId = companyId,
                CreatedBy = User.GetUserName()
            };
            _masterContext.UserCompanyAccesses.Add(access);
        }

        access.IsDeleted = false;
        access.IsActive = request.IsActive;
        access.IsDefault = request.IsDefault;
        access.RoleCode = Limit(request.RoleCode, 80);
        access.UpdatedAt = DateTime.UtcNow;
        access.UpdatedBy = User.GetUserName();

        if (request.IsDefault)
        {
            var otherDefaults = await _masterContext.UserCompanyAccesses
                .Where(x => x.UserId == request.UserId && x.Id != access.Id && x.IsDefault)
                .ToListAsync(cancellationToken);
            foreach (var other in otherDefaults)
            {
                other.IsDefault = false;
            }
        }

        await _masterContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{companyId:guid}/test-connection")]
    [RequirePermission(PermissionCodes.CompaniesManage)]
    public async Task<IActionResult> TestConnection(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var database = await _masterContext.CompanyDatabases
            .Include(x => x.Company)
            .FirstOrDefaultAsync(x => x.CompanyId == companyId, cancellationToken);

        if (database?.Company is null)
        {
            return NotFound(new { Message = "La base de la compañía no está registrada." });
        }

        var connectionString = _configuration[$"TenantDatabases:{database.ConnectionKey}"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return Conflict(new
            {
                Message = $"Falta configurar externamente TenantDatabases:{database.ConnectionKey}."
            });
        }

        var succeeded = false;
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);
            succeeded = true;
        }
        catch (SqlException)
        {
            // La respuesta no expone detalles de conexión ni del servidor.
        }

        database.LastConnectionTestAtUtc = DateTime.UtcNow;
        database.LastConnectionTestSucceeded = succeeded;
        database.Status = succeeded ? "Ready" : "Unavailable";
        database.Company.IsActive = succeeded;
        database.Company.Status = succeeded
            ? CompanyStatuses.Active
            : CompanyStatuses.Provisioning;
        database.Company.ActivatedAtUtc = succeeded
            ? DateTime.UtcNow
            : database.Company.ActivatedAtUtc;
        await _masterContext.SaveChangesAsync(cancellationToken);

        return Ok(new { Succeeded = succeeded, database.Status });
    }

    [HttpPost("{companyId:guid}/select")]
    public async Task<IActionResult> Select(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var sessionId = User.GetSessionId();
        if (!userId.HasValue || !sessionId.HasValue)
        {
            return Unauthorized();
        }

        var access = await _masterContext.UserCompanyAccesses
            .AsNoTracking()
            .Include(x => x.Company)
                .ThenInclude(x => x!.Databases)
            .FirstOrDefaultAsync(
                x => x.UserId == userId.Value &&
                     x.CompanyId == companyId &&
                     x.IsActive,
                cancellationToken);

        if (access?.Company is null ||
            !access.Company.IsActive ||
            !string.Equals(access.Company.Status, CompanyStatuses.Active, StringComparison.OrdinalIgnoreCase) ||
            !access.Company.Databases.Any(x =>
                string.Equals(x.Status, "Ready", StringComparison.OrdinalIgnoreCase)))
        {
            return Forbid();
        }

        var user = await _securityContext.AppUsers
            .Include(x => x.AppRole)
            .Include(x => x.Branch)
            .Include(x => x.ResponsiblePerson)
            .FirstOrDefaultAsync(x => x.Id == userId.Value, cancellationToken);
        var session = await _securityContext.UserSessions
            .FirstOrDefaultAsync(
                x => x.Id == sessionId.Value && x.AppUserId == userId.Value,
                cancellationToken);

        if (user?.AppRole is null || session is null || session.IsRevoked)
        {
            return Unauthorized();
        }

        session.CompanyId = companyId;
        session.LastActivityAtUtc = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        session.UpdatedBy = user.UserName;
        await _securityContext.SaveChangesAsync(cancellationToken);

        var permissions = BuildPermissions(user.AppRole.Code, user.AppRole.Permissions);
        var token = _tokenService.CreateAccessToken(user, session, permissions);

        return Ok(new SelectCompanyResponse(
            companyId,
            access.Company.Code,
            access.Company.Name,
            token.AccessToken,
            token.ExpiresAtUtc,
            token.SessionId));
    }

    [HttpPost("{companyId:guid}/backups")]
    [RequirePermission(PermissionCodes.CompaniesBackup)]
    public async Task<IActionResult> RequestBackup(
        Guid companyId,
        CancellationToken cancellationToken)
    {
        if (!await _masterContext.Companies.AnyAsync(
                x => x.Id == companyId && x.IsActive,
                cancellationToken))
        {
            return NotFound(new { Message = "La compañía activa no existe." });
        }

        var userId = User.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var now = DateTime.UtcNow;
        var backup = new CompanyBackup
        {
            CompanyId = companyId,
            BackupNumber = $"BKP-{companyId:N}-{now:yyyyMMddHHmmss}",
            StorageObjectKey = $"company-backups/{companyId:N}/{now:yyyyMMddHHmmss}.bak",
            RequestedAtUtc = now,
            RequestedByUserId = userId.Value,
            CreatedBy = User.GetUserName()
        };

        _masterContext.CompanyBackups.Add(backup);
        await _masterContext.SaveChangesAsync(cancellationToken);

        return Accepted(new
        {
            backup.Id,
            backup.BackupNumber,
            backup.Status,
            Message = "Solicitud registrada. El worker productivo debe procesarla."
        });
    }

    private static IReadOnlyList<string> BuildPermissions(
        string roleCode,
        string? serializedPermissions)
    {
        if (string.Equals(roleCode, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return PermissionCatalog.All.Select(x => x.Code).ToArray();
        }

        return (serializedPermissions ?? string.Empty)
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string? Limit(string? value, int maxLength)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        return normalized.Length <= maxLength
            ? normalized
            : normalized[..maxLength];
    }
}

public sealed record CompanySummaryResponse(
    Guid Id,
    string Code,
    string Name,
    string Status,
    bool IsActive,
    bool IsDefault,
    string? DatabaseStatus,
    string? SchemaVersion);

public sealed record CreateCompanyRequest(
    string? Code,
    string? Name,
    string? TaxIdentifier,
    string? PlanCode);

public sealed record GrantCompanyAccessRequest(
    Guid UserId,
    bool IsActive,
    bool IsDefault,
    string? RoleCode);

public sealed record SelectCompanyResponse(
    Guid CompanyId,
    string CompanyCode,
    string CompanyName,
    string AccessToken,
    DateTime TokenExpiresAtUtc,
    Guid SessionId);
