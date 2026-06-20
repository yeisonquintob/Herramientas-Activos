using Navi.ToolsAssets.Infrastructure.Storage;
using Navi.ToolsAssets.Application.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("NaviToolsAssetsDb")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'NaviToolsAssetsDb'.");

        services.AddDbContext<NaviToolsAssetsDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IDocumentStorageService, MinioDocumentStorageService>();

        return services;
    }
}

