namespace BuildingBlocks.Observability.Correlation;

/// <summary>
/// Hằng số tên header và tên thuộc tính dùng cho cơ chế tương quan/truy vết.
/// </summary>
public static class CorrelationIdConstants
{
    /// <summary>
    /// Tên header HTTP chứa mã tương quan.
    /// </summary>
    public const string CorrelationIdHeaderName = "X-Correlation-Id";

    /// <summary>
    /// Tên header HTTP chứa mã thông điệp.
    /// </summary>
    public const string MessageIdHeaderName = "X-Message-Id";

    /// <summary>
    /// Tên header HTTP chứa mã Thông báo.
    /// </summary>
    public const string NotificationIdHeaderName = "X-Notification-Id";

    /// <summary>
    /// Tên thuộc tính log/ngữ cảnh cho mã tương quan.
    /// </summary>
    public const string CorrelationIdPropertyName = "CorrelationId";

    /// <summary>
    /// Tên thuộc tính log/ngữ cảnh cho mã thông điệp.
    /// </summary>
    public const string MessageIdPropertyName = "MessageId";

    /// <summary>
    /// Tên thuộc tính log/ngữ cảnh cho mã Thông báo.
    /// </summary>
    public const string NotificationIdPropertyName = "NotificationId";

    /// <summary>
    /// Tên thuộc tính log/ngữ cảnh cho mã truy vết.
    /// </summary>
    public const string TraceIdPropertyName = "TraceId";
}
