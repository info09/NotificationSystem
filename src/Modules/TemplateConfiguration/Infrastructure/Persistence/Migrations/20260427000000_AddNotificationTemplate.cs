using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemplateConfiguration.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng Mẫu thông báo cho module TemplateConfiguration.
    /// </summary>
    public partial class AddNotificationTemplate : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo bảng và chỉ mục cho Mẫu thông báo.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "template_configuration");

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                schema: "template_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    Channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Subject = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Body = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Channel",
                schema: "template_configuration",
                table: "NotificationTemplates",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsActive",
                schema: "template_configuration",
                table: "NotificationTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_TemplateKey",
                schema: "template_configuration",
                table: "NotificationTemplates",
                column: "TemplateKey",
                unique: true);
        }

        /// <summary>
        /// Hoàn tác migration tạo bảng Mẫu thông báo.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationTemplates",
                schema: "template_configuration");
        }
    }
}
