namespace Navi.ToolsAssets.Worker.Jobs;

public class HealthCheckJob
{
    private readonly ILogger<HealthCheckJob> _logger;

    public HealthCheckJob(ILogger<HealthCheckJob> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync()
    {
        _logger.LogInformation("NAVI Herramientas Worker activo. Fecha: {Date}", DateTimeOffset.Now);
        return Task.CompletedTask;
    }
}
