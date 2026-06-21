using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Navi.ToolsAssets.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicalLifeRecordDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ToolAccessories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToolAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RequiresMaintenance = table.Column<bool>(type: "bit", nullable: false),
                    IsMeasurementEquipment = table.Column<bool>(type: "bit", nullable: false),
                    Observation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolAccessories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolAccessories_ToolAssets_ToolAssetId",
                        column: x => x.ToolAssetId,
                        principalSchema: "Inventory",
                        principalTable: "ToolAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ToolSafePractices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToolAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PracticeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolSafePractices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolSafePractices_ToolAssets_ToolAssetId",
                        column: x => x.ToolAssetId,
                        principalSchema: "Inventory",
                        principalTable: "ToolAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ToolAccessories_ToolAssetId",
                table: "ToolAccessories",
                column: "ToolAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolSafePractices_ToolAssetId",
                table: "ToolSafePractices",
                column: "ToolAssetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ToolAccessories");

            migrationBuilder.DropTable(
                name: "ToolSafePractices");
        }
    }
}
