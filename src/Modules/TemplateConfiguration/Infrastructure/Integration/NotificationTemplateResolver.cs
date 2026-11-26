using BuildingBlocks.SharedKernel.Notifications;
using TemplateConfiguration.Application.Abstractions;

namespace TemplateConfiguration.Infrastructure.Integration;

/// <summary>
/// Bộ phân giải Mẫu thông báo từ module TemplateConfiguration.
/// </summary>
public sealed class NotificationTemplateResolver : INotificationTemplateResolver
{
    private readonly INotificationTemplateRepository _repository;

    /// <summary>
    /// Khởi tạo bộ phân giải Mẫu thông báo.
    /// </summary>
    /// <param name="repository">Repository Mẫu thông báo.</param>
    public NotificationTemplateResolver(INotificationTemplateRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<NotificationTemplateContent?> ResolveAsync(Guid templateId, CancellationToken cancellationToken)
    {
        var template = await _repository.GetByIdAsync(templateId, cancellationToken);
        if (template is null)
        {
            return null;
        }

        return new NotificationTemplateContent(
            template.Id,
            template.Subject,
            template.Body,
            template.Channel,
            template.IsActive);
    }
}
