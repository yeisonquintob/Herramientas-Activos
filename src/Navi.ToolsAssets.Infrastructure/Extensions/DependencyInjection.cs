using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Navi.ToolsAssets.Application.Documents;
using Navi.ToolsAssets.Application.Tenancy;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;
using Navi.ToolsAssets.Infrastructure.Storage;
using Navi.ToolsAssets.Infrastructure.Tenancy;

namespace Navi.ToolsAssets.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var defaultConnectionString = configuration.GetConnectionString("NaviToolsAssetsDb")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'NaviToolsAssetsDb'.");
        var masterConnectionString = configuration.GetConnectionString("NaviMasterDb")
            ?? defaultConnectionString;

        services.AddScoped<TenantContext>();
        services.AddScoped<ICurrentTenant>(provider =>
            provider.GetRequiredService<TenantContext>());

        services.AddDbContext<NaviToolsAssetsDbContext>((provider, options) =>
        {
            var tenant = provider.GetRequiredService<ICurrentTenant>();
            options.UseSqlServer(tenant.ConnectionString ?? defaultConnectionString);
        });

        services.AddDbContext<NaviSecurityDbContext>(options =>
            options.UseSqlServer(defaultConnectionString));

        services.AddDbContext<NaviMasterDbContext>(options =>
            options.UseSqlServer(masterConnectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "Master")));

        services.AddScoped<ITenantAccessValidator, TenantAccessValidator>();
        services.AddScoped<ITenantDbContextFactory, TenantDbContextFactory>();
        services.AddScoped<IDocumentStorageService, MinioDocumentStorageService>();

        return services;
    }
}
