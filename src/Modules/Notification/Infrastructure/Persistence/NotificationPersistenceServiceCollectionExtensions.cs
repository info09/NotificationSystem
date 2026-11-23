using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Notifications;
using Notification.Application.Notifications.CreateNotification;
using Notification.Application.Notifications.GetNotificationById;
using Notification.Application.Notifications.GetNotifications;

namespace Notification.Infrastructure.Persistence;

/// <summary>
/// Phương thức mở rộng đăng ký persistence và use case cho module Notification.
/// </summary>
public static class NotificationPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Đăng ký DbContext, repository và use case của module Notification.
    /// </summary>
    /// <param name="services">Tập hợp dịch vụ của ứng dụng.</param>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    /// <returns>Tập hợp dịch vụ để chuỗi cấu hình.</returns>
    public static IServiceCollection AddNotificationPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Database");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Database' is not configured.");
        }

        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName)
                    .MigrationsHistoryTable("__EFMigrationsHistory", "notification")));

        services.AddScoped<INotificationRepository, NotificationRepository>();

        services.AddScoped<CreateNotificationUseCase>();
        services.AddScoped<GetNotificationByIdUseCase>();
        services.AddScoped<GetNotificationsUseCase>();

        return services;
    }
}
