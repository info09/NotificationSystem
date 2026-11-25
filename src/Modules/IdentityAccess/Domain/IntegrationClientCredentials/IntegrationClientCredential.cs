using BuildingBlocks.SharedKernel.Entity;

namespace IdentityAccess.Domain.IntegrationClientCredentials;

/// <summary>
/// Thực thể Thông tin xác thực IntegrationClient.
/// </summary>
public sealed class IntegrationClientCredential : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Thông tin xác thực IntegrationClient cho ORM.
    /// </summary>
    private IntegrationClientCredential()
    {
        ClientId = string.Empty;
        ClientSecretHash = string.Empty;
    }

    private IntegrationClientCredential(
        Guid id,
        Guid userAccountId,
        string clientId,
        string clientSecretHash,
        IntegrationClientCredentialStatus status,
        DateTime createdAtUtc,
        DateTime? expiresAtUtc,
        DateTime? revokedAtUtc,
        DateTime? lastRotatedAtUtc)
    {
        Id = id;
        UserAccountId = userAccountId;
        ClientId = clientId;
        ClientSecretHash = clientSecretHash;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        ExpiresAtUtc = expiresAtUtc;
        RevokedAtUtc = revokedAtUtc;
        LastRotatedAtUtc = lastRotatedAtUtc;
    }

    /// <summary>
    /// Mã định danh Tài khoản người dùng sở hữu credential.
    /// </summary>
    public Guid UserAccountId { get; private set; }

    /// <summary>
    /// Mã định danh client.
    /// </summary>
    public string ClientId { get; private set; }

    /// <summary>
    /// Giá trị băm client secret.
    /// </summary>
    public string ClientSecretHash { get; private set; }

    /// <summary>
    /// Trạng thái credential.
    /// </summary>
    public IntegrationClientCredentialStatus Status { get; private set; }

    /// <summary>
    /// Thời điểm tạo (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm hết hạn (UTC).
    /// </summary>
    public DateTime? ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm bị vô hiệu hóa (UTC).
    /// </summary>
    public DateTime? RevokedAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm xoay secret gần nhất (UTC).
    /// </summary>
    public DateTime? LastRotatedAtUtc { get; private set; }

    /// <summary>
    /// Tạo mới Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="id">Mã định danh credential.</param>
    /// <param name="userAccountId">Mã định danh Tài khoản người dùng.</param>
    /// <param name="clientId">Mã định danh client.</param>
    /// <param name="clientSecretHash">Giá trị băm client secret.</param>
    /// <param name="expiresAtUtc">Thời điểm hết hạn.</param>
    /// <param name="createdAtUtc">Thời điểm tạo.</param>
    /// <returns>Thực thể Thông tin xác thực IntegrationClient.</returns>
    public static IntegrationClientCredential Create(
        Guid id,
        Guid userAccountId,
        string clientId,
        string clientSecretHash,
        DateTime? expiresAtUtc = null,
        DateTime? createdAtUtc = null)
    {
        if (userAccountId == Guid.Empty)
        {
            throw new ArgumentException("UserAccountId is required.", nameof(userAccountId));
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("ClientId is required.", nameof(clientId));
        }

        if (string.IsNullOrWhiteSpace(clientSecretHash))
        {
            throw new ArgumentException("ClientSecretHash is required.", nameof(clientSecretHash));
        }

        return new IntegrationClientCredential(
            id,
            userAccountId,
            clientId.Trim(),
            clientSecretHash,
            IntegrationClientCredentialStatus.Active,
            createdAtUtc ?? DateTime.UtcNow,
            expiresAtUtc,
            null,
            null);
    }

    /// <summary>
    /// Vô hiệu hóa Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="revokedAtUtc">Thời điểm vô hiệu hóa.</param>
    public void Revoke(DateTime? revokedAtUtc = null)
    {
        if (Status == IntegrationClientCredentialStatus.Revoked)
        {
            return;
        }

        Status = IntegrationClientCredentialStatus.Revoked;
        RevokedAtUtc = revokedAtUtc ?? DateTime.UtcNow;
    }

    /// <summary>
    /// Xoay secret cho Thông tin xác thực IntegrationClient.
    /// </summary>
    /// <param name="clientSecretHash">Giá trị băm secret mới.</param>
    /// <param name="rotatedAtUtc">Thời điểm xoay secret.</param>
    public void RotateSecret(string clientSecretHash, DateTime? rotatedAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(clientSecretHash))
        {
            throw new ArgumentException("ClientSecretHash is required.", nameof(clientSecretHash));
        }

        if (Status == IntegrationClientCredentialStatus.Revoked)
        {
            throw new InvalidOperationException("Cannot rotate secret for a revoked credential.");
        }

        ClientSecretHash = clientSecretHash;
        LastRotatedAtUtc = rotatedAtUtc ?? DateTime.UtcNow;
    }
}
