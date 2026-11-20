using Administration.Infrastructure.Persistence;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Persistence;
using TemplateConfiguration.Infrastructure.Persistence;

namespace IntegrationTests.Persistence;

public sealed class ModuleDbContextDesignTimeFactoryTests
{
    [Fact]
    public void DesignTimeFactories_CreateDbContextsWithConfiguredConnectionString()
    {
        const string expectedDatabase = "Database=notification";

        using var notificationDbContext = new NotificationDbContextFactory().CreateDbContext([]);
        using var templateConfigurationDbContext = new TemplateConfigurationDbContextFactory().CreateDbContext([]);
        using var integrationMessagingDbContext = new IntegrationMessagingDbContextFactory().CreateDbContext([]);
        using var deliveryDbContext = new DeliveryDbContextFactory().CreateDbContext([]);
        using var administrationDbContext = new AdministrationDbContextFactory().CreateDbContext([]);
        using var identityAccessDbContext = new IdentityAccessDbContextFactory().CreateDbContext([]);

        Assert.Contains(expectedDatabase, notificationDbContext.Database.GetDbConnection().ConnectionString);
        Assert.Contains(expectedDatabase, templateConfigurationDbContext.Database.GetDbConnection().ConnectionString);
        Assert.Contains(expectedDatabase, integrationMessagingDbContext.Database.GetDbConnection().ConnectionString);
        Assert.Contains(expectedDatabase, deliveryDbContext.Database.GetDbConnection().ConnectionString);
        Assert.Contains(expectedDatabase, administrationDbContext.Database.GetDbConnection().ConnectionString);
        Assert.Contains(expectedDatabase, identityAccessDbContext.Database.GetDbConnection().ConnectionString);
    }
}
