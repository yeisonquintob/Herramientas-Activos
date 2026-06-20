using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Navi.ToolsAssets.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateCoreDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Damages");

            migrationBuilder.EnsureSchema(
                name: "Sync");

            migrationBuilder.EnsureSchema(
                name: "Maintenance");

            migrationBuilder.EnsureSchema(
                name: "PhysicalCounts");

            migrationBuilder.EnsureSchema(
                name: "Inventory");

            migrationBuilder.EnsureSchema(
                name: "Documents");

            migrationBuilder.EnsureSchema(
                name: "LifeCycle");

            migrationBuilder.EnsureSchema(
                name: "Loans");

            migrationBuilder.CreateTable(
                name: "ResponsiblePeople",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Area = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResponsiblePeople", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToolCategories",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ToolTypes",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    City = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPilot = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalSchema: "Organization",
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalCounts",
                schema: "PhysicalCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResponsibleBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalCounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhysicalCounts_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolLoans",
                schema: "Loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoanNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestedByPersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpectedReturnAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReturnedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolLoans_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolLoans_ResponsiblePeople_RequestedByPersonId",
                        column: x => x.RequestedByPersonId,
                        principalSchema: "Organization",
                        principalTable: "ResponsiblePeople",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolLocations",
                schema: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolLocations_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolAssets",
                schema: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InternalCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Model = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FixedAssetCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FenixCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AcquisitionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsefulLifeMonths = table.Column<int>(type: "int", nullable: true),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequiresMaintenance = table.Column<bool>(type: "bit", nullable: false),
                    RequiresPreOperationalCheck = table.Column<bool>(type: "bit", nullable: false),
                    RequiresCertification = table.Column<bool>(type: "bit", nullable: false),
                    CertificationExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZoneId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ResponsiblePersonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToolTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ToolCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperationalStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    PhysicalStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CustodyStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ReconciliationStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SyncStatus = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FenixName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    FenixStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FenixBranch = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FenixResponsible = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LastSyncAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolAssets_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "Organization",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolAssets_ResponsiblePeople_ResponsiblePersonId",
                        column: x => x.ResponsiblePersonId,
                        principalSchema: "Organization",
                        principalTable: "ResponsiblePeople",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolAssets_ToolCategories_ToolCategoryId",
                        column: x => x.ToolCategoryId,
                        principalSchema: "Inventory",
                        principalTable: "ToolCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolAssets_ToolLocations_LocationId",
                        column: x => x.LocationId,
                        principalSchema: "Organization",
                        principalTable: "ToolLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolAssets_ToolTypes_ToolTypeId",
                        column: x => x.ToolTypeId,
                        principalSchema: "Inventory",
                        principalTable: "ToolTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolAssets_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalSchema: "Organization",
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DamageReports",
                schema: "Damages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ToolAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReportedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ActionTaken = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    BlocksLoan = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DamageReports_ToolAssets_ToolAssetId",
                        column: x => x.ToolAssetId,
                        principalSchema: "Inventory",
                        principalTable: "ToolAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FenixReconciliationRecords",
                schema: "Sync",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToolAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceSystem = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FenixCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FixedAssetCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FenixStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FenixBranch = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    FenixResponsible = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ResultStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Differences = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FenixReconciliationRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FenixReconciliationRecords_ToolAssets_ToolAssetId",
                        column: x => x.ToolAssetId,
                        principalSchema: "Inventory",
                        principalTable: "ToolAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MaintenanceRecords",
                schema: "Maintenance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaintenanceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ToolAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinishedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Technician = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Result = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaintenanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaintenanceRecords_ToolAssets_ToolAssetId",
                        column: x => x.ToolAssetId,
                        principalSchema: "Inventory",
                        principalTable: "ToolAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhysicalCountItems",
                schema: "PhysicalCounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PhysicalCountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToolAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WasFound = table.Column<bool>(type: "bit", nullable: false),
                    ExpectedLocation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    FoundLocation = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Observation = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    CountedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalCountItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhysicalCountItems_PhysicalCounts_PhysicalCountId",
                        column: x => x.PhysicalCountId,
                        principalSchema: "PhysicalCounts",
                        principalTable: "PhysicalCounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhysicalCountItems_ToolAssets_ToolAssetId",
                        column: x => x.ToolAssetId,
                        principalSchema: "Inventory",
                        principalTable: "ToolAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolDocuments",
                schema: "Documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToolAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ObjectKey = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolDocuments_ToolAssets_ToolAssetId",
                        column: x => x.ToolAssetId,
                        principalSchema: "Inventory",
                        principalTable: "ToolAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolLifeCycleEvents",
                schema: "LifeCycle",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToolAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PreviousValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RegisteredBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolLifeCycleEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolLifeCycleEvents_ToolAssets_ToolAssetId",
                        column: x => x.ToolAssetId,
                        principalSchema: "Inventory",
                        principalTable: "ToolAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ToolLoanItems",
                schema: "Loans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToolLoanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ToolAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeliveryCondition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReturnCondition = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Returned = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolLoanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ToolLoanItems_ToolAssets_ToolAssetId",
                        column: x => x.ToolAssetId,
                        principalSchema: "Inventory",
                        principalTable: "ToolAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ToolLoanItems_ToolLoans_ToolLoanId",
                        column: x => x.ToolLoanId,
                        principalSchema: "Loans",
                        principalTable: "ToolLoans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                schema: "Organization",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Branches_ZoneId",
                schema: "Organization",
                table: "Branches",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ReportNumber",
                schema: "Damages",
                table: "DamageReports",
                column: "ReportNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DamageReports_ToolAssetId",
                schema: "Damages",
                table: "DamageReports",
                column: "ToolAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_FenixReconciliationRecords_ToolAssetId",
                schema: "Sync",
                table: "FenixReconciliationRecords",
                column: "ToolAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_MaintenanceNumber",
                schema: "Maintenance",
                table: "MaintenanceRecords",
                column: "MaintenanceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaintenanceRecords_ToolAssetId",
                schema: "Maintenance",
                table: "MaintenanceRecords",
                column: "ToolAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalCountItems_PhysicalCountId",
                schema: "PhysicalCounts",
                table: "PhysicalCountItems",
                column: "PhysicalCountId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalCountItems_ToolAssetId",
                schema: "PhysicalCounts",
                table: "PhysicalCountItems",
                column: "ToolAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalCounts_BranchId",
                schema: "PhysicalCounts",
                table: "PhysicalCounts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PhysicalCounts_CountNumber",
                schema: "PhysicalCounts",
                table: "PhysicalCounts",
                column: "CountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResponsiblePeople_EmployeeCode",
                schema: "Organization",
                table: "ResponsiblePeople",
                column: "EmployeeCode",
                filter: "[EmployeeCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_BranchId",
                schema: "Inventory",
                table: "ToolAssets",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_FenixCode",
                schema: "Inventory",
                table: "ToolAssets",
                column: "FenixCode",
                unique: true,
                filter: "[FenixCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_FixedAssetCode",
                schema: "Inventory",
                table: "ToolAssets",
                column: "FixedAssetCode",
                unique: true,
                filter: "[FixedAssetCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_InternalCode",
                schema: "Inventory",
                table: "ToolAssets",
                column: "InternalCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_LocationId",
                schema: "Inventory",
                table: "ToolAssets",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_ResponsiblePersonId",
                schema: "Inventory",
                table: "ToolAssets",
                column: "ResponsiblePersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_SerialNumber",
                schema: "Inventory",
                table: "ToolAssets",
                column: "SerialNumber",
                filter: "[SerialNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_ToolCategoryId",
                schema: "Inventory",
                table: "ToolAssets",
                column: "ToolCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_ToolTypeId",
                schema: "Inventory",
                table: "ToolAssets",
                column: "ToolTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolAssets_ZoneId",
                schema: "Inventory",
                table: "ToolAssets",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolCategories_Code",
                schema: "Inventory",
                table: "ToolCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolDocuments_ToolAssetId",
                schema: "Documents",
                table: "ToolDocuments",
                column: "ToolAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolLifeCycleEvents_ToolAssetId",
                schema: "LifeCycle",
                table: "ToolLifeCycleEvents",
                column: "ToolAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolLoanItems_ToolAssetId",
                schema: "Loans",
                table: "ToolLoanItems",
                column: "ToolAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolLoanItems_ToolLoanId",
                schema: "Loans",
                table: "ToolLoanItems",
                column: "ToolLoanId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolLoans_BranchId",
                schema: "Loans",
                table: "ToolLoans",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolLoans_LoanNumber",
                schema: "Loans",
                table: "ToolLoans",
                column: "LoanNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolLoans_RequestedByPersonId",
                schema: "Loans",
                table: "ToolLoans",
                column: "RequestedByPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolLocations_BranchId_Code",
                schema: "Organization",
                table: "ToolLocations",
                columns: new[] { "BranchId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToolTypes_Code",
                schema: "Inventory",
                table: "ToolTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Zones_Code",
                schema: "Organization",
                table: "Zones",
                column: "Code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DamageReports",
                schema: "Damages");

            migrationBuilder.DropTable(
                name: "FenixReconciliationRecords",
                schema: "Sync");

            migrationBuilder.DropTable(
                name: "MaintenanceRecords",
                schema: "Maintenance");

            migrationBuilder.DropTable(
                name: "PhysicalCountItems",
                schema: "PhysicalCounts");

            migrationBuilder.DropTable(
                name: "ToolDocuments",
                schema: "Documents");

            migrationBuilder.DropTable(
                name: "ToolLifeCycleEvents",
                schema: "LifeCycle");

            migrationBuilder.DropTable(
                name: "ToolLoanItems",
                schema: "Loans");

            migrationBuilder.DropTable(
                name: "PhysicalCounts",
                schema: "PhysicalCounts");

            migrationBuilder.DropTable(
                name: "ToolAssets",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ToolLoans",
                schema: "Loans");

            migrationBuilder.DropTable(
                name: "ToolCategories",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ToolLocations",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "ToolTypes",
                schema: "Inventory");

            migrationBuilder.DropTable(
                name: "ResponsiblePeople",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "Organization");

            migrationBuilder.DropTable(
                name: "Zones",
                schema: "Organization");
        }
    }
}
