using BuildingBlocks.SharedKernel.Repository;
using Notification.Application.Notifications;
using Notification.Application.Notifications.GetNotifications;
using System.Linq.Expressions;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;
using Recipient = global::Notification.Domain.Recipient;

namespace UnitTests.Notification.Application;

public sealed class GetNotificationsUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WhenNotificationsExist_ReturnsPagedDtos()
    {
        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("user@example.com"),
            NotificationChannel.Email,
            "Hello",
            null,
            "integration-client-1",
            "idem-key-1");
        var repository = new FakeNotificationRepository([notification], totalCount: 1);
        var useCase = new GetNotificationsUseCase(repository);
        var request = new GetNotificationsAppRequest(
            Status: null,
            Channel: null,
            CreatedFrom: null,
            CreatedTo: null,
            Page: 1,
            PageSize: 20,
            Caller: new NotificationCallerContext("admin-1", ["SystemAdministrator"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!.Items);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Equal(1, result.Value.Page);
        Assert.Equal(20, result.Value.PageSize);
        Assert.Equal(notification.Id, result.Value.Items[0].Id);
        Assert.Null(repository.LastQuery!.ScopeRestriction);
    }

    [Fact]
    public async Task HandleAsync_WithIntegrationClientCaller_ScopesBySubject()
    {
        var repository = new FakeNotificationRepository([], totalCount: 0);
        var useCase = new GetNotificationsUseCase(repository);
        var request = new GetNotificationsAppRequest(
            null, null, null, null, 1, 20,
            new NotificationCallerContext("integration-client-1", ["IntegrationClient"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.Ok, result.Status);
        Assert.Equal("integration-client-1", repository.LastQuery!.ScopeRestriction);
    }

    [Fact]
    public async Task HandleAsync_WithFilters_PassesParsedFiltersToRepository()
    {
        var repository = new FakeNotificationRepository([], totalCount: 0);
        var useCase = new GetNotificationsUseCase(repository);
        var request = new GetNotificationsAppRequest(
            Status: "pending",
            Channel: "inapp",
            CreatedFrom: "2026-04-01T00:00:00Z",
            CreatedTo: "2026-04-02T00:00:00Z",
            Page: 2,
            PageSize: 10,
            Caller: new NotificationCallerContext("admin-1", ["BusinessOperator"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.Ok, result.Status);
        Assert.Equal(NotificationStatus.Pending, repository.LastQuery!.Status);
        Assert.Equal(NotificationChannel.InApp, repository.LastQuery.Channel);
        Assert.Equal(new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc), repository.LastQuery.CreatedFromUtc);
        Assert.Equal(new DateTime(2026, 4, 2, 0, 0, 0, DateTimeKind.Utc), repository.LastQuery.CreatedToUtc);
        Assert.Equal(2, repository.LastQuery.Page);
        Assert.Equal(10, repository.LastQuery.PageSize);
        Assert.Null(repository.LastQuery.ScopeRestriction);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidQuery_ReturnsValidationErrors()
    {
        var repository = new FakeNotificationRepository([], totalCount: 0);
        var useCase = new GetNotificationsUseCase(repository);
        var request = new GetNotificationsAppRequest(
            Status: "Unknown",
            Channel: "sms",
            CreatedFrom: "not-a-date",
            CreatedTo: null,
            Page: 0,
            PageSize: 101,
            Caller: new NotificationCallerContext("admin-1", ["SystemAdministrator"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("status", result.ValidationErrors!.Keys);
        Assert.Contains("channel", result.ValidationErrors.Keys);
        Assert.Contains("createdFrom", result.ValidationErrors.Keys);
        Assert.Contains("page", result.ValidationErrors.Keys);
        Assert.Contains("pageSize", result.ValidationErrors.Keys);
    }

    [Fact]
    public async Task HandleAsync_WithInvalidDateRange_ReturnsValidationError()
    {
        var repository = new FakeNotificationRepository([], totalCount: 0);
        var useCase = new GetNotificationsUseCase(repository);
        var request = new GetNotificationsAppRequest(
            null, null,
            "2026-04-03T00:00:00Z",
            "2026-04-02T00:00:00Z",
            1, 20,
            new NotificationCallerContext("admin-1", ["SystemAdministrator"]));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(NotificationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.Contains("createdFrom", result.ValidationErrors!.Keys);
    }

    private sealed record CapturedQuery(
        NotificationStatus? Status,
        NotificationChannel? Channel,
        DateTime? CreatedFromUtc,
        DateTime? CreatedToUtc,
        int Page,
        int PageSize,
        string? ScopeRestriction);

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        private readonly IReadOnlyList<DomainNotification> _notifications;
        private readonly int _totalCount;

        public FakeNotificationRepository(IReadOnlyList<DomainNotification> notifications, int totalCount)
        {
            _notifications = notifications;
            _totalCount = totalCount;
        }

        public CapturedQuery? LastQuery { get; private set; }

        public Task<(IReadOnlyList<DomainNotification> Items, int TotalCount)> GetPagedAsync(
            NotificationStatus? status,
            NotificationChannel? channel,
            DateTime? createdFromUtc,
            DateTime? createdToUtc,
            int page,
            int pageSize,
            string? scopeRestriction,
            CancellationToken cancellationToken)
        {
            LastQuery = new CapturedQuery(status, channel, createdFromUtc, createdToUtc, page, pageSize, scopeRestriction);
            return Task.FromResult((_notifications, _totalCount));
        }

        public Task<DomainNotification?> GetByIdForScopeAsync(Guid id, string? scopeRestriction, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<DomainNotification> AddAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => Task.FromResult(entity);

        public Task DeleteAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<DomainNotification>> FindAsync(Expression<Func<DomainNotification, bool>> predicate, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<DomainNotification>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<DomainNotification>>(_notifications);

        public Task<DomainNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(_notifications.FirstOrDefault(n => n.Id == id));

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task UpdateAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
