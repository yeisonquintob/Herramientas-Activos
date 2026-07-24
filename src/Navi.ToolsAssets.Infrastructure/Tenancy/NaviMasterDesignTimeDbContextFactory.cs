using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Navi.ToolsAssets.Infrastructure.Tenancy;

public sealed class NaviMasterDesignTimeDbContextFactory
    : IDesignTimeDbContextFactory<NaviMasterDbContext>
{
    private const string ConnectionVariable = "NAVI_MASTER_DB_CONNECTION";

    public NaviMasterDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Configure {ConnectionVariable} para administrar migraciones de NaviMasterDb.");
        }

        var options = new DbContextOptionsBuilder<NaviMasterDbContext>()
            .UseSqlServer(connectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "Master"))
            .Options;

        return new NaviMasterDbContext(options);
    }
}
