using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Delivery.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng Nhật ký gửi thông báo và Lần thử gửi cho module Delivery.
    /// </summary>
    public partial class AddNotificationDeliveryLogs : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo bảng và chỉ mục cho Nhật ký gửi thông báo và Lần thử gửi.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeliveryAttempts",
                schema: "delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    AttemptedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAttempts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationDeliveryLogs",
                schema: "delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Result = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CorrelationId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDeliveryLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAttempts_CorrelationId",
                schema: "delivery",
                table: "DeliveryAttempts",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAttempts_NotificationId",
                schema: "delivery",
                table: "DeliveryAttempts",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAttempts_NotificationId_AttemptedAtUtc",
                schema: "delivery",
                table: "DeliveryAttempts",
                columns: new[] { "NotificationId", "AttemptedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveryLogs_CorrelationId",
                schema: "delivery",
                table: "NotificationDeliveryLogs",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveryLogs_NotificationId",
                schema: "delivery",
                table: "NotificationDeliveryLogs",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveryLogs_NotificationId_CreatedAtUtc",
                schema: "delivery",
                table: "NotificationDeliveryLogs",
                columns: new[] { "NotificationId", "CreatedAtUtc" });
        }

        /// <summary>
        /// Hoàn tác migration bằng cách xóa bảng Nhật ký gửi thông báo và Lần thử gửi.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryAttempts",
                schema: "delivery");

            migrationBuilder.DropTable(
                name: "NotificationDeliveryLogs",
                schema: "delivery");
        }
    }
}
