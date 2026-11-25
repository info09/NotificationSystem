using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration đảm bảo cột CreatedAt tồn tại cho bảng Notifications.
    /// </summary>
    public partial class EnsureNotificationCreatedAtColumn : Migration
    {
        /// <summary>
        /// Áp dụng migration bổ sung cột và chỉ mục CreatedAt nếu thiếu.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                ALTER TABLE notification."Notifications"
                ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW();
                """);

            migrationBuilder.Sql(
                """
                CREATE INDEX IF NOT EXISTS "IX_Notifications_CreatedAt"
                ON notification."Notifications" ("CreatedAt");
                """);
        }

        /// <summary>
        /// Hoàn tác migration bằng cách xóa chỉ mục và cột CreatedAt.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DROP INDEX IF EXISTS notification."IX_Notifications_CreatedAt";
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE notification."Notifications"
                DROP COLUMN IF EXISTS "CreatedAt";
                """);
        }
    }
}
