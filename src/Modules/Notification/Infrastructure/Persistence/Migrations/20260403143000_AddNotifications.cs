using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng lưu trữ Thông báo cho module Notification.
    /// </summary>
    public partial class AddNotifications : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo schema, bảng và chỉ mục cho Thông báo.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "notification");

            migrationBuilder.CreateTable(
                name: "Notifications",
                schema: "notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Recipient = table.Column<string>(type: "text", nullable: false),
                    Channel = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    NotificationTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyScope = table.Column<string>(type: "text", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IdempotencyScope_IdempotencyKey",
                schema: "notification",
                table: "Notifications",
                columns: new[] { "IdempotencyScope", "IdempotencyKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Status",
                schema: "notification",
                table: "Notifications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Channel",
                schema: "notification",
                table: "Notifications",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                schema: "notification",
                table: "Notifications",
                column: "CreatedAt");
        }

        /// <summary>
        /// Hoàn tác migration bằng cách xóa bảng Thông báo.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications",
                schema: "notification");
        }
    }
}
