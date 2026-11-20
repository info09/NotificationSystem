using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Persistence;
using Notification.Infrastructure.Persistence.Repositories;
using Testcontainers.PostgreSql;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;
using Recipient = global::Notification.Domain.Recipient;

namespace IntegrationTests.Persistence;

public sealed class NotificationPersistenceIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = TestcontainersConfiguration.CreatePostgreSqlBuilder().Build();

    public Task InitializeAsync() => _postgreSqlContainer.StartAsync();

    public Task DisposeAsync() => _postgreSqlContainer.DisposeAsync().AsTask();

    [Fact]
    public async Task AddAndGetByIdAsync_PersistsAndReturnsNotification()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new NotificationRepository(dbContext);
        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("user@example.com"),
            NotificationChannel.Email,
            "Xin chao",
            null);

        await repository.AddAsync(notification, CancellationToken.None);

        await using var queryDbContext = CreateDbContext();
        var queryRepository = new NotificationRepository(queryDbContext);
        var persisted = await queryRepository.GetByIdAsync(notification.Id, CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Equal(notification.Id, persisted!.Id);
        Assert.Equal("user@example.com", persisted.Recipient.Value);
        Assert.Equal(NotificationChannel.Email, persisted.Channel);
        Assert.Equal(NotificationStatus.Pending, persisted.Status);
        Assert.Equal("Xin chao", persisted.Content);
        Assert.Null(persisted.NotificationTemplateId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotificationDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new NotificationRepository(dbContext);

        var persisted = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(persisted);
    }

    [Fact]
    public async Task GetListAsync_ReturnsPersistedNotifications()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new NotificationRepository(dbContext);
        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("recipient-id"),
            NotificationChannel.InApp,
            null,
            Guid.NewGuid());

        await repository.AddAsync(notification, CancellationToken.None);

        await using var queryDbContext = CreateDbContext();
        var queryRepository = new NotificationRepository(queryDbContext);
        var notifications = await queryRepository.GetListAsync(CancellationToken.None);

        Assert.Single(notifications);
        Assert.Equal(notification.Id, notifications[0].Id);
        Assert.Equal(NotificationChannel.InApp, notifications[0].Channel);
        Assert.NotNull(notifications[0].NotificationTemplateId);
    }

    private NotificationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(
                _postgreSqlContainer.GetConnectionString(),
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "notification"))
            .Options;

        return new NotificationDbContext(options);
    }
}
