using BuildingBlocks.SharedKernel.Entity;

namespace IdentityAccess.Domain.EndUserProfiles;

/// <summary>
/// Thực thể Hồ sơ người nhận EndUser.
/// </summary>
public sealed class EndUserProfile : Entity
{
    /// <summary>
    /// Khởi tạo thực thể Hồ sơ người nhận EndUser cho ORM.
    /// </summary>
    private EndUserProfile()
    {
        ExternalUserId = string.Empty;
        DisplayName = string.Empty;
    }

    private EndUserProfile(
        Guid id,
        string externalUserId,
        string displayName,
        string? email,
        string? phoneNumber,
        EndUserProfileStatus status,
        DateTime createdAtUtc,
        DateTime updatedAtUtc)
    {
        Id = id;
        ExternalUserId = externalUserId;
        DisplayName = displayName;
        Email = email;
        PhoneNumber = phoneNumber;
        Status = status;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
    }

    /// <summary>
    /// Định danh người dùng từ hệ thống nguồn.
    /// </summary>
    public string ExternalUserId { get; private set; }

    /// <summary>
    /// Tên hiển thị.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Email liên hệ.
    /// </summary>
    public string? Email { get; private set; }

    /// <summary>
    /// Số điện thoại liên hệ.
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// Trạng thái hồ sơ.
    /// </summary>
    public EndUserProfileStatus Status { get; private set; }

    /// <summary>
    /// Thời điểm tạo (UTC).
    /// </summary>
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>
    /// Thời điểm cập nhật gần nhất (UTC).
    /// </summary>
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>
    /// Tạo mới Hồ sơ người nhận EndUser.
    /// </summary>
    /// <param name="id">Mã định danh hồ sơ.</param>
    /// <param name="externalUserId">Định danh người dùng từ hệ thống nguồn.</param>
    /// <param name="displayName">Tên hiển thị.</param>
    /// <param name="email">Email liên hệ.</param>
    /// <param name="phoneNumber">Số điện thoại liên hệ.</param>
    /// <param name="createdAtUtc">Thời điểm tạo.</param>
    /// <returns>Thực thể Hồ sơ người nhận EndUser.</returns>
    public static EndUserProfile Create(
        Guid id,
        string externalUserId,
        string displayName,
        string? email,
        string? phoneNumber,
        DateTime? createdAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            throw new ArgumentException("ExternalUserId is required.", nameof(externalUserId));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Display name is required.", nameof(displayName));
        }

        var now = createdAtUtc ?? DateTime.UtcNow;
        return new EndUserProfile(
            id,
            externalUserId.Trim(),
            displayName.Trim(),
            string.IsNullOrWhiteSpace(email) ? null : email.Trim(),
            string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim(),
            EndUserProfileStatus.Active,
            now,
            now);
    }
}
