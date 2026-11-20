using Administration.Infrastructure.Persistence;
using Delivery.Infrastructure.Persistence;
using IdentityAccess.Infrastructure.Persistence;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Persistence;
using TemplateConfiguration.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace IntegrationTests.Persistence;

public sealed class ModuleDbContextsConnectivityIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = TestcontainersConfiguration.CreatePostgreSqlBuilder().Build();

    public Task InitializeAsync() => _postgreSqlContainer.StartAsync();

    public Task DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task ModuleDbContexts_WithPostgreSqlContainer_CanConnect()
    {
        var connectionString = _postgreSqlContainer.GetConnectionString();

        await AssertCanConnectAsync(new NotificationDbContext(CreateOptions<NotificationDbContext>(
            connectionString,
            "notification",
            typeof(NotificationDbContext).Assembly.FullName!)));

        await AssertCanConnectAsync(new TemplateConfigurationDbContext(CreateOptions<TemplateConfigurationDbContext>(
            connectionString,
            "template_configuration",
            typeof(TemplateConfigurationDbContext).Assembly.FullName!)));

        await AssertCanConnectAsync(new IntegrationMessagingDbContext(CreateOptions<IntegrationMessagingDbContext>(
            connectionString,
            "integration_messaging",
            typeof(IntegrationMessagingDbContext).Assembly.FullName!)));

        await AssertCanConnectAsync(new DeliveryDbContext(CreateOptions<DeliveryDbContext>(
            connectionString,
            "delivery",
            typeof(DeliveryDbContext).Assembly.FullName!)));

        await AssertCanConnectAsync(new AdministrationDbContext(CreateOptions<AdministrationDbContext>(
            connectionString,
            "administration",
            typeof(AdministrationDbContext).Assembly.FullName!)));

        await AssertCanConnectAsync(new IdentityAccessDbContext(CreateOptions<IdentityAccessDbContext>(
            connectionString,
            "identity_access",
            typeof(IdentityAccessDbContext).Assembly.FullName!)));
    }

    private static DbContextOptions<TContext> CreateOptions<TContext>(
        string connectionString,
        string schema,
        string migrationsAssembly)
        where TContext : DbContext
    {
        return new DbContextOptionsBuilder<TContext>()
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsAssembly(migrationsAssembly)
                    .MigrationsHistoryTable("__EFMigrationsHistory", schema))
            .Options;
    }

    private static async Task AssertCanConnectAsync(DbContext dbContext)
    {
        await using (dbContext)
        {
            Assert.True(await dbContext.Database.CanConnectAsync());
        }
    }
}

