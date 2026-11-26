using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.NotificationTemplates;

/// <summary>
/// Use case cập nhật Mẫu thông báo.
/// </summary>
public sealed class UpdateNotificationTemplateUseCase
{
    private static readonly HashSet<string> AllowedChannels = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email",
        "InApp"
    };

    private readonly INotificationTemplateRepository _notificationTemplateRepository;

    /// <summary>
    /// Khởi tạo use case cập nhật Mẫu thông báo.
    /// </summary>
    /// <param name="notificationTemplateRepository">Repository Mẫu thông báo.</param>
    public UpdateNotificationTemplateUseCase(INotificationTemplateRepository notificationTemplateRepository)
    {
        _notificationTemplateRepository = notificationTemplateRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu cập nhật Mẫu thông báo.
    /// </summary>
    /// <param name="id">Mã định danh mẫu.</param>
    /// <param name="request">Yêu cầu cập nhật.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả cập nhật Mẫu thông báo.</returns>
    public async Task<TemplateConfigurationAppResult<NotificationTemplateDto>> HandleAsync(
        Guid id,
        UpdateNotificationTemplateAppRequest? request,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return TemplateConfigurationAppResult<NotificationTemplateDto>.Validation(
                new Dictionary<string, string[]> { ["id"] = ["Id is required."] });
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return TemplateConfigurationAppResult<NotificationTemplateDto>.Validation(errors);
        }

        var name = request.Name?.Trim();
        var channel = request.Channel?.Trim();
        var body = request.Body?.Trim();

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

        var template = await _notificationTemplateRepository.GetByIdAsync(id, cancellationToken);
        if (template is null)
        {
            return TemplateConfigurationAppResult<NotificationTemplateDto>.NotFound();
        }

        template.Update(
            name!,
            request.Description,
            NormalizeChannel(channel!),
            request.Subject,
            body!,
            request.IsActive ?? template.IsActive);

        await _notificationTemplateRepository.SaveChangesAsync(cancellationToken);
        return TemplateConfigurationAppResult<NotificationTemplateDto>.Ok(NotificationTemplateMapping.ToDto(template));
    }

    private static string NormalizeChannel(string channel) =>
        string.Equals(channel, "InApp", StringComparison.OrdinalIgnoreCase) ? "InApp" : "Email";
}
