using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Navi.ToolsAssets.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddImportQualityTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Decision",
                schema: "Imports",
                table: "ImportRows",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DecisionAt",
                schema: "Imports",
                table: "ImportRows",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DecisionBy",
                schema: "Imports",
                table: "ImportRows",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorsJson",
                schema: "Imports",
                table: "ImportRows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MatchKey",
                schema: "Imports",
                table: "ImportRows",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedDataJson",
                schema: "Imports",
                table: "ImportRows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetToolAssetId",
                schema: "Imports",
                table: "ImportRows",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarningsJson",
                schema: "Imports",
                table: "ImportRows",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IgnoredRows",
                schema: "Imports",
                table: "ImportBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarningRows",
                schema: "Imports",
                table: "ImportBatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_ImportBatchId_ResultStatus",
                schema: "Imports",
                table: "ImportRows",
                columns: new[] { "ImportBatchId", "ResultStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_ImportRows_TargetToolAssetId",
                schema: "Imports",
                table: "ImportRows",
                column: "TargetToolAssetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ImportRows_ImportBatchId_ResultStatus",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropIndex(
                name: "IX_ImportRows_TargetToolAssetId",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropColumn(
                name: "Decision",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropColumn(
                name: "DecisionAt",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropColumn(
                name: "DecisionBy",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropColumn(
                name: "ErrorsJson",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropColumn(
                name: "MatchKey",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropColumn(
                name: "NormalizedDataJson",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropColumn(
                name: "TargetToolAssetId",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropColumn(
                name: "WarningsJson",
                schema: "Imports",
                table: "ImportRows");

            migrationBuilder.DropColumn(
                name: "IgnoredRows",
                schema: "Imports",
                table: "ImportBatches");

            migrationBuilder.DropColumn(
                name: "WarningRows",
                schema: "Imports",
                table: "ImportBatches");
        }
    }
}
