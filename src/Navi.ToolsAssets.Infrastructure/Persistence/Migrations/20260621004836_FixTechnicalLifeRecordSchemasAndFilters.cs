using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Navi.ToolsAssets.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixTechnicalLifeRecordSchemasAndFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Safety");

            migrationBuilder.RenameTable(
                name: "ToolSafePractices",
                newName: "ToolSafePractices",
                newSchema: "Safety");

            migrationBuilder.RenameTable(
                name: "ToolAccessories",
                newName: "ToolAccessories",
                newSchema: "Inventory");

            migrationBuilder.AddColumn<bool>(
                name: "HasWarranty",
                schema: "Inventory",
                table: "ToolAssets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastMaintenanceDate",
                schema: "Inventory",
                table: "ToolAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoadCapacity",
                schema: "Inventory",
                table: "ToolAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaintenancePeriodMonths",
                schema: "Inventory",
                table: "ToolAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMaintenanceDate",
                schema: "Inventory",
                table: "ToolAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                schema: "Inventory",
                table: "ToolAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsefulLifeDays",
                schema: "Inventory",
                table: "ToolAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UsefulLifeStartDate",
                schema: "Inventory",
                table: "ToolAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Voltage",
                schema: "Inventory",
                table: "ToolAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarrantyType",
                schema: "Inventory",
                table: "ToolAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PracticeName",
                schema: "Safety",
                table: "ToolSafePractices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                schema: "Safety",
                table: "ToolSafePractices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Observation",
                schema: "Inventory",
                table: "ToolAccessories",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                schema: "Inventory",
                table: "ToolAccessories",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasWarranty",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.DropColumn(
                name: "LastMaintenanceDate",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.DropColumn(
                name: "LoadCapacity",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.DropColumn(
                name: "MaintenancePeriodMonths",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceDate",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.DropColumn(
                name: "Provider",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.DropColumn(
                name: "UsefulLifeDays",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.DropColumn(
                name: "UsefulLifeStartDate",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.DropColumn(
                name: "Voltage",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.DropColumn(
                name: "WarrantyType",
                schema: "Inventory",
                table: "ToolAssets");

            migrationBuilder.RenameTable(
                name: "ToolSafePractices",
                schema: "Safety",
                newName: "ToolSafePractices");

            migrationBuilder.RenameTable(
                name: "ToolAccessories",
                schema: "Inventory",
                newName: "ToolAccessories");

            migrationBuilder.AlterColumn<string>(
                name: "PracticeName",
                table: "ToolSafePractices",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "ToolSafePractices",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<string>(
                name: "Observation",
                table: "ToolAccessories",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ToolAccessories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
