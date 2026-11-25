using BuildingBlocks.SharedKernel.Entity;

namespace IdentityAccess.Domain.UserAccounts;

/// <summary>
/// Thực thể Tài khoản người dùng.
/// </summary>
public sealed class UserAccount : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Tài khoản người dùng cho ORM.
    /// </summary>
    private UserAccount()
    {
        UserName = string.Empty;
        Email = string.Empty;
        DisplayName = string.Empty;
    }

    private UserAccount(
        Guid id,
        string userName,
        string email,
        string displayName,
        UserRole role,
        UserAccountStatus status,
        string? passwordHash,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        UserName = userName;
        Email = email;
        DisplayName = displayName;
        Role = role;
        Status = status;
        PasswordHash = passwordHash;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Tên đăng nhập.
    /// </summary>
    public string UserName { get; private set; }

    /// <summary>
    /// Email của tài khoản.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Tên hiển thị.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Vai trò của tài khoản.
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// Trạng thái tài khoản.
    /// </summary>
    public UserAccountStatus Status { get; private set; }

    /// <summary>
    /// Giá trị băm mật khẩu.
    /// </summary>
    public string? PasswordHash { get; private set; }

    /// <summary>
    /// Thời điểm tạo (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất (UTC).
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Tạo mới Tài khoản người dùng.
    /// </summary>
    /// <param name="id">Mã định danh tài khoản.</param>
    /// <param name="userName">Tên đăng nhập.</param>
    /// <param name="email">Email.</param>
    /// <param name="displayName">Tên hiển thị.</param>
    /// <param name="role">Vai trò.</param>
    /// <param name="passwordHash">Giá trị băm mật khẩu.</param>
    /// <param name="createdAtUtc">Thời điểm tạo.</param>
    /// <returns>Thực thể Tài khoản người dùng.</returns>
    public static UserAccount Create(
        Guid id,
        string userName,
        string email,
        string displayName,
        UserRole role,
        string? passwordHash,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name is required.", nameof(userName));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email is required.", nameof(email));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        var now = createdAtUtc ?? DateTime.UtcNow;
        return new UserAccount(
            id,
            userName.Trim(),
            email.Trim(),
            displayName.Trim(),
            role,
            UserAccountStatus.Active,
            string.IsNullOrWhiteSpace(passwordHash) ? null : passwordHash,
            now,
            now);
    }
}
