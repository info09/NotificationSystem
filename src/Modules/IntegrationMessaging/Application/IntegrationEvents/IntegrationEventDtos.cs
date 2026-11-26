using System.Text.Json;

namespace IntegrationMessaging.Application.IntegrationEvents;

/// <summary>
/// Yêu cầu tiếp nhận Sự kiện tích hợp.
/// </summary>
/// <param name="MessageId">Mã định danh Thông điệp.</param>
/// <param name="EventId">Mã định danh Sự kiện tích hợp.</param>
/// <param name="EventType">Loại Sự kiện tích hợp.</param>
/// <param name="OccurredAt">Thời điểm phát sinh sự kiện.</param>
/// <param name="Source">Hệ thống nguồn.</param>
/// <param name="IdempotencyKey">Khóa chống trùng.</param>
/// <param name="Version">Phiên bản envelope.</param>
/// <param name="Payload">Nội dung payload.</param>
public sealed record ReceiveIntegrationEventAppRequest(
    Guid? MessageId,
    Guid? EventId,
    string? EventType,
    DateTime? OccurredAt,
    string? Source,
    string? IdempotencyKey,
    string? Version,
    JsonElement? Payload);

/// <summary>
/// Kết quả tiếp nhận Sự kiện tích hợp.
/// </summary>
/// <param name="Id">Mã định danh bản ghi.</param>
/// <param name="MessageId">Mã định danh Thông điệp.</param>
/// <param name="EventId">Mã định danh Sự kiện tích hợp.</param>
/// <param name="EventType">Loại Sự kiện tích hợp.</param>
/// <param name="ReceivedAtUtc">Thời điểm tiếp nhận.</param>
/// <param name="Duplicate">Đánh dấu sự kiện đã được tiếp nhận trước đó.</param>
/// <param name="NotificationId">Mã định danh Thông báo được tạo hoặc ánh xạ.</param>
public sealed record ReceiveIntegrationEventResponseDto(
    Guid Id,
    Guid MessageId,
    Guid EventId,
    string EventType,
    DateTime ReceivedAtUtc,
    bool Duplicate,
    Guid? NotificationId = null);
