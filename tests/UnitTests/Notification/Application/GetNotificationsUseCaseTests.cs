using BuildingBlocks.SharedKernel.Repository;
using Notification.Application.Notifications.GetNotifications;
using System.Linq.Expressions;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using Recipient = global::Notification.Domain.Recipient;

namespace UnitTests.Notification.Application;

public sealed class GetNotificationsUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WhenNotificationsExist_ReturnsDtos()
    {
        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("user@example.com"),
            NotificationChannel.Email,
            "Hello",
            null,
            "integration-client-1",
            "idem-key-1");
        var repository = new FakeNotificationRepository([notification]);
        var useCase = new GetNotificationsUseCase(repository);

        var result = await useCase.HandleAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(notification.Id, result[0].Id);
        Assert.Equal("user@example.com", result[0].Recipient);
    }

    [Fact]
    public async Task HandleAsync_WhenNoNotifications_ReturnsEmptyList()
    {
        var repository = new FakeNotificationRepository([]);
        var useCase = new GetNotificationsUseCase(repository);

        var result = await useCase.HandleAsync(CancellationToken.None);

        Assert.Empty(result);
    }

    private sealed class FakeNotificationRepository : IRepository<DomainNotification>
    {
        private readonly IReadOnlyList<DomainNotification> _notifications;

        public FakeNotificationRepository(IReadOnlyList<DomainNotification> notifications)
        {
            _notifications = notifications;
        }

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
