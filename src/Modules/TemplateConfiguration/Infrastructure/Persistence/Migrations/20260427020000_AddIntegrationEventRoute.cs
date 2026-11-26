using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemplateConfiguration.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng Định tuyến sự kiện tích hợp cho module TemplateConfiguration.
    /// </summary>
    public partial class AddIntegrationEventRoute : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo bảng và chỉ mục cho Định tuyến sự kiện tích hợp.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntegrationEventRoutes",
                schema: "template_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NotificationTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IntegrationEventRoutes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventRoutes_Channel",
                schema: "template_configuration",
                table: "IntegrationEventRoutes",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventRoutes_EventType",
                schema: "template_configuration",
                table: "IntegrationEventRoutes",
                column: "EventType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventRoutes_IsActive",
                schema: "template_configuration",
                table: "IntegrationEventRoutes",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_IntegrationEventRoutes_NotificationTemplateId",
                schema: "template_configuration",
                table: "IntegrationEventRoutes",
                column: "NotificationTemplateId");
        }

        /// <summary>
        /// Hoàn tác migration tạo bảng Định tuyến sự kiện tích hợp.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntegrationEventRoutes",
                schema: "template_configuration");
        }
    }
}
