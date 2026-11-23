using IntegrationMessaging.Domain;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace IntegrationTests.IntegrationMessaging;

/// <summary>
/// Integration test cho persistence InboxMessage.
/// </summary>
public sealed class InboxMessagePersistenceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = TestcontainersConfiguration.CreatePostgreSqlBuilder().Build();

    /// <summary>
    /// Khởi động container PostgreSQL.
    /// </summary>
    public Task InitializeAsync() => _postgreSqlContainer.StartAsync();

    /// <summary>
    /// Dọn dẹp container PostgreSQL.
    /// </summary>
    public Task DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task SaveChanges_WithDuplicateMessageIdAndConsumerName_Throws()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var messageId = Guid.NewGuid();
        dbContext.InboxMessages.Add(InboxMessage.Create(Guid.NewGuid(), messageId, "NotificationWorker", "NotificationCreated", DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        dbContext.InboxMessages.Add(InboxMessage.Create(Guid.NewGuid(), messageId, "NotificationWorker", "NotificationCreated", DateTime.UtcNow));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_WithSameMessageIdDifferentConsumer_Succeeds()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var messageId = Guid.NewGuid();
        dbContext.InboxMessages.Add(InboxMessage.Create(Guid.NewGuid(), messageId, "NotificationWorker", "NotificationCreated", DateTime.UtcNow));
        dbContext.InboxMessages.Add(InboxMessage.Create(Guid.NewGuid(), messageId, "AnotherWorker", "NotificationCreated", DateTime.UtcNow));

        await dbContext.SaveChangesAsync();

        var count = await dbContext.InboxMessages.CountAsync();
        Assert.Equal(2, count);
    }

    private IntegrationMessagingDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<IntegrationMessagingDbContext>()
            .UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "integration_messaging"))
            .Options;

        return new IntegrationMessagingDbContext(options);
    }
}
