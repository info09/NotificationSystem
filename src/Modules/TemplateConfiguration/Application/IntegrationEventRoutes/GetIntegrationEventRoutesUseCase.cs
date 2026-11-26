using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.IntegrationEventRoutes;

/// <summary>
/// Use case truy vấn danh sách Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class GetIntegrationEventRoutesUseCase
{
    private readonly IIntegrationEventRouteRepository _repository;

    /// <summary>
    /// Khởi tạo use case truy vấn danh sách Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="repository">Repository Định tuyến sự kiện tích hợp.</param>
    public GetIntegrationEventRoutesUseCase(IIntegrationEventRouteRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý yêu cầu truy vấn danh sách Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="request">Yêu cầu truy vấn.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả danh sách định tuyến.</returns>
    public async Task<TemplateConfigurationAppResult<GetIntegrationEventRoutesResponse>> HandleAsync(
        GetIntegrationEventRoutesAppRequest? request,
        CancellationToken cancellationToken)
    {
        var safeRequest = request ?? new GetIntegrationEventRoutesAppRequest(null, null, null, 1, 20);
        var page = safeRequest.Page <= 0 ? 1 : safeRequest.Page;
        var pageSize = safeRequest.PageSize <= 0 ? 20 : Math.Min(safeRequest.PageSize, 100);
        var eventType = string.IsNullOrWhiteSpace(safeRequest.EventType) ? null : safeRequest.EventType.Trim();
        var channel = string.IsNullOrWhiteSpace(safeRequest.Channel) ? null : safeRequest.Channel.Trim();

        var (items, totalCount) = await _repository.GetPagedAsync(
            eventType,
            channel,
            safeRequest.IsActive,
            page,
            pageSize,
            cancellationToken);

        return TemplateConfigurationAppResult<GetIntegrationEventRoutesResponse>.Ok(
            new GetIntegrationEventRoutesResponse(
                items.Select(IntegrationEventRouteMapping.ToDto).ToArray(),
                page,
                pageSize,
                totalCount));
    }
}
