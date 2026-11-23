namespace Administration.Application.DeadLetterQueue;

/// <summary>
/// Use case truy vấn danh sách Thông điệp trong Hàng đợi thư chết.
/// </summary>
public sealed class GetDeadLetterMessagesUseCase
{
    private static readonly HashSet<string> AllowedQueues = new(StringComparer.OrdinalIgnoreCase)
    {
        "notifications.email.dlq",
        "notifications.inapp.dlq"
    };

    private readonly IDeadLetterQueueGateway _gateway;

    /// <summary>
    /// Khởi tạo use case truy vấn Hàng đợi thư chết.
    /// </summary>
    /// <param name="gateway">Cổng truy cập Hàng đợi thư chết.</param>
    public GetDeadLetterMessagesUseCase(IDeadLetterQueueGateway gateway)
    {
        _gateway = gateway;
    }

    /// <summary>
    /// Xử lý truy vấn danh sách Thông điệp trong Hàng đợi thư chết.
    /// </summary>
    /// <param name="queueName">Tên Hàng đợi thư chết.</param>
    /// <param name="limit">Số lượng tối đa.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả truy vấn danh sách Thông điệp.</returns>
    public async Task<AdministrationAppResult<GetDeadLetterQueueMessagesResponse>> HandleAsync(
        string? queueName,
        int? limit,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (string.IsNullOrWhiteSpace(queueName) || !AllowedQueues.Contains(queueName))
        {
            errors["queueName"] = ["Queue name is not supported."];
        }

        var resolvedLimit = limit ?? 50;
        if (resolvedLimit < 1 || resolvedLimit > 100)
        {
            errors["limit"] = ["Limit must be between 1 and 100."];
        }

        if (errors.Count > 0)
        {
            return AdministrationAppResult<GetDeadLetterQueueMessagesResponse>.Validation(errors);
        }

        var items = await _gateway.GetMessagesAsync(queueName!, resolvedLimit, cancellationToken);
        var response = new GetDeadLetterQueueMessagesResponse(queueName!, items, items.Count);
        return AdministrationAppResult<GetDeadLetterQueueMessagesResponse>.Ok(response);
    }
}
