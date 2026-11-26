using TemplateConfiguration.Application;
using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Application.NotificationTemplates;
using TemplateConfiguration.Domain.NotificationTemplates;

namespace UnitTests.TemplateConfiguration;

/// <summary>
/// Unit test cho use case Mẫu thông báo.
/// </summary>
public sealed class NotificationTemplateUseCaseTests
{
    /// <summary>
    /// Kiểm tra tạo mẫu hợp lệ trả về Created.
    /// </summary>
    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var repository = new FakeNotificationTemplateRepository();
        var useCase = new CreateNotificationTemplateUseCase(repository);

        var result = await useCase.HandleAsync(
            new CreateNotificationTemplateAppRequest("order-created", "Đơn hàng mới", "Mô tả", "Email", "Tiêu đề", "Nội dung", true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("order-created", result.Value!.TemplateKey);
        Assert.Single(repository.Items);
    }

    /// <summary>
    /// Kiểm tra tạo mẫu trùng khóa trả về Conflict.
    /// </summary>
    [Fact]
    public async Task Create_WithDuplicateTemplateKey_ReturnsConflict()
    {
        var existing = NotificationTemplate.Create(Guid.NewGuid(), "order-created", "Tên", null, "Email", null, "Body");
        var repository = new FakeNotificationTemplateRepository(existing);
        var useCase = new CreateNotificationTemplateUseCase(repository);

        var result = await useCase.HandleAsync(
            new CreateNotificationTemplateAppRequest("order-created", "Tên 2", null, "Email", null, "Body", true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.Conflict, result.Status);
    }

    /// <summary>
    /// Kiểm tra cập nhật mẫu không tồn tại trả về NotFound.
    /// </summary>
    [Fact]
    public async Task Update_WhenMissing_ReturnsNotFound()
    {
        var repository = new FakeNotificationTemplateRepository();
        var useCase = new UpdateNotificationTemplateUseCase(repository);

        var result = await useCase.HandleAsync(
            Guid.NewGuid(),
            new UpdateNotificationTemplateAppRequest("Tên", null, "Email", null, "Body", true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.NotFound, result.Status);
    }

    /// <summary>
    /// Kiểm tra xóa mẫu hợp lệ trả về Ok.
    /// </summary>
    [Fact]
    public async Task Delete_WithExistingTemplate_ReturnsOk()
    {
        var existing = NotificationTemplate.Create(Guid.NewGuid(), "order-created", "Tên", null, "Email", null, "Body");
        var repository = new FakeNotificationTemplateRepository(existing);
        var useCase = new DeleteNotificationTemplateUseCase(repository);

        var result = await useCase.HandleAsync(existing.Id, CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.Ok, result.Status);
        Assert.Empty(repository.Items);
    }

    /// <summary>
    /// Kiểm tra tạo mẫu thiếu dữ liệu bắt buộc trả về ValidationError.
    /// </summary>
    [Fact]
    public async Task Create_WithInvalidRequest_ReturnsValidationError()
    {
        var repository = new FakeNotificationTemplateRepository();
        var useCase = new CreateNotificationTemplateUseCase(repository);

        var result = await useCase.HandleAsync(
            new CreateNotificationTemplateAppRequest("", "", null, "SMS", null, "", true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
    }

    private sealed class FakeNotificationTemplateRepository : INotificationTemplateRepository
    {
        public FakeNotificationTemplateRepository(params NotificationTemplate[] templates)
        {
            Items = templates.ToList();
        }

        public List<NotificationTemplate> Items { get; }

        public Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<(IReadOnlyList<NotificationTemplate> Items, int TotalCount)> GetPagedAsync(
            string? channel,
            bool? isActive,
            string? keyword,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            IEnumerable<NotificationTemplate> query = Items;
            if (!string.IsNullOrWhiteSpace(channel))
            {
                query = query.Where(x => x.Channel == channel);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            var list = query.ToList();
            return Task.FromResult(((IReadOnlyList<NotificationTemplate>)list, list.Count));
        }

        public Task<bool> ExistsByTemplateKeyAsync(string templateKey, Guid? excludeId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(x => x.TemplateKey == templateKey && (!excludeId.HasValue || x.Id != excludeId.Value)));

        public Task AddAsync(NotificationTemplate template, CancellationToken cancellationToken)
        {
            Items.Add(template);
            return Task.CompletedTask;
        }

        public void Remove(NotificationTemplate template)
        {
            Items.Remove(template);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
