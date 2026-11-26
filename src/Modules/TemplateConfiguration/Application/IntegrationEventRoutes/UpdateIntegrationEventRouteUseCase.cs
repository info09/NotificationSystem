using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.IntegrationEventRoutes;

/// <summary>
/// Use case cập nhật Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class UpdateIntegrationEventRouteUseCase
{
    private static readonly HashSet<string> AllowedChannels = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email",
        "InApp"
    };

    private readonly IIntegrationEventRouteRepository _repository;
    private readonly INotificationTemplateRepository _templateRepository;

    /// <summary>
    /// Khởi tạo use case cập nhật Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="repository">Repository Định tuyến sự kiện tích hợp.</param>
    /// <param name="templateRepository">Repository Mẫu thông báo.</param>
    public UpdateIntegrationEventRouteUseCase(
        IIntegrationEventRouteRepository repository,
        INotificationTemplateRepository templateRepository)
    {
        _repository = repository;
        _templateRepository = templateRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu cập nhật Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="id">Mã định danh định tuyến.</param>
    /// <param name="request">Yêu cầu cập nhật.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả cập nhật định tuyến.</returns>
    public async Task<TemplateConfigurationAppResult<IntegrationEventRouteDto>> HandleAsync(
        Guid id,
        UpdateIntegrationEventRouteAppRequest? request,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return TemplateConfigurationAppResult<IntegrationEventRouteDto>.Validation(
                new Dictionary<string, string[]> { ["id"] = ["Id is required."] });
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return TemplateConfigurationAppResult<IntegrationEventRouteDto>.Validation(errors);
        }

        var eventType = request.EventType?.Trim();
        var channel = request.Channel?.Trim();

        if (string.IsNullOrWhiteSpace(eventType))
        {
            errors["eventType"] = ["EventType is required."];
        }
        else if (eventType.Length > 256)
        {
            errors["eventType"] = ["EventType is invalid."];
        }

        if (request.NotificationTemplateId.HasValue && request.NotificationTemplateId.Value == Guid.Empty)
        {
            errors["notificationTemplateId"] = ["NotificationTemplateId is invalid."];
        }

        if (!request.NotificationTemplateId.HasValue && string.IsNullOrWhiteSpace(channel))
        {
            errors["mapping"] = ["NotificationTemplateId or Channel is required."];
        }

        if (!string.IsNullOrWhiteSpace(channel) && !AllowedChannels.Contains(channel))
        {
            errors["channel"] = ["Channel is invalid."];
        }

        if (errors.Count > 0)
        {
            return TemplateConfigurationAppResult<IntegrationEventRouteDto>.Validation(errors);
        }

        var route = await _repository.GetByIdAsync(id, cancellationToken);
        if (route is null)
        {
            return TemplateConfigurationAppResult<IntegrationEventRouteDto>.NotFound();
        }

        if (await _repository.ExistsByEventTypeAsync(eventType!, id, cancellationToken))
        {
            return TemplateConfigurationAppResult<IntegrationEventRouteDto>.Conflict();
        }

        if (request.NotificationTemplateId.HasValue)
        {
            var template = await _templateRepository.GetByIdAsync(request.NotificationTemplateId.Value, cancellationToken);
            if (template is null)
            {
                return TemplateConfigurationAppResult<IntegrationEventRouteDto>.Validation(
                    new Dictionary<string, string[]> { ["notificationTemplateId"] = ["NotificationTemplate not found."] });
            }
        }

        var normalizedChannel = string.IsNullOrWhiteSpace(channel) ? null : NormalizeChannel(channel);
        route.Update(
            eventType!,
            request.NotificationTemplateId,
            normalizedChannel,
            request.Description,
            request.IsActive ?? route.IsActive);

        await _repository.SaveChangesAsync(cancellationToken);

        return TemplateConfigurationAppResult<IntegrationEventRouteDto>.Ok(IntegrationEventRouteMapping.ToDto(route));
    }

    private static string NormalizeChannel(string channel) =>
        string.Equals(channel, "InApp", StringComparison.OrdinalIgnoreCase) ? "InApp" : "Email";
}
