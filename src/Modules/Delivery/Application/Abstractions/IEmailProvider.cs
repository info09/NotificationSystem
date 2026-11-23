using Delivery.Domain.Email;

namespace Delivery.Application.Abstractions;

/// <summary>
/// Abstraction Nhà cung cấp Email để gửi Thông điệp Email.
/// </summary>
public interface IEmailProvider
{
    /// <summary>
    /// Gửi Thông điệp Email.
    /// </summary>
    /// <param name="message">Thông điệp Email cần gửi.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken);
}
