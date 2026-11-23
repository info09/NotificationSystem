using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Administration.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Migration tạo bảng Nhật ký kiểm toán và yêu cầu Gửi lại từ Hàng đợi thư chết.
    /// </summary>
    public partial class AddAuditLogAndDeadLetterResendRequest : Migration
    {
        /// <summary>
        /// Áp dụng migration tạo bảng và chỉ mục cho Administration.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogRecords",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorUserId = table.Column<string>(type: "text", nullable: false),
                    ActorRole = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    TargetType = table.Column<string>(type: "text", nullable: false),
                    TargetId = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    MetadataJson = table.Column<string>(type: "text", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeadLetterResendRequestRecords",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "text", nullable: false),
                    ActorUserId = table.Column<string>(type: "text", nullable: false),
                    QueueName = table.Column<string>(type: "text", nullable: false),
                    MessageId = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResultMessageId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadLetterResendRequestRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogRecords_OccurredAtUtc",
                schema: "administration",
                table: "AuditLogRecords",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogRecords_TargetType_TargetId",
                schema: "administration",
                table: "AuditLogRecords",
                columns: new[] { "TargetType", "TargetId" });

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterResendRequestRecords_IdempotencyKey",
                schema: "administration",
                table: "DeadLetterResendRequestRecords",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <summary>
        /// Hoàn tác migration bằng cách xóa bảng đã tạo.
        /// </summary>
        /// <param name="migrationBuilder">Builder dùng để tạo thao tác migration.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogRecords",
                schema: "administration");

            migrationBuilder.DropTable(
                name: "DeadLetterResendRequestRecords",
                schema: "administration");
        }
    }
}
