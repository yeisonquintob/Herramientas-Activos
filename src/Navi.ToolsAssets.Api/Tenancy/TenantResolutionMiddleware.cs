using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Application.Tenancy;

namespace Navi.ToolsAssets.Api.Tenancy;

public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        TenantContext tenantContext,
        ITenantAccessValidator accessValidator,
        IOptions<TenancyOptions> options)
    {
        if (!options.Value.Enabled ||
            httpContext.User.Identity?.IsAuthenticated != true ||
            IsTenantNeutralPath(httpContext.Request.Path))
        {
            await _next(httpContext);
            return;
        }

        var userId = httpContext.User.GetUserId();
        var companyId = httpContext.User.GetCompanyId();

        if (!userId.HasValue || !companyId.HasValue)
        {
            await WriteProblemAsync(
                httpContext,
                StatusCodes.Status409Conflict,
                "Compañía activa requerida",
                "Seleccione una compañía autorizada antes de consultar información operativa.",
                "ActiveCompanyRequired");
            return;
        }

        var resolution = await accessValidator.ResolveAsync(
            userId.Value,
            companyId.Value,
            httpContext.RequestAborted);

        if (!resolution.IsAllowed ||
            string.IsNullOrWhiteSpace(resolution.CompanyCode) ||
            string.IsNullOrWhiteSpace(resolution.ConnectionString))
        {
            await WriteProblemAsync(
                httpContext,
                StatusCodes.Status403Forbidden,
                "Acceso de compañía denegado",
                "La compañía no está disponible o el usuario no tiene acceso.",
                resolution.ErrorCode ?? "CompanyAccessDenied");
            return;
        }

        tenantContext.Resolve(
            companyId.Value,
            resolution.CompanyCode,
            resolution.ConnectionString);

        await _next(httpContext);
    }

    private static bool IsTenantNeutralPath(PathString path) =>
        path.StartsWithSegments("/api/auth") ||
        path.StartsWithSegments("/api/companies") ||
        path.StartsWithSegments("/health") ||
        path == "/";

    private static Task WriteProblemAsync(
        HttpContext context,
        int status,
        string title,
        string detail,
        string errorCode)
    {
        context.Response.StatusCode = status;

        return context.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Extensions =
                {
                    ["errorCode"] = errorCode,
                    ["traceId"] = context.TraceIdentifier
                }
            },
            context.RequestAborted);
    }
}
