using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivery.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng Thông báo trong ứng dụng cho module Delivery.
    /// </summary>
    public partial class AddInAppNotifications : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo schema, bảng và chỉ mục cho Thông báo trong ứng dụng.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "delivery");

            migrationBuilder.CreateTable(
                name: "InAppNotifications",
                schema: "delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipientUserId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    ReadStatus = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReadAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InAppNotifications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InAppNotifications_NotificationId_RecipientUserId",
                schema: "delivery",
                table: "InAppNotifications",
                columns: new[] { "NotificationId", "RecipientUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InAppNotifications_RecipientUserId_CreatedAtUtc",
                schema: "delivery",
                table: "InAppNotifications",
                columns: new[] { "RecipientUserId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_InAppNotifications_RecipientUserId_ReadStatus_CreatedAtUtc",
                schema: "delivery",
                table: "InAppNotifications",
                columns: new[] { "RecipientUserId", "ReadStatus", "CreatedAtUtc" });
        }

        /// <summary>
        /// Hoàn tác migration bằng cách xóa bảng Thông báo trong ứng dụng.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InAppNotifications",
                schema: "delivery");
        }
    }
}
