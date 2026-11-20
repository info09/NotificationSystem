using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Abstractions;
using Notification.Application.Notifications;
using Notification.Infrastructure.Persistence;

namespace UnitTests.Persistence;

public sealed class NotificationPersistenceServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNotificationPersistence_WhenConnectionStringMissing_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            services.AddNotificationPersistence(configuration));

        Assert.Equal("Connection string 'Database' is not configured.", exception.Message);
    }

    [Fact]
    public void AddNotificationPersistence_WithConnectionString_RegistersPersistenceAndApplicationServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Database"] = "Host=localhost;Port=5432;Database=notification_test;Username=postgres;Password=postgres"
            })
            .Build();

        services.AddNotificationPersistence(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetRequiredService<NotificationDbContext>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<INotificationRepository>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<CreateNotificationUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<GetNotificationByIdUseCase>());
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<GetNotificationsUseCase>());
    }
}
