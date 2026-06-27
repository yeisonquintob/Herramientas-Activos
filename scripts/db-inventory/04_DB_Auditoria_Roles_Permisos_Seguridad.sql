SET NOCOUNT ON;

PRINT '============================================================';
PRINT 'NAVI HERRAMIENTAS - AUDITORIA DE USUARIOS, ROLES Y PERMISOS';
PRINT '============================================================';

IF OBJECT_ID('tempdb..#ExpectedPermissions') IS NOT NULL DROP TABLE #ExpectedPermissions;

CREATE TABLE #ExpectedPermissions
(
    Code nvarchar(200) NOT NULL PRIMARY KEY,
    ModuleName nvarchar(200) NOT NULL,
    ActionName nvarchar(100) NOT NULL
);

INSERT INTO #ExpectedPermissions(Code, ModuleName, ActionName)
VALUES
('Dashboard.View', 'Dashboard', 'Ver'),
('Tools.View', 'Inventario AF', 'Ver'),
('Tools.Create', 'Inventario AF', 'Crear'),
('Tools.Edit', 'Inventario AF', 'Editar'),
('Tools.Delete', 'Inventario AF', 'Eliminar'),
('AssetAvailability.View', 'Disponible y ubicación', 'Ver'),
('AssetAvailability.Edit', 'Disponible y ubicación', 'Editar'),
('AssetAssignment.View', 'Asignar AF', 'Ver'),
('AssetAssignment.Assign', 'Asignar AF', 'Asignar'),
('AssetAssignment.Return', 'Asignar AF', 'Regresar'),
('AssetAssignment.History', 'Asignar AF', 'Historial'),
('TechnicalLifeRecord.View', 'Hoja de vida', 'Ver'),
('TechnicalLifeRecord.Edit', 'Hoja de vida', 'Editar'),
('TechnicalLifeRecord.Export', 'Hoja de vida', 'Exportar'),
('Documents.View', 'Documentos', 'Ver'),
('Documents.Upload', 'Documentos', 'Cargar'),
('Documents.Download', 'Documentos', 'Descargar'),
('Documents.Delete', 'Documentos', 'Eliminar'),
('Maintenance.View', 'Mantenimiento', 'Ver'),
('Maintenance.Request', 'Mantenimiento', 'Solicitar'),
('Maintenance.Execute', 'Mantenimiento', 'Ejecutar'),
('Maintenance.Close', 'Mantenimiento', 'Cerrar'),
('Purchases.View', 'Compras AF', 'Ver'),
('Purchases.Request', 'Compras AF', 'Solicitar'),
('Purchases.Approve', 'Compras AF', 'Aprobar'),
('Purchases.Reject', 'Compras AF', 'Rechazar'),
('PhysicalCounts.View', 'Tomas físicas', 'Ver'),
('PhysicalCounts.Create', 'Tomas físicas', 'Crear'),
('PhysicalCounts.Close', 'Tomas físicas', 'Cerrar'),
('SafePractices.View', 'Prácticas seguras', 'Ver'),
('SafePractices.Manage', 'Prácticas seguras', 'Administrar'),
('Reports.View', 'Reportes', 'Ver'),
('Reconciliation.View', 'Conciliación', 'Ver'),
('Reconciliation.Manage', 'Conciliación', 'Gestionar'),
('Settings.View', 'Configuración', 'Ver'),
('Settings.Manage', 'Configuración', 'Administrar'),
('Security.Users', 'Seguridad', 'Usuarios'),
('Security.Roles', 'Seguridad', 'Roles');

PRINT '';
PRINT '1. CATALOGO DE PERMISOS ESPERADOS';

SELECT * FROM #ExpectedPermissions ORDER BY ModuleName, Code;

DECLARE @RolesFullName nvarchar(300);
DECLARE @UsersFullName nvarchar(300);
DECLARE @Sql nvarchar(max);

SELECT TOP 1 @RolesFullName = QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.name = 'AppRoles';

SELECT TOP 1 @UsersFullName = QUOTENAME(s.name) + '.' + QUOTENAME(t.name)
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.name = 'AppUsers';

PRINT '';
PRINT '2. ROLES DETECTADOS';

IF @RolesFullName IS NULL
BEGIN
    PRINT 'No se encontró tabla AppRoles.';
