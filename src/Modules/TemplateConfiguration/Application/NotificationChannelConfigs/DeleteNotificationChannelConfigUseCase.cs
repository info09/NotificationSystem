using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.NotificationChannelConfigs;

/// <summary>
/// Use case xóa Cấu hình kênh thông báo.
/// </summary>
public sealed class DeleteNotificationChannelConfigUseCase
{
    private readonly INotificationChannelConfigRepository _repository;

    /// <summary>
    /// Khởi tạo use case xóa Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="repository">Repository Cấu hình kênh thông báo.</param>
    public DeleteNotificationChannelConfigUseCase(INotificationChannelConfigRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý yêu cầu xóa Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="id">Mã định danh cấu hình.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả xóa cấu hình.</returns>
    public async Task<TemplateConfigurationAppResult<bool>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return TemplateConfigurationAppResult<bool>.Validation(
                new Dictionary<string, string[]> { ["id"] = ["Id is required."] });
        }

        var config = await _repository.GetByIdAsync(id, cancellationToken);
        if (config is null)
        {
            return TemplateConfigurationAppResult<bool>.NotFound();
        }

        _repository.Remove(config);
        await _repository.SaveChangesAsync(cancellationToken);
        return TemplateConfigurationAppResult<bool>.Ok(true);
    }
}
