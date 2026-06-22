using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Navi.ToolsAssets.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingCatalogItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SettingCatalogItems",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CatalogType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SettingCatalogItems", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SettingCatalogItems_CatalogType_Code",
                schema: "Organization",
                table: "SettingCatalogItems",
                columns: new[] { "CatalogType", "Code" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SettingCatalogItems",
                schema: "Organization");
        }
    }
}
