using IntegrationMessaging.Domain;
using IntegrationMessaging.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace E2ETests.IntegrationMessaging;

/// <summary>
/// E2E test cho persistence InboxMessage.
/// </summary>
public sealed class InboxMessagePersistenceE2ETests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16")
        .WithDatabase("notification_test")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    /// <summary>
    /// Khởi động container PostgreSQL.
    /// </summary>
    public Task InitializeAsync() => _postgreSqlContainer.StartAsync();

    /// <summary>
    /// Dọn dẹp container PostgreSQL.
    /// </summary>
    public Task DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task SaveChanges_WithDuplicateMessageIdAndConsumerName_RejectsDuplicate()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var messageId = Guid.NewGuid();
        dbContext.InboxMessages.Add(InboxMessage.Create(Guid.NewGuid(), messageId, "NotificationWorker", "NotificationCreated", DateTime.UtcNow));
        await dbContext.SaveChangesAsync();

        dbContext.InboxMessages.Add(InboxMessage.Create(Guid.NewGuid(), messageId, "NotificationWorker", "NotificationCreated", DateTime.UtcNow));

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());
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
