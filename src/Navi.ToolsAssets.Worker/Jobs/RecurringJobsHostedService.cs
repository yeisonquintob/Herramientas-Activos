using Hangfire;

namespace Navi.ToolsAssets.Worker.Jobs;

public class RecurringJobsHostedService : IHostedService
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<RecurringJobsHostedService> _logger;

    public RecurringJobsHostedService(
        IRecurringJobManager recurringJobManager,
        ILogger<RecurringJobsHostedService> logger)
    {
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Registrando jobs recurrentes de NAVI Herramientas.");

        _recurringJobManager.AddOrUpdate<HealthCheckJob>(
            "navi-tools-health-check",
            job => job.ExecuteAsync(),
            Cron.Hourly);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deteniendo registro de jobs recurrentes de NAVI Herramientas.");
        return Task.CompletedTask;
    }
}
