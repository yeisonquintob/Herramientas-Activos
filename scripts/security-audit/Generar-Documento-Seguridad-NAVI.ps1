param(
    [string]$ServerInstance = "localhost,1434",
    [string]$Database = "NaviToolsAssetsDb",
    [string]$SqlUser = "sa",
    [switch]$UseWindowsAuth
)

$ErrorActionPreference = "Stop"

$Root = Split-Path -Parent $MyInvocation.MyCommand.Path
$OutputFolder = Join-Path $Root ("outputs\seguridad_{0}" -f (Get-Date -Format "yyyyMMdd_HHmmss"))

New-Item -ItemType Directory -Force -Path $OutputFolder | Out-Null

function Get-PlainPassword {
    param([string]$User)

    $SecurePassword = Read-Host "Ingrese password SQL para usuario $User" -AsSecureString
    $BSTR = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword)

    try {
        return [Runtime.InteropServices.Marshal]::PtrToStringBSTR($BSTR)
    }
    finally {
        [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($BSTR)
    }
}

if ($UseWindowsAuth) {
    $ConnectionString = "Server=$ServerInstance;Database=$Database;Integrated Security=True;TrustServerCertificate=True;"
}
else {
    $PlainPassword = Get-PlainPassword -User $SqlUser
    $ConnectionString = "Server=$ServerInstance;Database=$Database;User ID=$SqlUser;Password=$PlainPassword;TrustServerCertificate=True;"
}

Add-Type -AssemblyName System.Data

function Invoke-NaviQuery {
    param(
        [string]$Name,
        [string]$Sql
    )

    $connection = New-Object System.Data.SqlClient.SqlConnection $ConnectionString
    $command = $connection.CreateCommand()
    $command.CommandText = $Sql
    $command.CommandTimeout = 180

    $adapter = New-Object System.Data.SqlClient.SqlDataAdapter $command
    $table = New-Object System.Data.DataTable

    try {
        [void]$adapter.Fill($table)
        return $table
    }
    catch {
        Write-Warning "Error ejecutando consulta [$Name]: $($_.Exception.Message)"
        $errorTable = New-Object System.Data.DataTable
        [void]$errorTable.Columns.Add("Consulta")
        [void]$errorTable.Columns.Add("Error")
        $row = $errorTable.NewRow()
        $row["Consulta"] = $Name
        $row["Error"] = $_.Exception.Message
        $errorTable.Rows.Add($row)
        return $errorTable
    }
    finally {
        $connection.Close()
        $connection.Dispose()
    }
}

function Export-NaviTable {
    param(
        [string]$Name,
        [System.Data.DataTable]$Table
    )

    $CsvPath = Join-Path $OutputFolder "$Name.csv"
    $Table | Export-Csv -Path $CsvPath -NoTypeInformation -Encoding UTF8
    return $CsvPath
}

function ConvertTo-MarkdownTable {
    param(
        [System.Data.DataTable]$Table,
        [int]$MaxRows = 80
    )

    if ($null -eq $Table -or $Table.Columns.Count -eq 0) {
        return "_Sin información._`r`n"
    }

    if ($Table.Rows.Count -eq 0) {
        return "_Sin registros._`r`n"
    }

    $columns = @()
    foreach ($col in $Table.Columns) {
        $columns += $col.ColumnName
    }

    $markdown = ""
    $markdown += "| " + ($columns -join " | ") + " |`r`n"
    $markdown += "| " + (($columns | ForEach-Object { "---" }) -join " | ") + " |`r`n"

    $rowCount = 0
    foreach ($row in $Table.Rows) {
        if ($rowCount -ge $MaxRows) {
            break
        }

        $values = @()
        foreach ($col in $columns) {
            $value = [string]$row[$col]
            $value = $value -replace "\r", " "
            $value = $value -replace "\n", " "
            $value = $value -replace "\|", "/"
            if ($value.Length -gt 160) {
                $value = $value.Substring(0, 160) + "..."
            }
            $values += $value
        }

        $markdown += "| " + ($values -join " | ") + " |`r`n"
        $rowCount++
    }

    if ($Table.Rows.Count -gt $MaxRows) {
        $markdown += "`r`n_Se muestran $MaxRows de $($Table.Rows.Count) registros. Ver CSV completo._`r`n"
    }

    return $markdown
}

