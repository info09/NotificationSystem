using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.NotificationChannelConfigs;

/// <summary>
/// Use case cập nhật Cấu hình kênh thông báo.
/// </summary>
public sealed class UpdateNotificationChannelConfigUseCase
{
    private readonly INotificationChannelConfigRepository _repository;

    /// <summary>
    /// Khởi tạo use case cập nhật Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="repository">Repository Cấu hình kênh thông báo.</param>
    public UpdateNotificationChannelConfigUseCase(INotificationChannelConfigRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý yêu cầu cập nhật Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="id">Mã định danh cấu hình.</param>
    /// <param name="request">Yêu cầu cập nhật.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả cập nhật cấu hình.</returns>
    public async Task<TemplateConfigurationAppResult<NotificationChannelConfigDto>> HandleAsync(
        Guid id,
        UpdateNotificationChannelConfigAppRequest? request,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Validation(
                new Dictionary<string, string[]> { ["id"] = ["Id is required."] });
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (request is null)
        {
            errors["request"] = ["Request body is required."];
            return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Validation(errors);
        }

        var displayName = request.DisplayName?.Trim();
        if (string.IsNullOrWhiteSpace(displayName))
        {
            errors["displayName"] = ["DisplayName is required."];
        }

        if (errors.Count > 0)
        {
            return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Validation(errors);
        }

        var config = await _repository.GetByIdAsync(id, cancellationToken);
        if (config is null)
        {
            return TemplateConfigurationAppResult<NotificationChannelConfigDto>.NotFound();
        }

        config.Update(displayName!, request.Description, request.IsEnabled ?? config.IsEnabled);
        await _repository.SaveChangesAsync(cancellationToken);

        return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Ok(NotificationChannelConfigMapping.ToDto(config));
    }
}
