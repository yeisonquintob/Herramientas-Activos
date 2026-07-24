using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Infrastructure.Tenancy;

public interface ITenantDbContextFactory
{
    NaviToolsAssetsDbContext Create(string connectionString);
}

public sealed class TenantDbContextFactory : ITenantDbContextFactory
{
    public NaviToolsAssetsDbContext Create(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("La conexión de compañía es obligatoria.", nameof(connectionString));
        }

        var options = new DbContextOptionsBuilder<NaviToolsAssetsDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new NaviToolsAssetsDbContext(options);
    }
}
