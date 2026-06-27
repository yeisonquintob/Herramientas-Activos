SET NOCOUNT ON;

PRINT '============================================================';
PRINT 'NAVI HERRAMIENTAS - DOCUMENTO COMPLETO DE SEGURIDAD';
PRINT '============================================================';
PRINT 'Base de datos: ' + DB_NAME();
PRINT 'Servidor: ' + @@SERVERNAME;
PRINT 'Fecha: ' + CONVERT(varchar(30), SYSDATETIME(), 120);
PRINT '============================================================';

PRINT '';
PRINT '1. TODAS LAS TABLAS DE LA BASE DE DATOS';
PRINT '------------------------------------------------------------';

SELECT
    s.name AS Esquema,
    t.name AS Tabla,
    SUM(CASE WHEN p.index_id IN (0,1) THEN p.rows ELSE 0 END) AS RegistrosAprox,
    t.create_date AS FechaCreacion,
    t.modify_date AS FechaModificacion
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.partitions p ON p.object_id = t.object_id
WHERE t.is_ms_shipped = 0
GROUP BY s.name, t.name, t.create_date, t.modify_date
ORDER BY s.name, t.name;

PRINT '';
PRINT '2. COLUMNAS DE TODAS LAS TABLAS';
PRINT '------------------------------------------------------------';

SELECT
    s.name AS Esquema,
    t.name AS Tabla,
    c.column_id AS Orden,
    c.name AS Columna,
    ty.name AS TipoDato,
    CASE
        WHEN ty.name IN ('nvarchar','nchar') AND c.max_length > 0 THEN c.max_length / 2
        ELSE c.max_length
    END AS Longitud,
    c.is_nullable AS PermiteNull,
    c.is_identity AS EsIdentity,
    dc.definition AS ValorDefecto
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints dc
    ON dc.parent_object_id = t.object_id
   AND dc.parent_column_id = c.column_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name, c.column_id;

PRINT '';
PRINT '3. RELACIONES ENTRE TABLAS / FOREIGN KEYS';
PRINT '------------------------------------------------------------';

SELECT
    fk.name AS NombreRelacion,
    schChild.name AS EsquemaHijo,
    tblChild.name AS TablaHija,
    STRING_AGG(colChild.name, ', ') WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS ColumnasHijas,
    schParent.name AS EsquemaPadre,
    tblParent.name AS TablaPadre,
    STRING_AGG(colParent.name, ', ') WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS ColumnasPadre,
    fk.delete_referential_action_desc AS AccionDelete,
    fk.update_referential_action_desc AS AccionUpdate,
    fk.is_disabled AS EstaDeshabilitada,
    fk.is_not_trusted AS NoConfiable
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.tables tblChild ON tblChild.object_id = fk.parent_object_id
INNER JOIN sys.schemas schChild ON schChild.schema_id = tblChild.schema_id
INNER JOIN sys.columns colChild ON colChild.object_id = tblChild.object_id AND colChild.column_id = fkc.parent_column_id
INNER JOIN sys.tables tblParent ON tblParent.object_id = fk.referenced_object_id
INNER JOIN sys.schemas schParent ON schParent.schema_id = tblParent.schema_id
INNER JOIN sys.columns colParent ON colParent.object_id = tblParent.object_id AND colParent.column_id = fkc.referenced_column_id
GROUP BY
    fk.name,
    schChild.name,
    tblChild.name,
    schParent.name,
    tblParent.name,
    fk.delete_referential_action_desc,
    fk.update_referential_action_desc,
    fk.is_disabled,
    fk.is_not_trusted
ORDER BY schChild.name, tblChild.name, fk.name;

PRINT '';
PRINT '4. ROLES CONFIGURADOS';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.AppRoles') IS NOT NULL
BEGIN
    SELECT
        Id,
        Code,
        Name,
        Description,
        IsActive,
        Permissions,
        CASE
            WHEN NULLIF(Permissions, '') IS NULL THEN 0
            ELSE LEN(Permissions) - LEN(REPLACE(Permissions, ';', '')) + 1
        END AS CantidadPermisos,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        IsDeleted
    FROM dbo.AppRoles
    ORDER BY Code;
