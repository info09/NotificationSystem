using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Observability;

/// <summary>
/// Giao diện khởi tạo module. Mỗi module nghiệp vụ phải implement giao diện này
/// để đăng ký dịch vụ và cấu hình middleware vào WebHost.
/// </summary>
public interface IModuleInitializer
{
    /// <summary>
    /// Đăng ký các dịch vụ của module vào DI container.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Cấu hình pipeline middleware của module khi ứng dụng khởi động.
    /// </summary>
    /// <param name="app">Ứng dụng web đang xây dựng.</param>
    /// <param name="environment">Môi trường hosting hiện tại.</param>
    void Configure(IServiceProvider app, IWebHostEnvironment env);
}
