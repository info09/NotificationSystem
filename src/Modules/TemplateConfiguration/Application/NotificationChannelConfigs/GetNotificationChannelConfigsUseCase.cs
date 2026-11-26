using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.NotificationChannelConfigs;

/// <summary>
/// Use case truy vấn danh sách Cấu hình kênh thông báo.
/// </summary>
public sealed class GetNotificationChannelConfigsUseCase
{
    private readonly INotificationChannelConfigRepository _repository;

    /// <summary>
    /// Khởi tạo use case truy vấn danh sách Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="repository">Repository Cấu hình kênh thông báo.</param>
    public GetNotificationChannelConfigsUseCase(INotificationChannelConfigRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý yêu cầu truy vấn danh sách Cấu hình kênh thông báo.
    /// </summary>
    /// <param name="request">Yêu cầu truy vấn.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả danh sách cấu hình.</returns>
    public async Task<TemplateConfigurationAppResult<GetNotificationChannelConfigsResponse>> HandleAsync(
        GetNotificationChannelConfigsAppRequest? request,
        CancellationToken cancellationToken)
    {
        var safeRequest = request ?? new GetNotificationChannelConfigsAppRequest(null, null, 1, 20);
        var page = safeRequest.Page <= 0 ? 1 : safeRequest.Page;
        var pageSize = safeRequest.PageSize <= 0 ? 20 : Math.Min(safeRequest.PageSize, 100);
        var channel = string.IsNullOrWhiteSpace(safeRequest.Channel) ? null : safeRequest.Channel.Trim();

        var (items, totalCount) = await _repository.GetPagedAsync(
            channel,
            safeRequest.IsEnabled,
            page,
            pageSize,
            cancellationToken);

        return TemplateConfigurationAppResult<GetNotificationChannelConfigsResponse>.Ok(
            new GetNotificationChannelConfigsResponse(
                items.Select(NotificationChannelConfigMapping.ToDto).ToArray(),
                page,
                pageSize,
                totalCount));
    }
}
