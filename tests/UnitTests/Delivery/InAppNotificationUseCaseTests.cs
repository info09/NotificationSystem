using Delivery.Application.Abstractions;
using Delivery.Application.InAppNotifications;
using Delivery.Application.InAppNotifications.CreateInAppNotification;
using Delivery.Application.InAppNotifications.GetInAppNotificationById;
using Delivery.Application.InAppNotifications.GetInAppNotifications;
using Delivery.Application.InAppNotifications.GetUnreadInAppNotificationCount;
using Delivery.Application.InAppNotifications.MarkInAppNotificationAsRead;
using Delivery.Domain.InAppNotifications;

namespace UnitTests.Delivery;

/// <summary>
/// Unit test cho các use case Thông báo trong ứng dụng.
/// </summary>
public sealed class InAppNotificationUseCaseTests
{
    [Fact]
    public async Task GetInAppNotifications_WithInvalidPage_ReturnsValidationError()
    {
        var useCase = new GetInAppNotificationsUseCase(new FakeInAppNotificationRepository());

        var result = await useCase.HandleAsync(new GetInAppNotificationsQuery("user-1", false, 0, 20), CancellationToken.None);

        Assert.Equal(InAppNotificationAppStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task GetInAppNotifications_WithValidQuery_ReturnsPagedItems()
    {
        var repository = new FakeInAppNotificationRepository();
        repository.Seed(InAppNotification.Create(Guid.NewGuid(), Guid.NewGuid(), "user-1", "A", "B"));

        var useCase = new GetInAppNotificationsUseCase(repository);
        var result = await useCase.HandleAsync(new GetInAppNotificationsQuery("user-1", false, 1, 20), CancellationToken.None);

        Assert.Equal(InAppNotificationAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!.Items);
    }

    [Fact]
    public async Task GetById_WhenNotOwned_ReturnsNotFound()
    {
        var repository = new FakeInAppNotificationRepository();
        var entity = InAppNotification.Create(Guid.NewGuid(), Guid.NewGuid(), "user-1", "A", "B");
        repository.Seed(entity);

        var useCase = new GetInAppNotificationByIdUseCase(repository);
        var result = await useCase.HandleAsync(new GetInAppNotificationByIdQuery(entity.Id, "user-2"), CancellationToken.None);

        Assert.Equal(InAppNotificationAppStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task GetUnreadCount_ReturnsCount()
    {
        var repository = new FakeInAppNotificationRepository();
        repository.Seed(InAppNotification.Create(Guid.NewGuid(), Guid.NewGuid(), "user-1", "A", "B"));
        repository.Seed(InAppNotification.Create(Guid.NewGuid(), Guid.NewGuid(), "user-1", "A2", "B2"));

        var useCase = new GetUnreadInAppNotificationCountUseCase(repository);
        var result = await useCase.HandleAsync(new GetUnreadInAppNotificationCountQuery("user-1"), CancellationToken.None);

        Assert.Equal(InAppNotificationAppStatus.Ok, result.Status);
        Assert.Equal(2, result.Value!.UnreadCount);
    }

    [Fact]
    public async Task Create_WhenDuplicateExists_ReturnsExisting()
    {
        var repository = new FakeInAppNotificationRepository();
        var notificationId = Guid.NewGuid();
        var existing = InAppNotification.Create(Guid.NewGuid(), notificationId, "user-1", "A", "B");
        repository.Seed(existing);

        var useCase = new CreateInAppNotificationUseCase(repository);
        var result = await useCase.HandleAsync(new CreateInAppNotificationCommand(notificationId, "user-1", "A", "B"), CancellationToken.None);

        Assert.Equal(InAppNotificationAppStatus.Ok, result.Status);
        Assert.Equal(existing.Id, result.Value!.Id);
        Assert.Equal(1, repository.Items.Count);
    }

    [Fact]
    public async Task MarkAsRead_WhenOwned_SetsReadStatus()
    {
        var repository = new FakeInAppNotificationRepository();
        var entity = InAppNotification.Create(Guid.NewGuid(), Guid.NewGuid(), "user-1", "A", "B");
        repository.Seed(entity);

        var useCase = new MarkInAppNotificationAsReadUseCase(repository);
        var result = await useCase.HandleAsync(new MarkInAppNotificationAsReadCommand(entity.Id, "user-1"), CancellationToken.None);

        Assert.Equal(InAppNotificationAppStatus.Ok, result.Status);
        Assert.Equal("Read", result.Value!.ReadStatus);
        Assert.NotNull(result.Value.ReadAtUtc);
    }

    [Fact]
    public async Task MarkAsRead_WhenNotOwned_ReturnsNotFound()
    {
        var repository = new FakeInAppNotificationRepository();
        var entity = InAppNotification.Create(Guid.NewGuid(), Guid.NewGuid(), "user-1", "A", "B");
        repository.Seed(entity);

        var useCase = new MarkInAppNotificationAsReadUseCase(repository);
        var result = await useCase.HandleAsync(new MarkInAppNotificationAsReadCommand(entity.Id, "user-2"), CancellationToken.None);

        Assert.Equal(InAppNotificationAppStatus.NotFound, result.Status);
    }

    private sealed class FakeInAppNotificationRepository : IInAppNotificationRepository
    {
        public List<InAppNotification> Items { get; } = [];

        public void Seed(InAppNotification entity) => Items.Add(entity);

        public Task<InAppNotification> AddAsync(InAppNotification entity, CancellationToken cancellationToken = default)
        {
            Items.Add(entity);
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(InAppNotification entity, CancellationToken cancellationToken = default)
        {
            Items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<InAppNotification>> FindAsync(System.Linq.Expressions.Expression<Func<InAppNotification, bool>> predicate, CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<InAppNotification>>(Items.AsQueryable().Where(predicate).ToList());

        public Task<IEnumerable<InAppNotification>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<InAppNotification>>(Items.ToList());

        public Task<InAppNotification?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(0);

        public Task UpdateAsync(InAppNotification entity, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<(IReadOnlyList<InAppNotification> Items, int TotalCount)> GetPagedAsync(string recipientUserId, bool unreadOnly, int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = Items.Where(x => x.RecipientUserId == recipientUserId);
            if (unreadOnly)
            {
                query = query.Where(x => x.ReadStatus == ReadStatus.Unread);
            }

            var list = query.OrderByDescending(x => x.CreatedAtUtc).ToList();
            var paged = list.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult<(IReadOnlyList<InAppNotification>, int)>((paged, list.Count));
        }

        public Task<InAppNotification?> GetByIdForRecipientAsync(Guid id, string recipientUserId, CancellationToken cancellationToken)
            => Task.FromResult(Items.FirstOrDefault(x => x.Id == id && x.RecipientUserId == recipientUserId));

        public Task<int> GetUnreadCountAsync(string recipientUserId, CancellationToken cancellationToken)
            => Task.FromResult(Items.Count(x => x.RecipientUserId == recipientUserId && x.ReadStatus == ReadStatus.Unread));

        public Task<InAppNotification?> FindByNotificationAndRecipientAsync(Guid notificationId, string recipientUserId, CancellationToken cancellationToken)
            => Task.FromResult(Items.FirstOrDefault(x => x.NotificationId == notificationId && x.RecipientUserId == recipientUserId));

        public Task<InAppNotification?> GetTrackedByIdForRecipientAsync(Guid id, string recipientUserId, CancellationToken cancellationToken)
            => Task.FromResult(Items.FirstOrDefault(x => x.Id == id && x.RecipientUserId == recipientUserId));
    }
}
