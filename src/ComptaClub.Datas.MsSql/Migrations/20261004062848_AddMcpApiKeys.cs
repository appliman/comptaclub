using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComptaClub.Datas.Migrations
{
    /// <inheritdoc />
    public partial class AddMcpApiKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "McpApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KeyIdentifier = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    SecretHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    SecretLastFour = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    CreationDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsageCount = table.Column<long>(type: "bigint", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_McpApiKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_McpApiKeys_CreatedByUserId",
                table: "McpApiKeys",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_McpApiKeys_KeyIdentifier",
                table: "McpApiKeys",
                column: "KeyIdentifier",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "McpApiKeys");
        }
    }
}
