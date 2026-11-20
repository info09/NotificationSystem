using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
}
