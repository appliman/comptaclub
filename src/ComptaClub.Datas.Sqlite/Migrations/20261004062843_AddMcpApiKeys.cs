using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ComptaClub.Datas.Sqlite.Migrations
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
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    KeyIdentifier = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                    SecretHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    SecretLastFour = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    CreationDateUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ExpirationDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RevokedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ArchivedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastUsedDateUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsageCount = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<Guid>(type: "TEXT", nullable: false)
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
