using BuildingBlocks.Observability.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Hosting;
using Serilog.Extensions.Logging;

namespace UnitTests.Observability;

public sealed class SerilogHostExtensionsTests
{
    [Fact]
    public void UseStructuredLogging_WhenBuilderIsNull_ThrowsArgumentNullException()
    {
        IHostApplicationBuilder? builder = null;

        Assert.Throws<ArgumentNullException>(() => builder!.UseStructuredLogging());
    }

    [Fact]
    public void UseStructuredLogging_WhenConfigured_RegistersSerilogLoggerFactory()
    {
        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            EnvironmentName = "Development"
        });

        builder.UseStructuredLogging();

        using var host = builder.Build();

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var diagnosticContext = host.Services.GetService<DiagnosticContext>();

        Assert.IsType<SerilogLoggerFactory>(loggerFactory);
        Assert.NotNull(diagnosticContext);
    }
}
