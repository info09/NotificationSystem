namespace BuildingBlocks.SharedKernel.Entity;

/// <summary>    /// Thực thể cơ sở — chứa các trường chung cho tất cả thực thể trong domain.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Khóa Id duy nhất của thực thể, tự động tạo khi khởi tạo.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    ///// <summary>
    ///// Thời gian tạo thực thể (UTC).
    ///// </summary>
    //public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    ///// <summary>
    ///// Thời gian cập nhật cuối cùng (UTC), null nếu chưa cập nhật.
    ///// </summary>
    //public DateTime? UpdatedAt { get; set; }

    ///// <summary>
    ///// Cờ xóa mềm — true nếu thực thể đã bị đánh dấu xóa.
    ///// </summary>
    //public bool IsDeleted { get; set; } = false;
}

/// <summary>
/// Dấu hiệu cho tất cả thực thể trong domain — kế thừa từ BaseEntity.
/// </summary>
public abstract class Entity : BaseEntity
{
}
