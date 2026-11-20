using System.Net;
using System.Net.Sockets;
using Api.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UnitTests.HealthChecks;

public sealed class RabbitMqTcpHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_WhenRabbitMqEndpointIsReachable_ReturnsHealthy()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RABBITMQ_HOST"] = "127.0.0.1",
                ["RABBITMQ_PORT"] = port.ToString()
            })
            .Build();

        var healthCheck = new RabbitMqTcpHealthCheck(configuration);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_WhenRabbitMqEndpointIsUnreachable_ReturnsUnhealthy()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["RABBITMQ_HOST"] = "127.0.0.1",
                ["RABBITMQ_PORT"] = port.ToString()
            })
            .Build();

        var healthCheck = new RabbitMqTcpHealthCheck(configuration);

        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
    }
}
