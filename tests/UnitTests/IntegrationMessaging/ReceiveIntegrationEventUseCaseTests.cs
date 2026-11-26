using System.Text.Json;
using BuildingBlocks.SharedKernel.IntegrationEvents;
using IntegrationMessaging.Application;
using IntegrationMessaging.Application.Abstractions;
using IntegrationMessaging.Application.IntegrationEvents;
using IntegrationMessaging.Domain;

namespace UnitTests.IntegrationMessaging;

/// <summary>
/// Unit test cho use case tiếp nhận Sự kiện tích hợp.
/// </summary>
public sealed class ReceiveIntegrationEventUseCaseTests
{
    /// <summary>
    /// Kiểm tra tiếp nhận sự kiện hợp lệ tạo truy vết và ánh xạ Thông báo.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsCreatedAndCreatesNotification()
    {
        var repository = new FakeIntegrationEventRepository();
        var routeResolver = new FakeRouteResolver(new IntegrationEventRouteResolution("OrderCreated", Guid.NewGuid(), "Email"));
        var notificationCreator = new FakeNotificationCreator();
        var useCase = new ReceiveIntegrationEventUseCase(repository, routeResolver, notificationCreator);

        var result = await useCase.HandleAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(IntegrationMessagingAppStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.False(result.Value!.Duplicate);
        Assert.NotNull(result.Value.NotificationId);
        Assert.Single(repository.Items);
        Assert.Single(notificationCreator.Commands);
        Assert.Equal("user@example.com", notificationCreator.Commands[0].Recipient);
    }

    /// <summary>
    /// Kiểm tra tiếp nhận trùng MessageId trả về Ok với Duplicate.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithDuplicateMessageId_ReturnsOkDuplicate()
    {
        var messageId = Guid.NewGuid();
        var existing = IntegrationEvent.Create(Guid.NewGuid(), messageId, Guid.NewGuid(), "OrderCreated", DateTime.UtcNow, "order-service", null, "1.0", "{}", DateTime.UtcNow);
        var repository = new FakeIntegrationEventRepository(existing);
        var useCase = new ReceiveIntegrationEventUseCase(repository, new FakeRouteResolver(null), new FakeNotificationCreator());

        var result = await useCase.HandleAsync(CreateRequest(messageId: messageId), CancellationToken.None);

        Assert.Equal(IntegrationMessagingAppStatus.Ok, result.Status);
        Assert.True(result.Value!.Duplicate);
        Assert.Single(repository.Items);
    }

    /// <summary>
    /// Kiểm tra thiếu trường bắt buộc trả về ValidationError.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithMissingRequiredFields_ReturnsValidationError()
    {
        var useCase = new ReceiveIntegrationEventUseCase(new FakeIntegrationEventRepository(), new FakeRouteResolver(null), new FakeNotificationCreator());

        var result = await useCase.HandleAsync(
            new ReceiveIntegrationEventAppRequest(null, null, "", null, "", null, "", null),
            CancellationToken.None);

        Assert.Equal(IntegrationMessagingAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.True(result.ValidationErrors.ContainsKey("messageId"));
        Assert.True(result.ValidationErrors.ContainsKey("eventId"));
        Assert.True(result.ValidationErrors.ContainsKey("eventType"));
    }

    /// <summary>
    /// Kiểm tra không có Định tuyến sự kiện tích hợp hoạt động trả về ValidationError.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithoutActiveRoute_ReturnsValidationError()
    {
        var useCase = new ReceiveIntegrationEventUseCase(new FakeIntegrationEventRepository(), new FakeRouteResolver(null), new FakeNotificationCreator());

        var result = await useCase.HandleAsync(CreateRequest(), CancellationToken.None);

        Assert.Equal(IntegrationMessagingAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.True(result.ValidationErrors.ContainsKey("eventType"));
    }

    /// <summary>
    /// Kiểm tra payload thiếu người nhận trả về ValidationError.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithoutRecipientInPayload_ReturnsValidationError()
    {
        var routeResolver = new FakeRouteResolver(new IntegrationEventRouteResolution("OrderCreated", Guid.NewGuid(), "Email"));
        var useCase = new ReceiveIntegrationEventUseCase(new FakeIntegrationEventRepository(), routeResolver, new FakeNotificationCreator());
        var request = new ReceiveIntegrationEventAppRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "OrderCreated",
            DateTime.UtcNow,
            "order-service",
            "idem-1",
            "1.0",
            JsonSerializer.SerializeToElement(new { orderId = 1 }));

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(IntegrationMessagingAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.True(result.ValidationErrors.ContainsKey("payload.recipient"));
    }

    private static ReceiveIntegrationEventAppRequest CreateRequest(Guid? messageId = null)
    {
        return new ReceiveIntegrationEventAppRequest(
            messageId ?? Guid.NewGuid(),
            Guid.NewGuid(),
            "OrderCreated",
            DateTime.UtcNow,
            "order-service",
            "idem-1",
            "1.0",
            JsonSerializer.SerializeToElement(new { orderId = 1, recipient = "user@example.com", content = "Đơn hàng đã tạo" }));
    }

    private sealed class FakeIntegrationEventRepository : IIntegrationEventRepository
    {
        public FakeIntegrationEventRepository(params IntegrationEvent[] items)
        {
            Items = items.ToList();
        }

        public List<IntegrationEvent> Items { get; }

        public Task<IntegrationEvent?> GetByMessageIdAsync(Guid messageId, CancellationToken cancellationToken) =>
            Task.FromResult(Items.FirstOrDefault(x => x.MessageId == messageId));

        public Task AddAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            Items.Add(integrationEvent);
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class FakeRouteResolver : IIntegrationEventRouteResolver
    {
        private readonly IntegrationEventRouteResolution? _resolution;

        public FakeRouteResolver(IntegrationEventRouteResolution? resolution)
        {
            _resolution = resolution;
        }

        public Task<IntegrationEventRouteResolution?> ResolveAsync(string eventType, CancellationToken cancellationToken) =>
            Task.FromResult(_resolution);
    }

    private sealed class FakeNotificationCreator : IIntegrationNotificationCreator
    {
        public List<CreateIntegrationNotificationCommand> Commands { get; } = [];

        public Task<IntegrationNotificationCreationResult> CreateAsync(
            CreateIntegrationNotificationCommand command,
            CancellationToken cancellationToken)
        {
            Commands.Add(command);
            return Task.FromResult(new IntegrationNotificationCreationResult(Guid.NewGuid(), "Pending", true));
        }
    }
}
