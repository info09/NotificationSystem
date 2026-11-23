namespace BuildingBlocks.Observability.Correlation;

/// <summary>
/// Ngữ cảnh tương quan dùng để truyền các mã truy vết trong quá trình xử lý.
/// </summary>
public sealed class CorrelationContext
{
    /// <summary>
    /// Mã tương quan của request/luồng xử lý.
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// Mã định danh thông điệp liên quan.
    /// </summary>
    public string? MessageId { get; init; }

    /// <summary>
    /// Mã định danh Thông báo liên quan.
    /// </summary>
    public string? NotificationId { get; init; }

    /// <summary>
    /// Mã truy vết của hệ thống tracing.
    /// </summary>
    public string? TraceId { get; init; }
}
