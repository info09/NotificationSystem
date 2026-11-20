using BuildingBlocks.SharedKernel.Repository;
using Notification.Application.Notifications;
using Notification.Application.Notifications.CreateNotification;
using System.Linq.Expressions;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using Recipient = global::Notification.Domain.Recipient;

namespace UnitTests.Notification.Application;

public sealed class CreateNotificationUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WithValidRequest_PersistsNotificationAndReturnsCreatedResult()
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);
        var request = new CreateNotificationAppRequest(
            IdempotencyKey: "idem-key-1",
            Recipient: "user@example.com",
            Channel: "email",
            Content: "Xin chao",
            NotificationTemplateId: null,
            Caller: new NotificationCallerContext("integration-client-1", ["IntegrationClient"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value!.NotificationId);
        Assert.Equal("Pending", result.Value.Status);
        Assert.NotNull(repository.AddedNotification);
        Assert.Equal(result.Value.NotificationId, repository.AddedNotification!.Id);
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
        var request = new CreateNotificationAppRequest(
            IdempotencyKey: "idem-key-1",
            Recipient: "user@example.com",
            Channel: "email",
            Content: "Xin chao",
            NotificationTemplateId: null,
            Caller: new NotificationCallerContext("integration-client-1", ["IntegrationClient"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(existingNotification.Id, result.Value!.NotificationId);
        Assert.Null(repository.AddedNotification);
        Assert.Equal(0, repository.SaveChangesCallCount);
    }

    [Fact]
    public async Task HandleAsync_WithNullRequest_ReturnsValidationError()
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);

        var result = await useCase.HandleAsync(null, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("request", result.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidFields_ReturnsValidationErrors()
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);
        var request = new CreateNotificationAppRequest(
            IdempotencyKey: "",
            Recipient: "",
            Channel: "sms",
            Content: null,
            NotificationTemplateId: null,
            Caller: new NotificationCallerContext("integration-client-1", ["IntegrationClient"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("idempotencyKey", result.ValidationErrors!.Keys);
        Assert.Contains("recipient", result.ValidationErrors.Keys);
        Assert.Contains("channel", result.ValidationErrors.Keys);
        Assert.Contains("content", result.ValidationErrors.Keys);
    }

    [Fact]
    public async Task HandleAsync_WithMissingCallerScope_ReturnsValidationError()
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);
        var request = new CreateNotificationAppRequest(
            IdempotencyKey: "idem-key-1",
            Recipient: "user@example.com",
            Channel: "email",
            Content: "Xin chao",
            NotificationTemplateId: null,
            Caller: new NotificationCallerContext(null, ["IntegrationClient"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("idempotencyScope", result.ValidationErrors!.Keys);
    }

    [Fact]
    public async Task HandleAsync_WithInAppChannel_ParsesChannelAndCreatesNotification()
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);
        var request = new CreateNotificationAppRequest(
            IdempotencyKey: "idem-key-2",
            Recipient: "user-1",
            Channel: "inapp",
            Content: "Hello",
            NotificationTemplateId: null,
            Caller: new NotificationCallerContext("integration-client-1", ["IntegrationClient"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.Created, result.Status);
        Assert.NotNull(repository.AddedNotification);
        Assert.Equal(NotificationChannel.InApp, repository.AddedNotification!.Channel);
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
