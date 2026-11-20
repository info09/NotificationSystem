namespace BuildingBlocks.Observability.Correlation;

public sealed class CorrelationContext
{
    public string? CorrelationId { get; init; }
    public string? MessageId { get; init; }
    public string? NotificationId { get; init; }
    public string? TraceId { get; init; }
}
