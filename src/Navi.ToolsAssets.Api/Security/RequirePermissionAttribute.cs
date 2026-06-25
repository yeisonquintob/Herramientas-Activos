using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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

        var headers = context.HttpContext.Request.Headers;

        var roleCode = headers.TryGetValue("X-Navi-Role-Code", out var roleCodeValue)
            ? roleCodeValue.ToString()
            : string.Empty;

        var roleName = headers.TryGetValue("X-Navi-Role", out var roleNameValue)
            ? roleNameValue.ToString()
            : string.Empty;

        if (string.Equals(roleCode, "ADMIN", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(roleName, "Administrador", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var permissionsHeader = headers.TryGetValue("X-Navi-Permissions", out var value)
            ? value.ToString()
            : string.Empty;

        var permissions = permissionsHeader
            .Split(new[] { ',', ';', '|', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
                CurrentRoleName = roleName,
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
