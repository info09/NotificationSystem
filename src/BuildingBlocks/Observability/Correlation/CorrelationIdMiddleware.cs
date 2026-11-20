using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace BuildingBlocks.Observability.Correlation;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var correlationId = GetFirstHeaderValue(context, CorrelationIdConstants.CorrelationIdHeaderName);
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Activity.Current?.Id ?? context.TraceIdentifier ?? Guid.NewGuid().ToString();
        }

        var messageId = GetFirstHeaderValue(context, CorrelationIdConstants.MessageIdHeaderName);
        var notificationId = GetFirstHeaderValue(context, CorrelationIdConstants.NotificationIdHeaderName);
        var traceId = Activity.Current?.TraceId.ToString();
        if (string.IsNullOrWhiteSpace(traceId))
        {
            traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        }

        context.Items[CorrelationIdConstants.CorrelationIdPropertyName] = correlationId;
        if (!string.IsNullOrWhiteSpace(messageId))
        {
            context.Items[CorrelationIdConstants.MessageIdPropertyName] = messageId;
        }

        if (!string.IsNullOrWhiteSpace(notificationId))
        {
            context.Items[CorrelationIdConstants.NotificationIdPropertyName] = notificationId;
        }

        if (!string.IsNullOrWhiteSpace(traceId))
        {
            context.Items[CorrelationIdConstants.TraceIdPropertyName] = traceId;
        }

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdConstants.CorrelationIdHeaderName))
            {
                context.Response.Headers[CorrelationIdConstants.CorrelationIdHeaderName] = correlationId;
            }

            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(CorrelationIdConstants.CorrelationIdPropertyName, correlationId))
        using (PushPropertyIfNotEmpty(CorrelationIdConstants.MessageIdPropertyName, messageId))
        using (PushPropertyIfNotEmpty(CorrelationIdConstants.NotificationIdPropertyName, notificationId))
        using (PushPropertyIfNotEmpty(CorrelationIdConstants.TraceIdPropertyName, traceId))
        {
            await next(context);
        }
    }

    private static string? GetFirstHeaderValue(HttpContext context, string headerName)
    {
        if (!context.Request.Headers.TryGetValue(headerName, out var values))
        {
            return null;
        }

        var value = values.ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static IDisposable? PushPropertyIfNotEmpty(string propertyName, string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : LogContext.PushProperty(propertyName, value);
    }
}
