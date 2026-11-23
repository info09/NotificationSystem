using System.Diagnostics;
using BuildingBlocks.Observability.OpenTelemetry;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Worker;

/// <summary>
/// Worker tạm thời dùng để giữ host nền hoạt động và phát tín hiệu heartbeat.
/// </summary>
/// <param name="logger">Logger ghi nhật ký hoạt động worker.</param>
public sealed class PlaceholderWorker(ILogger<PlaceholderWorker> logger) : BackgroundService
{
    private static readonly ActivitySource ActivitySource = new(OpenTelemetryHostExtensions.WorkerActivitySourceName);

    /// <summary>
    /// Thực thi vòng lặp nền của worker cho đến khi bị hủy.
    /// </summary>
    /// <param name="stoppingToken">Token báo dừng worker.</param>
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
