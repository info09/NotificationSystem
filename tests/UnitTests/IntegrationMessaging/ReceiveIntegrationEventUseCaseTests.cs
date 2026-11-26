using System.Text.Json;
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
    /// Kiểm tra tiếp nhận sự kiện hợp lệ trả về Created.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsCreated()
    {
        var repository = new FakeIntegrationEventRepository();
        var useCase = new ReceiveIntegrationEventUseCase(repository);
        var request = CreateRequest();

        var result = await useCase.HandleAsync(request, CancellationToken.None);

        Assert.Equal(IntegrationMessagingAppStatus.Created, result.Status);
        Assert.NotNull(result.Value);
        Assert.False(result.Value!.Duplicate);
        Assert.Single(repository.Items);
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
        var useCase = new ReceiveIntegrationEventUseCase(repository);
        var request = CreateRequest(messageId: messageId);

        var result = await useCase.HandleAsync(request, CancellationToken.None);

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
        var repository = new FakeIntegrationEventRepository();
        var useCase = new ReceiveIntegrationEventUseCase(repository);

        var result = await useCase.HandleAsync(
            new ReceiveIntegrationEventAppRequest(null, null, "", null, "", null, "", null),
            CancellationToken.None);

        Assert.Equal(IntegrationMessagingAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.True(result.ValidationErrors.ContainsKey("messageId"));
        Assert.True(result.ValidationErrors.ContainsKey("eventId"));
        Assert.True(result.ValidationErrors.ContainsKey("eventType"));
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
            JsonSerializer.SerializeToElement(new { orderId = 1 }));
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
}
