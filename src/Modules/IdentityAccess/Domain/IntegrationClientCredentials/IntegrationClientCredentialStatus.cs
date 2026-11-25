namespace IdentityAccess.Domain.IntegrationClientCredentials;

/// <summary>
/// Trạng thái Thông tin xác thực IntegrationClient.
/// </summary>
public enum IntegrationClientCredentialStatus
{
    /// <summary>
    /// Đang hoạt động.
    /// </summary>
    Active = 1,

    /// <summary>
    /// Đã bị vô hiệu hóa.
    /// </summary>
    Revoked = 2
}