END
ELSE
BEGIN
    SELECT 'No existe la tabla dbo.AppRoles' AS Mensaje;
END;

PRINT '';
PRINT '5. USUARIOS CONFIGURADOS';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.AppUsers') IS NOT NULL
BEGIN
    SELECT
        u.Id,
        u.UserName,
        u.DisplayName,
        CASE WHEN u.Email IS NULL THEN NULL ELSE '***MASKED***' END AS Email,
        u.Position,
        u.Area,
        u.IsActive,
        u.IsDeleted,
        u.AppRoleId,
        u.BranchId,
        u.ResponsiblePersonId,
        u.LastLoginAt,
        u.CreatedAt,
        u.CreatedBy,
        u.UpdatedAt,
        u.UpdatedBy
    FROM dbo.AppUsers u
    ORDER BY u.UserName;
END
ELSE
BEGIN
    SELECT 'No existe la tabla dbo.AppUsers' AS Mensaje;
END;

PRINT '';
PRINT '6. USUARIOS CON ROL, SEDE Y RESPONSABLE';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.AppUsers') IS NOT NULL AND OBJECT_ID('dbo.AppRoles') IS NOT NULL
BEGIN
    SELECT
        u.UserName,
        u.DisplayName,
        CASE WHEN u.Email IS NULL THEN NULL ELSE '***MASKED***' END AS Email,
        u.Position,
        u.Area,
        u.IsActive AS UsuarioActivo,
        u.IsDeleted AS UsuarioEliminado,
        r.Code AS CodigoRol,
        r.Name AS NombreRol,
        r.IsActive AS RolActivo,
        b.Code AS CodigoSede,
        b.Name AS NombreSede,
        rp.FullName AS ResponsableTecnico,
        CASE
            WHEN u.IsDeleted = 1 THEN 'USUARIO_ELIMINADO_LOGICO'
            WHEN u.IsActive = 0 THEN 'USUARIO_INACTIVO'
            WHEN r.Id IS NULL THEN 'USUARIO_SIN_ROL'
            WHEN r.IsActive = 0 THEN 'ROL_INACTIVO'
            WHEN NULLIF(r.Permissions, '') IS NULL THEN 'ROL_SIN_PERMISOS'
            ELSE 'OK'
        END AS Validacion
    FROM dbo.AppUsers u
    LEFT JOIN dbo.AppRoles r ON r.Id = u.AppRoleId
    LEFT JOIN dbo.Branches b ON b.Id = u.BranchId
    LEFT JOIN dbo.ResponsiblePeople rp ON rp.Id = u.ResponsiblePersonId
    ORDER BY Validacion DESC, u.UserName;
END
ELSE
BEGIN
    SELECT 'No existen AppUsers o AppRoles' AS Mensaje;
END;

PRINT '';
PRINT '7. CATALOGO ESPERADO DE PERMISOS DEL APLICATIVO';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('tempdb..#ExpectedPermissions') IS NOT NULL DROP TABLE #ExpectedPermissions;

CREATE TABLE #ExpectedPermissions
(
    Code nvarchar(200) NOT NULL PRIMARY KEY,
    Modulo nvarchar(200) NOT NULL,
    Accion nvarchar(100) NOT NULL,
    Descripcion nvarchar(500) NOT NULL
);

