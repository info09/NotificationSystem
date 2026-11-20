namespace BuildingBlocks.Observability.Correlation;

public static class CorrelationIdConstants
{
    public const string CorrelationIdHeaderName = "X-Correlation-Id";
    public const string MessageIdHeaderName = "X-Message-Id";
    public const string NotificationIdHeaderName = "X-Notification-Id";

    public const string CorrelationIdPropertyName = "CorrelationId";
    public const string MessageIdPropertyName = "MessageId";
    public const string NotificationIdPropertyName = "NotificationId";
    public const string TraceIdPropertyName = "TraceId";
}
