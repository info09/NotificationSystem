using Delivery.Application.Abstractions;
using Delivery.Application.Email;
using Delivery.Application.InAppNotifications;
using Delivery.Application.InAppNotifications.CreateInAppNotification;
using Delivery.Domain.Email;
using Delivery.Domain.InAppNotifications;
using Delivery.Infrastructure.Dispatching;

namespace UnitTests.Delivery;

/// <summary>
/// Unit test cho dispatcher kênh Thông báo.
/// </summary>
public sealed class NotificationChannelDispatcherTests
{
    [Fact]
    public async Task DispatchAsync_WithEmailChannel_CallsEmailSender()
    {
        var emailSender = new FakeEmailSender();
        var dispatcher = new NotificationChannelDispatcher(emailSender, CreateInAppUseCase(new FakeInAppNotificationRepository()));

        await dispatcher.DispatchAsync(
            new NotificationChannelDispatchRequest(Guid.NewGuid(), "Email", "user@example.com", "Hello", null),
            CancellationToken.None);

        Assert.NotNull(emailSender.LastRequest);
        Assert.Equal("user@example.com", emailSender.LastRequest!.To);
    }

    [Fact]
    public async Task DispatchAsync_WithInAppChannel_CreatesInAppNotification()
    {
        var repository = new FakeInAppNotificationRepository();
        var dispatcher = new NotificationChannelDispatcher(new FakeEmailSender(), CreateInAppUseCase(repository));

        var notificationId = Guid.NewGuid();
        await dispatcher.DispatchAsync(
            new NotificationChannelDispatchRequest(notificationId, "InApp", "user-1", "Hello", null),
            CancellationToken.None);

        Assert.Single(repository.Items);
        Assert.Equal(notificationId, repository.Items[0].NotificationId);
        Assert.Equal("user-1", repository.Items[0].RecipientUserId);
    }

    private static CreateInAppNotificationUseCase CreateInAppUseCase(IInAppNotificationRepository repository) => new(repository);

    private sealed class FakeEmailSender : IEmailSender
    {
        public SendEmailRequest? LastRequest { get; private set; }

        public Task SendAsync(SendEmailRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeInAppNotificationRepository : IInAppNotificationRepository
    {
        public List<InAppNotification> Items { get; } = [];

        public Task<InAppNotification> AddAsync(InAppNotification entity, CancellationToken cancellationToken = default)
        {
            Items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(InAppNotification entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<IEnumerable<InAppNotification>> FindAsync(System.Linq.Expressions.Expression<Func<InAppNotification, bool>> predicate, CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<InAppNotification>>(Items.AsQueryable().Where(predicate).ToList());
        public Task<IEnumerable<InAppNotification>> GetAllAsync(CancellationToken cancellationToken = default) => Task.FromResult<IEnumerable<InAppNotification>>(Items.ToList());
        public Task<InAppNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);
        public Task UpdateAsync(InAppNotification entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<(IReadOnlyList<InAppNotification> Items, int TotalCount)> GetPagedAsync(string recipientUserId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken) => Task.FromResult<(IReadOnlyList<InAppNotification>, int)>((Items, Items.Count));
        public Task<InAppNotification?> GetByIdForRecipientAsync(Guid id, string recipientUserId, CancellationToken cancellationToken) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id && x.RecipientUserId == recipientUserId));
        public Task<int> GetUnreadCountAsync(string recipientUserId, CancellationToken cancellationToken) => Task.FromResult(Items.Count(x => x.RecipientUserId == recipientUserId && x.ReadStatus == ReadStatus.Unread));
        public Task<InAppNotification?> FindByNotificationAndRecipientAsync(Guid notificationId, string recipientUserId, CancellationToken cancellationToken) => Task.FromResult(Items.FirstOrDefault(x => x.NotificationId == notificationId && x.RecipientUserId == recipientUserId));
        public Task<InAppNotification?> GetTrackedByIdForRecipientAsync(Guid id, string recipientUserId, CancellationToken cancellationToken) => Task.FromResult(Items.FirstOrDefault(x => x.Id == id && x.RecipientUserId == recipientUserId));
    }
}
