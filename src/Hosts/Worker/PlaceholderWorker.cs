using System.Diagnostics;
using BuildingBlocks.Observability.OpenTelemetry;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WorkerHost;

public sealed class PlaceholderWorker(ILogger<PlaceholderWorker> logger) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new(OpenTelemetryHostExtensions.WorkerActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Worker host started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var activity = ActivitySource.StartActivity("worker.heartbeat", ActivityKind.Internal);
            activity?.SetTag("worker.component", "placeholder");

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
