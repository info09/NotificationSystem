using BuildingBlocks.SharedKernel.Repository;
using Notification.Application.Notifications.CreateNotification;
using System.Linq.Expressions;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;
using Recipient = global::Notification.Domain.Recipient;

namespace UnitTests.Notification.Application;

public sealed class CreateNotificationUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_PersistsNotificationAndReturnsCreatedResult()
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);
        var command = new CreateNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Xin chao",
            NotificationTemplateId: null,
            IdempotencyScope: "integration-client-1",
            IdempotencyKey: "idem-key-1");

        var result = await useCase.HandleAsync(command, CancellationToken.None);

        Assert.True(result.Created);
        Assert.NotEqual(Guid.Empty, result.Notification.Id);
        Assert.Equal(NotificationStatus.Pending, result.Notification.Status);
        Assert.Equal("user@example.com", result.Notification.Recipient);
        Assert.Equal(NotificationChannel.Email, result.Notification.Channel);
        Assert.Equal("Xin chao", result.Notification.Content);
        Assert.Null(result.Notification.NotificationTemplateId);
        Assert.NotNull(repository.AddedNotification);
        Assert.Equal(result.Notification.Id, repository.AddedNotification!.Id);
        Assert.Equal(1, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WithExistingIdempotencyKey_ReturnsExistingNotificationWithoutCreatingDuplicate()
    {
        var existingNotification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("user@example.com"),
            NotificationChannel.Email,
            "Xin chao",
            null,
            "integration-client-1",
            "idem-key-1");
        var repository = new FakeNotificationRepository(existingNotification);
        var useCase = new CreateNotificationUseCase(repository);
        var command = new CreateNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Xin chao",
            NotificationTemplateId: null,
            IdempotencyScope: "integration-client-1",
            IdempotencyKey: "idem-key-1");

        var result = await useCase.HandleAsync(command, CancellationToken.None);

        Assert.False(result.Created);
        Assert.Equal(existingNotification.Id, result.Notification.Id);
        Assert.Null(repository.AddedNotification);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WithoutContentAndTemplate_ThrowsArgumentException()
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);
        var command = new CreateNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: null,
            NotificationTemplateId: null,
            IdempotencyScope: "integration-client-1",
            IdempotencyKey: "idem-key-2");

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.HandleAsync(command, CancellationToken.None));
        Assert.Null(repository.AddedNotification);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithoutIdempotencyScope_ThrowsArgumentException(string? scope)
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);
        var command = new CreateNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Xin chao",
            NotificationTemplateId: null,
            IdempotencyScope: scope!,
            IdempotencyKey: "idem-key-3");

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.HandleAsync(command, CancellationToken.None));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task HandleAsync_WithoutIdempotencyKey_ThrowsArgumentException(string? key)
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);
        var command = new CreateNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Xin chao",
            NotificationTemplateId: null,
            IdempotencyScope: "integration-client-1",
            IdempotencyKey: key!);

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.HandleAsync(command, CancellationToken.None));
    }

    private sealed class FakeNotificationRepository : IRepository<DomainNotification>
    {
        private readonly IReadOnlyList<DomainNotification> _existingNotifications;

        public FakeNotificationRepository(params DomainNotification[] existingNotifications)
        {
            _existingNotifications = existingNotifications;
        }

        public DomainNotification? AddedNotification { get; private set; }

        public int SaveChangesCallCount { get; private set; }

        public Task<DomainNotification> AddAsync(DomainNotification entity, CancellationToken cancellationToken = default)
        {
            AddedNotification = entity;
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<DomainNotification>> FindAsync(Expression<Func<DomainNotification, bool>> predicate, CancellationToken cancellationToken = default)
        {
            var compiled = predicate.Compile();
            return Task.FromResult(_existingNotifications.Where(compiled));
        }

        public Task<IEnumerable<DomainNotification>> GetAllAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<DomainNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_existingNotifications.FirstOrDefault(n => n.Id == id));

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }

        public Task UpdateAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
