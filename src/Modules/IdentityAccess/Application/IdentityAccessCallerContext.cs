namespace IdentityAccess.Application;

/// <summary>
/// Ngữ cảnh người gọi dùng cho các thao tác quản trị IdentityAccess.
/// </summary>
/// <param name="Subject">Định danh người gọi.</param>
/// <param name="Roles">Danh sách vai trò của người gọi.</param>
public sealed record IdentityAccessCallerContext(string? Subject, IReadOnlyCollection<string> Roles)
{
    /// <summary>
    /// Ngữ cảnh rỗng khi không có thông tin người gọi.
    /// </summary>
    public static IdentityAccessCallerContext Empty { get; } = new(null, Array.Empty<string>());
}