INSERT INTO #ExpectedPermissions(Code, Modulo, Accion, Descripcion)
VALUES
('Dashboard.View', 'Dashboard', 'Ver', 'Ver dashboard ejecutivo.'),
('Tools.View', 'Inventario AF', 'Ver', 'Ver inventario de herramientas y activos.'),
('Tools.Create', 'Inventario AF', 'Crear', 'Crear herramientas o activos.'),
('Tools.Edit', 'Inventario AF', 'Editar', 'Editar herramientas o activos.'),
('Tools.Delete', 'Inventario AF', 'Eliminar', 'Eliminar herramientas o activos.'),
('AssetAvailability.View', 'Disponible y ubicación', 'Ver', 'Ver disponibilidad y ubicación.'),
('AssetAvailability.Edit', 'Disponible y ubicación', 'Editar', 'Cambiar disponibilidad, sede o ubicación.'),
('AssetAssignment.View', 'Asignar AF', 'Ver', 'Ver módulo de asignaciones.'),
('AssetAssignment.Assign', 'Asignar AF', 'Asignar', 'Asignar activos a responsables.'),
('AssetAssignment.Return', 'Asignar AF', 'Regresar', 'Regresar activos a almacén/taller.'),
('AssetAssignment.History', 'Asignar AF', 'Historial', 'Ver historial de asignaciones.'),
('TechnicalLifeRecord.View', 'Hoja de vida', 'Ver', 'Ver hoja de vida técnica.'),
('TechnicalLifeRecord.Edit', 'Hoja de vida', 'Editar', 'Editar datos técnicos de hoja de vida.'),
('TechnicalLifeRecord.Export', 'Hoja de vida', 'Exportar', 'Exportar hoja de vida a PDF o Excel.'),
('Documents.View', 'Documentos', 'Ver', 'Ver documentos y evidencias.'),
('Documents.Upload', 'Documentos', 'Cargar', 'Cargar documentos y evidencias.'),
('Documents.Download', 'Documentos', 'Descargar', 'Descargar documentos.'),
('Documents.Delete', 'Documentos', 'Eliminar', 'Eliminar documentos.'),
('Maintenance.View', 'Mantenimiento', 'Ver', 'Ver mantenimientos.'),
('Maintenance.Request', 'Mantenimiento', 'Solicitar', 'Solicitar mantenimiento.'),
('Maintenance.Execute', 'Mantenimiento', 'Ejecutar', 'Registrar ejecución de mantenimiento.'),
('Maintenance.Close', 'Mantenimiento', 'Cerrar', 'Cerrar mantenimiento.'),
('Purchases.View', 'Compras AF', 'Ver', 'Ver solicitudes de compra.'),
('Purchases.Request', 'Compras AF', 'Solicitar', 'Solicitar compra de activo fijo.'),
('Purchases.Approve', 'Compras AF', 'Aprobar', 'Aprobar compra de activo fijo.'),
('Purchases.Reject', 'Compras AF', 'Rechazar', 'Rechazar compra de activo fijo.'),
('PhysicalCounts.View', 'Tomas físicas', 'Ver', 'Ver tomas físicas.'),
('PhysicalCounts.Create', 'Tomas físicas', 'Crear', 'Crear toma física.'),
('PhysicalCounts.Close', 'Tomas físicas', 'Cerrar', 'Cerrar toma física.'),
('SafePractices.View', 'Prácticas seguras', 'Ver', 'Ver prácticas seguras.'),
('SafePractices.Manage', 'Prácticas seguras', 'Administrar', 'Administrar prácticas seguras.'),
('Reports.View', 'Reportes', 'Ver', 'Ver reportes.'),
('Reconciliation.View', 'Conciliación', 'Ver', 'Ver conciliación administrativa.'),
('Reconciliation.Manage', 'Conciliación', 'Gestionar', 'Aclarar, aprobar creación, conciliar o rechazar.'),
('Settings.View', 'Configuración', 'Ver', 'Ver configuración.'),
('Settings.Manage', 'Configuración', 'Administrar', 'Administrar configuración base.'),
('Security.Users', 'Seguridad', 'Usuarios', 'Administrar usuarios.'),
('Security.Roles', 'Seguridad', 'Roles', 'Administrar roles y permisos.');

SELECT * FROM #ExpectedPermissions ORDER BY Modulo, Code;

