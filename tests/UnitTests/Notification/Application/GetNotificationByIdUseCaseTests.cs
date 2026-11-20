using Notification.Application.Abstractions;
using Notification.Application.Notifications;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;
using Recipient = global::Notification.Domain.Recipient;

namespace UnitTests.Notification.Application;

public sealed class GetNotificationByIdUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WhenNotificationExists_ReturnsDto()
    {
        var notification = DomainNotification.Create(
            Guid.NewGuid(),
            new Recipient("user@example.com"),
            NotificationChannel.InApp,
            "Noi dung",
            null);
        var repository = new FakeNotificationRepository(notification);
        var useCase = new GetNotificationByIdUseCase(repository);

        var result = await useCase.HandleAsync(notification.Id, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(notification.Id, result!.Id);
        Assert.Equal(NotificationStatus.Pending, result.Status);
        Assert.Equal("user@example.com", result.Recipient);
        Assert.Equal(NotificationChannel.InApp, result.Channel);
        Assert.Equal("Noi dung", result.Content);
        Assert.Null(result.NotificationTemplateId);
    }

    [Fact]
    public async Task HandleAsync_WhenNotificationDoesNotExist_ReturnsNull()
    {
        var repository = new FakeNotificationRepository(null);
        var useCase = new GetNotificationByIdUseCase(repository);

        var result = await useCase.HandleAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Null(result);
    }

    private sealed class FakeNotificationRepository : INotificationRepository
    {
        private readonly DomainNotification? _notification;

        public FakeNotificationRepository(DomainNotification? notification)
        {
            _notification = notification;
        }

        public Task AddAsync(DomainNotification notification, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<DomainNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(_notification is not null && _notification.Id == id ? _notification : null);

        public Task<IReadOnlyList<DomainNotification>> GetListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DomainNotification>>(_notification is null ? [] : [_notification]);
    }
}
