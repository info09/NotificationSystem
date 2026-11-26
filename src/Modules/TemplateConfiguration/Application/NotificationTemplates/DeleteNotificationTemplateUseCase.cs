using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.NotificationTemplates;

/// <summary>
/// Use case xóa Mẫu thông báo.
/// </summary>
public sealed class DeleteNotificationTemplateUseCase
{
    private readonly INotificationTemplateRepository _notificationTemplateRepository;

    /// <summary>
    /// Khởi tạo use case xóa Mẫu thông báo.
    /// </summary>
    /// <param name="notificationTemplateRepository">Repository Mẫu thông báo.</param>
    public DeleteNotificationTemplateUseCase(INotificationTemplateRepository notificationTemplateRepository)
    {
        _notificationTemplateRepository = notificationTemplateRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu xóa Mẫu thông báo.
    /// </summary>
    /// <param name="id">Mã định danh mẫu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả xóa Mẫu thông báo.</returns>
    public async Task<TemplateConfigurationAppResult<bool>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return TemplateConfigurationAppResult<bool>.Validation(
                new Dictionary<string, string[]> { ["id"] = ["Id is required."] });
        }

        var template = await _notificationTemplateRepository.GetByIdAsync(id, cancellationToken);
        if (template is null)
        {
            return TemplateConfigurationAppResult<bool>.NotFound();
        }

        _notificationTemplateRepository.Remove(template);
        await _notificationTemplateRepository.SaveChangesAsync(cancellationToken);
        return TemplateConfigurationAppResult<bool>.Ok(true);
    }
}
