using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntegrationMessaging.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng Sự kiện tích hợp cho module IntegrationMessaging.
    /// </summary>
    public partial class AddIntegrationEvents : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo bảng và chỉ mục cho Sự kiện tích hợp.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntegrationEvents",
                schema: "integration_messaging",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Source = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Version = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    ReceivedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEvents_EventId",
                schema: "integration_messaging",
                table: "IntegrationEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEvents_EventType",
                schema: "integration_messaging",
                table: "IntegrationEvents",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEvents_MessageId",
                schema: "integration_messaging",
                table: "IntegrationEvents",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEvents_ReceivedAtUtc",
                schema: "integration_messaging",
                table: "IntegrationEvents",
                column: "ReceivedAtUtc");
        }

        /// <summary>
        /// Hoàn tác migration tạo bảng Sự kiện tích hợp.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationEvents",
                schema: "integration_messaging");
        }
    }
}
