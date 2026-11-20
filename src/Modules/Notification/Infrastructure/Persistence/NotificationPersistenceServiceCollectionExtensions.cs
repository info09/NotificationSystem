using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Notifications.CreateNotification;
using Notification.Application.Notifications.GetNotificationById;
using Notification.Application.Notifications.GetNotifications;

namespace Notification.Infrastructure.Persistence;

public static class NotificationPersistenceServiceCollectionExtensions
{
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

        services.AddScoped<CreateNotificationUseCase>();
        services.AddScoped<GetNotificationByIdUseCase>();
        services.AddScoped<GetNotificationsUseCase>();

        return services;
    }
}
