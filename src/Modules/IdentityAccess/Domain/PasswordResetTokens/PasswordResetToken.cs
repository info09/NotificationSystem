using BuildingBlocks.SharedKernel.Entity;

namespace IdentityAccess.Domain.PasswordResetTokens;

/// <summary>
/// Thực thể Mã đặt lại mật khẩu.
/// </summary>
public sealed class PasswordResetToken : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Mã đặt lại mật khẩu cho ORM.
    /// </summary>
    private PasswordResetToken()
    {
        TokenHash = string.Empty;
    }

    private PasswordResetToken(
        Guid id,
        Guid userAccountId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime? usedAtUtc,
        DateTime createdAtUtc)
    {
        Id = id;
        UserAccountId = userAccountId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        UsedAtUtc = usedAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    /// <summary>
    /// Mã định danh Tài khoản người dùng.
    /// </summary>
    public Guid UserAccountId { get; private set; }

    /// <summary>
    /// Giá trị băm của token đặt lại mật khẩu.
    /// </summary>
    public string TokenHash { get; private set; }

    /// <summary>
    /// Thời điểm hết hạn (UTC).
    /// </summary>
    public DateTime ExpiresAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm token đã được sử dụng (UTC).
    /// </summary>
    public DateTime? UsedAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm tạo (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Tạo mới Mã đặt lại mật khẩu.
    /// </summary>
    /// <param name="id">Mã định danh token.</param>
    /// <param name="userAccountId">Mã định danh Tài khoản người dùng.</param>
    /// <param name="tokenHash">Giá trị băm token.</param>
    /// <param name="expiresAtUtc">Thời điểm hết hạn.</param>
    /// <param name="createdAtUtc">Thời điểm tạo.</param>
    /// <returns>Thực thể Mã đặt lại mật khẩu.</returns>
    public static PasswordResetToken Create(
        Guid id,
        Guid userAccountId,
        string tokenHash,
        DateTime expiresAtUtc,
        DateTime? createdAtUtc = null)
    {
        if (userAccountId == Guid.Empty)
        {
            throw new ArgumentException("UserAccountId is required.", nameof(userAccountId));
        }

        if (string.IsNullOrWhiteSpace(tokenHash))
        {
            throw new ArgumentException("TokenHash is required.", nameof(tokenHash));
        }

        if (expiresAtUtc <= (createdAtUtc ?? DateTime.UtcNow))
        {
            throw new ArgumentException("ExpiresAtUtc must be greater than CreatedAtUtc.", nameof(expiresAtUtc));
        }

        return new PasswordResetToken(
            id,
            userAccountId,
            tokenHash,
            expiresAtUtc,
            null,
            createdAtUtc ?? DateTime.UtcNow);
    }

    /// <summary>
    /// Kiểm tra token đã hết hạn hay chưa.
    /// </summary>
    /// <param name="utcNow">Thời điểm hiện tại UTC.</param>
    /// <returns><c>true</c> nếu đã hết hạn; ngược lại <c>false</c>.</returns>
    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

    /// <summary>
    /// Kiểm tra token đã được sử dụng hay chưa.
    /// </summary>
    /// <returns><c>true</c> nếu đã sử dụng; ngược lại <c>false</c>.</returns>
    public bool IsUsed() => UsedAtUtc.HasValue;

    /// <summary>
    /// Đánh dấu token đã được sử dụng.
    /// </summary>
    /// <param name="usedAtUtc">Thời điểm sử dụng.</param>
    public void MarkUsed(DateTime? usedAtUtc = null)
    {
        if (UsedAtUtc.HasValue)
        {
            return;
        }

        UsedAtUtc = usedAtUtc ?? DateTime.UtcNow;
    }
}
