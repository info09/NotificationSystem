using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.NotificationChannelConfigs;

/// <summary>
/// Use case lấy chi tiết Cấu hình kênh thông báo.
/// </summary>
public sealed class GetNotificationChannelConfigByIdUseCase
{
    private readonly INotificationChannelConfigRepository _repository;

    /// <summary>
    /// Khởi tạo use case lấy chi tiết Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="repository">Repository Cấu hình kênh thông báo.</param>
    public GetNotificationChannelConfigByIdUseCase(INotificationChannelConfigRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="id">Mã định danh cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả chi tiết cấu hình.</returns>
    public async Task<TemplateConfigurationAppResult<NotificationChannelConfigDto>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Validation(
                new Dictionary<string, string[]> { ["id"] = ["Id is required."] });
        }

        var config = await _repository.GetByIdAsync(id, cancellationToken);
        if (config is null)
        {
            return TemplateConfigurationAppResult<NotificationChannelConfigDto>.NotFound();
        }

        return TemplateConfigurationAppResult<NotificationChannelConfigDto>.Ok(NotificationChannelConfigMapping.ToDto(config));
    }
}
