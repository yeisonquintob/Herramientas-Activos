using System.Text.Json;
using Microsoft.JSInterop;
using Navi.ToolsAssets.MobilePwa.Models;

namespace Navi.ToolsAssets.MobilePwa.Services;

public sealed class MobileAuthSessionService
{
    private const string StorageKey = "navi-mobile-auth-session";
    private readonly IJSRuntime _js;
    private bool _initialized;

    public MobileAuthSessionService(IJSRuntime js)
    {
        _js = js;
    }

    public MobileUser? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public string AuditUser => CurrentUser?.UserName ?? "mobile";

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _initialized = true;

        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);

            if (!string.IsNullOrWhiteSpace(json))
            {
                CurrentUser = JsonSerializer.Deserialize<MobileUser>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
        }
        catch
        {
            CurrentUser = null;
            await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
    }

    public async Task LoginAsync(MobileUser user)
    {
        CurrentUser = user;

        var json = JsonSerializer.Serialize(user);

        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
    }

    public async Task LogoutAsync()
    {
        CurrentUser = null;
        await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey);
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
        {
            return false;
        }

        var permissions = CurrentUser?.Permissions ?? new();

        return permissions.Any(x => string.Equals(x, "ALL", StringComparison.OrdinalIgnoreCase))
            || permissions.Any(x => string.Equals(x, permission, StringComparison.OrdinalIgnoreCase));
    }













    public bool HasAnyPermission(params string[] permissions)
    {
        return permissions.Any(HasPermission);
    }













    public bool CanAccessMobile()
    {
        return HasPermission("Mobile.Access");
    }


    public bool IsAdmin()
    {
        var role = NormalizeRole(CurrentUser?.RoleCode);

        return role is "ADMIN" or "SUPERADMIN";
    }





    public bool IsTecnico()
    {
        var role = NormalizeRole(CurrentUser?.RoleCode);

        return role is "TECNICO" or "TÉCNICO";
    }

    public bool IsAuditor()
    {
        var role = NormalizeRole(CurrentUser?.RoleCode);

        return role is "AUDITOR" or "AUDITORIA";
    }

    public bool IsGerencial()
    {
        var role = NormalizeRole(CurrentUser?.RoleCode);

        return role is "GERENCIAL" or "GERENCIA";
    }

    public bool IsHerramientero()
    {
        var role = NormalizeRole(CurrentUser?.RoleCode);

        return role is "HERRAMIENTAS"
            or "HERRAMENTERO"
            or "HERRAMIENTERO"
            or "HERRAMIENTA";
    }

    public bool IsSedeOrCoordinator()
    {
        var role = NormalizeRole(CurrentUser?.RoleCode);

        return role is "SEDE"
            or "RESPONSABLE_SEDE"
            or "COORDINADOR_TALLER"
            or "COORDINADOR_DE_TALLER"
            or "COORD_TALLER";
    }

    public bool IsIngenieroServicios()
    {
        var role = NormalizeRole(CurrentUser?.RoleCode);

        return role is "ING_SERVICIOS"
            or "INGENIERO_SERVICIOS"
            or "ING_SERVICIO"
            or "INGENIERIA_SERVICIOS";
    }

    public bool CanSeeAllTools()
    {
        return IsAdmin()
            || IsAuditor()
            || IsGerencial()
            || IsHerramientero();
    }

    public bool CanSeeBranchTools()
    {
        return IsSedeOrCoordinator()
            || IsHerramientero()
            || IsIngenieroServicios();
    }

    public bool CanUseTechnicalMenu()
    {
        return IsTecnico();
    }

    private static string NormalizeRole(string? roleCode)
    {
        return string.IsNullOrWhiteSpace(roleCode)
            ? string.Empty
            : roleCode.Trim().ToUpperInvariant();
    }
}
