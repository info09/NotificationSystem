namespace BuildingBlocks.SharedKernel.EndUserProfiles;

/// <summary>
/// Thông tin tham chiếu nghiệp vụ của Hồ sơ người nhận EndUser.
/// </summary>
/// <param name="ExternalUserId">Định danh hệ thống nguồn.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Status">Trạng thái hồ sơ.</param>
/// <param name="Email">Email liên hệ.</param>
/// <param name="PhoneNumber">Số điện thoại liên hệ.</param>
public sealed record EndUserProfileReference(
    string ExternalUserId,
    string DisplayName,
    string Status,
    string? Email,
    string? PhoneNumber);
