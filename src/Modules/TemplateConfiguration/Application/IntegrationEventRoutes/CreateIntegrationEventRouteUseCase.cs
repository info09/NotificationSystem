using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Domain.IntegrationEventRoutes;

namespace TemplateConfiguration.Application.IntegrationEventRoutes;

/// <summary>
/// Use case tạo Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class CreateIntegrationEventRouteUseCase
{
    private static readonly HashSet<string> AllowedChannels = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email",
        "InApp"
    };

    private readonly IIntegrationEventRouteRepository _repository;
    private readonly INotificationTemplateRepository _templateRepository;

    /// <summary>
    /// Khởi tạo use case tạo Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="repository">Repository Định tuyến sự kiện tích hợp.</param>
    /// <param name="templateRepository">Repository Mẫu thông báo.</param>
    public CreateIntegrationEventRouteUseCase(
        IIntegrationEventRouteRepository repository,
        INotificationTemplateRepository templateRepository)
    {
        _repository = repository;
        _templateRepository = templateRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu tạo Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="request">Yêu cầu tạo định tuyến.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tạo định tuyến.</returns>
    public async Task<TemplateConfigurationAppResult<IntegrationEventRouteDto>> HandleAsync(
        CreateIntegrationEventRouteAppRequest? request,
        CancellationToken cancellationToken)
    {
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

        if (await _repository.ExistsByEventTypeAsync(eventType!, null, cancellationToken))
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
        var route = IntegrationEventRoute.Create(
            Guid.NewGuid(),
            eventType!,
            request.NotificationTemplateId,
            normalizedChannel,
            request.Description,
            request.IsActive ?? true);

        await _repository.AddAsync(route, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return TemplateConfigurationAppResult<IntegrationEventRouteDto>.Created(IntegrationEventRouteMapping.ToDto(route));
    }

    private static string NormalizeChannel(string channel) =>
        string.Equals(channel, "InApp", StringComparison.OrdinalIgnoreCase) ? "InApp" : "Email";
}
