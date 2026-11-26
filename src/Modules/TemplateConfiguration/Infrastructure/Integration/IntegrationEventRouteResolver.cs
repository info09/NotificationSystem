using BuildingBlocks.SharedKernel.IntegrationEvents;
using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Infrastructure.Integration;

/// <summary>
/// Bộ phân giải Định tuyến sự kiện tích hợp từ module TemplateConfiguration.
/// </summary>
public sealed class IntegrationEventRouteResolver : IIntegrationEventRouteResolver
{
    private readonly IIntegrationEventRouteRepository _repository;

    /// <summary>
    /// Khởi tạo bộ phân giải Định tuyến sự kiện tích hợp.
    /// </summary>
    /// <param name="repository">Repository Định tuyến sự kiện tích hợp.</param>
    public IntegrationEventRouteResolver(IIntegrationEventRouteRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<IntegrationEventRouteResolution?> ResolveAsync(string eventType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(eventType))
        {
            return null;
        }

        var route = await _repository.GetActiveByEventTypeAsync(eventType, cancellationToken);
        if (route is null)
        {
            return null;
        }

        return new IntegrationEventRouteResolution(route.EventType, route.NotificationTemplateId, route.Channel);
    }
}
