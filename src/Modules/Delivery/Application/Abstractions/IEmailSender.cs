using Delivery.Application.Email;

namespace Delivery.Application.Abstractions;

/// <summary>
/// Dịch vụ gửi Email theo yêu cầu nghiệp vụ.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Gửi Email theo yêu cầu.
    /// </summary>
    /// <param name="request">Yêu cầu gửi Email.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task SendAsync(SendEmailRequest request, CancellationToken cancellationToken);
}
