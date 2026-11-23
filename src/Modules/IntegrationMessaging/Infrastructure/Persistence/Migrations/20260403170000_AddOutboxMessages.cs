using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntegrationMessaging.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng lưu trữ Bản ghi Outbox cho module IntegrationMessaging.
    /// </summary>
    public partial class AddOutboxMessages : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo schema, bảng và chỉ mục cho OutboxMessage.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "integration_messaging");

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                schema: "integration_messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_MessageId",
                schema: "integration_messaging",
                table: "OutboxMessages",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OccurredAtUtc",
                schema: "integration_messaging",
                table: "OutboxMessages",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_Status",
                schema: "integration_messaging",
                table: "OutboxMessages",
                column: "Status");
        }

        /// <summary>
        /// Hoàn tác migration bằng cách xóa bảng OutboxMessage.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxMessages",
                schema: "integration_messaging");
        }
    }
}
