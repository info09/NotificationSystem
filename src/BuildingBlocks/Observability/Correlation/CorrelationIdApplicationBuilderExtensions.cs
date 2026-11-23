using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Observability.Correlation;

/// <summary>
/// Phương thức mở rộng đăng ký middleware mã tương quan vào pipeline.
/// </summary>
public static class CorrelationIdApplicationBuilderExtensions
{
    /// <summary>
    /// Thêm middleware mã tương quan vào pipeline xử lý HTTP.
    /// </summary>
    /// <param name="app">Đối tượng xây dựng pipeline ứng dụng.</param>
    /// <returns>Đối tượng <see cref="IApplicationBuilder"/> để chuỗi cấu hình.</returns>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
