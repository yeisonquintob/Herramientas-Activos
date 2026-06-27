using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/security/roles")]
public sealed class SecurityRolesEduardoController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public SecurityRolesEduardoController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpPost("eduardo/sync")]
    public async Task<IActionResult> SyncEduardoRoles(CancellationToken cancellationToken)
    {
        var rolesTable = await FindAppRolesTableAsync(cancellationToken);

        if (rolesTable is null)
        {
            return NotFound(new
            {
                Message = "No se encontró la tabla AppRoles en la base de datos."
            });
        }

        var roles = GetRoles();

        foreach (var role in roles)
        {
            await UpsertRoleAsync(rolesTable.Value.Schema, rolesTable.Value.Table, role, cancellationToken);
        }

        return Ok(new
        {
            Message = "Roles de Eduardo sincronizados correctamente.",
            Roles = roles.Select(x => new
            {
                x.Code,
                x.Name,
                Permissions = x.Permissions.Split(';', StringSplitOptions.RemoveEmptyEntries).Length
            })
        });
    }

    private async Task<(string Schema, string Table)?> FindAppRolesTableAsync(CancellationToken cancellationToken)
    {
        var connection = _context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = @"
SELECT TOP 1 TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'AppRoles'
ORDER BY TABLE_SCHEMA";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return (reader.GetString(0), reader.GetString(1));
    }

    private async Task UpsertRoleAsync(string schema, string table, EduardoRole role, CancellationToken cancellationToken)
    {
        var fullName = $"[{schema}].[{table}]";
        var connection = _context.Database.GetDbConnection();

        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();

        command.CommandText = $@"
IF EXISTS (
    SELECT 1
    FROM {fullName}
    WHERE UPPER([Code]) = UPPER(@Code)
      AND [IsDeleted] = 0
)
BEGIN
    UPDATE {fullName}
    SET
        [Name] = @Name,
        [Description] = @Description,
        [Permissions] = @Permissions,
        [IsActive] = 1,
        [UpdatedAt] = SYSUTCDATETIME(),
        [UpdatedBy] = 'eduardo-role-sync'
    WHERE UPPER([Code]) = UPPER(@Code)
      AND [IsDeleted] = 0
END
ELSE
BEGIN
    INSERT INTO {fullName}
    (
        [Id],
        [Code],
        [Name],
        [Description],
        [Permissions],
        [IsActive],
        [IsDeleted],
        [CreatedAt],
        [CreatedBy],
        [UpdatedAt],
        [UpdatedBy]
    )
    VALUES
    (
        @Id,
        @Code,
        @Name,
        @Description,
        @Permissions,
        1,
        0,
        SYSUTCDATETIME(),
        'eduardo-role-sync',
        SYSUTCDATETIME(),
        'eduardo-role-sync'
    )
END";

        AddParameter(command, "@Id", Guid.NewGuid());
        AddParameter(command, "@Code", role.Code);
        AddParameter(command, "@Name", role.Name);
        AddParameter(command, "@Description", role.Description);
        AddParameter(command, "@Permissions", role.Permissions);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static void AddParameter(DbCommand command, string name, object? value)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        command.Parameters.Add(parameter);
    }

    private static List<EduardoRole> GetRoles()
    {
        return new()
        {
            new EduardoRole(
                "ADMIN",
                "Administrador",
                "App Escritorio - Acceso total al sistema.",
                Join(
                    "Dashboard.View",

                    "Tools.View",
                    "Tools.Create",
                    "Tools.Edit",
                    "Tools.Delete",

                    "AssetAvailability.View",
                    "AssetAvailability.Edit",

                    "AssetAssignment.View",
                    "AssetAssignment.Assign",
                    "AssetAssignment.Return",
                    "AssetAssignment.History",

                    "TechnicalLifeRecord.View",
                    "TechnicalLifeRecord.Edit",
                    "TechnicalLifeRecord.Export",

                    "Documents.View",
                    "Documents.Upload",
                    "Documents.Download",
                    "Documents.Delete",

                    "Maintenance.View",
                    "Maintenance.Request",
                    "Maintenance.Execute",
                    "Maintenance.Close",

                    "Purchases.View",
                    "Purchases.Request",
                    "Purchases.Approve",
                    "Purchases.Reject",

                    "PhysicalCounts.View",
                    "PhysicalCounts.Create",
                    "PhysicalCounts.Close",

                    "SafePractices.View",
                    "SafePractices.Manage",

                    "Reports.View",

                    "Settings.View",
                    "Settings.Manage",
                    "Security.Users",
                    "Security.Roles",

                    "Mobile.Access",
                    "Mobile.Tools.View",
                    "Mobile.Tools.Review",
                    "Mobile.PreOperational.Report",
                    "Mobile.Damage.Report",
                    "Mobile.Loans.Request",

                    "DeliveryAct.Generate",
                    "FixedAssets.Disposal.Request",
                    "FixedAssets.Disposal.Approve"
                )
            ),

            new EduardoRole(
                "GERENCIAL",
                "Gerencial",
                "App Escritorio - Visualiza y aprueba información de su zona.",
                Join(
                    "Dashboard.View",
                    "Tools.View",
                    "AssetAvailability.View",
                    "AssetAssignment.View",
                    "AssetAssignment.History",
                    "TechnicalLifeRecord.View",
                    "TechnicalLifeRecord.Export",
                    "Documents.View",
                    "Documents.Download",
                    "Maintenance.View",
                    "Maintenance.Close",
                    "Purchases.View",
                    "Purchases.Approve",
                    "Purchases.Reject",
                    "PhysicalCounts.View",
                    "Reports.View"
                )
            ),

            new EduardoRole(
                "HERRAMIENTERO",
                "Herramientero",
                "App Escritorio y App Móvil - Solicita compra/mantenimiento, genera acta de entrega y solicita baja con aprobación.",
                Join(
                    "Dashboard.View",
                    "Tools.View",
                    "Tools.Create",
                    "Tools.Edit",
                    "AssetAvailability.View",
                    "AssetAvailability.Edit",
                    "AssetAssignment.View",
                    "AssetAssignment.Assign",
                    "AssetAssignment.Return",
                    "AssetAssignment.History",
                    "TechnicalLifeRecord.View",
                    "TechnicalLifeRecord.Edit",
                    "TechnicalLifeRecord.Export",
                    "Documents.View",
                    "Documents.Upload",
                    "Documents.Download",
                    "Maintenance.View",
                    "Maintenance.Request",
                    "Purchases.View",
                    "Purchases.Request",
                    "PhysicalCounts.View",
                    "PhysicalCounts.Create",
                    "Reports.View",
                    "Mobile.Access",
                    "DeliveryAct.Generate",
                    "FixedAssets.Disposal.Request"
                )
            ),

            new EduardoRole(
                "ING_SERVICIOS",
                "Ingeniero de servicios",
                "App Escritorio y App Móvil - Aprueba solicitudes de compra y mantenimiento por sede.",
                Join(
                    "Dashboard.View",
                    "Tools.View",
                    "AssetAvailability.View",
                    "AssetAssignment.View",
                    "AssetAssignment.History",
                    "TechnicalLifeRecord.View",
                    "TechnicalLifeRecord.Export",
                    "Documents.View",
                    "Documents.Download",
                    "Maintenance.View",
                    "Maintenance.Close",
                    "Purchases.View",
                    "Purchases.Approve",
                    "Purchases.Reject",
                    "PhysicalCounts.View",
                    "Reports.View",
                    "Mobile.Access",
                    "FixedAssets.Disposal.Approve"
                )
            ),

            new EduardoRole(
                "COORDINADOR_TALLER",
                "Coordinador de taller",
                "App Escritorio y App Móvil - Solicita y aprueba compra/mantenimiento, genera acta de entrega.",
                Join(
                    "Dashboard.View",
                    "Tools.View",
                    "Tools.Create",
                    "Tools.Edit",
                    "AssetAvailability.View",
                    "AssetAvailability.Edit",
                    "AssetAssignment.View",
                    "AssetAssignment.Assign",
                    "AssetAssignment.Return",
                    "AssetAssignment.History",
                    "TechnicalLifeRecord.View",
                    "TechnicalLifeRecord.Edit",
                    "TechnicalLifeRecord.Export",
                    "Documents.View",
                    "Documents.Upload",
                    "Documents.Download",
                    "Documents.Delete",
                    "Maintenance.View",
                    "Maintenance.Request",
                    "Maintenance.Execute",
                    "Maintenance.Close",
                    "Purchases.View",
                    "Purchases.Request",
                    "Purchases.Approve",
                    "Purchases.Reject",
                    "PhysicalCounts.View",
                    "PhysicalCounts.Create",
                    "PhysicalCounts.Close",
                    "Reports.View",
                    "Mobile.Access",
                    "DeliveryAct.Generate",
                    "FixedAssets.Disposal.Request",
                    "FixedAssets.Disposal.Approve"
                )
            ),

            new EduardoRole(
                "TECNICO",
                "Técnico",
                "App Móvil + consulta web limitada - Revisa herramientas asignadas, consulta historial, reporta preoperacional SSTA, daños/novedades y solicita préstamo.",
                Join(
                    "Tools.View",
                    "AssetAssignment.History",
                    "TechnicalLifeRecord.View",

                    "Documents.View",
                    "Documents.Upload",

                    "Maintenance.View",
                    "Maintenance.Request",

                    "Mobile.Access",
                    "Mobile.Tools.View",
                    "Mobile.Tools.Review",
                    "Mobile.PreOperational.Report",
                    "Mobile.Damage.Report",
                    "Mobile.Loans.Request"
                )
            )
        };
    }

    private static string Join(params string[] permissions)
    {
        return string.Join(";", permissions.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private sealed record EduardoRole(
        string Code,
        string Name,
        string Description,
        string Permissions);
}


