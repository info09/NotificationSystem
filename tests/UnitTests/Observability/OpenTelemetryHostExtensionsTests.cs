using BuildingBlocks.Observability.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

namespace UnitTests.Observability;

public sealed class OpenTelemetryHostExtensionsTests
{
    [Fact]
    public void AddOpenTelemetryObservability_WhenEnabled_RegistersTracerProvider()
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = "Development"
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OpenTelemetry:Enabled"] = "true",
            ["OpenTelemetry:Tracing:Enabled"] = "true",
            ["OpenTelemetry:Metrics:Enabled"] = "true",
            ["OpenTelemetry:Exporter:Console:Enabled"] = "false"
        });

        builder.AddOpenTelemetryObservability(instrumentAspNetCore: false);

        using var host = builder.Build();
        using var scope = host.Services.CreateScope();

        var tracerProvider = scope.ServiceProvider.GetService<TracerProvider>();

        Assert.NotNull(tracerProvider);
    }

    [Fact]
    public void AddOpenTelemetryObservability_WhenDisabled_DoesNotRegisterTracerProvider()
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = "Development"
        });

        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["OpenTelemetry:Enabled"] = "false"
        });

        builder.AddOpenTelemetryObservability(instrumentAspNetCore: false);

        using var host = builder.Build();
        using var scope = host.Services.CreateScope();

        var tracerProvider = scope.ServiceProvider.GetService<TracerProvider>();

        Assert.Null(tracerProvider);
    }
}
