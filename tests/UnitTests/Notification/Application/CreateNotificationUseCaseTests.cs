using BuildingBlocks.SharedKernel.Repository;
using Notification.Application.Notifications.CreateNotification;
using System.Linq.Expressions;
using DomainNotification = global::Notification.Domain.Notification;
using NotificationChannel = global::Notification.Domain.NotificationChannel;
using NotificationStatus = global::Notification.Domain.NotificationStatus;

namespace UnitTests.Notification.Application;

public sealed class CreateNotificationUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WithValidCommand_PersistsNotificationAndReturnsResult()
    {
        var repository = new FakeNotificationRepository();
        var useCase = new CreateNotificationUseCase(repository);
        var command = new CreateNotificationCommand(
            Recipient: "user@example.com",
            Channel: NotificationChannel.Email,
            Content: "Xin chao",
            NotificationTemplateId: null);

        var result = await useCase.HandleAsync(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(NotificationStatus.Pending, result.Status);
        Assert.Equal("user@example.com", result.Recipient);
        Assert.Equal(NotificationChannel.Email, result.Channel);
        Assert.Equal("Xin chao", result.Content);
        Assert.Null(result.NotificationTemplateId);
        Assert.NotNull(repository.AddedNotification);
        Assert.Equal(result.Id, repository.AddedNotification!.Id);
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
            NotificationTemplateId: null);

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.HandleAsync(command, CancellationToken.None));
        Assert.Null(repository.AddedNotification);
    }

    private sealed class FakeNotificationRepository : IRepository<DomainNotification>
    {
        public DomainNotification? AddedNotification { get; private set; }

        public Task<DomainNotification> AddAsync(DomainNotification entity, CancellationToken cancellationToken = default)
        {
            AddedNotification = entity;
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<DomainNotification>> FindAsync(Expression<Func<DomainNotification, bool>> predicate, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<IEnumerable<DomainNotification>> GetAllAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<DomainNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult<DomainNotification?>(null);

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task UpdateAsync(DomainNotification entity, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }
}
