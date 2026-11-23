namespace IntegrationMessaging.Domain;

/// <summary>
/// Trạng thái xử lý của bản ghi Outbox.
/// </summary>
public enum OutboxMessageStatus
{
    /// <summary>
    /// Bản ghi Outbox đang chờ publish.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Bản ghi Outbox đã được publish thành công.
    /// </summary>
    Published = 1,

    /// <summary>
    /// Bản ghi Outbox publish thất bại.
    /// </summary>
    Failed = 2
}
