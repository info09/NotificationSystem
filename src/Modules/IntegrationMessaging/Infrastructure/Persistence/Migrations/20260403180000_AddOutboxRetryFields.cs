using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntegrationMessaging.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration bổ sung trường retry cho Bản ghi Outbox.
    /// </summary>
    public partial class AddOutboxRetryFields : Migration
    {
        /// <summary>
        /// Áp dụng migration thêm cột retry và chỉ mục phục vụ publish.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastError",
                schema: "integration_messaging",
                table: "OutboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextAttemptAtUtc",
                schema: "integration_messaging",
                table: "OutboxMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc));

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                schema: "integration_messaging",
                table: "OutboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(
                """
                UPDATE integration_messaging."OutboxMessages"
                SET "NextAttemptAtUtc" = "OccurredAtUtc"
                WHERE "NextAttemptAtUtc" = TIMESTAMPTZ '1970-01-01 00:00:00+00';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status_NextAttemptAtUtc",
                schema: "integration_messaging",
                table: "OutboxMessages",
                columns: new[] { "Status", "NextAttemptAtUtc" });
        }

        /// <summary>
        /// Hoàn tác migration bằng cách xóa cột retry và chỉ mục liên quan.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_Status_NextAttemptAtUtc",
                schema: "integration_messaging",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "LastError",
                schema: "integration_messaging",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "NextAttemptAtUtc",
                schema: "integration_messaging",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                schema: "integration_messaging",
                table: "OutboxMessages");
        }
    }
}
