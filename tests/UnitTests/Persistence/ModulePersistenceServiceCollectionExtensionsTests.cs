using Administration.Infrastructure.Persistence;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notification.Infrastructure.Persistence;
using TemplateConfiguration.Infrastructure.Persistence;

namespace UnitTests.Persistence;

public sealed class ModulePersistenceServiceCollectionExtensionsTests
{
    public static TheoryData<string, Action<IServiceCollection, IConfiguration>> Modules => new()
    {
        { "Notification", (services, configuration) => services.AddNotificationPersistence(configuration) },
        { "TemplateConfiguration", (services, configuration) => services.AddTemplateConfigurationPersistence(configuration) },
        { "IntegrationMessaging", (services, configuration) => services.AddIntegrationMessagingPersistence(configuration) },
        { "Delivery", (services, configuration) => services.AddDeliveryPersistence(configuration) },
        { "Administration", (services, configuration) => services.AddAdministrationPersistence(configuration) },
        { "IdentityAccess", (services, configuration) => services.AddIdentityAccessPersistence(configuration) }
    };

    [Theory]
    [MemberData(nameof(Modules))]
    public void AddModulePersistence_WhenConnectionStringMissing_ThrowsInvalidOperationException(
        string moduleName,
        Action<IServiceCollection, IConfiguration> addPersistence)
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOperationException>(() => addPersistence(services, configuration));

        Assert.Equal("Connection string 'Database' is not configured.", exception.Message);
        Assert.NotEmpty(moduleName);
    }
}
