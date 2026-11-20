using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.HealthChecks;

public sealed class RabbitMqTcpHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var host = configuration["RABBITMQ_HOST"] ?? configuration["RabbitMq:Host"] ?? "localhost";
        var portValue = configuration["RABBITMQ_PORT"] ?? configuration["RabbitMq:Port"];
        if (!int.TryParse(portValue, out var port))
        {
            port = 5672;
        }

        try
        {
            using var tcpClient = new TcpClient();
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(3));

            await tcpClient.ConnectAsync(host, port, timeoutCts.Token);

            return HealthCheckResult.Healthy($"RabbitMQ is reachable at {host}:{port}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"RabbitMQ is unreachable at {host}:{port}.", ex);
        }
    }
}