END
ELSE
BEGIN
    SET @Sql = N'
    SELECT
        Id,
        Code,
        Name,
        Description,
        IsActive,
        Permissions,
        LEN(ISNULL(Permissions, '''')) - LEN(REPLACE(ISNULL(Permissions, ''''), '';'', '''')) + CASE WHEN NULLIF(Permissions, '''') IS NULL THEN 0 ELSE 1 END AS ApproxPermissionCount
    FROM ' + @RolesFullName + N'
    ORDER BY Code;';
    EXEC sp_executesql @Sql;

    PRINT '';
    PRINT '3. PERMISOS POR ROL';

    SET @Sql = N'
    ;WITH RolePerm AS
    (
        SELECT
            r.Code AS RoleCode,
            r.Name AS RoleName,
            r.IsActive,
            LTRIM(RTRIM(value)) AS PermissionCode
        FROM ' + @RolesFullName + N' r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''''), '';'')
        WHERE LTRIM(RTRIM(value)) <> ''''
    )
    SELECT
        rp.RoleCode,
        rp.RoleName,
        rp.IsActive,
        rp.PermissionCode,
        ep.ModuleName,
        ep.ActionName,
        CASE WHEN ep.Code IS NULL THEN ''NO_EXISTE_EN_CATALOGO'' ELSE ''OK'' END AS ValidationStatus
    FROM RolePerm rp
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.PermissionCode
    ORDER BY rp.RoleCode, rp.PermissionCode;';
    EXEC sp_executesql @Sql;

    PRINT '';
    PRINT '4. PERMISOS EN BASE QUE NO EXISTEN EN EL CATALOGO DEL CODIGO';

    SET @Sql = N'
    ;WITH RolePerm AS
    (
        SELECT
            r.Code AS RoleCode,
            r.Name AS RoleName,
            LTRIM(RTRIM(value)) AS PermissionCode
        FROM ' + @RolesFullName + N' r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''''), '';'')
        WHERE LTRIM(RTRIM(value)) <> ''''
    )
    SELECT rp.*
    FROM RolePerm rp
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.PermissionCode
    WHERE ep.Code IS NULL
    ORDER BY rp.RoleCode, rp.PermissionCode;';
    EXEC sp_executesql @Sql;

    PRINT '';
    PRINT '5. PERMISOS ESPERADOS QUE NO ESTAN EN NINGUN ROL ACTIVO';

    SET @Sql = N'
    ;WITH RolePerm AS
    (
        SELECT DISTINCT
            LTRIM(RTRIM(value)) AS PermissionCode
        FROM ' + @RolesFullName + N' r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''''), '';'')
        WHERE r.IsActive = 1
          AND LTRIM(RTRIM(value)) <> ''''
    )
    SELECT ep.*
    FROM #ExpectedPermissions ep
    LEFT JOIN RolePerm rp ON rp.PermissionCode = ep.Code
    WHERE rp.PermissionCode IS NULL
    ORDER BY ep.ModuleName, ep.Code;';
    EXEC sp_executesql @Sql;
END

PRINT '';
PRINT '6. USUARIOS DETECTADOS';

IF @UsersFullName IS NULL
BEGIN
    PRINT 'No se encontró tabla AppUsers.';
END
ELSE
BEGIN
    SET @Sql = N'
    SELECT
        u.Id,
        u.UserName,
        u.DisplayName,
        CASE WHEN u.Email IS NULL THEN NULL ELSE ''***MASKED***'' END AS Email,
        u.Position,
        u.Area,
        u.AppRoleId,
        u.BranchId,
        u.ResponsiblePersonId,
        u.IsActive,
        u.IsDeleted,
        u.LastLoginAt,
        u.CreatedAt,
        u.UpdatedAt
    FROM ' + @UsersFullName + N' u
    ORDER BY u.IsActive DESC, u.UserName;';
    EXEC sp_executesql @Sql;

    IF @RolesFullName IS NOT NULL
    BEGIN
        PRINT '';
        PRINT '7. USUARIOS CON ROL Y VALIDACION';

        SET @Sql = N'
        SELECT
            u.UserName,
            u.DisplayName,
            u.IsActive AS UserIsActive,
            u.IsDeleted AS UserIsDeleted,
            r.Code AS RoleCode,
            r.Name AS RoleName,
            r.IsActive AS RoleIsActive,
            CASE
                WHEN u.IsActive = 1 AND ISNULL(u.IsDeleted,0) = 0 AND r.Id IS NULL THEN ''USUARIO_SIN_ROL''
                WHEN u.IsActive = 1 AND ISNULL(u.IsDeleted,0) = 0 AND r.IsActive = 0 THEN ''ROL_INACTIVO''
                ELSE ''OK''
            END AS ValidationStatus
        FROM ' + @UsersFullName + N' u
        LEFT JOIN ' + @RolesFullName + N' r ON r.Id = u.AppRoleId
        ORDER BY ValidationStatus DESC, u.UserName;';
        EXEC sp_executesql @Sql;
    END
END

PRINT '';
PRINT '8. COLUMNAS SENSIBLES EN ESQUEMA';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    'Validar hash, cifrado, no exposición en API y no exportar sin máscara.' AS Recommendation
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE
    LOWER(c.name) LIKE '%password%'
    OR LOWER(c.name) LIKE '%token%'
    OR LOWER(c.name) LIKE '%secret%'
    OR LOWER(c.name) LIKE '%hash%'
    OR LOWER(c.name) LIKE '%salt%'
    OR LOWER(c.name) LIKE '%credential%'
    OR LOWER(c.name) LIKE '%authorization%'
ORDER BY s.name, t.name, c.name;

DROP TABLE #ExpectedPermissions;
