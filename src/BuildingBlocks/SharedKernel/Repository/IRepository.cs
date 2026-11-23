using System.Linq.Expressions;

namespace BuildingBlocks.SharedKernel.Repository;

/// <summary>
/// Giao diện repository cơ sở cho các thao tác truy cập dữ liệu của thực thể.
/// </summary>
/// <typeparam name="T">Kiểu thực thể domain.</typeparam>
public interface IRepository<T> where T : Entity.Entity
{
    /// <summary>
    /// Lấy thực thể theo khóa định danh.
    /// </summary>
    /// <param name="id">Khóa định danh của thực thể.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Thực thể nếu tìm thấy; ngược lại null.</returns>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy toàn bộ thực thể.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách tất cả thực thể.</returns>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Tìm các thực thể thỏa điều kiện.
    /// </summary>
    /// <param name="predicate">Điều kiện lọc.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Danh sách thực thể thỏa điều kiện.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Thêm thực thể mới vào ngữ cảnh dữ liệu.
    /// </summary>
    /// <param name="entity">Thực thể cần thêm.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Thực thể đã được thêm.</returns>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cập nhật thực thể trong ngữ cảnh dữ liệu.
    /// </summary>
    /// <param name="entity">Thực thể cần cập nhật.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Xóa thực thể khỏi ngữ cảnh dữ liệu.
    /// </summary>
    /// <param name="entity">Thực thể cần xóa.</param>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lưu các thay đổi vào cơ sở dữ liệu.
    /// </summary>
    /// <param name="cancellationToken">Token hủy thao tác.</param>
    /// <returns>Số bản ghi bị ảnh hưởng.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}