namespace BuildingBlocks.SharedKernel.SeedWorks;

/// <summary>
/// Kết quả phân trang dùng chung cho các truy vấn danh sách.
/// </summary>
/// <typeparam name="T">Kiểu phần tử trong danh sách.</typeparam>
/// <param name="Items">Danh sách phần tử của trang hiện tại.</param>
/// <param name="TotalCount">Tổng số phần tử thỏa điều kiện truy vấn.</param>
/// <param name="Page">Số trang hiện tại.</param>
/// <param name="PageSize">Kích thước trang.</param>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize);