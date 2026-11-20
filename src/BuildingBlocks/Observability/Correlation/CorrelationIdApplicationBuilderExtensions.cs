using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Observability.Correlation;

public static class CorrelationIdApplicationBuilderExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
