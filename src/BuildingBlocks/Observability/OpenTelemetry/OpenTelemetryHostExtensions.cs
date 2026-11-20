using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace BuildingBlocks.Observability.OpenTelemetry;

public static class OpenTelemetryHostExtensions
{
    public const string WorkerActivitySourceName = "NotificationSystem.Worker";
    public const string MessagingActivitySourceName = "NotificationSystem.Messaging";

    public static IHostApplicationBuilder AddOpenTelemetryObservability(
        this IHostApplicationBuilder builder,
        bool instrumentAspNetCore = false)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (!IsEnabled(builder.Configuration, "OpenTelemetry:Enabled", defaultValue: true))
        {
            return builder;
        }

        var serviceName = builder.Configuration["OpenTelemetry:ServiceName"];
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            serviceName = builder.Environment.ApplicationName;
        }

        var serviceVersion = typeof(OpenTelemetryHostExtensions).Assembly.GetName().Version?.ToString();
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(serviceName, serviceVersion: serviceVersion)
            .AddAttributes(
            [
                new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)
            ]);

        var otelBuilder = builder.Services.AddOpenTelemetry().ConfigureResource(resource => resource
            .AddService(serviceName, serviceVersion: serviceVersion)
            .AddAttributes(
            [
                new KeyValuePair<string, object>("deployment.environment", builder.Environment.EnvironmentName)
            ]));

        if (IsEnabled(builder.Configuration, "OpenTelemetry:Tracing:Enabled", defaultValue: true))
        {
            otelBuilder.WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(WorkerActivitySourceName)
                    .AddSource(MessagingActivitySourceName)
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.SetDbStatementForText = builder.Environment.IsDevelopment();
                    });

                if (instrumentAspNetCore)
                {
                    tracing.AddAspNetCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                    });
                }

                ConfigureTracingExporters(tracing, builder.Configuration);
            });
        }

        if (IsEnabled(builder.Configuration, "OpenTelemetry:Metrics:Enabled", defaultValue: true))
        {
            otelBuilder.WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddRuntimeInstrumentation()
                    .AddHttpClientInstrumentation();

                if (instrumentAspNetCore)
                {
                    metrics.AddAspNetCoreInstrumentation();
                }

                ConfigureMetricsExporters(metrics, builder.Configuration);
            });
        }

        return builder;
    }

    private static void ConfigureTracingExporters(TracerProviderBuilder tracing, IConfiguration configuration)
    {
        var otlpEndpoint = configuration["OpenTelemetry:Exporter:Otlp:Endpoint"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(options => ConfigureOtlpExporter(options, configuration, otlpEndpoint));
        }

        if (IsEnabled(configuration, "OpenTelemetry:Exporter:Console:Enabled", defaultValue: false))
        {
            tracing.AddConsoleExporter();
        }
    }

    private static void ConfigureMetricsExporters(MeterProviderBuilder metrics, IConfiguration configuration)
    {
        var otlpEndpoint = configuration["OpenTelemetry:Exporter:Otlp:Endpoint"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(options => ConfigureOtlpExporter(options, configuration, otlpEndpoint));
        }

        if (IsEnabled(configuration, "OpenTelemetry:Exporter:Console:Enabled", defaultValue: false))
        {
            metrics.AddConsoleExporter();
        }
    }

    private static void ConfigureOtlpExporter(OtlpExporterOptions options, IConfiguration configuration, string endpoint)
    {
        options.Endpoint = new Uri(endpoint);
        var protocol = configuration["OpenTelemetry:Exporter:Otlp:Protocol"];
        if (!string.IsNullOrWhiteSpace(protocol)
            && Enum.TryParse<OtlpExportProtocol>(protocol, ignoreCase: true, out var parsedProtocol))
        {
            options.Protocol = parsedProtocol;
        }
    }

    private static bool IsEnabled(IConfiguration configuration, string key, bool defaultValue)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? defaultValue : bool.TryParse(value, out var parsed) ? parsed : defaultValue;
    }
}
