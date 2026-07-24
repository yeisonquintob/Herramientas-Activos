using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Navi.ToolsAssets.Infrastructure.Persistence.Context;

/// <summary>
/// Creates the operational context for EF tooling without starting the API,
/// Hangfire, MinIO, or any other runtime integration.
/// </summary>
public sealed class NaviToolsAssetsDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<NaviToolsAssetsDbContext>
{
    private const string ConnectionVariable = "NAVI_TOOLS_DB_CONNECTION";

    public NaviToolsAssetsDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Configure {ConnectionVariable} para ejecutar herramientas de Entity Framework.");
        }

        var options = new DbContextOptionsBuilder<NaviToolsAssetsDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new NaviToolsAssetsDbContext(options);
    }
}
