using IdentityAccess.Domain.UserAccounts;

namespace IdentityAccess.Application.UserAccounts;

/// <summary>
/// DTO thông tin Tài khoản người dùng.
/// </summary>
/// <param name="Id">Mã định danh tài khoản.</param>
/// <param name="UserName">Tên đăng nhập.</param>
/// <param name="Email">Email.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Role">Vai trò.</param>
/// <param name="Status">Trạng thái.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo.</param>
/// <param name="UpdatedAtUtc">Thời điểm cập nhật.</param>
public sealed record UserAccountDto(
    Guid Id,
    string UserName,
    string Email,
    string DisplayName,
    string Role,
    string Status,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

/// <summary>
/// Yêu cầu tạo Tài khoản người dùng.
/// </summary>
/// <param name="UserName">Tên đăng nhập.</param>
/// <param name="Email">Email.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Role">Vai trò.</param>
/// <param name="Password">Mật khẩu plaintext cho tài khoản quản trị/vận hành.</param>
public sealed record CreateUserAccountAppRequest(
    string? UserName,
    string? Email,
    string? DisplayName,
    string? Role,
    string? Password);

/// <summary>
/// Yêu cầu cập nhật Tài khoản người dùng.
/// </summary>
/// <param name="UserName">Tên đăng nhập.</param>
/// <param name="Email">Email.</param>
/// <param name="DisplayName">Tên hiển thị.</param>
/// <param name="Role">Vai trò.</param>
/// <param name="Password">Mật khẩu mới, nếu cần đổi.</param>
public sealed record UpdateUserAccountAppRequest(
    string? UserName,
    string? Email,
    string? DisplayName,
    string? Role,
    string? Password);

/// <summary>
/// Yêu cầu truy vấn danh sách Tài khoản người dùng.
/// </summary>
/// <param name="Status">Trạng thái lọc.</param>
/// <param name="Role">Vai trò lọc.</param>
/// <param name="Page">Số trang.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record GetUserAccountsAppRequest(
    string? Status,
    string? Role,
    int Page,
    int PageSize);

/// <summary>
/// Kết quả truy vấn danh sách Tài khoản người dùng.
/// </summary>
/// <param name="Items">Danh sách tài khoản.</param>
/// <param name="Page">Số trang hiện tại.</param>
/// <param name="PageSize">Kích thước trang.</param>
/// <param name="TotalCount">Tổng số bản ghi.</param>
public sealed record GetUserAccountsResponse(
    IReadOnlyList<UserAccountDto> Items,
    int Page,
    int PageSize,
    int TotalCount);

/// <summary>
/// DTO thông tin Thông tin xác thực IntegrationClient.
/// </summary>
/// <param name="Id">Mã định danh credential.</param>
/// <param name="UserAccountId">Mã định danh tài khoản.</param>
/// <param name="ClientId">ClientId.</param>
/// <param name="Status">Trạng thái.</param>
/// <param name="CreatedAtUtc">Thời điểm tạo.</param>
/// <param name="ExpiresAtUtc">Thời điểm hết hạn.</param>
/// <param name="RevokedAtUtc">Thời điểm vô hiệu hóa.</param>
/// <param name="LastRotatedAtUtc">Thời điểm xoay secret gần nhất.</param>
public sealed record IntegrationClientCredentialDto(
    Guid Id,
    Guid UserAccountId,
    string ClientId,
    string Status,
    DateTime CreatedAtUtc,
    DateTime? ExpiresAtUtc,
    DateTime? RevokedAtUtc,
    DateTime? LastRotatedAtUtc);

/// <summary>
/// Yêu cầu tạo Thông tin xác thực IntegrationClient.
/// </summary>
/// <param name="ClientId">ClientId tùy chọn.</param>
/// <param name="ExpiresAtUtc">Thời điểm hết hạn.</param>
public sealed record CreateIntegrationClientCredentialAppRequest(
    string? ClientId,
    DateTime? ExpiresAtUtc);

/// <summary>
/// Kết quả tạo Thông tin xác thực IntegrationClient có chứa secret plaintext một lần.
/// </summary>
/// <param name="Credential">Thông tin credential.</param>
/// <param name="ClientSecret">Client secret plaintext chỉ trả về một lần.</param>
public sealed record CreatedIntegrationClientCredentialResponse(
    IntegrationClientCredentialDto Credential,
    string ClientSecret);

/// <summary>
/// Kết quả xoay secret Thông tin xác thực IntegrationClient.
/// </summary>
/// <param name="Credential">Thông tin credential.</param>
/// <param name="ClientSecret">Client secret plaintext mới.</param>
public sealed record RotatedIntegrationClientCredentialResponse(
    IntegrationClientCredentialDto Credential,
    string ClientSecret);

/// <summary>
/// Yêu cầu đăng nhập bằng credential nội bộ.
/// </summary>
/// <param name="UserName">Tên đăng nhập.</param>
/// <param name="Password">Mật khẩu.</param>
public sealed record LoginAppRequest(string? UserName, string? Password);

/// <summary>
/// Phản hồi đăng nhập thành công.
/// </summary>
/// <param name="AccessToken">JWT truy cập.</param>
/// <param name="TokenType">Loại token.</param>
/// <param name="ExpiresIn">Thời gian hết hạn tính theo giây.</param>
public sealed record LoginResponse(string AccessToken, string TokenType, int ExpiresIn);

/// <summary>
/// Yêu cầu gửi email đặt lại mật khẩu.
/// </summary>
/// <param name="Identifier">Email hoặc tên đăng nhập.</param>
public sealed record RequestPasswordResetAppRequest(string? Identifier);

/// <summary>
/// Phản hồi yêu cầu đặt lại mật khẩu.
/// </summary>
/// <param name="Accepted">Trạng thái đã tiếp nhận yêu cầu.</param>
public sealed record RequestPasswordResetResponse(bool Accepted);

/// <summary>
/// Yêu cầu xác nhận đặt lại mật khẩu.
/// </summary>
/// <param name="Token">Token đặt lại mật khẩu.</param>
/// <param name="NewPassword">Mật khẩu mới.</param>
public sealed record ConfirmPasswordResetAppRequest(string? Token, string? NewPassword);

/// <summary>
/// Phản hồi xác nhận đặt lại mật khẩu.
/// </summary>
/// <param name="Reset">Trạng thái đặt lại mật khẩu.</param>
public sealed record ConfirmPasswordResetResponse(bool Reset);
