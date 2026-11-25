using IdentityAccess.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace IdentityAccess.Infrastructure.Email;

/// <summary>
/// Dịch vụ tạo liên kết đặt lại mật khẩu từ cấu hình hệ thống.
/// </summary>
public sealed class PasswordResetLinkFactory : IPasswordResetLinkFactory
{
    private readonly string _baseUrl;
    private readonly TimeSpan _tokenLifetime;

    /// <summary>
    /// Khởi tạo dịch vụ tạo liên kết đặt lại mật khẩu.
    /// </summary>
    /// <param name="configuration">Cấu hình ứng dụng.</param>
    public PasswordResetLinkFactory(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var baseUrl = configuration["ADMINUI_BASE_URL"]
            ?? configuration["AdminUI:BaseUrl"]
            ?? "http://localhost:4200";
        _baseUrl = baseUrl.TrimEnd('/');

        var minutesValue = configuration["PASSWORD_RESET_TOKEN_MINUTES"]
            ?? configuration["IdentityAccess:PasswordResetTokenMinutes"];
        _tokenLifetime = int.TryParse(minutesValue, out var minutes) && minutes > 0
            ? TimeSpan.FromMinutes(minutes)
            : TimeSpan.FromMinutes(30);
    }

    /// <inheritdoc />
    public string CreateResetLink(string rawToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawToken);
        return $"{_baseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";
    }

    /// <inheritdoc />
    public TimeSpan GetTokenLifetime() => _tokenLifetime;
}
