using Notification.Application.Abstractions;
using Notification.Application.Notifications;
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
            null);
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

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        private readonly IReadOnlyList<DomainNotification> _notifications;

        public FakeNotificationRepository(IReadOnlyList<DomainNotification> notifications)
        {
            _notifications = notifications;
        }

        public Task AddAsync(DomainNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<DomainNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_notifications.FirstOrDefault(n => n.Id == id));

        public Task<IReadOnlyList<DomainNotification>> GetListAsync(CancellationToken cancellationToken)
            => Task.FromResult(_notifications);
    }
}
