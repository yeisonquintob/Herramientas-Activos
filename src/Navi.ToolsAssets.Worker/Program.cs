using Hangfire;
using Hangfire.SqlServer;
using Navi.ToolsAssets.Worker.Jobs;

var builder = Host.CreateApplicationBuilder(args);

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

builder.Services.AddHangfireServer(options =>
{
    options.ServerName = "navi-tools-worker";
    options.Queues = new[] { "default", "imports", "sync", "maintenance", "notifications" };
});

builder.Services.AddTransient<HealthCheckJob>();
builder.Services.AddHostedService<RecurringJobsHostedService>();

var app = builder.Build();

app.Run();
