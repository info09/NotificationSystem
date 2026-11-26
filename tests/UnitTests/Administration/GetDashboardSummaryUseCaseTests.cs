using Administration.Application;
using Administration.Application.Abstractions;
using Administration.Application.Dashboard;
using Administration.Application.DeadLetterQueue;
using BuildingBlocks.SharedKernel.Abstractions;

namespace UnitTests.Administration;

/// <summary>
/// Unit test cho use case tổng hợp dữ liệu Tổng quan.
/// </summary>
public sealed class GetDashboardSummaryUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WhenAllSourcesSucceed_ReturnsSummary()
    {
        var notificationReader = new FakeNotificationStatusSummaryReader(new Dictionary<string, int>
        {
            ["Pending"] = 2,
            ["Processing"] = 1,
            ["Sent"] = 5,
            ["Failed"] = 0
        });
        var dlqGateway = new FakeDeadLetterQueueGateway(new Dictionary<string, int>
        {
            ["notifications.email.dlq"] = 3,
            ["notifications.inapp.dlq"] = 0
        });
        var healthReader = new FakeDashboardHealthSummaryReader(new HealthSummaryDto("Healthy", "database:Healthy"));
        var useCase = new GetDashboardSummaryUseCase(notificationReader, dlqGateway, healthReader);

        var result = await useCase.HandleAsync(CancellationToken.None);

        Assert.Equal(AdministrationAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value!.Notifications.Pending);
        Assert.Equal(3, result.Value.DeadLetterQueues.Single(x => x.QueueName == "notifications.email.dlq").Count);
        Assert.Equal("Healthy", result.Value.Health.Status);
    }

    [Fact]
    public async Task HandleAsync_WhenNotificationSourceFails_ReturnsNotificationBlockError()
    {
        var notificationReader = new FakeNotificationStatusSummaryReader(new InvalidOperationException("boom"));
        var dlqGateway = new FakeDeadLetterQueueGateway(new Dictionary<string, int>
        {
            ["notifications.email.dlq"] = 1,
            ["notifications.inapp.dlq"] = 2
        });
        var healthReader = new FakeDashboardHealthSummaryReader(new HealthSummaryDto("Healthy", null));
        var useCase = new GetDashboardSummaryUseCase(notificationReader, dlqGateway, healthReader);

        var result = await useCase.HandleAsync(CancellationToken.None);

        Assert.Equal(AdministrationAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value!.Notifications.Error);
        Assert.Equal(2, result.Value.DeadLetterQueues.Count);
        Assert.Equal("Healthy", result.Value.Health.Status);
    }

    private sealed class FakeNotificationStatusSummaryReader : INotificationStatusSummaryReader
    {
        private readonly IReadOnlyDictionary<string, int>? _counts;
        private readonly Exception? _exception;

        public FakeNotificationStatusSummaryReader(IReadOnlyDictionary<string, int> counts)
        {
            _counts = counts;
        }

        public FakeNotificationStatusSummaryReader(Exception exception)
        {
            _exception = exception;
        }

        public Task<IReadOnlyDictionary<string, int>> GetStatusCountsAsync(CancellationToken cancellationToken)
        {
            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_counts!);
        }
    }

    private sealed class FakeDashboardHealthSummaryReader : IDashboardHealthSummaryReader
    {
        private readonly HealthSummaryDto _summary;

        public FakeDashboardHealthSummaryReader(HealthSummaryDto summary)
        {
            _summary = summary;
        }

        public Task<HealthSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
            => Task.FromResult(_summary);
    }

    private sealed class FakeDeadLetterQueueGateway : IDeadLetterQueueGateway
    {
        private readonly IReadOnlyDictionary<string, int> _counts;

        public FakeDeadLetterQueueGateway(IReadOnlyDictionary<string, int> counts)
        {
            _counts = counts;
        }

        public Task<IReadOnlyList<DeadLetterQueueMessageDto>> GetMessagesAsync(string queueName, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DeadLetterQueueMessageDto>>([]);

        public Task<int> GetMessageCountAsync(string queueName, CancellationToken cancellationToken)
            => Task.FromResult(_counts.TryGetValue(queueName, out var count) ? count : 0);

        public Task<DeadLetterResendGatewayResult?> ResendAsync(string queueName, string messageId, CancellationToken cancellationToken)
            => Task.FromResult<DeadLetterResendGatewayResult?>(null);
    }
}
