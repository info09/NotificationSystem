namespace IdentityAccess.Application.EndUserProfiles;

/// <summary>
/// DTO thông tin Hồ sơ người nhận EndUser.
/// </summary>
/// <param name="Id">Mã định danh hồ sơ.</param>
/// <param name="ExternalUserId">Định danh hệ thống nguồn.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Email">Email liên hệ.</param>
/// <param name="PhoneNumber">Số điện thoại liên hệ.</param>
/// <param name="Status">Trạng thái hồ sơ.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo.</param>
/// <param name="UpdatedAtUtc">Thời điểm cập nhật.</param>
public sealed record EndUserProfileDto(
    Guid Id,
    string ExternalUserId,
    string DisplayName,
    string? Email,
    string? PhoneNumber,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>
/// Yêu cầu tạo Hồ sơ người nhận EndUser.
/// </summary>
/// <param name="ExternalUserId">Định danh hệ thống nguồn.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Email">Email liên hệ.</param>
/// <param name="PhoneNumber">Số điện thoại liên hệ.</param>
public sealed record CreateEndUserProfileAppRequest(
    string? ExternalUserId,
    string? DisplayName,
    string? Email,
    string? PhoneNumber);

/// <summary>
/// Yêu cầu cập nhật Hồ sơ người nhận EndUser.
/// </summary>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Email">Email liên hệ.</param>
/// <param name="PhoneNumber">Số điện thoại liên hệ.</param>
public sealed record UpdateEndUserProfileAppRequest(
    string? DisplayName,
    string? Email,
    string? PhoneNumber);

/// <summary>
/// Yêu cầu truy vấn danh sách Hồ sơ người nhận EndUser.
/// </summary>
/// <param name="Status">Trạng thái lọc.</param>
/// <param name="ExternalUserId">Định danh hệ thống nguồn lọc.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetEndUserProfilesAppRequest(
    string? Status,
    string? ExternalUserId,
    int Page,
    int PageSize);

/// <summary>
/// Kết quả truy vấn danh sách Hồ sơ người nhận EndUser.
/// </summary>
/// <param name="Items">Danh sách hồ sơ.</param>
/// <param name="Page">Số trang hiện tại.</param>
/// <param name="PageSize">Kích thước trang.</param>
/// <param name="TotalCount">Tổng số bản ghi.</param>
public sealed record GetEndUserProfilesResponse(
    IReadOnlyList<EndUserProfileDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
