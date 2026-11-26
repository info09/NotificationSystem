namespace BuildingBlocks.SharedKernel.Constants;

/// <summary>
/// Hằng số tên policy RBAC dùng trong toàn hệ thống.
/// </summary>
public static class RbacPolicies
{
    /// <summary>
    /// Policy dành cho vai trò Quản trị viên hệ thống.
    /// </summary>
    public const string SystemAdministrator = "SystemAdministrator";

    /// <summary>
    /// Policy dành cho vai trò Nhân viên vận hành.
    /// </summary>
    public const string BusinessOperator = "BusinessOperator";

    /// <summary>
    /// Policy dành cho vai trò Nhà phát triển tích hợp.
    /// </summary>
    public const string IntegrationClient = "IntegrationClient";

    /// <summary>
    /// Policy cho phép đọc dữ liệu Thông báo theo quyền.
    /// </summary>
    public const string NotificationRead = "NotificationRead";

    /// <summary>
    /// Policy cho phép xem Hàng đợi thư chết.
    /// </summary>
    public const string DeadLetterQueueRead = "DeadLetterQueueRead";

    /// <summary>
    /// Policy cho phép gửi lại Thông điệp từ Hàng đợi thư chết.
    /// </summary>
    public const string DeadLetterQueueResend = "DeadLetterQueueResend";

    /// <summary>
    /// Policy cho phép đọc Nhật ký kiểm toán.
    /// </summary>
    public const string AuditLogRead = "AuditLogRead";
}
