using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Navi.ToolsAssets.Infrastructure.Persistence.Migrations;

public partial class AddEnterpriseSecurity : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: "Security");

        migrationBuilder.Sql(
            """
            IF EXISTS (
                SELECT 1
                FROM dbo.AppUsers
                GROUP BY UserName
                HAVING COUNT(*) > 1
            )
                THROW 51000, 'No se puede crear el índice único: existen AppUsers.UserName duplicados.', 1;

            IF EXISTS (SELECT 1 FROM dbo.AppUsers WHERE LEN(UserName) > 150)
                THROW 51001, 'No se puede limitar AppUsers.UserName: existen valores mayores a 150 caracteres.', 1;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "UserName",
            table: "AppUsers",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.Sql(
            """
            IF COL_LENGTH('dbo.AppUsers', 'PasswordHash') IS NULL
                ALTER TABLE dbo.AppUsers ADD PasswordHash nvarchar(500) NULL;

            IF COL_LENGTH('dbo.AppUsers', 'FailedLoginAttempts') IS NULL
                ALTER TABLE dbo.AppUsers ADD FailedLoginAttempts int NOT NULL
                    CONSTRAINT DF_AppUsers_FailedLoginAttempts DEFAULT (0);

            IF COL_LENGTH('dbo.AppUsers', 'LockoutEndAt') IS NULL
                ALTER TABLE dbo.AppUsers ADD LockoutEndAt datetime2 NULL;

            IF COL_LENGTH('dbo.AppUsers', 'PasswordChangedAt') IS NULL
                ALTER TABLE dbo.AppUsers ADD PasswordChangedAt datetime2 NULL;

            IF COL_LENGTH('dbo.AppUsers', 'SecurityStamp') IS NULL
                ALTER TABLE dbo.AppUsers ADD SecurityStamp uniqueidentifier NULL;

            UPDATE dbo.AppUsers
            SET SecurityStamp = NEWID()
            WHERE SecurityStamp IS NULL OR SecurityStamp = '00000000-0000-0000-0000-000000000000';

            ALTER TABLE dbo.AppUsers ALTER COLUMN SecurityStamp uniqueidentifier NOT NULL;
            """);

        migrationBuilder.CreateTable(
            name: "AuditLogs",
            schema: "Security",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UserName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Action = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Module = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                EntityType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                EntityId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                Result = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                CorrelationId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                BeforeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                AfterJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ErrorCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AuditLogs", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "UserSessions",
            schema: "Security",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                SecurityStamp = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                LastActivityAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                AbsoluteExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RevokedReason = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_UserSessions", x => x.Id);
                table.ForeignKey(
                    name: "FK_UserSessions_AppUsers_AppUserId",
                    column: x => x.AppUserId,
                    principalTable: "AppUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_IsActive_LockoutEndAt",
            table: "AppUsers",
            columns: new[] { "IsActive", "LockoutEndAt" });

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_UserName",
            table: "AppUsers",
            column: "UserName",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_CompanyId_TimestampUtc",
            schema: "Security",
            table: "AuditLogs",
            columns: new[] { "CompanyId", "TimestampUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_TimestampUtc",
            schema: "Security",
            table: "AuditLogs",
            column: "TimestampUtc");

        migrationBuilder.CreateIndex(
            name: "IX_AuditLogs_UserId_TimestampUtc",
            schema: "Security",
            table: "AuditLogs",
            columns: new[] { "UserId", "TimestampUtc" });

        migrationBuilder.CreateIndex(
            name: "IX_UserSessions_AbsoluteExpiresAtUtc",
            schema: "Security",
            table: "UserSessions",
            column: "AbsoluteExpiresAtUtc");

        migrationBuilder.CreateIndex(
            name: "IX_UserSessions_AppUserId_RevokedAtUtc",
            schema: "Security",
            table: "UserSessions",
            columns: new[] { "AppUserId", "RevokedAtUtc" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "AuditLogs", schema: "Security");
        migrationBuilder.DropTable(name: "UserSessions", schema: "Security");
        migrationBuilder.DropIndex(name: "IX_AppUsers_IsActive_LockoutEndAt", table: "AppUsers");
        migrationBuilder.DropIndex(name: "IX_AppUsers_UserName", table: "AppUsers");

        migrationBuilder.DropColumn(name: "FailedLoginAttempts", table: "AppUsers");
        migrationBuilder.DropColumn(name: "LockoutEndAt", table: "AppUsers");
        migrationBuilder.DropColumn(name: "PasswordChangedAt", table: "AppUsers");
        migrationBuilder.DropColumn(name: "SecurityStamp", table: "AppUsers");

        // PasswordHash se conserva porque instalaciones anteriores pudieron
        // haberlo creado mediante el adaptador legado y no debe perderse.
        migrationBuilder.AlterColumn<string>(
            name: "UserName",
            table: "AppUsers",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(150)",
            oldMaxLength: 150);
    }
}
