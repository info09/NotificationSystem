using TemplateConfiguration.Application;
using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Application.IntegrationEventRoutes;
using TemplateConfiguration.Domain.IntegrationEventRoutes;
using TemplateConfiguration.Domain.NotificationTemplates;

namespace UnitTests.TemplateConfiguration;

/// <summary>
/// Unit test cho use case Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class IntegrationEventRouteUseCaseTests
{
    /// <summary>
    /// Kiểm tra tạo định tuyến hợp lệ trả về Created.
    /// </summary>
    [Fact]
    public async Task Create_WithValidRequest_ReturnsCreated()
    {
        var template = NotificationTemplate.Create(Guid.NewGuid(), "order-created", "Order Created", null, "Email", "Subject", "Body", true);
        var repository = new FakeIntegrationEventRouteRepository();
        var templateRepository = new FakeNotificationTemplateRepository(template);
        var useCase = new CreateIntegrationEventRouteUseCase(repository, templateRepository);

        var result = await useCase.HandleAsync(
            new CreateIntegrationEventRouteAppRequest("OrderCreated", template.Id, "Email", "Mô tả", true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.Single(repository.Items);
    }

    /// <summary>
    /// Kiểm tra tạo định tuyến thiếu mapping trả về ValidationError.
    /// </summary>
    [Fact]
    public async Task Create_WithoutMapping_ReturnsValidationError()
    {
        var repository = new FakeIntegrationEventRouteRepository();
        var templateRepository = new FakeNotificationTemplateRepository();
        var useCase = new CreateIntegrationEventRouteUseCase(repository, templateRepository);

        var result = await useCase.HandleAsync(
            new CreateIntegrationEventRouteAppRequest("OrderCreated", null, null, null, true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.True(result.ValidationErrors.ContainsKey("mapping"));
    }

    /// <summary>
    /// Kiểm tra tạo định tuyến trùng EventType trả về Conflict.
    /// </summary>
    [Fact]
    public async Task Create_WithDuplicateEventType_ReturnsConflict()
    {
        var existing = IntegrationEventRoute.Create(Guid.NewGuid(), "OrderCreated", Guid.NewGuid(), "Email", null, true);
        var repository = new FakeIntegrationEventRouteRepository(existing);
        var templateRepository = new FakeNotificationTemplateRepository();
        var useCase = new CreateIntegrationEventRouteUseCase(repository, templateRepository);

        var result = await useCase.HandleAsync(
            new CreateIntegrationEventRouteAppRequest("OrderCreated", Guid.NewGuid(), "Email", null, true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.Conflict, result.Status);
    }

    /// <summary>
    /// Kiểm tra tạo định tuyến với EventType không hợp lệ trả về ValidationError.
    /// </summary>
    [Fact]
    public async Task Create_WithInvalidEventType_ReturnsValidationError()
    {
        var repository = new FakeIntegrationEventRouteRepository();
        var templateRepository = new FakeNotificationTemplateRepository();
        var useCase = new CreateIntegrationEventRouteUseCase(repository, templateRepository);

        var result = await useCase.HandleAsync(
            new CreateIntegrationEventRouteAppRequest(" ", Guid.NewGuid(), "Email", null, true),
            CancellationToken.None);

        Assert.Equal(TemplateConfigurationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.True(result.ValidationErrors.ContainsKey("eventType"));
    }

    private sealed class FakeIntegrationEventRouteRepository : IIntegrationEventRouteRepository
    {
        public FakeIntegrationEventRouteRepository(params IntegrationEventRoute[] routes)
        {
            Items = routes.ToList();
        }

        public List<IntegrationEventRoute> Items { get; }

        public Task<IntegrationEventRoute?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(Items.FirstOrDefault(x => x.Id == id));

        public Task<(IReadOnlyList<IntegrationEventRoute> Items, int TotalCount)> GetPagedAsync(
            string? eventType,
            string? channel,
            bool? isActive,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            IEnumerable<IntegrationEventRoute> query = Items;
            if (!string.IsNullOrWhiteSpace(eventType))
            {
                query = query.Where(x => x.EventType == eventType);
            }

            if (!string.IsNullOrWhiteSpace(channel))
            {
                query = query.Where(x => x.Channel == channel);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            var list = query.ToList();
            return Task.FromResult(((IReadOnlyList<IntegrationEventRoute>)list, list.Count));
        }

        public Task<bool> ExistsByEventTypeAsync(string eventType, Guid? excludeId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.Any(x => x.EventType == eventType && (!excludeId.HasValue || x.Id != excludeId.Value)));

        public Task AddAsync(IntegrationEventRoute route, CancellationToken cancellationToken)
        {
            Items.Add(route);
            return Task.CompletedTask;
        }

        public void Remove(IntegrationEventRoute route)
        {
            Items.Remove(route);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
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
            CancellationToken cancellationToken) =>
            Task.FromResult(((IReadOnlyList<NotificationTemplate>)Items, Items.Count));

        public Task<bool> ExistsByTemplateKeyAsync(string templateKey, Guid? excludeId, CancellationToken cancellationToken) =>
            Task.FromResult(false);

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
