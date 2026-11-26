using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TemplateConfiguration.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng Cấu hình kênh thông báo cho module TemplateConfiguration.
    /// </summary>
    public partial class AddNotificationChannelConfig : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo bảng và chỉ mục cho Cấu hình kênh thông báo.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationChannelConfigs",
                schema: "template_configuration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Channel = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationChannelConfigs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannelConfigs_Channel",
                schema: "template_configuration",
                table: "NotificationChannelConfigs",
                column: "Channel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannelConfigs_IsEnabled",
                schema: "template_configuration",
                table: "NotificationChannelConfigs",
                column: "IsEnabled");
        }

        /// <summary>
        /// Hoàn tác migration tạo bảng Cấu hình kênh thông báo.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationChannelConfigs",
                schema: "template_configuration");
        }
    }
}
