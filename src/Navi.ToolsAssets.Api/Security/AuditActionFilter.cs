using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Filters;
using Navi.ToolsAssets.Domain.Entities.Security;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Security;

public sealed class AuditActionFilter : IAsyncActionFilter
{
    private static readonly HashSet<string> ReadMethods =
        new(StringComparer.OrdinalIgnoreCase) { "GET", "HEAD", "OPTIONS" };

    private readonly NaviToolsAssetsDbContext _context;
    private readonly ILogger<AuditActionFilter> _logger;

    public AuditActionFilter(
        NaviToolsAssetsDbContext context,
        ILogger<AuditActionFilter> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var executed = await next();

        if (ReadMethods.Contains(context.HttpContext.Request.Method) ||
            context.Controller is Controllers.AuthController)
        {
            return;
        }

        try
        {
            var route = context.ActionDescriptor.RouteValues;
            var user = context.HttpContext.User;
            var statusCode = executed.HttpContext.Response.StatusCode;
            var safeMetadata = context.ActionArguments
                .Where(x => !IsSensitiveArgument(x.Key))
                .ToDictionary(
                    x => x.Key,
                    x => x.Value is null ? null : x.Value.GetType().Name);

            _context.AuditLogs.Add(new AuditLog
            {
                UserId = user.GetUserId(),
                UserName = user.GetUserName(),
                CompanyId = user.GetCompanyId(),
                BranchId = user.GetBranchId(),
                Action = GetRouteValue(route, "action") ?? context.HttpContext.Request.Method,
                Module = GetRouteValue(route, "controller") ?? "Api",
                EntityType = GetRouteValue(route, "controller"),
                EntityId = context.RouteData.Values.TryGetValue("id", out var id) ? id?.ToString() : null,
                Result = statusCode < 400 ? "Success" : "Failure",
                CorrelationId = context.HttpContext.TraceIdentifier,
                IpAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString(),
                UserAgent = Limit(context.HttpContext.Request.Headers.UserAgent.ToString(), 300),
                MetadataJson = JsonSerializer.Serialize(safeMetadata),
                ErrorCode = statusCode >= 400 ? statusCode.ToString() : null,
                CreatedBy = user.GetUserName() ?? "anonymous"
            });

            await _context.SaveChangesAsync(context.HttpContext.RequestAborted);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No fue posible persistir la auditoría de {Method} {Path}.",
                context.HttpContext.Request.Method,
                context.HttpContext.Request.Path);
        }
    }

    private static bool IsSensitiveArgument(string name) =>
        name.Contains("password", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("secret", StringComparison.OrdinalIgnoreCase);

    private static string? Limit(string? value, int length) =>
        string.IsNullOrEmpty(value)
            ? value
            : value[..Math.Min(value.Length, length)];

    private static string? GetRouteValue(
        IDictionary<string, string?> routeValues,
        string key) =>
        routeValues.TryGetValue(key, out var value)
            ? value
            : null;
}
