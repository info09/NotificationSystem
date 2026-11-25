namespace IdentityAccess.Application.Abstractions;

/// <summary>
/// Kết quả phát hành JWT.
/// </summary>
/// <param name="AccessToken">Chuỗi JWT truy cập.</param>
/// <param name="TokenType">Loại token.</param>
/// <param name="ExpiresInSeconds">Thời gian hết hạn tính theo giây.</param>
public sealed record JwtTokenResult(string AccessToken, string TokenType, int ExpiresInSeconds);

/// <summary>
/// Dịch vụ phát hành JWT.
/// </summary>
public interface IJwtTokenIssuer
{
    /// <summary>
    /// Phát hành JWT truy cập cho người dùng/tích hợp.
    /// </summary>
    /// <param name="subject">Định danh subject.</param>
    /// <param name="role">Vai trò RBAC.</param>
    /// <param name="additionalClaims">Các claim bổ sung.</param>
    /// <returns>Kết quả phát hành token.</returns>
    JwtTokenResult IssueAccessToken(string subject, string role, IReadOnlyDictionary<string, string>? additionalClaims = null);
}
