using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntegrationMessaging.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng Bản ghi Inbox cho module IntegrationMessaging.
    /// </summary>
    public partial class AddInboxMessages : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo bảng và chỉ mục cho InboxMessage.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InboxMessages",
                schema: "integration_messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerName = table.Column<string>(type: "text", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InboxMessages_MessageId_ConsumerName",
                schema: "integration_messaging",
                table: "InboxMessages",
                columns: new[] { "MessageId", "ConsumerName" },
                unique: true);
        }

        /// <summary>
        /// Hoàn tác migration bằng cách xóa bảng InboxMessage.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxMessages",
                schema: "integration_messaging");
        }
    }
}
