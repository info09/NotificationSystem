using Administration.Application.DeadLetterQueue;

namespace UnitTests.Administration;

/// <summary>
/// Unit test cho use case truy vấn Hàng đợi thư chết.
/// </summary>
public sealed class GetDeadLetterMessagesUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WithInvalidQueue_ReturnsValidationError()
    {
        var useCase = new GetDeadLetterMessagesUseCase(new FakeGateway());

        var result = await useCase.HandleAsync("invalid.queue", 10, CancellationToken.None);

        Assert.Equal(Administration.Application.AdministrationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
    }

    [Fact]
    public async Task HandleAsync_WithValidQueue_ReturnsMessages()
    {
        var gateway = new FakeGateway
        {
            Messages =
            [
                new DeadLetterQueueMessageDto("notifications.email.dlq", "msg-1", Guid.NewGuid().ToString(), "Email", "notification.email.created", "NotificationCreated", "rejected", DateTime.UtcNow, 1, "{}")
            ]
        };
        var useCase = new GetDeadLetterMessagesUseCase(gateway);

        var result = await useCase.HandleAsync("notifications.email.dlq", 10, CancellationToken.None);

        Assert.Equal(Administration.Application.AdministrationAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!.Items);
    }

    private sealed class FakeGateway : IDeadLetterQueueGateway
    {
        public IReadOnlyList<DeadLetterQueueMessageDto> Messages { get; set; } = [];

        public Task<IReadOnlyList<DeadLetterQueueMessageDto>> GetMessagesAsync(string queueName, int limit, CancellationToken cancellationToken)
            => Task.FromResult(Messages);

        public Task<DeadLetterResendGatewayResult?> ResendAsync(string queueName, string messageId, CancellationToken cancellationToken)
            => Task.FromResult<DeadLetterResendGatewayResult?>(null);
    }
}
