using Delivery.Application.Abstractions;
using Delivery.Application.Email;
using Delivery.Application.InAppNotifications.CreateInAppNotification;

namespace Delivery.Infrastructure.Dispatching;

/// <summary>
/// Dispatcher kênh Thông báo, điều phối gửi Email hoặc lưu Thông báo trong ứng dụng.
/// </summary>
public sealed class NotificationChannelDispatcher : INotificationChannelDispatcher
{
    private readonly IEmailSender _emailSender;
    private readonly CreateInAppNotificationUseCase _createInAppNotificationUseCase;

    /// <summary>
    /// Khởi tạo dispatcher kênh Thông báo.
    /// </summary>
    /// <param name="emailSender">Dịch vụ gửi Email.</param>
    /// <param name="createInAppNotificationUseCase">Use case tạo Thông báo trong ứng dụng.</param>
    public NotificationChannelDispatcher(
        IEmailSender emailSender,
        CreateInAppNotificationUseCase createInAppNotificationUseCase)
    {
        _emailSender = emailSender;
        _createInAppNotificationUseCase = createInAppNotificationUseCase;
    }

    /// <summary>
    /// Dispatch Thông báo theo kênh tương ứng.
    /// </summary>
    /// <param name="request">Yêu cầu dispatch Thông báo.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    public async Task DispatchAsync(NotificationChannelDispatchRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.Equals(request.Channel, "Email", StringComparison.OrdinalIgnoreCase))
        {
            var subject = string.IsNullOrWhiteSpace(request.Content) ? "Notification" : "Notification";
            await _emailSender.SendAsync(
                new SendEmailRequest(request.Recipient, subject, request.Content, null, request.NotificationTemplateId, null),
                cancellationToken);
            return;
        }

        if (string.Equals(request.Channel, "InApp", StringComparison.OrdinalIgnoreCase))
        {
            var result = await _createInAppNotificationUseCase.HandleAsync(
                new CreateInAppNotificationCommand(request.NotificationId, request.Recipient, "Notification", request.Content),
                cancellationToken);

            if (result.Status == Application.InAppNotifications.InAppNotificationAppStatus.ValidationError)
            {
                var message = string.Join("; ", result.ValidationErrors?.SelectMany(x => x.Value) ?? []);
                throw new ArgumentException(string.IsNullOrWhiteSpace(message) ? "Invalid in-app notification dispatch request." : message);
            }

            return;
        }

        throw new NotSupportedException($"Notification channel '{request.Channel}' is not supported.");
    }
}
