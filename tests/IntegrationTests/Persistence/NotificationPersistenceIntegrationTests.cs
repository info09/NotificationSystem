using BuildingBlocks.SharedKernel.Repository;
using Microsoft.EntityFrameworkCore;
using Notification.Infrastructure.Persistence;
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

        var repository = new RepositoryBase<DomainNotification>(dbContext);
        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("user@example.com"),
            NotificationChannel.Email,
            "Xin chao",
            null,
            "integration-client-1",
            "idem-key-1");

        await repository.AddAsync(notification, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        await using var queryDbContext = CreateDbContext();
        var queryRepository = new RepositoryBase<DomainNotification>(queryDbContext);
        var persisted = await queryRepository.GetByIdAsync(notification.Id, CancellationToken.None);

        Assert.NotNull(persisted);
        Assert.Equal(notification.Id, persisted!.Id);
        Assert.Equal("user@example.com", persisted.Recipient.Value);
        Assert.Equal(NotificationChannel.Email, persisted.Channel);
        Assert.Equal(NotificationStatus.Pending, persisted.Status);
        Assert.Equal("Xin chao", persisted.Content);
        Assert.Null(persisted.NotificationTemplateId);
        Assert.Equal("integration-client-1", persisted.IdempotencyScope);
        Assert.Equal("idem-key-1", persisted.IdempotencyKey);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotificationDoesNotExist_ReturnsNull()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new RepositoryBase<DomainNotification>(dbContext);

        var persisted = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(persisted);
    }

    [Fact]
    public async Task GetListAsync_ReturnsPersistedNotifications()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureCreatedAsync();

        var repository = new RepositoryBase<DomainNotification>(dbContext);
        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("recipient-id"),
            NotificationChannel.InApp,
            null,
            Guid.NewGuid(),
            "integration-client-1",
            "idem-key-2");

        await repository.AddAsync(notification, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        await using var queryDbContext = CreateDbContext();
        var queryRepository = new RepositoryBase<DomainNotification>(queryDbContext);
        var notifications = await queryRepository.GetAllAsync(CancellationToken.None);

        var notificationList = notifications.ToList();
        Assert.Single(notificationList);
        Assert.Equal(notification.Id, notificationList[0].Id);
        Assert.Equal(NotificationChannel.InApp, notificationList[0].Channel);
        Assert.NotNull(notificationList[0].NotificationTemplateId);
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
