using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Navi.ToolsAssets.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceTechnicalScheduleFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EvidenceDocumentId",
                schema: "Maintenance",
                table: "MaintenanceRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutionNotes",
                schema: "Maintenance",
                table: "MaintenanceRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                schema: "Maintenance",
                table: "MaintenanceRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsToolOperational",
                schema: "Maintenance",
                table: "MaintenanceRecords",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MaintenanceActivities",
                schema: "Maintenance",
                table: "MaintenanceRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsibleName",
                schema: "Maintenance",
                table: "MaintenanceRecords",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePosition",
                schema: "Maintenance",
                table: "MaintenanceRecords",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EvidenceDocumentId",
                schema: "Maintenance",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "ExecutionNotes",
                schema: "Maintenance",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                schema: "Maintenance",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "IsToolOperational",
                schema: "Maintenance",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "MaintenanceActivities",
                schema: "Maintenance",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "ResponsibleName",
                schema: "Maintenance",
                table: "MaintenanceRecords");

            migrationBuilder.DropColumn(
                name: "ResponsiblePosition",
                schema: "Maintenance",
                table: "MaintenanceRecords");
        }
    }
}
