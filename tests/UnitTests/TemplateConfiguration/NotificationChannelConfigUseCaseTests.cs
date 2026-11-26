using TemplateConfiguration.Application;
using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Application.NotificationChannelConfigs;
using TemplateConfiguration.Domain.NotificationChannelConfigs;

namespace UnitTests.TemplateConfiguration;

/// <summary>
/// Unit test cho use case Cấu hình kênh thông báo.
/// </summary>
public sealed class NotificationChannelConfigUseCaseTests
{
    /// <summary>
    /// Kiểm tra tạo cấu hình hợp lệ trả về Created.
    /// </summary>
    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var repository = new FakeNotificationChannelConfigRepository();
        var useCase = new CreateNotificationChannelConfigUseCase(repository);

        var result = await useCase.HandleAsync(
            new CreateNotificationChannelConfigAppRequest("Email", "Email", "Mô tả", true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Single(repository.Items);
    }

    /// <summary>
    /// Kiểm tra tạo trùng kênh trả về Conflict.
    /// </summary>
    [Fact]
    public async Task Create_WithDuplicateChannel_ReturnsConflict()
    {
        var existing = NotificationChannelConfig.Create(Guid.NewGuid(), "Email", "Email", null, true);
        var repository = new FakeNotificationChannelConfigRepository(existing);
        var useCase = new CreateNotificationChannelConfigUseCase(repository);

        var result = await useCase.HandleAsync(
            new CreateNotificationChannelConfigAppRequest("Email", "Email 2", null, true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.Conflict, result.Status);
    }

    /// <summary>
    /// Kiểm tra cập nhật cấu hình không tồn tại trả về NotFound.
    /// </summary>
    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        var repository = new FakeNotificationChannelConfigRepository();
        var useCase = new UpdateNotificationChannelConfigUseCase(repository);

        var result = await useCase.HandleAsync(
            Guid.NewGuid(),
            new UpdateNotificationChannelConfigAppRequest("Email", null, true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.NotFound, result.Status);
    }

    private sealed class FakeNotificationChannelConfigRepository : INotificationChannelConfigRepository
    {
        public FakeNotificationChannelConfigRepository(params NotificationChannelConfig[] configs)
        {
            Items = configs.ToList();
        }

        public List<NotificationChannelConfig> Items { get; }

        public Task<NotificationChannelConfig?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<(IReadOnlyList<NotificationChannelConfig> Items, int TotalCount)> GetPagedAsync(
            string? channel,
            bool? isEnabled,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            IEnumerable<NotificationChannelConfig> query = Items;
            if (!string.IsNullOrWhiteSpace(channel))
            {
                query = query.Where(x => x.Channel == channel);
            }

            if (isEnabled.HasValue)
            {
                query = query.Where(x => x.IsEnabled == isEnabled.Value);
            }

            var list = query.ToList();
            return Task.FromResult(((IReadOnlyList<NotificationChannelConfig>)list, list.Count));
        }

        public Task<bool> ExistsByChannelAsync(string channel, Guid? excludeId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(x => x.Channel == channel && (!excludeId.HasValue || x.Id != excludeId.Value)));

        public Task AddAsync(NotificationChannelConfig config, CancellationToken cancellationToken)
        {
            Items.Add(config);
            return Task.CompletedTask;
        }

        public void Remove(NotificationChannelConfig config)
        {
            Items.Remove(config);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
