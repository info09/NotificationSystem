using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Application.IntegrationEventRoutes;

/// <summary>
/// Use case xóa Định tuyến sự kiện tích hợp.
/// </summary>
public sealed class DeleteIntegrationEventRouteUseCase
{
    private readonly IIntegrationEventRouteRepository _repository;

    /// <summary>
    /// Khởi tạo use case xóa Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="repository">Repository Định tuyến sự kiện tích hợp.</param>
    public DeleteIntegrationEventRouteUseCase(IIntegrationEventRouteRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Xử lý yêu cầu xóa Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="id">Mã định danh định tuyến.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Kết quả xóa định tuyến.</returns>
    public async Task<TemplateConfigurationAppResult<bool>> HandleAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        if (id == Guid.Empty)
        {
            return TemplateConfigurationAppResult<bool>.Validation(
                new Dictionary<string, string[]> { ["id"] = ["Id is required."] });
        }

        var route = await _repository.GetByIdAsync(id, cancellationToken);
        if (route is null)
        {
            return TemplateConfigurationAppResult<bool>.NotFound();
        }

        _repository.Remove(route);
        await _repository.SaveChangesAsync(cancellationToken);
        return TemplateConfigurationAppResult<bool>.Ok(true);
    }
}
