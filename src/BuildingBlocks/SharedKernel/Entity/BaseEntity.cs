namespace BuildingBlocks.SharedKernel.Entity;

/// <summary>
/// Thực thể cơ sở chứa các trường chung cho tất cả thực thể trong domain.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Khóa định danh duy nhất của thực thể, tự động tạo khi khởi tạo.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
}

/// <summary>
/// Lớp cơ sở cho tất cả thực thể domain, kế thừa từ <see cref="BaseEntity"/>.
/// </summary>
public abstract class Entity : BaseEntity
{
}
