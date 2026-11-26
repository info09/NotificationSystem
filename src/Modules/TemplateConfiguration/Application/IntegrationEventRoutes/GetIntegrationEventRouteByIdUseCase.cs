using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.IntegrationEventRoutes;

/// <summary>
/// Use case lấy chi tiết Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class GetIntegrationEventRouteByIdUseCase
{
    private readonly IIntegrationEventRouteRepository _repository;

    /// <summary>
    /// Khởi tạo use case lấy chi tiết Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="repository">Repository Định tuyến sự kiện tích hợp.</param>
    public GetIntegrationEventRouteByIdUseCase(IIntegrationEventRouteRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý yêu cầu lấy chi tiết Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="id">Mã định danh định tuyến.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả chi tiết định tuyến.</returns>
    public async Task<TemplateConfigurationAppResult<IntegrationEventRouteDto>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return TemplateConfigurationAppResult<IntegrationEventRouteDto>.Validation(
                new Dictionary<string, string[]> { ["id"] = ["Id is required."] });
        }

        var route = await _repository.GetByIdAsync(id, cancellationToken);
        if (route is null)
        {
            return TemplateConfigurationAppResult<IntegrationEventRouteDto>.NotFound();
        }

        return TemplateConfigurationAppResult<IntegrationEventRouteDto>.Ok(IntegrationEventRouteMapping.ToDto(route));
    }
}