function Add-Section {
    param(
        [System.Collections.Generic.List[string]]$Lines,
        [string]$Title,
        [string]$Description,
        [System.Data.DataTable]$Table,
        [string]$CsvPath
    )

    $Lines.Add("")
    $Lines.Add("## $Title")
    $Lines.Add("")
    if (-not [string]::IsNullOrWhiteSpace($Description)) {
        $Lines.Add($Description)
        $Lines.Add("")
    }

    $Lines.Add("Archivo CSV: `$CsvPath`")
    $Lines.Add("")
    $Lines.Add((ConvertTo-MarkdownTable -Table $Table -MaxRows 80))
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "NAVI Herramientas - Documento de seguridad y permisos" -ForegroundColor Cyan
Write-Host "Servidor: $ServerInstance" -ForegroundColor Cyan
Write-Host "Base: $Database" -ForegroundColor Cyan
Write-Host "Salida: $OutputFolder" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

$ExpectedPermissionsSql = @"
IF OBJECT_ID('tempdb..#ExpectedPermissions') IS NOT NULL DROP TABLE #ExpectedPermissions;

CREATE TABLE #ExpectedPermissions
(
    Code nvarchar(200) NOT NULL PRIMARY KEY,
    ModuleName nvarchar(200) NOT NULL,
    ActionName nvarchar(100) NOT NULL,
    Description nvarchar(500) NOT NULL
);

INSERT INTO #ExpectedPermissions(Code, ModuleName, ActionName, Description)
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
"@

$Queries = [ordered]@{}

$Queries["00_Resumen_Base_Datos"] = @"
SELECT
    DB_NAME() AS DatabaseName,
    @@SERVERNAME AS ServerName,
    SYSDATETIME() AS GeneratedAt,
    SYSTEM_USER AS ExecutedBy,
    SERVERPROPERTY('ProductVersion') AS SqlServerVersion,
    SERVERPROPERTY('Edition') AS SqlServerEdition;
"@

$Queries["01_Tablas_Registros"] = @"
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    SUM(CASE WHEN p.index_id IN (0,1) THEN p.rows ELSE 0 END) AS ApproxRows,
    t.create_date AS CreatedAt,
    t.modify_date AS ModifiedAt
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.partitions p ON p.object_id = t.object_id
WHERE t.is_ms_shipped = 0
GROUP BY s.name, t.name, t.create_date, t.modify_date
ORDER BY s.name, t.name;
"@

$Queries["02_Catalogo_Permisos_Esperados"] = @"
$ExpectedPermissionsSql
SELECT * FROM #ExpectedPermissions ORDER BY ModuleName, Code;
"@

$Queries["03_Roles"] = @"
IF OBJECT_ID('dbo.AppRoles') IS NULL
BEGIN
    SELECT 'No existe dbo.AppRoles' AS Message;
END
ELSE
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
        END AS PermissionCount,
        CreatedAt,
        CreatedBy,
        UpdatedAt,
        UpdatedBy,
        IsDeleted
    FROM dbo.AppRoles
    ORDER BY Code;
END
"@

$Queries["04_Permisos_Por_Rol"] = @"
$ExpectedPermissionsSql

IF OBJECT_ID('dbo.AppRoles') IS NULL
BEGIN
    SELECT 'No existe dbo.AppRoles' AS Message;
END
ELSE
BEGIN
    ;WITH RolePerm AS
    (
        SELECT
            r.Id AS RoleId,
            r.Code AS RoleCode,
            r.Name AS RoleName,
            r.IsActive AS RoleIsActive,
            LTRIM(RTRIM(value)) AS PermissionCode
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        rp.RoleCode,
        rp.RoleName,
        rp.RoleIsActive,
        rp.PermissionCode,
        ep.ModuleName,
        ep.ActionName,
        ep.Description,
        CASE
            WHEN ep.Code IS NULL THEN 'PERMISO_NO_EXISTE_EN_CATALOGO'
            WHEN rp.RoleIsActive = 0 THEN 'ROL_INACTIVO'
            ELSE 'OK'
        END AS ValidationStatus
    FROM RolePerm rp
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.PermissionCode
    ORDER BY rp.RoleCode, ep.ModuleName, rp.PermissionCode;
END
"@

$Queries["05_Matriz_Usuario_Rol_Permiso"] = @"
$ExpectedPermissionsSql

IF OBJECT_ID('dbo.AppUsers') IS NULL OR OBJECT_ID('dbo.AppRoles') IS NULL
BEGIN
    SELECT 'No existen dbo.AppUsers o dbo.AppRoles' AS Message;
END
ELSE
BEGIN
    ;WITH RolePerm AS
    (
        SELECT
            r.Id AS RoleId,
            r.Code AS RoleCode,
            r.Name AS RoleName,
            r.IsActive AS RoleIsActive,
            LTRIM(RTRIM(value)) AS PermissionCode
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        u.UserName,
        u.DisplayName,
        CASE WHEN u.Email IS NULL THEN NULL ELSE '***MASKED***' END AS Email,
        u.Position,
        u.Area,
        u.IsActive AS UserIsActive,
        u.IsDeleted AS UserIsDeleted,
        b.Code AS BranchCode,
        b.Name AS BranchName,
        rp.RoleCode,
        rp.RoleName,
        rp.RoleIsActive,
        rp.PermissionCode,
        ep.ModuleName,
        ep.ActionName,
        ep.Description,
        CASE
            WHEN u.IsDeleted = 1 THEN 'USUARIO_ELIMINADO_LOGICO'
            WHEN u.IsActive = 0 THEN 'USUARIO_INACTIVO'
            WHEN rp.RoleCode IS NULL THEN 'USUARIO_SIN_ROL_O_SIN_PERMISOS'
            WHEN rp.RoleIsActive = 0 THEN 'ROL_INACTIVO'
            WHEN ep.Code IS NULL THEN 'PERMISO_NO_EXISTE_EN_CATALOGO'
            ELSE 'OK'
        END AS ValidationStatus
    FROM dbo.AppUsers u
    LEFT JOIN dbo.AppRoles r ON r.Id = u.AppRoleId
    LEFT JOIN RolePerm rp ON rp.RoleId = r.Id
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.PermissionCode
    LEFT JOIN dbo.Branches b ON b.Id = u.BranchId
    ORDER BY u.UserName, ep.ModuleName, rp.PermissionCode;
END
"@

$Queries["06_Usuarios_Resumen"] = @"
IF OBJECT_ID('dbo.AppUsers') IS NULL
BEGIN
    SELECT 'No existe dbo.AppUsers' AS Message;
END
ELSE
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
        r.Code AS RoleCode,
        r.Name AS RoleName,
        r.IsActive AS RoleIsActive,
        b.Code AS BranchCode,
        b.Name AS BranchName,
        rp.FullName AS ResponsiblePersonName,
        u.LastLoginAt,
        u.CreatedAt,
        u.CreatedBy,
        u.UpdatedAt,
        u.UpdatedBy,
        CASE
            WHEN u.IsDeleted = 1 THEN 'USUARIO_ELIMINADO_LOGICO'
            WHEN u.IsActive = 0 THEN 'USUARIO_INACTIVO'
            WHEN r.Id IS NULL THEN 'USUARIO_SIN_ROL'
            WHEN r.IsActive = 0 THEN 'ROL_INACTIVO'
            ELSE 'OK'
        END AS ValidationStatus
    FROM dbo.AppUsers u
    LEFT JOIN dbo.AppRoles r ON r.Id = u.AppRoleId
    LEFT JOIN dbo.Branches b ON b.Id = u.BranchId
    LEFT JOIN dbo.ResponsiblePeople rp ON rp.Id = u.ResponsiblePersonId
    ORDER BY ValidationStatus DESC, u.UserName;
END
"@

$Queries["07_Sedes"] = @"
IF OBJECT_ID('dbo.Branches') IS NULL
BEGIN
    SELECT 'No existe dbo.Branches' AS Message;
END
ELSE
BEGIN
    SELECT
        z.Code AS ZoneCode,
        z.Name AS ZoneName,
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
"@

$Queries["08_Zonas"] = @"
IF OBJECT_ID('dbo.Zones') IS NULL
BEGIN
    SELECT 'No existe dbo.Zones' AS Message;
END
ELSE
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
"@

$Queries["09_Ubicaciones"] = @"
IF OBJECT_ID('dbo.ToolLocations') IS NULL
BEGIN
    SELECT 'No existe dbo.ToolLocations' AS Message;
END
ELSE
BEGIN
    SELECT
        b.Code AS BranchCode,
        b.Name AS BranchName,
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
"@

$Queries["10_Responsables_Tecnicos"] = @"
IF OBJECT_ID('dbo.ResponsiblePeople') IS NULL
BEGIN
    SELECT 'No existe dbo.ResponsiblePeople' AS Message;
END
ELSE
BEGIN
    SELECT
        rp.Id,
        rp.DocumentNumber,
        rp.FullName,
        CASE WHEN rp.Email IS NULL THEN NULL ELSE '***MASKED***' END AS Email,
        rp.Position,
        rp.Area,
        b.Code AS BranchCode,
        b.Name AS BranchName,
        rp.IsActive,
        rp.IsDeleted,
        rp.CreatedAt,
        rp.UpdatedAt
    FROM dbo.ResponsiblePeople rp
    LEFT JOIN dbo.Branches b ON b.Id = rp.BranchId
    ORDER BY b.Code, rp.FullName;
END
"@

$Queries["11_Permisos_Desconocidos_En_Base"] = @"
$ExpectedPermissionsSql

IF OBJECT_ID('dbo.AppRoles') IS NULL
BEGIN
    SELECT 'No existe dbo.AppRoles' AS Message;
END
ELSE
BEGIN
    ;WITH RolePerm AS
    (
        SELECT
            r.Code AS RoleCode,
            r.Name AS RoleName,
            LTRIM(RTRIM(value)) AS PermissionCode
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        rp.RoleCode,
        rp.RoleName,
        rp.PermissionCode,
        'Este permiso está en base de datos, pero no está en el catálogo esperado del código.' AS Finding
    FROM RolePerm rp
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.PermissionCode
    WHERE ep.Code IS NULL
    ORDER BY rp.RoleCode, rp.PermissionCode;
END
"@

$Queries["12_Permisos_Esperados_No_Asignados"] = @"
$ExpectedPermissionsSql

IF OBJECT_ID('dbo.AppRoles') IS NULL
BEGIN
    SELECT 'No existe dbo.AppRoles' AS Message;
END
ELSE
BEGIN
    ;WITH RolePerm AS
    (
        SELECT DISTINCT
            LTRIM(RTRIM(value)) AS PermissionCode
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE r.IsActive = 1
          AND LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        ep.Code,
        ep.ModuleName,
        ep.ActionName,
        ep.Description,
        'Permiso esperado que no está asignado a ningún rol activo.' AS Finding
    FROM #ExpectedPermissions ep
    LEFT JOIN RolePerm rp ON rp.PermissionCode = ep.Code
    WHERE rp.PermissionCode IS NULL
    ORDER BY ep.ModuleName, ep.Code;
END
"@

$Queries["13_Hallazgos_Usuarios_Roles"] = @"
IF OBJECT_ID('dbo.AppUsers') IS NULL OR OBJECT_ID('dbo.AppRoles') IS NULL
BEGIN
    SELECT 'No existen dbo.AppUsers o dbo.AppRoles' AS Message;
END
ELSE
BEGIN
    SELECT
        u.UserName,
        u.DisplayName,
        u.IsActive AS UserIsActive,
        u.IsDeleted AS UserIsDeleted,
        r.Code AS RoleCode,
        r.Name AS RoleName,
        r.IsActive AS RoleIsActive,
        CASE
            WHEN u.IsDeleted = 1 THEN 'Usuario con eliminación lógica.'
            WHEN u.IsActive = 0 THEN 'Usuario inactivo.'
            WHEN r.Id IS NULL THEN 'Usuario activo sin rol asignado.'
            WHEN r.IsActive = 0 THEN 'Usuario activo con rol inactivo.'
            WHEN NULLIF(r.Permissions, '') IS NULL THEN 'Usuario con rol sin permisos.'
            ELSE 'OK'
        END AS Finding
    FROM dbo.AppUsers u
    LEFT JOIN dbo.AppRoles r ON r.Id = u.AppRoleId
    WHERE
        u.IsDeleted = 1
        OR u.IsActive = 0
        OR r.Id IS NULL
        OR r.IsActive = 0
        OR NULLIF(r.Permissions, '') IS NULL
    ORDER BY Finding, u.UserName;
END
"@

$Queries["14_Columnas_Sensibles"] = @"
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    'Validar hash/cifrado/enmascaramiento y que no se exponga en API ni reportes.' AS Recommendation
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
"@

$Queries["15_Matriz_Rol_Modulo"] = @"
$ExpectedPermissionsSql

IF OBJECT_ID('dbo.AppRoles') IS NULL
BEGIN
    SELECT 'No existe dbo.AppRoles' AS Message;
END
ELSE
BEGIN
    ;WITH RolePerm AS
    (
        SELECT
            r.Code AS RoleCode,
            r.Name AS RoleName,
            r.IsActive AS RoleIsActive,
            LTRIM(RTRIM(value)) AS PermissionCode
        FROM dbo.AppRoles r
        CROSS APPLY STRING_SPLIT(ISNULL(r.Permissions, ''), ';')
        WHERE LTRIM(RTRIM(value)) <> ''
    )
    SELECT
        rp.RoleCode,
        rp.RoleName,
        rp.RoleIsActive,
        ISNULL(ep.ModuleName, 'NO CATALOGADO') AS ModuleName,
        COUNT(*) AS PermissionCount,
        STRING_AGG(rp.PermissionCode, '; ') AS Permissions
    FROM RolePerm rp
    LEFT JOIN #ExpectedPermissions ep ON ep.Code = rp.PermissionCode
    GROUP BY
        rp.RoleCode,
        rp.RoleName,
        rp.RoleIsActive,
        ISNULL(ep.ModuleName, 'NO CATALOGADO')
    ORDER BY rp.RoleCode, ModuleName;
END
"@

$Results = [ordered]@{}
$CsvFiles = [ordered]@{}

foreach ($key in $Queries.Keys) {
    Write-Host "Ejecutando consulta: $key" -ForegroundColor Yellow
    $table = Invoke-NaviQuery -Name $key -Sql $Queries[$key]
    $Results[$key] = $table
    $CsvFiles[$key] = Export-NaviTable -Name $key -Table $table
    Write-Host "OK: $key -> $($CsvFiles[$key])" -ForegroundColor Green
}

$DocLines = New-Object System.Collections.Generic.List[string]

$DocLines.Add("# NAVI Herramientas - Documento de seguridad, usuarios, roles y permisos")
$DocLines.Add("")
$DocLines.Add("Generado: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$DocLines.Add("")
$DocLines.Add("Servidor: `$ServerInstance`")
$DocLines.Add("")
$DocLines.Add("Base de datos: `$Database`")
$DocLines.Add("")
$DocLines.Add("Carpeta de salida: `$OutputFolder`")
$DocLines.Add("")
$DocLines.Add("## Objetivo")
$DocLines.Add("")
$DocLines.Add("Este documento consolida la información actual de seguridad del aplicativo NAVI Herramientas: usuarios, roles, permisos, sedes, zonas, ubicaciones, responsables y hallazgos. Sirve como base para validar si la matriz de acceso cumple con el alcance funcional y para decidir ajustes antes de continuar con la PWA móvil.")
$DocLines.Add("")
$DocLines.Add("## Lectura rápida recomendada")
$DocLines.Add("")
$DocLines.Add("1. Revisar `05_Matriz_Usuario_Rol_Permiso.csv` para saber qué puede hacer cada usuario.")
$DocLines.Add("2. Revisar `15_Matriz_Rol_Modulo.csv` para entender qué módulos tiene cada rol.")
$DocLines.Add("3. Revisar `11_Permisos_Desconocidos_En_Base.csv` para detectar permisos en base que no existen en el código esperado.")
$DocLines.Add("4. Revisar `12_Permisos_Esperados_No_Asignados.csv` para detectar permisos definidos en código que ningún rol tiene.")
$DocLines.Add("5. Revisar `13_Hallazgos_Usuarios_Roles.csv` para corregir usuarios sin rol, inactivos o con rol sin permisos.")
$DocLines.Add("")

Add-Section -Lines $DocLines -Title "Resumen de base de datos" -Description "Información general de la base usada para el análisis." -Table $Results["00_Resumen_Base_Datos"] -CsvPath $CsvFiles["00_Resumen_Base_Datos"]
Add-Section -Lines $DocLines -Title "Tablas y cantidad de registros" -Description "Permite identificar qué información existe actualmente y qué tablas están pobladas." -Table $Results["01_Tablas_Registros"] -CsvPath $CsvFiles["01_Tablas_Registros"]
Add-Section -Lines $DocLines -Title "Catálogo de permisos esperados" -Description "Permisos definidos como matriz base del aplicativo." -Table $Results["02_Catalogo_Permisos_Esperados"] -CsvPath $CsvFiles["02_Catalogo_Permisos_Esperados"]
Add-Section -Lines $DocLines -Title "Roles" -Description "Roles configurados en base de datos y permisos asignados." -Table $Results["03_Roles"] -CsvPath $CsvFiles["03_Roles"]
Add-Section -Lines $DocLines -Title "Permisos por rol" -Description "Detalle de cada permiso asignado a cada rol." -Table $Results["04_Permisos_Por_Rol"] -CsvPath $CsvFiles["04_Permisos_Por_Rol"]
Add-Section -Lines $DocLines -Title "Matriz usuario, rol y permiso" -Description "Vista principal para validar qué puede hacer cada usuario en el sistema." -Table $Results["05_Matriz_Usuario_Rol_Permiso"] -CsvPath $CsvFiles["05_Matriz_Usuario_Rol_Permiso"]
Add-Section -Lines $DocLines -Title "Usuarios resumen" -Description "Usuarios con su rol, sede, responsable asociado y estado." -Table $Results["06_Usuarios_Resumen"] -CsvPath $CsvFiles["06_Usuarios_Resumen"]
Add-Section -Lines $DocLines -Title "Sedes" -Description "Sedes actuales registradas en la base de datos." -Table $Results["07_Sedes"] -CsvPath $CsvFiles["07_Sedes"]
Add-Section -Lines $DocLines -Title "Zonas" -Description "Zonas actuales registradas en la base de datos." -Table $Results["08_Zonas"] -CsvPath $CsvFiles["08_Zonas"]
Add-Section -Lines $DocLines -Title "Ubicaciones" -Description "Ubicaciones, almacenes o talleres disponibles por sede." -Table $Results["09_Ubicaciones"] -CsvPath $CsvFiles["09_Ubicaciones"]
Add-Section -Lines $DocLines -Title "Responsables y técnicos" -Description "Responsables o técnicos que pueden quedar asociados a activos, préstamos, asignaciones o procesos." -Table $Results["10_Responsables_Tecnicos"] -CsvPath $CsvFiles["10_Responsables_Tecnicos"]
Add-Section -Lines $DocLines -Title "Permisos desconocidos en base" -Description "Permisos asignados en base de datos que no aparecen en el catálogo esperado del código. Deben revisarse." -Table $Results["11_Permisos_Desconocidos_En_Base"] -CsvPath $CsvFiles["11_Permisos_Desconocidos_En_Base"]
Add-Section -Lines $DocLines -Title "Permisos esperados no asignados" -Description "Permisos definidos en el catálogo esperado, pero no asignados a ningún rol activo." -Table $Results["12_Permisos_Esperados_No_Asignados"] -CsvPath $CsvFiles["12_Permisos_Esperados_No_Asignados"]
Add-Section -Lines $DocLines -Title "Hallazgos de usuarios y roles" -Description "Usuarios sin rol, usuarios inactivos, roles inactivos o roles sin permisos." -Table $Results["13_Hallazgos_Usuarios_Roles"] -CsvPath $CsvFiles["13_Hallazgos_Usuarios_Roles"]
Add-Section -Lines $DocLines -Title "Columnas sensibles" -Description "Columnas que deben revisarse para cifrado, hash, enmascaramiento o no exposición." -Table $Results["14_Columnas_Sensibles"] -CsvPath $CsvFiles["14_Columnas_Sensibles"]
Add-Section -Lines $DocLines -Title "Matriz rol por módulo" -Description "Resumen para entender qué módulos tiene habilitado cada rol." -Table $Results["15_Matriz_Rol_Modulo"] -CsvPath $CsvFiles["15_Matriz_Rol_Modulo"]

$DocLines.Add("")
$DocLines.Add("## Conclusión inicial")
$DocLines.Add("")
$DocLines.Add("Con este documento se debe validar si cada rol tiene únicamente los permisos necesarios. La regla recomendada es mínimo privilegio: ningún usuario debe tener permisos de creación, edición, aprobación, eliminación, asignación o seguridad si su operación real no lo requiere.")
$DocLines.Add("")
$DocLines.Add("Para la PWA móvil, el menú debe construirse con base en esta misma matriz: si el usuario no tiene permiso de ver un módulo, no debe ver el menú; si no tiene permiso de ejecutar una acción, debe poder consultar solamente o no acceder.")
$DocLines.Add("")

$MarkdownPath = Join-Path $OutputFolder "NAVI_Documento_Seguridad_Usuarios_Roles_Permisos.md"
$HtmlPath = Join-Path $OutputFolder "NAVI_Documento_Seguridad_Usuarios_Roles_Permisos.html"

Set-Content -Path $MarkdownPath -Value $DocLines -Encoding UTF8

$HtmlBody = @()
$HtmlBody += "<html><head><meta charset='utf-8'>"
$HtmlBody += "<title>NAVI Seguridad Usuarios Roles Permisos</title>"
$HtmlBody += "<style>"
$HtmlBody += "body{font-family:Segoe UI,Arial,sans-serif;margin:32px;color:#1f2937;background:#f8fafc;}"
$HtmlBody += "h1{color:#1F4E79;} h2{color:#1F4E79;border-bottom:2px solid #1F4E79;padding-bottom:6px;margin-top:34px;}"
$HtmlBody += "table{border-collapse:collapse;width:100%;margin:14px 0;background:white;font-size:12px;}"
$HtmlBody += "th{background:#1F4E79;color:white;text-align:left;padding:8px;}"
$HtmlBody += "td{border:1px solid #d1d5db;padding:6px;vertical-align:top;}"
$HtmlBody += "tr:nth-child(even){background:#f3f4f6;}"
$HtmlBody += ".note{background:#fff7ed;border-left:5px solid #f97316;padding:12px;margin:16px 0;}"
$HtmlBody += "</style></head><body>"
$HtmlBody += "<h1>NAVI Herramientas - Documento de seguridad, usuarios, roles y permisos</h1>"
$HtmlBody += "<div class='note'><b>Generado:</b> $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')<br><b>Servidor:</b> $ServerInstance<br><b>Base:</b> $Database<br><b>Salida:</b> $OutputFolder</div>"
$HtmlBody += "<h2>Objetivo</h2>"
$HtmlBody += "<p>Este documento consolida usuarios, roles, permisos, sedes, zonas, ubicaciones, responsables y hallazgos para validar la matriz de seguridad antes de continuar con la PWA móvil.</p>"

foreach ($key in $Results.Keys) {
    $HtmlBody += "<h2>$key</h2>"
    $HtmlBody += "<p><b>CSV:</b> $($CsvFiles[$key])</p>"
    $HtmlBody += ($Results[$key] | Select-Object -First 200 | ConvertTo-Html -Fragment)
}

$HtmlBody += "</body></html>"

Set-Content -Path $HtmlPath -Value $HtmlBody -Encoding UTF8

Write-Host ""
Write-Host "============================================================" -ForegroundColor Green
Write-Host "DOCUMENTO GENERADO CORRECTAMENTE" -ForegroundColor Green
Write-Host "Markdown:" -ForegroundColor Green
Write-Host $MarkdownPath -ForegroundColor Cyan
Write-Host "HTML:" -ForegroundColor Green
Write-Host $HtmlPath -ForegroundColor Cyan
Write-Host "CSV completos:" -ForegroundColor Green
Write-Host $OutputFolder -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Green

Invoke-Item $OutputFolder
