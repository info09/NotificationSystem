using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Domain.NotificationTemplates;

namespace TemplateConfiguration.Application.NotificationTemplates;

/// <summary>
/// Use case tạo Mẫu thông báo.
/// </summary>
public sealed class CreateNotificationTemplateUseCase
{
    private static readonly HashSet<string> AllowedChannels = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email",
        "InApp"
    };

    private readonly INotificationTemplateRepository _notificationTemplateRepository;

    /// <summary>
    /// Khởi tạo use case tạo Mẫu thông báo.
    /// </summary>
    /// <param name="notificationTemplateRepository">Repository Mẫu thông báo.</param>
    public CreateNotificationTemplateUseCase(INotificationTemplateRepository notificationTemplateRepository)
    {
        _notificationTemplateRepository = notificationTemplateRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu tạo Mẫu thông báo.
    /// </summary>
    /// <param name="request">Yêu cầu tạo mẫu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tạo Mẫu thông báo.</returns>
    public async Task<TemplateConfigurationAppResult<NotificationTemplateDto>> HandleAsync(
        CreateNotificationTemplateAppRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return TemplateConfigurationAppResult<NotificationTemplateDto>.Validation(errors);
        }

        var templateKey = request.TemplateKey?.Trim();
        var name = request.Name?.Trim();
        var channel = request.Channel?.Trim();
        var body = request.Body?.Trim();

        if (string.IsNullOrWhiteSpace(templateKey))
        {
            errors["templateKey"] = ["TemplateKey is required."];
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            errors["name"] = ["Name is required."];
        }

        if (string.IsNullOrWhiteSpace(channel))
        {
            errors["channel"] = ["Channel is required."];
        }
        else if (!AllowedChannels.Contains(channel))
        {
            errors["channel"] = ["Channel is invalid."];
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            errors["body"] = ["Body is required."];
        }

        if (errors.Count > 0)
        {
            return TemplateConfigurationAppResult<NotificationTemplateDto>.Validation(errors);
        }

        var normalizedChannel = NormalizeChannel(channel!);
        if (await _notificationTemplateRepository.ExistsByTemplateKeyAsync(templateKey!, null, cancellationToken))
        {
            return TemplateConfigurationAppResult<NotificationTemplateDto>.Conflict();
        }

        var template = NotificationTemplate.Create(
            Guid.NewGuid(),
            templateKey!,
            name!,
            request.Description,
            normalizedChannel,
            request.Subject,
            body!,
            request.IsActive ?? true);

        await _notificationTemplateRepository.AddAsync(template, cancellationToken);
        await _notificationTemplateRepository.SaveChangesAsync(cancellationToken);

        return TemplateConfigurationAppResult<NotificationTemplateDto>.Created(NotificationTemplateMapping.ToDto(template));
    }

    private static string NormalizeChannel(string channel) =>
        string.Equals(channel, "InApp", StringComparison.OrdinalIgnoreCase) ? "InApp" : "Email";
}
