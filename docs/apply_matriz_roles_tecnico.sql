SET NOCOUNT ON;

DECLARE @SchemaName sysname = N'Security';
DECLARE @TableName sysname = N'AppRoles';
DECLARE @FullTable nvarchar(300) = QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName);

IF OBJECT_ID(@FullTable, 'U') IS NULL
BEGIN
    THROW 50000, 'No existe la tabla Security.AppRoles. Verifique el esquema de seguridad.', 1;
END;

DECLARE @TecnicoPermissions nvarchar(max) =
    N'Tools.View;' +
    N'AssetAssignment.History;' +
    N'TechnicalLifeRecord.View;' +
    N'Documents.View;' +
    N'Documents.Upload;' +
    N'Mobile.Access;' +
    N'Mobile.Tools.View;' +
    N'Mobile.Tools.Review;' +
    N'Mobile.PreOperational.Report;' +
    N'Mobile.Damage.Report;' +
    N'Mobile.Loans.Request';

BEGIN TRANSACTION;

BEGIN TRY

    UPDATE Security.AppRoles
    SET
        [Name] = N'Técnico',
        [Description] = N'App Móvil - Revisa herramientas asignadas, consulta historial propio, reporta preoperacional SSTA, daños/novedades, evidencias y préstamos. Sin compras, sin planes de mantenimiento y sin conciliación.',
        [Permissions] = @TecnicoPermissions,
        [IsActive] = 1,
        [UpdatedAt] = SYSUTCDATETIME(),
        [UpdatedBy] = N'matriz-roles-permisos'
    WHERE UPPER([Code]) IN (N'TECNICO', N'TÉCNICO')
      AND ISNULL([IsDeleted], 0) = 0;

    IF NOT EXISTS (
        SELECT 1
        FROM Security.AppRoles
        WHERE UPPER([Code]) IN (N'TECNICO', N'TÉCNICO')
          AND ISNULL([IsDeleted], 0) = 0
    )
    BEGIN
        INSERT INTO Security.AppRoles
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
            NEWID(),
            N'TECNICO',
            N'Técnico',
            N'App Móvil - Revisa herramientas asignadas, consulta historial propio, reporta preoperacional SSTA, daños/novedades, evidencias y préstamos. Sin compras, sin planes de mantenimiento y sin conciliación.',
            @TecnicoPermissions,
            1,
            0,
            SYSUTCDATETIME(),
            N'matriz-roles-permisos',
            SYSUTCDATETIME(),
            N'matriz-roles-permisos'
        );
    END;

    COMMIT TRANSACTION;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

SELECT
    [Code],
    [Name],
    [Description],
    [Permissions],
    [IsActive],
    [UpdatedAt],
    [UpdatedBy]
FROM Security.AppRoles
WHERE UPPER([Code]) IN (N'TECNICO', N'TÉCNICO')
  AND ISNULL([IsDeleted], 0) = 0;
