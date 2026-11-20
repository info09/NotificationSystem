using BuildingBlocks.SharedKernel.Repository;
using Notification.Application.Notifications.GetNotificationById;
using System.Linq.Expressions;
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
            null,
            "integration-client-1",
            "idem-key-1");
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

    private sealed class FakeNotificationRepository : IRepository<DomainNotification>
    {
        private readonly DomainNotification? _notification;

        public FakeNotificationRepository(DomainNotification? notification)
        {
            _notification = notification;
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
