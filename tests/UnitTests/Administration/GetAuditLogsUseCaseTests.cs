using Administration.Application;
using Administration.Application.Abstractions;
using Administration.Application.AuditLogs;
using Administration.Domain;

namespace UnitTests.Administration;

/// <summary>
/// Unit test cho use case truy vấn Nhật ký kiểm toán.
/// </summary>
public sealed class GetAuditLogsUseCaseTests
{
    /// <summary>
    /// Kiểm tra truy vấn hợp lệ trả về danh sách Nhật ký kiểm toán.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithValidRequest_ReturnsPagedItems()
    {
        var record = AuditLogRecord.Create("admin-1", "SystemAdministrator", "CreateNotificationTemplate", "NotificationTemplate", Guid.NewGuid().ToString(), "Success");
        var useCase = new GetAuditLogsUseCase(new FakeAuditLogRecordRepository(record));

        var result = await useCase.HandleAsync(null, null, null, null, null, 1, 20, CancellationToken.None);

        Assert.Equal(AdministrationAppStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Single(result.Value!.Items);
        Assert.Equal("CreateNotificationTemplate", result.Value.Items[0].Action);
    }

    /// <summary>
    /// Kiểm tra pageSize không hợp lệ trả về ValidationError.
    /// </summary>
    [Fact]
    public async Task HandleAsync_WithInvalidPageSize_ReturnsValidationError()
    {
        var useCase = new GetAuditLogsUseCase(new FakeAuditLogRecordRepository());

        var result = await useCase.HandleAsync(null, null, null, null, null, 1, 0, CancellationToken.None);

        Assert.Equal(AdministrationAppStatus.ValidationError, result.Status);
        Assert.NotNull(result.ValidationErrors);
        Assert.True(result.ValidationErrors.ContainsKey("pageSize"));
    }

    private sealed class FakeAuditLogRecordRepository : IAuditLogRecordRepository
    {
        private readonly IReadOnlyList<AuditLogRecord> _items;

        public FakeAuditLogRecordRepository(params AuditLogRecord[] items)
        {
            _items = items;
        }

        public Task<(IReadOnlyList<AuditLogRecord> Items, int TotalCount)> GetPagedAsync(
            string? action,
            string? targetType,
            string? actorUserId,
            DateTime? fromUtc,
            DateTime? toUtc,
            int page,
            int pageSize,
            CancellationToken cancellationToken)
        {
            return Task.FromResult((_items, _items.Count));
        }
    }
}
