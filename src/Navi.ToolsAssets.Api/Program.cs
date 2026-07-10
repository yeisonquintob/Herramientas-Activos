using Hangfire;
using Hangfire.SqlServer;
using Navi.ToolsAssets.Infrastructure.Extensions;
using Navi.ToolsAssets.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();

var defaultCorsOrigins = new[]
{
    "http://localhost:5285",
    "https://localhost:7285",
    "http://localhost:5264",
    "https://localhost:7264"
};

var configuredCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .GetChildren()
    .Select(x => x.Value)
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .Select(x => x!.Trim())
    .ToArray();

var corsOrigins = configuredCorsOrigins.Length > 0
    ? configuredCorsOrigins
    : defaultCorsOrigins;

var allowLocalDevelopmentOrigins = builder.Environment.IsDevelopment();

builder.Services.AddCors(options =>
{
    options.AddPolicy("NaviMobileCors", policy =>
    {
        policy
            .SetIsOriginAllowed(origin => IsAllowedCorsOrigin(origin, corsOrigins, allowLocalDevelopmentOrigins))
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddInfrastructure(builder.Configuration);

var connectionString = builder.Configuration.GetConnectionString("NaviToolsAssetsDb")
    ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'NaviToolsAssetsDb'.");

builder.Services.AddHangfire(configuration =>
{
    configuration
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.FromSeconds(15),
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true,
            SchemaName = "HangFire",
            PrepareSchemaIfNecessary = true
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDatabaseAsync();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHangfireDashboard("/hangfire");
}

app.UseCors("NaviMobileCors");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Ok(new
{
    App = "NAVI Herramientas API",
    Status = "Running",
    Database = "NaviToolsAssetsDb",
    Seed = "AGU / Zona Antioquia / Catálogos base",
    Hangfire = "/hangfire"
}));

app.Run();

static bool IsAllowedCorsOrigin(string? origin, string[] allowedOrigins, bool allowLocalDevelopmentOrigins)
{
    if (string.IsNullOrWhiteSpace(origin))
    {
        return false;
    }

    if (allowedOrigins.Any(x => string.Equals(x, origin, StringComparison.OrdinalIgnoreCase)))
    {
        return true;
    }

    if (!allowLocalDevelopmentOrigins)
    {
        return false;
    }

    if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
    {
        return false;
    }

    if (uri.Scheme is not ("http" or "https"))
    {
        return false;
    }

    return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(uri.Host, "127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
           string.Equals(uri.Host, "::1", StringComparison.OrdinalIgnoreCase);
}
