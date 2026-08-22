using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentNotificationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentNotificationSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NotificationKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecipientType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentNotificationSettings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotificationSettings_IsEnabled",
                table: "PaymentNotificationSettings",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotificationSettings_TenantId_ApplicationCode_NotificationKey_Channel_RecipientType",
                table: "PaymentNotificationSettings",
                columns: new[] { "TenantId", "ApplicationCode", "NotificationKey", "Channel", "RecipientType" },
                unique: true,
                filter: "[DeletedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotificationSettings_TenantId_NotificationKey",
                table: "PaymentNotificationSettings",
                columns: new[] { "TenantId", "NotificationKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentNotificationSettings");
        }
    }
}
