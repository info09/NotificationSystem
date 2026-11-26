using TemplateConfiguration.Application.Abstractions;
using TemplateConfiguration.Domain.NotificationChannelConfigs;

namespace TemplateConfiguration.Application.NotificationChannelConfigs;

/// <summary>
/// Use case tạo Cấu hình kênh thông báo.
/// </summary>
public sealed class CreateNotificationChannelConfigUseCase
{
    private static readonly HashSet<string> AllowedChannels = new(StringComparer.OrdinalIgnoreCase)
    {
        "Email",
        "InApp"
    };

    private readonly INotificationChannelConfigRepository _repository;

    /// <summary>
    /// Khởi tạo use case tạo Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="repository">Repository Cấu hình kênh thông báo.</param>
    public CreateNotificationChannelConfigUseCase(INotificationChannelConfigRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý yêu cầu tạo Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="request">Yêu cầu tạo cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả tạo cấu hình.</returns>
    public async Task<TemplateConfigurationAppResult<NotificationChannelConfigDto>> HandleAsync(
        CreateNotificationChannelConfigAppRequest? request,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Validation(errors);
        }

        var channel = request.Channel?.Trim();
        var displayName = request.DisplayName?.Trim();

        if (string.IsNullOrWhiteSpace(channel))
        {
            errors["channel"] = ["Channel is required."];
        }
        else if (!AllowedChannels.Contains(channel))
        {
            errors["channel"] = ["Channel is invalid."];
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors["displayName"] = ["DisplayName is required."];
        }

        if (errors.Count > 0)
        {
            return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Validation(errors);
        }

        var normalizedChannel = NormalizeChannel(channel!);
        if (await _repository.ExistsByChannelAsync(normalizedChannel, null, cancellationToken))
        {
            return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Conflict();
        }

        var config = NotificationChannelConfig.Create(
            Guid.NewGuid(),
            normalizedChannel,
            displayName!,
            request.Description,
            request.IsEnabled ?? true);

        await _repository.AddAsync(config, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Created(NotificationChannelConfigMapping.ToDto(config));
    }

    private static string NormalizeChannel(string channel) =>
        string.Equals(channel, "InApp", StringComparison.OrdinalIgnoreCase) ? "InApp" : "Email";
}
