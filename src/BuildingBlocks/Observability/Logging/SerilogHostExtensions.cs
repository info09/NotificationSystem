using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace BuildingBlocks.Observability.Logging;

/// <summary>
/// Phương thức mở rộng cấu hình logging có cấu trúc với Serilog.
/// </summary>
public static class SerilogHostExtensions
{
    /// <summary>
    /// Đăng ký và cấu hình Serilog cho ứng dụng host.
    /// </summary>
    /// <param name="builder">Builder của host ứng dụng.</param>
    /// <returns>Builder để chuỗi cấu hình.</returns>
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

    /// <summary>
    /// Kiểm tra cấu hình Serilog có khai báo đích ghi log hay không.
    /// </summary>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>True nếu có cấu hình WriteTo; ngược lại false.</returns>
    private static bool HasWriteToConfiguration(IConfiguration configuration)
    {
        return configuration.GetSection("Serilog:WriteTo").Exists();
    }
}
