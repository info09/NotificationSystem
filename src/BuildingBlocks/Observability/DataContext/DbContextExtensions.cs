using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Observability.DataContext;

/// <summary>
/// Phương thức mở rộng hỗ trợ thao tác DbContext dùng chung.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Áp dụng các migration đang chờ cho cơ sở dữ liệu.
    /// </summary>
    /// <typeparam name="TContext">Kiểu DbContext cần áp dụng migration.</typeparam>
    /// <param name="services">Service provider dùng để tạo scope và resolve DbContext.</param>
    public static void ApplyMigrations<TContext>(this IServiceProvider services)
        where TContext : DbContext
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        context.Database.Migrate();
    }
}
