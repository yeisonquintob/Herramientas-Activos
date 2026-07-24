using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Navi.ToolsAssets.Shared.Security;

namespace Navi.ToolsAssets.Api.Security;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public sealed class RequirePermissionAttribute : Attribute, IActionFilter
{
    private readonly string[] _permissions;

    public RequirePermissionAttribute(params string[] permissions)
    {
        _permissions = permissions ?? Array.Empty<string>();
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (_permissions.Length == 0)
        {
            return;
        }

        var user = context.HttpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                Message = "Debe iniciar sesión para ejecutar esta acción."
            });
            return;
        }

        var roleCode = user.GetRoleCode() ?? string.Empty;

        if (string.Equals(roleCode, "ADMIN", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var permissions = user.Claims
            .Where(x => x.Type == NaviClaimTypes.Permission)
            .Select(x => x.Value)
            .ToArray();

        var isAllowed = _permissions.Any(required =>
            permissions.Any(current =>
                string.Equals(current, required, StringComparison.OrdinalIgnoreCase)));

        if (!isAllowed)
        {
            context.Result = new ObjectResult(new
            {
                Message = "No tienes permiso para ejecutar esta acción.",
                RequiredPermissions = _permissions,
                CurrentRoleCode = roleCode,
                CurrentPermissions = permissions
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
    }
}
