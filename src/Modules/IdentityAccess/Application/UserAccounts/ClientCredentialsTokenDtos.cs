namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// Yêu cầu cấp token bằng client credentials.
/// </summary>
/// <param name="ClientId">ClientId.</param>
/// <param name="ClientSecret">Client secret plaintext.</param>
public sealed record IssueClientCredentialsTokenAppRequest(string? ClientId, string? ClientSecret);

/// <summary>
/// Phản hồi cấp token bằng client credentials.
/// </summary>
/// <param name="AccessToken">JWT truy cập.</param>
/// <param name="TokenType">Loại token.</param>
/// <param name="ExpiresIn">Thời gian hết hạn tính theo giây.</param>
public sealed record IssueClientCredentialsTokenResponse(string AccessToken, string TokenType, int ExpiresIn);
