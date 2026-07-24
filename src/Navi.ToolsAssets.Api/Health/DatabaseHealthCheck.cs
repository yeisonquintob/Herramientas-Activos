using Microsoft.Extensions.Diagnostics.HealthChecks;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Health;

/// <summary>
/// Readiness probe for the operational/security database selected by the
/// current deployment. It does not create schemas or apply migrations.
/// </summary>
public sealed class DatabaseHealthCheck : IHealthCheck
{
    private readonly NaviToolsAssetsDbContext _context;

    public DatabaseHealthCheck(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Operational database is reachable.")
                : HealthCheckResult.Unhealthy("Operational database is not reachable.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Operational database readiness check failed.",
                exception);
        }
    }
}
