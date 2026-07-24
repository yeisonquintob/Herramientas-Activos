using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Domain.Entities.Security;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;
using Navi.ToolsAssets.Shared.Security;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;
    private readonly IPasswordHasher<AppUser> _passwordHasher;
    private readonly JwtTokenService _tokenService;
    private readonly JwtOptions _jwtOptions;

    public AuthController(
        NaviToolsAssetsDbContext context,
        IPasswordHasher<AppUser> passwordHasher,
        JwtTokenService tokenService,
        IOptions<JwtOptions> jwtOptions)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _jwtOptions = jwtOptions.Value;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var userName = request.UserName?.Trim();

        if (string.IsNullOrWhiteSpace(userName))
        {
            return BadRequest(new { Message = "Debe ingresar el documento o usuario." });
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { Message = "Debe ingresar la contraseña." });
        }

        var user = await _context.AppUsers
            .Include(x => x.AppRole)
            .Include(x => x.Branch)
            .Include(x => x.ResponsiblePerson)
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                x.UserName.ToLower() == userName.ToLower(),
                cancellationToken);

        if (user is null)
        {
            await WriteAuditAsync(
                "LoginFailed",
                "Failure",
                null,
                userName,
                "InvalidCredentials",
                cancellationToken);
            return Unauthorized(new { Message = "Usuario o contraseña inválidos." });
        }

        if (!user.IsActive)
        {
            await WriteAuditAsync(
                "LoginFailed",
                "Failure",
                user.Id,
                user.UserName,
                "InactiveUser",
                cancellationToken);
            return Unauthorized(new { Message = "El usuario está inactivo o bloqueado." });
        }

        if (user.LockoutEndAt.HasValue && user.LockoutEndAt.Value > DateTime.UtcNow)
        {
            await WriteAuditAsync(
                "LoginFailed",
                "Failure",
                user.Id,
                user.UserName,
                "TemporarilyLocked",
                cancellationToken);
            return Unauthorized(new
            {
                Message = "El usuario está temporalmente bloqueado. Intente más tarde."
            });
        }

        var verification = VerifyPassword(user, request.Password);

        if (!verification.IsValid)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 5)
            {
                user.LockoutEndAt = DateTime.UtcNow.AddMinutes(15);
                user.FailedLoginAttempts = 0;
            }

            await WriteAuditAsync(
                "LoginFailed",
                "Failure",
                user.Id,
                user.UserName,
                string.IsNullOrWhiteSpace(user.PasswordHash)
                    ? "PasswordResetRequired"
                    : "InvalidCredentials",
                cancellationToken,
                saveChanges: false);
            await _context.SaveChangesAsync(cancellationToken);

            return Unauthorized(new
            {
                Message = string.IsNullOrWhiteSpace(user.PasswordHash)
                    ? "El usuario requiere restablecer su contraseña antes de ingresar."
                    : "Usuario o contraseña inválidos."
            });
        }

        if (user.AppRole is null || !user.AppRole.IsActive)
        {
            return Unauthorized(new { Message = "El usuario no tiene un rol activo asignado." });
        }

        var now = DateTime.UtcNow;
        user.FailedLoginAttempts = 0;
        user.LockoutEndAt = null;
        user.LastLoginAt = now;
        user.UpdatedAt = now;
        user.UpdatedBy = "auth-login";

        if (verification.NeedsRehash)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            user.PasswordChangedAt ??= now;
        }

        var permissions = BuildPermissions(user.AppRole.Code, user.AppRole.Permissions);
        var session = new UserSession
        {
            AppUserId = user.Id,
            SecurityStamp = user.SecurityStamp,
            LastActivityAtUtc = now,
            AbsoluteExpiresAtUtc = now.AddHours(
                Math.Clamp(_jwtOptions.AbsoluteSessionHours, 1, 72)),
            UserAgent = Limit(Request.Headers.UserAgent.ToString(), 300),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            CreatedBy = user.UserName
        };

        _context.UserSessions.Add(session);
        await WriteAuditAsync(
            "Login",
            "Success",
            user.Id,
            user.UserName,
            null,
            cancellationToken,
            saveChanges: false);
        await _context.SaveChangesAsync(cancellationToken);

        var issuedToken = _tokenService.CreateAccessToken(user, session, permissions);

        return Ok(new LoginResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            DocumentNumber = user.ResponsiblePerson?.DocumentNumber ?? user.UserName,
            EmployeeCode = user.ResponsiblePerson?.EmployeeCode,
            Position = user.Position,
            Area = user.Area,
            RoleId = user.AppRole.Id,
            RoleCode = user.AppRole.Code,
            RoleName = user.AppRole.Name,
            BranchId = user.BranchId,
            BranchCode = user.Branch?.Code ?? "TODAS",
            BranchName = user.Branch?.Name ?? "Todas las sedes",
            ResponsiblePersonId = user.ResponsiblePersonId,
            ResponsiblePersonName = user.ResponsiblePerson?.FullName,
            Permissions = permissions,
            LastLoginAt = user.LastLoginAt,
            AccessToken = issuedToken.AccessToken,
            TokenExpiresAtUtc = issuedToken.ExpiresAtUtc,
            SessionId = issuedToken.SessionId
        });
    }








    [HttpGet("mobile-session")]
    [HttpGet("mobile-session/{userName}")]
    public async Task<IActionResult> GetMobileSession(
        [FromRoute] string? userName,
        [FromQuery(Name = "userName")] string? queryUserName,
        CancellationToken cancellationToken)
    {
        var normalizedUserName = (queryUserName ?? userName)?.Trim();
        var authenticatedUserName = User.GetUserName();

        if (string.IsNullOrWhiteSpace(authenticatedUserName))
        {
            return Unauthorized(new { Message = "La sesión autenticada no contiene un usuario válido." });
        }

        if (!string.IsNullOrWhiteSpace(normalizedUserName) &&
            !string.Equals(
                normalizedUserName,
                authenticatedUserName,
                StringComparison.OrdinalIgnoreCase))
        {
            return Forbid();
        }

        normalizedUserName = authenticatedUserName;

        var user = await _context.AppUsers
            .Include(x => x.AppRole)
            .Include(x => x.Branch)
            .Include(x => x.ResponsiblePerson)
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                x.UserName.ToLower() == normalizedUserName.ToLower(),
                cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { Message = "La sesión móvil no corresponde a un usuario activo." });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { Message = "El usuario está inactivo o bloqueado." });
        }

        if (user.AppRole is null || !user.AppRole.IsActive)
        {
            return Unauthorized(new { Message = "El usuario no tiene un rol activo asignado." });
        }

        var permissions = BuildPermissions(user.AppRole.Code, user.AppRole.Permissions);

        return Ok(new LoginResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            DocumentNumber = user.ResponsiblePerson?.DocumentNumber ?? user.UserName,
            EmployeeCode = user.ResponsiblePerson?.EmployeeCode,
            Position = user.Position,
            Area = user.Area,
            RoleId = user.AppRole.Id,
            RoleCode = user.AppRole.Code,
            RoleName = user.AppRole.Name,
            BranchId = user.BranchId,
            BranchCode = user.Branch?.Code ?? "TODAS",
            BranchName = user.Branch?.Name ?? "Todas las sedes",
            ResponsiblePersonId = user.ResponsiblePersonId,
            ResponsiblePersonName = user.ResponsiblePerson?.FullName,
            Permissions = permissions,
            LastLoginAt = user.LastLoginAt,
            AccessToken = GetCurrentBearerToken(),
            TokenExpiresAtUtc = GetCurrentTokenExpiration(),
            SessionId = User.GetSessionId()
        });
    }


    [HttpPut("mobile-profile")]
    public async Task<IActionResult> UpdateMobileProfile([FromBody] MobileProfileUpdateRequest request, CancellationToken cancellationToken)
    {
        var currentUserName = GetCurrentMobileUserName();

        if (string.IsNullOrWhiteSpace(currentUserName))
        {
            return Unauthorized(new { Message = "No se recibió usuario de sesión móvil." });
        }

        var user = await _context.AppUsers
            .Include(x => x.AppRole)
            .Include(x => x.Branch)
            .Include(x => x.ResponsiblePerson)
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                x.UserName.ToLower() == currentUserName.ToLower(),
                cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { Message = "La sesión móvil no corresponde a un usuario activo." });
        }

        if (!user.IsActive)
        {
            return Unauthorized(new { Message = "El usuario está inactivo o bloqueado." });
        }

        if (user.AppRole is null || !user.AppRole.IsActive)
        {
            return Unauthorized(new { Message = "El usuario no tiene un rol activo asignado." });
        }

        var currentDocument = user.ResponsiblePerson?.DocumentNumber;

        if (string.IsNullOrWhiteSpace(currentDocument))
        {
            currentDocument = user.UserName;
        }

        var requestedDocument = request.DocumentNumber?.Trim();

        if (!string.IsNullOrWhiteSpace(currentDocument))
        {
            if (string.IsNullOrWhiteSpace(requestedDocument))
            {
                return BadRequest(new { Message = "La cédula/documento ya existe y no se puede borrar." });
            }

            if (!string.Equals(currentDocument.Trim(), requestedDocument, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { Message = "La cédula/documento ya existe y no se puede cambiar desde el perfil móvil." });
            }
        }
        else if (!string.IsNullOrWhiteSpace(requestedDocument))
        {
            var duplicatedUser = await _context.AppUsers
                .AnyAsync(x =>
                    !x.IsDeleted &&
                    x.Id != user.Id &&
                    x.UserName.ToLower() == requestedDocument.ToLower(),
                    cancellationToken);

            if (duplicatedUser)
            {
                return Conflict(new { Message = "Ya existe un usuario con esa cédula/documento." });
            }

            user.UserName = requestedDocument.Trim().ToLowerInvariant();
        }

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return BadRequest(new { Message = "El nombre completo es obligatorio." });
        }

        user.DisplayName = request.DisplayName.Trim();
        user.Email = request.Email?.Trim();
        user.Position = request.Position?.Trim();
        user.Area = request.Area?.Trim();
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserName;

        if (user.ResponsiblePerson is not null)
        {
            if (!string.IsNullOrWhiteSpace(request.EmployeeCode))
            {
                user.ResponsiblePerson.EmployeeCode = request.EmployeeCode.Trim();
            }

            if (string.IsNullOrWhiteSpace(user.ResponsiblePerson.DocumentNumber) &&
                !string.IsNullOrWhiteSpace(requestedDocument))
            {
                user.ResponsiblePerson.DocumentNumber = requestedDocument;
            }

            user.ResponsiblePerson.FullName = request.DisplayName.Trim();
            user.ResponsiblePerson.Email = request.Email?.Trim();
            user.ResponsiblePerson.Position = request.Position?.Trim();
            user.ResponsiblePerson.Area = request.Area?.Trim();
            user.ResponsiblePerson.UpdatedAt = DateTime.UtcNow;
            user.ResponsiblePerson.UpdatedBy = currentUserName;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var permissions = BuildPermissions(user.AppRole.Code, user.AppRole.Permissions);

        return Ok(new LoginResponse
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            DocumentNumber = user.ResponsiblePerson?.DocumentNumber ?? user.UserName,
            EmployeeCode = user.ResponsiblePerson?.EmployeeCode,
            Position = user.Position,
            Area = user.Area,
            RoleId = user.AppRole.Id,
            RoleCode = user.AppRole.Code,
            RoleName = user.AppRole.Name,
            BranchId = user.BranchId,
            BranchCode = user.Branch?.Code ?? "TODAS",
            BranchName = user.Branch?.Name ?? "Todas las sedes",
            ResponsiblePersonId = user.ResponsiblePersonId,
            ResponsiblePersonName = user.ResponsiblePerson?.FullName,
            Permissions = permissions,
            LastLoginAt = user.LastLoginAt,
            AccessToken = GetCurrentBearerToken(),
            TokenExpiresAtUtc = GetCurrentTokenExpiration(),
            SessionId = User.GetSessionId()
        });
    }

    [HttpPut("mobile-password")]
    public async Task<IActionResult> ChangeMobilePassword([FromBody] MobilePasswordChangeRequest request, CancellationToken cancellationToken)
    {
        var currentUserName = GetCurrentMobileUserName();

        if (string.IsNullOrWhiteSpace(currentUserName))
        {
            return Unauthorized(new { Message = "No se recibió usuario de sesión móvil." });
        }

        var user = await _context.AppUsers
            .FirstOrDefaultAsync(x =>
                !x.IsDeleted &&
                x.UserName.ToLower() == currentUserName.ToLower(),
                cancellationToken);

        if (user is null)
        {
            return Unauthorized(new { Message = "La sesión móvil no corresponde a un usuario activo." });
        }

        if (!IsPasswordPolicyValid(request.NewPassword))
        {
            return BadRequest(new
            {
                Message = "La nueva contraseña debe tener mínimo 8 caracteres, mayúscula, minúscula y número."
            });
        }

        if (string.Equals(request.NewPassword, request.ConfirmPassword) is false)
        {
            return BadRequest(new { Message = "La confirmación de contraseña no coincide." });
        }

        if (!VerifyPassword(user, request.CurrentPassword).IsValid)
        {
            return BadRequest(new { Message = "La contraseña actual no es correcta." });
        }

        user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword!);
        user.PasswordChangedAt = DateTime.UtcNow;
        user.SecurityStamp = Guid.NewGuid();
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserName;

        var sessions = await _context.UserSessions
            .Where(x => x.AppUserId == user.Id && !x.RevokedAtUtc.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAtUtc = DateTime.UtcNow;
            session.RevokedReason = "PasswordChanged";
            session.UpdatedAt = DateTime.UtcNow;
            session.UpdatedBy = currentUserName;
        }

        await WriteAuditAsync(
            "PasswordChanged",
            "Success",
            user.Id,
            user.UserName,
            null,
            cancellationToken,
            saveChanges: false);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Contraseña actualizada. Debe iniciar sesión nuevamente."
        });
    }

    [HttpGet("permissions")]
    public IActionResult GetPermissions()
    {
        return Ok(PermissionCatalog.All);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var sessionId = User.GetSessionId();
        var session = sessionId.HasValue
            ? await _context.UserSessions.FirstOrDefaultAsync(
                x => x.Id == sessionId.Value,
                cancellationToken)
            : null;

        if (session is not null && !session.RevokedAtUtc.HasValue)
        {
            session.RevokedAtUtc = DateTime.UtcNow;
            session.RevokedReason = "Logout";
            session.UpdatedAt = DateTime.UtcNow;
            session.UpdatedBy = User.GetUserName();
        }

        await WriteAuditAsync(
            "Logout",
            "Success",
            User.GetUserId(),
            User.GetUserName(),
            null,
            cancellationToken,
            saveChanges: false);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("sessions/revoke-all")]
    [RequirePermission("Security.Users")]
    public async Task<IActionResult> RevokeAllSessions(
        [FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        var sessions = await _context.UserSessions
            .Where(x => x.AppUserId == userId && !x.RevokedAtUtc.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var session in sessions)
        {
            session.RevokedAtUtc = DateTime.UtcNow;
            session.RevokedReason = "AdministrativeRevocation";
            session.UpdatedAt = DateTime.UtcNow;
            session.UpdatedBy = User.GetUserName();
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(new { RevokedSessions = sessions.Count });
    }

    private PasswordCheckResult VerifyPassword(AppUser user, string? password)
    {
        if (string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            return PasswordCheckResult.Invalid;
        }

        if (IsLegacySha256Hash(user.PasswordHash))
        {
            var candidate = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(password.Trim())));
            var isValid = CryptographicOperations.FixedTimeEquals(
                Encoding.ASCII.GetBytes(candidate),
                Encoding.ASCII.GetBytes(user.PasswordHash.ToUpperInvariant()));

            return isValid
                ? PasswordCheckResult.ValidWithRehash
                : PasswordCheckResult.Invalid;
        }

        try
        {
            var result = _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password);

            return result switch
            {
                PasswordVerificationResult.Success => PasswordCheckResult.Valid,
                PasswordVerificationResult.SuccessRehashNeeded => PasswordCheckResult.ValidWithRehash,
                _ => PasswordCheckResult.Invalid
            };
        }
        catch (FormatException)
        {
            return PasswordCheckResult.Invalid;
        }
    }

    private static bool IsLegacySha256Hash(string hash) =>
        hash.Length == 64 && hash.All(Uri.IsHexDigit);

    private static bool IsPasswordPolicyValid(string? password) =>
        !string.IsNullOrWhiteSpace(password) &&
        password.Length >= 8 &&
        password.Any(char.IsUpper) &&
        password.Any(char.IsLower) &&
        password.Any(char.IsDigit);

    private async Task WriteAuditAsync(
        string action,
        string result,
        Guid? userId,
        string? userName,
        string? reason,
        CancellationToken cancellationToken,
        bool saveChanges = true)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            UserName = userName,
            BranchId = User.GetBranchId(),
            CompanyId = User.GetCompanyId(),
            Action = action,
            Module = "Authentication",
            EntityType = nameof(AppUser),
            EntityId = userId?.ToString(),
            Result = result,
            CorrelationId = HttpContext.TraceIdentifier,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Limit(Request.Headers.UserAgent.ToString(), 300),
            Reason = reason,
            CreatedBy = userName ?? "anonymous"
        });

        if (saveChanges)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private string? GetCurrentBearerToken()
    {
        var value = Request.Headers.Authorization.ToString();
        return value.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? value["Bearer ".Length..].Trim()
            : null;
    }

    private DateTime? GetCurrentTokenExpiration()
    {
        var value = User.FindFirst("exp")?.Value;
        return long.TryParse(value, out var seconds)
            ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
            : null;
    }

    private static string? Limit(string? value, int length) =>
        string.IsNullOrEmpty(value)
            ? value
            : value[..Math.Min(value.Length, length)];

    private sealed record PasswordCheckResult(bool IsValid, bool NeedsRehash)
    {
        public static readonly PasswordCheckResult Invalid = new(false, false);
        public static readonly PasswordCheckResult Valid = new(true, false);
        public static readonly PasswordCheckResult ValidWithRehash = new(true, true);
    }

    private static List<string> BuildPermissions(string roleCode, string? permissions)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var parsed = (permissions ?? string.Empty)
            .Split(new[] { ',', ';', '\n', '\r', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        foreach (var permission in parsed)
        {
            foreach (var normalized in NormalizePermission(permission))
            {
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    result.Add(normalized);
                }
            }
        }

        return result
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x)
            .ToList();
    }


    private static void ApplyRoleMatrixRestrictions(string roleCode, HashSet<string> permissions)
    {
        var code = roleCode.Trim().ToUpperInvariant();

        if (code is "TECNICO" or "TÉCNICO")
        {
            // Matriz aplicada:
            // Técnico = principalmente App Móvil.
            // No compra, no conciliación, no planes/mantenimiento formal,
            // no historial global, no configuración, no edición de activos.

            var blocked = new[]
            {
                "Dashboard.View",

                "Tools.Create",
                "Tools.Edit",
                "Tools.Delete",

                "AssetAvailability.View",
                "AssetAvailability.Edit",

                "AssetAssignment.View",
                "AssetAssignment.Assign",
                "AssetAssignment.Return",

                "TechnicalLifeRecord.Edit",
                "TechnicalLifeRecord.Export",

                "Documents.Download",
                "Documents.Delete",

                "Maintenance.View",
                "Maintenance.Request",
                "Maintenance.Execute",
                "Maintenance.Close",
                "Maintenance.Plans.View",
                "Maintenance.Plans.Manage",

                "Purchases.View",
                "Purchases.Request",
                "Purchases.Approve",
                "Purchases.Reject",

                "PhysicalCounts.View",
                "PhysicalCounts.Create",
                "PhysicalCounts.Close",

                "Reconciliation.View",
                "Reconciliation.Manage",

                "Reports.View",

                "Settings.View",
                "Settings.Manage",
                "Security.Users",
                "Security.Roles",

                "DeliveryAct.Generate",
                "FixedAssets.Disposal.Request",
                "FixedAssets.Disposal.Approve"
            };

            foreach (var permission in blocked)
            {
                permissions.Remove(permission);
            }

            permissions.Add("Tools.View");
            permissions.Add("AssetAssignment.History");
            permissions.Add("TechnicalLifeRecord.View");

            permissions.Add("Documents.View");
            permissions.Add("Documents.Upload");

            permissions.Add("Mobile.Access");
            permissions.Add("Mobile.Tools.View");
            permissions.Add("Mobile.Tools.Review");
            permissions.Add("Mobile.PreOperational.Report");
            permissions.Add("Mobile.Damage.Report");
            permissions.Add("Mobile.Loans.Request");
        }
    }

    private static void RemoveAdministrativePermissionsForNonAdmin(string roleCode, HashSet<string> permissions)
    {
        if (roleCode is "ADMIN" or "ADMINISTRADOR")
        {
            return;
        }

        permissions.Remove("Settings.View");
        permissions.Remove("Settings.Manage");
        permissions.Remove("Security.Users");
        permissions.Remove("Security.Roles");

        permissions.Remove("USERS.MANAGE");
        permissions.Remove("ROLES.MANAGE");
        permissions.Remove("SETTINGS.VIEW");
        permissions.Remove("SETTINGS.MANAGE");
        permissions.Remove("BRANCHES.MANAGE");
        permissions.Remove("WAREHOUSES.MANAGE");
        permissions.Remove("CATALOGS.MANAGE");
    }

    private static IEnumerable<string> NormalizePermission(string permission)
    {
        var value = permission.Trim();
        var code = value.ToUpperInvariant();

        if (PermissionCatalog.All.Any(x => string.Equals(x.Code, value, StringComparison.OrdinalIgnoreCase)))
        {
            yield return PermissionCatalog.All
                .First(x => string.Equals(x.Code, value, StringComparison.OrdinalIgnoreCase))
                .Code;

            yield break;
        }

        var mapped = code switch
        {
            "DASHBOARD.VIEW" => new[] { "Dashboard.View" },
            "DASHBOARD.EXECUTIVE" => new[] { "Dashboard.View" },
            "DASHBOARD.REFRESH" => new[] { "Dashboard.View" },

            "INVENTORY.VIEW" => new[] { "Tools.View" },
            "INVENTORY.CREATE" => new[] { "Tools.Create" },
            "INVENTORY.EDIT" => new[] { "Tools.Edit" },
            "INVENTORY.DELETE" => new[] { "Tools.Delete" },
            "INVENTORY.EXPORT" => new[] { "Tools.View", "Reports.View" },

            "LOCATION.VIEW" => new[] { "AssetAvailability.View" },
            "LOCATION.MANAGE" => new[] { "AssetAvailability.View", "AssetAvailability.Edit" },

            "ASSIGNMENT.VIEW" => new[] { "AssetAssignment.View" },
            "ASSIGNMENT.CREATE" => new[] { "AssetAssignment.View", "AssetAssignment.Assign" },
            "ASSIGNMENT.CLOSE" => new[] { "AssetAssignment.View", "AssetAssignment.Return" },
            "ASSIGNMENT.HISTORY" => new[] { "AssetAssignment.History" },

            "LIFERECORD.VIEW" => new[] { "TechnicalLifeRecord.View" },
            "LIFERECORD.EDIT" => new[] { "TechnicalLifeRecord.View", "TechnicalLifeRecord.Edit" },
            "LIFERECORD.EXPORT" => new[] { "TechnicalLifeRecord.View", "TechnicalLifeRecord.Export" },
            "LIFERECORD.EVENTS" => new[] { "TechnicalLifeRecord.View" },

            "MAINTENANCE.VIEW" => new[] { "Maintenance.View" },
            "MAINTENANCE.REQUEST" => new[] { "Maintenance.View", "Maintenance.Request" },
            "MAINTENANCE.SCHEDULE" => new[] { "Maintenance.View", "Maintenance.Request" },
            "MAINTENANCE.EXECUTE" => new[] { "Maintenance.View", "Maintenance.Execute" },
            "MAINTENANCE.APPROVE" => new[] { "Maintenance.View", "Maintenance.Close" },

            "LOAN.VIEW" => new[] { "AssetAssignment.View", "AssetAssignment.History" },
            "LOAN.CREATE" => new[] { "Mobile.Loans.Request" },
            "LOAN.DELIVER" => new[] { "AssetAssignment.View", "AssetAssignment.Assign" },
            "LOAN.RETURN" => new[] { "AssetAssignment.View", "AssetAssignment.Return" },
            "LOAN.APPROVE" => new[] { "AssetAssignment.View", "AssetAssignment.Assign" },

            "DAMAGE.VIEW" => new[] { "Mobile.Damage.Report" },
            "DAMAGE.REPORT" => new[] { "Mobile.Damage.Report" },
            "DAMAGE.EDIT" => new[] { "Maintenance.View", "Maintenance.Execute" },
            "DAMAGE.CLOSE" => new[] { "Maintenance.View", "Maintenance.Close" },

            "PHYSICALCOUNT.VIEW" => new[] { "PhysicalCounts.View" },
            "PHYSICALCOUNT.CREATE" => new[] { "PhysicalCounts.View", "PhysicalCounts.Create" },
            "PHYSICALCOUNT.EXECUTE" => new[] { "Mobile.Tools.Review" },
            "PHYSICALCOUNT.CLOSE" => new[] { "PhysicalCounts.View", "PhysicalCounts.Close" },

            "DOCUMENT.VIEW" => new[] { "Documents.View" },
            "DOCUMENT.UPLOAD" => new[] { "Documents.View", "Documents.Upload" },
            "DOCUMENT.DOWNLOAD" => new[] { "Documents.View", "Documents.Download" },
            "DOCUMENT.DELETE" => new[] { "Documents.View", "Documents.Delete" },

            "PURCHASE.VIEW" => new[] { "Purchases.View" },
            "PURCHASE.REQUEST" => new[] { "Purchases.View", "Purchases.Request" },
            "PURCHASE.APPROVE" => new[] { "Purchases.View", "Purchases.Approve" },
            "PURCHASE.REJECT" => new[] { "Purchases.View", "Purchases.Reject" },

            "REPORT.VIEW" => new[] { "Reports.View" },
            "REPORT.EXPORT" => new[] { "Reports.View" },
            "AUDIT.VIEW" => new[] { "Reports.View" },

            "RECONCILIATION.VIEW" => new[] { "Reconciliation.View" },
            "RECONCILIATION.MANAGE" => new[] { "Reconciliation.View", "Reconciliation.Manage" },

            "SETTINGS.VIEW" => new[] { "Settings.View" },
            "SETTINGS.MANAGE" => new[] { "Settings.View", "Settings.Manage" },
            "USERS.MANAGE" => new[] { "Security.Users" },
            "ROLES.MANAGE" => new[] { "Security.Roles" },
            "BRANCHES.MANAGE" => new[] { "Settings.View", "Settings.Manage" },
            "WAREHOUSES.MANAGE" => new[] { "Settings.View", "Settings.Manage" },
            "CATALOGS.MANAGE" => new[] { "Settings.View", "Settings.Manage" },

            _ => new[] { value }
        };

        foreach (var item in mapped)
        {
            yield return item;
        }
    }


    private string? GetCurrentMobileUserName()
    {
        return User.GetUserName();
    }

    private static List<string> GetDefaultPermissionsByRole(string roleCode)
    {
        var code = roleCode.Trim().ToUpperInvariant();

        if (code is "ADMIN" or "ADMINISTRADOR")
        {
            return PermissionCatalog.All.Select(x => x.Code).ToList();
        }

        if (code is "GERENCIAL" or "GERENCIA" or "AUDITOR" or "AUDITORIA")
        {
            return new()
            {
                "Dashboard.View",
                "Tools.View",
                "AssetAvailability.View",
                "AssetAssignment.View",
                "AssetAssignment.History",
                "TechnicalLifeRecord.View",
                "TechnicalLifeRecord.Export",
                "Documents.View",
                "Documents.Download",
                "Maintenance.View",
                "Maintenance.Close",
                "Purchases.View",
                "Purchases.Approve",
                "Purchases.Reject",
                "PhysicalCounts.View",
                "Reports.View"
            };
        }

        if (code is "HERRAMIENTAS" or "HERRAMENTERO" or "HERRAMIENTA" or "HERRAMIENTERO")
        {
            return new()
            {
                "Dashboard.View",
                "Tools.View",
                "Tools.Create",
                "Tools.Edit",
                "AssetAvailability.View",
                "AssetAvailability.Edit",
                "AssetAssignment.View",
                "AssetAssignment.Assign",
                "AssetAssignment.Return",
                "AssetAssignment.History",
                "TechnicalLifeRecord.View",
                "TechnicalLifeRecord.Edit",
                "TechnicalLifeRecord.Export",
                "Documents.View",
                "Documents.Upload",
                "Documents.Download",
                "Maintenance.View",
                "Maintenance.Request",
                "Purchases.View",
                "Purchases.Request",
                "PhysicalCounts.View",
                "PhysicalCounts.Create",
                "Reports.View",
                "Mobile.Access",
                "Mobile.Tools.View",
                "Mobile.Tools.Review",
                "Mobile.PreOperational.Report",
                "Mobile.Damage.Report",
                "DeliveryAct.Generate",
                "FixedAssets.Disposal.Request"
            };
        }

        if (code is "ING_SERVICIOS" or "INGENIERO_SERVICIOS" or "ING_SERVICIO" or "INGENIERIA_SERVICIOS")
        {
            return new()
            {
                "Dashboard.View",
                "Tools.View",
                "AssetAvailability.View",
                "AssetAssignment.View",
                "AssetAssignment.History",
                "TechnicalLifeRecord.View",
                "TechnicalLifeRecord.Export",
                "Documents.View",
                "Documents.Download",
                "Maintenance.View",
                "Maintenance.Close",
                "Purchases.View",
                "Purchases.Approve",
                "Purchases.Reject",
                "PhysicalCounts.View",
                "Reports.View",
                "Mobile.Access",
                "Mobile.Tools.View",
                "Mobile.Tools.Review",
                "Mobile.PreOperational.Report",
                "Mobile.Damage.Report",
                "FixedAssets.Disposal.Approve"
            };
        }

        if (code is "COORDINADOR_TALLER" or "COORDINADOR_DE_TALLER" or "COORD_TALLER" or "SEDE" or "RESPONSABLE_SEDE")
        {
            return new()
            {
                "Dashboard.View",
                "Tools.View",
                "Tools.Create",
                "Tools.Edit",
                "AssetAvailability.View",
                "AssetAvailability.Edit",
                "AssetAssignment.View",
                "AssetAssignment.Assign",
                "AssetAssignment.Return",
                "AssetAssignment.History",
                "TechnicalLifeRecord.View",
                "TechnicalLifeRecord.Edit",
                "TechnicalLifeRecord.Export",
                "Documents.View",
                "Documents.Upload",
                "Documents.Download",
                "Documents.Delete",
                "Maintenance.View",
                "Maintenance.Request",
                "Maintenance.Execute",
                "Maintenance.Close",
                "Purchases.View",
                "Purchases.Request",
                "Purchases.Approve",
                "Purchases.Reject",
                "PhysicalCounts.View",
                "PhysicalCounts.Create",
                "PhysicalCounts.Close",
                "Reports.View",
                "Mobile.Access",
                "Mobile.Tools.View",
                "Mobile.Tools.Review",
                "Mobile.PreOperational.Report",
                "Mobile.Damage.Report",
                "DeliveryAct.Generate",
                "FixedAssets.Disposal.Request",
                "FixedAssets.Disposal.Approve"
            };
        }

        if (code is "TECNICO" or "TÉCNICO")
        {
            return new()
            {
                "Tools.View",
                "AssetAssignment.History",
                "TechnicalLifeRecord.View",
                "Documents.View",
                "Documents.Upload",
                "Mobile.Access",
                "Mobile.Tools.View",
                "Mobile.Tools.Review",
                "Mobile.PreOperational.Report",
                "Mobile.Damage.Report",
                "Mobile.Loans.Request"
            };
        }

        return new()
        {
            "Dashboard.View"
        };
    }
}

public sealed class LoginRequest
{
    public string? UserName { get; set; }

    public string? Password { get; set; }
}


public sealed class MobileProfileUpdateRequest
{
    public string? DocumentNumber { get; set; }
    public string? EmployeeCode { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Position { get; set; }
    public string? Area { get; set; }
}

public sealed class MobilePasswordChangeRequest
{
    public string? CurrentPassword { get; set; }
    public string? NewPassword { get; set; }
    public string? ConfirmPassword { get; set; }
}

public sealed class LoginResponse
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? DocumentNumber { get; set; }

    public string? EmployeeCode { get; set; }

    public string? Position { get; set; }

    public string? Area { get; set; }

    public Guid RoleId { get; set; }

    public string RoleCode { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public Guid? BranchId { get; set; }

    public string BranchCode { get; set; } = "TODAS";

    public string BranchName { get; set; } = "Todas las sedes";

    public Guid? ResponsiblePersonId { get; set; }

    public string? ResponsiblePersonName { get; set; }

    public List<string> Permissions { get; set; } = new();

    public DateTime? LastLoginAt { get; set; }

    public string? AccessToken { get; set; }

    public DateTime? TokenExpiresAtUtc { get; set; }

    public Guid? SessionId { get; set; }
}
