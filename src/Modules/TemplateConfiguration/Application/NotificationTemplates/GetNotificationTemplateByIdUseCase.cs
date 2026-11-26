using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.NotificationTemplates;

/// <summary>
/// Use case lấy chi tiết Mẫu thông báo.
/// </summary>
public sealed class GetNotificationTemplateByIdUseCase
{
    private readonly INotificationTemplateRepository _notificationTemplateRepository;

    /// <summary>
    /// Khởi tạo use case lấy chi tiết Mẫu thông báo.
    /// </summary>
    /// <param name="notificationTemplateRepository">Repository Mẫu thông báo.</param>
    public GetNotificationTemplateByIdUseCase(INotificationTemplateRepository notificationTemplateRepository)
    {
        _notificationTemplateRepository = notificationTemplateRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Mẫu thông báo.
    /// </summary>
    /// <param name="id">Mã định danh mẫu.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả chi tiết Mẫu thông báo.</returns>
    public async Task<TemplateConfigurationAppResult<NotificationTemplateDto>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return TemplateConfigurationAppResult<NotificationTemplateDto>.Validation(
                new Dictionary<string, string[]> { ["id"] = ["Id is required."] });
        }

        var template = await _notificationTemplateRepository.GetByIdAsync(id, cancellationToken);
        if (template is null)
        {
            return TemplateConfigurationAppResult<NotificationTemplateDto>.NotFound();
        }

        return TemplateConfigurationAppResult<NotificationTemplateDto>.Ok(NotificationTemplateMapping.ToDto(template));
    }
}
