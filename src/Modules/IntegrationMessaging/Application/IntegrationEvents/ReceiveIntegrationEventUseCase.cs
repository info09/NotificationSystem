using System.Text.Json;
using IntegrationMessaging.Application.Abstractions;
using IntegrationMessaging.Domain;

namespace IntegrationMessaging.Application.IntegrationEvents;

/// <summary>
/// Use case tiếp nhận và ghi nhận truy vết Sự kiện tích hợp.
/// </summary>
public sealed class ReceiveIntegrationEventUseCase
{
    private readonly IIntegrationEventRepository _repository;

    /// <summary>
    /// Khởi tạo use case tiếp nhận Sự kiện tích hợp.
    /// </summary>
    /// <param name="repository">Repository Sự kiện tích hợp.</param>
    public ReceiveIntegrationEventUseCase(IIntegrationEventRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý yêu cầu tiếp nhận Sự kiện tích hợp.
    /// </summary>
    /// <param name="request">Yêu cầu tiếp nhận.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tiếp nhận Sự kiện tích hợp.</returns>
    public async Task<IntegrationMessagingAppResult<ReceiveIntegrationEventResponseDto>> HandleAsync(
        ReceiveIntegrationEventAppRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return IntegrationMessagingAppResult<ReceiveIntegrationEventResponseDto>.Validation(errors);
        }

        if (!request.MessageId.HasValue || request.MessageId.Value == Guid.Empty)
        {
            errors["messageId"] = ["MessageId is required."];
        }

        if (!request.EventId.HasValue || request.EventId.Value == Guid.Empty)
        {
            errors["eventId"] = ["EventId is required."];
        }

        if (string.IsNullOrWhiteSpace(request.EventType))
        {
            errors["eventType"] = ["EventType is required."];
        }

        if (!request.OccurredAt.HasValue)
        {
            errors["occurredAt"] = ["OccurredAt is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Source))
        {
            errors["source"] = ["Source is required."];
        }

        if (string.IsNullOrWhiteSpace(request.Version))
        {
            errors["version"] = ["Version is required."];
        }

        var payloadJson = GetPayloadJson(request.Payload);
        if (payloadJson is null)
        {
            errors["payload"] = ["Payload is required."];
        }

        if (errors.Count > 0)
        {
            return IntegrationMessagingAppResult<ReceiveIntegrationEventResponseDto>.Validation(errors);
        }

        var messageId = request.MessageId!.Value;
        var existing = await _repository.GetByMessageIdAsync(messageId, cancellationToken);
        if (existing is not null)
        {
            return IntegrationMessagingAppResult<ReceiveIntegrationEventResponseDto>.Ok(
                new ReceiveIntegrationEventResponseDto(
                    existing.Id,
                    existing.MessageId,
                    existing.EventId,
                    existing.EventType,
                    existing.ReceivedAtUtc,
                    true));
        }

        var now = DateTime.UtcNow;
        var integrationEvent = IntegrationEvent.Create(
            Guid.NewGuid(),
            messageId,
            request.EventId!.Value,
            request.EventType!.Trim(),
            request.OccurredAt!.Value.ToUniversalTime(),
            request.Source!.Trim(),
            request.IdempotencyKey,
            request.Version!.Trim(),
            payloadJson!,
            now);

        await _repository.AddAsync(integrationEvent, cancellationToken);

        try
        {
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            var duplicated = await _repository.GetByMessageIdAsync(messageId, cancellationToken);
            if (duplicated is not null)
            {
                return IntegrationMessagingAppResult<ReceiveIntegrationEventResponseDto>.Ok(
                    new ReceiveIntegrationEventResponseDto(
                        duplicated.Id,
                        duplicated.MessageId,
                        duplicated.EventId,
                        duplicated.EventType,
                        duplicated.ReceivedAtUtc,
                        true));
            }

            throw;
        }

        return IntegrationMessagingAppResult<ReceiveIntegrationEventResponseDto>.Created(
            new ReceiveIntegrationEventResponseDto(
                integrationEvent.Id,
                integrationEvent.MessageId,
                integrationEvent.EventId,
                integrationEvent.EventType,
                integrationEvent.ReceivedAtUtc,
                false));
    }

    private static string? GetPayloadJson(JsonElement? payload)
    {
        if (!payload.HasValue)
        {
            return null;
        }

        var value = payload.Value;
        if (value.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return null;
        }

        return value.GetRawText();
    }
}
