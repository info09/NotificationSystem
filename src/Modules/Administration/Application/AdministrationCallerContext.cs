namespace Administration.Application;

/// <summary>
/// Ngữ cảnh người gọi dùng cho các thao tác quản trị.
/// </summary>
/// <param name="UserId">Mã định danh người dùng.</param>
/// <param name="Roles">Danh sách vai trò của người dùng.</param>
/// <param name="CorrelationId">Mã tương quan.</param>
public sealed record AdministrationCallerContext(
    string? UserId,
    IReadOnlyCollection<string> Roles,
    string? CorrelationId);
