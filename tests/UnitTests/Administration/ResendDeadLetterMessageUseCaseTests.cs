using Administration.Application;
using Administration.Application.Abstractions;
using Administration.Application.DeadLetterQueue;
using Administration.Domain;
using BuildingBlocks.SharedKernel.Notifications;

namespace UnitTests.Administration;

/// <summary>
/// Unit test cho use case gửi lại Thông điệp từ Hàng đợi thư chết.
/// </summary>
public sealed class ResendDeadLetterMessageUseCaseTests
{
    [Fact]
    public async Task HandleAsync_WithMissingIdempotencyKey_ReturnsValidationError()
    {
        var useCase = CreateUseCase(new FakeGateway(), new FakeResendRequestRepository(), new FakeAuditLogWriter(), new FakeNotificationResendStateService());

        var result = await useCase.HandleAsync(
            "notifications.email.dlq",
            "msg-1",
            new ResendDeadLetterMessageRequest(""),
            new AdministrationCallerContext("admin-1", ["SystemAdministrator"], "corr-1"),
            CancellationToken.None);

        Assert.Equal(AdministrationAppStatus.ValidationError, result.Status);
    }

    [Fact]
    public async Task HandleAsync_WithExistingIdempotencyKey_ReturnsDuplicate()
    {
        var repo = new FakeResendRequestRepository();
        var existing = DeadLetterResendRequestRecord.Create("idem-1", "admin-1", "notifications.email.dlq", "msg-1");
        existing.MarkCompleted(Guid.NewGuid());
        repo.Items.Add(existing);
        var audit = new FakeAuditLogWriter();
        var useCase = CreateUseCase(new FakeGateway { ResendResult = new DeadLetterResendGatewayResult("msg-1", Guid.NewGuid(), Guid.NewGuid(), "notifications.email.dlq") }, repo, audit, new FakeNotificationResendStateService());

        var result = await useCase.HandleAsync(
            "notifications.email.dlq",
            "msg-1",
            new ResendDeadLetterMessageRequest("idem-1"),
            new AdministrationCallerContext("admin-1", ["SystemAdministrator"], "corr-1"),
            CancellationToken.None);

        Assert.Equal(AdministrationAppStatus.Ok, result.Status);
        Assert.Equal("Duplicate", result.Value!.Status);
        Assert.Single(audit.Items);
    }

    [Fact]
    public async Task HandleAsync_WithMessageNotFound_ReturnsNotFoundAndWritesAudit()
    {
        var audit = new FakeAuditLogWriter();
        var useCase = CreateUseCase(new FakeGateway(), new FakeResendRequestRepository(), audit, new FakeNotificationResendStateService());

        var result = await useCase.HandleAsync(
            "notifications.email.dlq",
            "missing",
            new ResendDeadLetterMessageRequest("idem-2"),
            new AdministrationCallerContext("admin-1", ["SystemAdministrator"], "corr-1"),
            CancellationToken.None);

        Assert.Equal(AdministrationAppStatus.NotFound, result.Status);
        Assert.Single(audit.Items);
        Assert.Equal("NotFound", audit.Items[0].Result);
    }

    [Fact]
    public async Task HandleAsync_WithValidRequest_ResendsResetsNotificationAndWritesAudit()
    {
        var notificationId = Guid.NewGuid();
        var gateway = new FakeGateway { ResendResult = new DeadLetterResendGatewayResult("msg-1", Guid.NewGuid(), notificationId, "notifications.email.dlq") };
        var repo = new FakeResendRequestRepository();
        var audit = new FakeAuditLogWriter();
        var reset = new FakeNotificationResendStateService();
        var useCase = CreateUseCase(gateway, repo, audit, reset);

        var result = await useCase.HandleAsync(
            "notifications.email.dlq",
            "msg-1",
            new ResendDeadLetterMessageRequest("idem-3"),
            new AdministrationCallerContext("admin-1", ["BusinessOperator"], "corr-1"),
            CancellationToken.None);

        Assert.Equal(AdministrationAppStatus.Ok, result.Status);
        Assert.Equal("Resent", result.Value!.Status);
        Assert.Single(repo.Items);
        Assert.Single(audit.Items);
        Assert.Equal(notificationId, reset.ResetNotificationId);
        Assert.Equal("Success", audit.Items[0].Result);
    }

    private static ResendDeadLetterMessageUseCase CreateUseCase(
        IDeadLetterQueueGateway gateway,
        IDeadLetterResendRequestRepository resendRequestRepository,
        IAdministrationAuditLogWriter auditLogWriter,
        INotificationResendStateService notificationResendStateService)
        => new(gateway, resendRequestRepository, auditLogWriter, notificationResendStateService);

    private sealed class FakeGateway : IDeadLetterQueueGateway
    {
        public DeadLetterResendGatewayResult? ResendResult { get; set; }

        public Task<IReadOnlyList<DeadLetterQueueMessageDto>> GetMessagesAsync(string queueName, int limit, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<DeadLetterQueueMessageDto>>([]);

        public Task<DeadLetterResendGatewayResult?> ResendAsync(string queueName, string messageId, CancellationToken cancellationToken)
            => Task.FromResult(ResendResult);
    }

    private sealed class FakeResendRequestRepository : IDeadLetterResendRequestRepository
    {
        public List<DeadLetterResendRequestRecord> Items { get; } = [];

        public Task<DeadLetterResendRequestRecord?> GetByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken)
            => Task.FromResult(Items.FirstOrDefault(x => x.IdempotencyKey == idempotencyKey));

        public Task AddAsync(DeadLetterResendRequestRecord record, CancellationToken cancellationToken)
        {
            Items.Add(record);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuditLogWriter : IAdministrationAuditLogWriter
    {
        public List<AuditLogRecord> Items { get; } = [];

        public Task WriteAsync(AuditLogRecord record, CancellationToken cancellationToken)
        {
            Items.Add(record);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeNotificationResendStateService : INotificationResendStateService
    {
        public Guid? ResetNotificationId { get; private set; }

        public Task<bool> ResetForResendAsync(Guid notificationId, CancellationToken cancellationToken)
        {
            ResetNotificationId = notificationId;
            return Task.FromResult(true);
        }
    }
}
