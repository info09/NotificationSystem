using BuildingBlocks.Observability;
using BuildingBlocks.Observability.Logging;
using BuildingBlocks.Observability.OpenTelemetry;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Worker.Modules;

namespace Worker;

/// <summary>
/// Điểm vào của ứng dụng Worker.
/// </summary>
public partial class Program
{
    /// <summary>
    /// Khởi động host Worker.
    /// </summary>
    /// <param name="args">Đối số dòng lệnh.</param>
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.UseStructuredLogging();
        builder.AddOpenTelemetryObservability();

        builder.Services.AddModules(builder.Configuration);

        builder.Services.AddHostedService<OutboxPublisherBackgroundService>();
        builder.Services.AddHostedService<PlaceholderWorker>();

        var host = builder.Build();

        var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
        var webHostEnvironment = host.Services.GetService<IWebHostEnvironment>()
            ?? new WorkerHostEnvironmentAdapter(hostEnvironment);

        foreach (var module in host.Services.GetServices<IModuleInitializer>())
        {
            module.Configure(host.Services, webHostEnvironment);
        }

        host.Run();
    }
}

/// <summary>
/// Bộ chuyển đổi IHostEnvironment cho ngữ cảnh Worker.
/// </summary>
internal sealed class WorkerHostEnvironmentAdapter : IWebHostEnvironment
{
    /// <summary>
    /// Khởi tạo adapter từ IHostEnvironment.
    /// </summary>
    /// <param name="environment">Môi trường host hiện tại.</param>
    public WorkerHostEnvironmentAdapter(IHostEnvironment environment)
    {
        ApplicationName = environment.ApplicationName;
        EnvironmentName = environment.EnvironmentName;
        ContentRootPath = environment.ContentRootPath;
        ContentRootFileProvider = environment.ContentRootFileProvider;
        WebRootPath = environment.ContentRootPath;
        WebRootFileProvider = new NullFileProvider();
    }

    /// <summary>
    /// Tên ứng dụng.
    /// </summary>
    public string ApplicationName { get; set; }

    /// <summary>
    /// Nhà cung cấp file cho web root.
    /// </summary>
    public IFileProvider WebRootFileProvider { get; set; }

    /// <summary>
    /// Đường dẫn web root.
    /// </summary>
    public string WebRootPath { get; set; }

    /// <summary>
    /// Tên môi trường.
    /// </summary>
    public string EnvironmentName { get; set; }

    /// <summary>
    /// Đường dẫn content root.
    /// </summary>
    public string ContentRootPath { get; set; }

    /// <summary>
    /// Nhà cung cấp file cho content root.
    /// </summary>
    public IFileProvider ContentRootFileProvider { get; set; }
}
