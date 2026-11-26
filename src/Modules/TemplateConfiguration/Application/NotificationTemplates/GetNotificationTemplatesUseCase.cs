using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.NotificationTemplates;

/// <summary>
/// Use case truy vấn danh sách Mẫu thông báo.
/// </summary>
public sealed class GetNotificationTemplatesUseCase
{
    private readonly INotificationTemplateRepository _notificationTemplateRepository;

    /// <summary>
    /// Khởi tạo use case truy vấn danh sách Mẫu thông báo.
    /// </summary>
    /// <param name="notificationTemplateRepository">Repository Mẫu thông báo.</param>
    public GetNotificationTemplatesUseCase(INotificationTemplateRepository notificationTemplateRepository)
    {
        _notificationTemplateRepository = notificationTemplateRepository;
    }

    /// <summary>
    /// Xử lý yêu cầu truy vấn danh sách Mẫu thông báo.
    /// </summary>
    /// <param name="request">Yêu cầu truy vấn.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả danh sách Mẫu thông báo.</returns>
    public async Task<TemplateConfigurationAppResult<GetNotificationTemplatesResponse>> HandleAsync(
        GetNotificationTemplatesAppRequest? request,
        CancellationToken cancellationToken)
    {
        var safeRequest = request ?? new GetNotificationTemplatesAppRequest(null, null, null, 1, 20);
        var page = safeRequest.Page <= 0 ? 1 : safeRequest.Page;
        var pageSize = safeRequest.PageSize <= 0 ? 20 : Math.Min(safeRequest.PageSize, 100);
        var channel = string.IsNullOrWhiteSpace(safeRequest.Channel) ? null : safeRequest.Channel.Trim();
        var keyword = string.IsNullOrWhiteSpace(safeRequest.Keyword) ? null : safeRequest.Keyword.Trim();

        var (items, totalCount) = await _notificationTemplateRepository.GetPagedAsync(
            channel,
            safeRequest.IsActive,
            keyword,
            page,
            pageSize,
            cancellationToken);

        return TemplateConfigurationAppResult<GetNotificationTemplatesResponse>.Ok(
            new GetNotificationTemplatesResponse(
                items.Select(NotificationTemplateMapping.ToDto).ToArray(),
                page,
                pageSize,
                totalCount));
    }
}
