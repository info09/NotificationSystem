using Notification.Application.Notifications;
using Notification.Application.Notifications.GetNotificationById;
using Notification.Domain;
using System.Linq.Expressions;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using Recipient = global::Notification.Domain.Recipient;

namespace UnitTests.Notification.Application;

public sealed class GetNotificationByIdUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WhenNotificationExists_ReturnsOkResult()
    {
        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("user@example.com"),
            NotificationChannel.InApp,
            "Noi dung",
            null,
            "integration-client-1",
            "idem-key-1");
        var repository = new FakeNotificationRepository(notification);
        var useCase = new GetNotificationByIdUseCase(repository);
        var request = new GetNotificationByIdAppRequest(
            notification.Id,
            new NotificationCallerContext("admin-1", ["SystemAdministrator"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(notification.Id, result.Value!.Id);
        Assert.Equal("user@example.com", result.Value.Recipient);
        Assert.Equal("InApp", result.Value.Channel);
        Assert.Equal("Pending", result.Value.Status);
        Assert.Equal(notification.CreatedAt, result.Value.CreatedAt);
        Assert.Null(repository.LastScopeRestriction);
    }

    [Fact]
    public async Task HandleAsync_WithIntegrationClientCaller_ScopesBySubject()
    {
        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("user@example.com"),
            NotificationChannel.Email,
            "Hello",
            null,
            "integration-client-1",
            "idem-key-1");
        var repository = new FakeNotificationRepository(notification);
        var useCase = new GetNotificationByIdUseCase(repository);
        var request = new GetNotificationByIdAppRequest(
            notification.Id,
            new NotificationCallerContext("integration-client-1", ["IntegrationClient"]));

        await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal("integration-client-1", repository.LastScopeRestriction);
    }

    [Fact]
    public async Task HandleAsync_WhenNotificationDoesNotExist_ReturnsNotFound()
    {
        var repository = new FakeNotificationRepository(null);
        var useCase = new GetNotificationByIdUseCase(repository);
        var request = new GetNotificationByIdAppRequest(
            Guid.NewGuid(),
            new NotificationCallerContext("admin-1", ["BusinessOperator"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.NotFound, result.Status);
        Assert.Null(result.Value);
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        private readonly DomainNotification? _notification;

        public FakeNotificationRepository(DomainNotification? notification)
        {
            _notification = notification;
        }

        public Guid? LastRequestedId { get; private set; }
        public string? LastScopeRestriction { get; private set; }

        public Task<(IReadOnlyList<DomainNotification> Items, int TotalCount)> GetPagedAsync(
            NotificationStatus? status,
            NotificationChannel? channel,
            DateTime? createdFromUtc,
            DateTime? createdToUtc,
            int page,
            int pageSize,
            string? scopeRestriction,
            CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<DomainNotification?> GetByIdForScopeAsync(Guid id, string? scopeRestriction, CancellationToken cancellationToken)
        {
            LastRequestedId = id;
            LastScopeRestriction = scopeRestriction;
            return Task.FromResult(_notification is not null && _notification.Id == id ? _notification : null);
        }

        public Task<DomainNotification> AddAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => Task.FromResult(entity);

        public Task DeleteAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<DomainNotification>> FindAsync(Expression<Func<DomainNotification, bool>> predicate, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<DomainNotification>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<DomainNotification>>(_notification is null ? [] : [_notification]);

        public Task<DomainNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_notification is not null && _notification.Id == id ? _notification : null);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task UpdateAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