PRINT '';
PRINT '8. PERMISOS POR ROL';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.AppRoles') IS NOT NULL
BEGIN
    ;WITH RolePerm AS
    (
        SELECT
            r.Id AS RoleId,
            r.Code AS CodigoRol,
            r.Name AS NombreRol,
            r.IsActive AS RolActivo,
            LTRIM(RTRIM(value)) AS Permiso
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        rp.CodigoRol,
        rp.NombreRol,
        rp.RolActivo,
        rp.Permiso,
        ep.Modulo,
        ep.Accion,
        ep.Descripcion,
        CASE
            WHEN ep.Code IS NULL THEN 'PERMISO_NO_EXISTE_EN_CATALOGO'
            WHEN rp.RolActivo = 0 THEN 'ROL_INACTIVO'
            ELSE 'OK'
        END AS Validacion
    FROM RolePerm rp
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.Permiso
    ORDER BY rp.CodigoRol, ep.Modulo, rp.Permiso;
END
ELSE
BEGIN
    SELECT 'No existe AppRoles' AS Mensaje;
END;

PRINT '';
PRINT '9. MATRIZ USUARIO - ROL - PERMISO';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.AppUsers') IS NOT NULL AND OBJECT_ID('dbo.AppRoles') IS NOT NULL
BEGIN
    ;WITH RolePerm AS
    (
        SELECT
            r.Id AS RoleId,
            r.Code AS CodigoRol,
            r.Name AS NombreRol,
            r.IsActive AS RolActivo,
            LTRIM(RTRIM(value)) AS Permiso
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        u.UserName,
        u.DisplayName,
        u.Position,
        u.Area,
        u.IsActive AS UsuarioActivo,
        u.IsDeleted AS UsuarioEliminado,
        b.Code AS CodigoSede,
        b.Name AS NombreSede,
        rp.CodigoRol,
        rp.NombreRol,
        rp.RolActivo,
        rp.Permiso,
        ep.Modulo,
        ep.Accion,
        CASE
            WHEN u.IsDeleted = 1 THEN 'USUARIO_ELIMINADO_LOGICO'
            WHEN u.IsActive = 0 THEN 'USUARIO_INACTIVO'
            WHEN rp.CodigoRol IS NULL THEN 'USUARIO_SIN_ROL_O_SIN_PERMISOS'
            WHEN rp.RolActivo = 0 THEN 'ROL_INACTIVO'
            WHEN ep.Code IS NULL THEN 'PERMISO_NO_EXISTE_EN_CATALOGO'
            ELSE 'OK'
        END AS Validacion
    FROM dbo.AppUsers u
    LEFT JOIN dbo.AppRoles r ON r.Id = u.AppRoleId
    LEFT JOIN RolePerm rp ON rp.RoleId = r.Id
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.Permiso
    LEFT JOIN dbo.Branches b ON b.Id = u.BranchId
    ORDER BY u.UserName, ep.Modulo, rp.Permiso;
END
ELSE
BEGIN
    SELECT 'No existen AppUsers o AppRoles' AS Mensaje;
END;

PRINT '';
PRINT '10. MATRIZ ROL - MODULO';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.AppRoles') IS NOT NULL
BEGIN
    ;WITH RolePerm AS
    (
        SELECT
            r.Code AS CodigoRol,
            r.Name AS NombreRol,
            r.IsActive AS RolActivo,
            LTRIM(RTRIM(value)) AS Permiso
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        rp.CodigoRol,
        rp.NombreRol,
        rp.RolActivo,
        ISNULL(ep.Modulo, 'NO CATALOGADO') AS Modulo,
        COUNT(*) AS CantidadPermisos,
        STRING_AGG(rp.Permiso, '; ') AS Permisos
    FROM RolePerm rp
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.Permiso
    GROUP BY
        rp.CodigoRol,
        rp.NombreRol,
        rp.RolActivo,
        ISNULL(ep.Modulo, 'NO CATALOGADO')
    ORDER BY rp.CodigoRol, Modulo;
END
ELSE
BEGIN
    SELECT 'No existe AppRoles' AS Mensaje;
END;

PRINT '';
PRINT '11. PERMISOS EN BASE QUE NO EXISTEN EN CATALOGO';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.AppRoles') IS NOT NULL
BEGIN
    ;WITH RolePerm AS
    (
        SELECT
            r.Code AS CodigoRol,
            r.Name AS NombreRol,
            LTRIM(RTRIM(value)) AS Permiso
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        rp.CodigoRol,
        rp.NombreRol,
        rp.Permiso,
        'Este permiso existe en base, pero no está en el catálogo esperado.' AS Hallazgo
    FROM RolePerm rp
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.Permiso
    WHERE ep.Code IS NULL
    ORDER BY rp.CodigoRol, rp.Permiso;
END
ELSE
BEGIN
    SELECT 'No existe AppRoles' AS Mensaje;
END;

PRINT '';
PRINT '12. PERMISOS ESPERADOS NO ASIGNADOS A NINGUN ROL ACTIVO';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.AppRoles') IS NOT NULL
BEGIN
    ;WITH RolePerm AS
    (
        SELECT DISTINCT
            LTRIM(RTRIM(value)) AS Permiso
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE r.IsActive = 1
          AND LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        ep.Code,
        ep.Modulo,
        ep.Accion,
        ep.Descripcion,
        'Permiso esperado no asignado a ningún rol activo.' AS Hallazgo
    FROM #ExpectedPermissions ep
    LEFT JOIN RolePerm rp ON rp.Permiso = ep.Code
    WHERE rp.Permiso IS NULL
    ORDER BY ep.Modulo, ep.Code;
END
ELSE
BEGIN
    SELECT 'No existe AppRoles' AS Mensaje;
END;

PRINT '';
PRINT '13. ZONAS';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.Zones') IS NOT NULL
BEGIN
    SELECT
        Id,
        Code,
        Name,
        IsActive,
        IsDeleted,
        CreatedAt,
        UpdatedAt
    FROM dbo.Zones
    ORDER BY Code;
END
ELSE
BEGIN
    SELECT 'No existe dbo.Zones' AS Mensaje;
END;

PRINT '';
PRINT '14. SEDES';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.Branches') IS NOT NULL
BEGIN
    SELECT
        z.Code AS CodigoZona,
        z.Name AS NombreZona,
        b.Id,
        b.Code,
        b.Name,
        b.City,
        b.IsActive,
        b.IsDeleted,
        b.CreatedAt,
        b.UpdatedAt
    FROM dbo.Branches b
    LEFT JOIN dbo.Zones z ON z.Id = b.ZoneId
    ORDER BY z.Code, b.Code;
END
ELSE
BEGIN
    SELECT 'No existe dbo.Branches' AS Mensaje;
END;

PRINT '';
PRINT '15. UBICACIONES / ALMACENES / TALLERES';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.ToolLocations') IS NOT NULL
BEGIN
    SELECT
        b.Code AS CodigoSede,
        b.Name AS NombreSede,
        l.Id,
        l.Code,
        l.Name,
        l.LocationType,
        l.IsActive,
        l.IsDeleted,
        l.CreatedAt,
        l.UpdatedAt
    FROM dbo.ToolLocations l
    LEFT JOIN dbo.Branches b ON b.Id = l.BranchId
    ORDER BY b.Code, l.Code;
END
ELSE
BEGIN
    SELECT 'No existe dbo.ToolLocations' AS Mensaje;
END;

PRINT '';
PRINT '16. RESPONSABLES / TECNICOS';
PRINT '------------------------------------------------------------';

IF OBJECT_ID('dbo.ResponsiblePeople') IS NOT NULL
BEGIN
    SELECT
        rp.Id,
        rp.DocumentNumber,
        rp.FullName,
        CASE WHEN rp.Email IS NULL THEN NULL ELSE '***MASKED***' END AS Email,
        rp.Position,
        rp.Area,
        b.Code AS CodigoSede,
        b.Name AS NombreSede,
        rp.IsActive,
        rp.IsDeleted,
        rp.CreatedAt,
        rp.UpdatedAt
    FROM dbo.ResponsiblePeople rp
    LEFT JOIN dbo.Branches b ON b.Id = rp.BranchId
    ORDER BY b.Code, rp.FullName;
END
ELSE
BEGIN
    SELECT 'No existe dbo.ResponsiblePeople' AS Mensaje;
END;

PRINT '';
PRINT '17. COLUMNAS SENSIBLES';
PRINT '------------------------------------------------------------';

SELECT
    s.name AS Esquema,
    t.name AS Tabla,
    c.name AS Columna,
    ty.name AS TipoDato,
    'Validar hash, cifrado, mascara y no exposicion en API.' AS Recomendacion
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
    OR LOWER(c.name) LIKE '%apikey%'
    OR LOWER(c.name) LIKE '%api_key%'
ORDER BY s.name, t.name, c.name;

DROP TABLE #ExpectedPermissions;

PRINT '';
PRINT '============================================================';
PRINT 'FIN DEL DOCUMENTO DE SEGURIDAD';
PRINT '============================================================';
