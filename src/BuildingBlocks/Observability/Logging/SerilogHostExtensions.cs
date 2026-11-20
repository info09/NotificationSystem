using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BuildingBlocks.Observability.Logging;

public static class SerilogHostExtensions
{
    public static IHostApplicationBuilder UseStructuredLogging(this IHostApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSerilog((services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", builder.Environment.ApplicationName)
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName);

            if (!HasWriteToConfiguration(builder.Configuration))
            {
                loggerConfiguration.WriteTo.Console();
            }
        });

        return builder;
    }

    private static bool HasWriteToConfiguration(IConfiguration configuration)
    {
        return configuration.GetSection("Serilog:WriteTo").Exists();
    }
}
