DECLARE @schemaName SYSNAME;
DECLARE @tableName SYSNAME;

SELECT TOP 1
    @schemaName = TABLE_SCHEMA,
    @tableName = TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
  AND TABLE_NAME IN ('ToolAssets', 'ToolAsset', 'Tools', 'Assets')
ORDER BY
    CASE TABLE_NAME
        WHEN 'ToolAssets' THEN 1
        WHEN 'ToolAsset' THEN 2
        WHEN 'Tools' THEN 3
        WHEN 'Assets' THEN 4
        ELSE 99
    END;

IF @schemaName IS NULL OR @tableName IS NULL
BEGIN
    PRINT 'No se encontró tabla ToolAssets/ToolAsset/Tools/Assets en esta base.';
    PRINT 'Tablas disponibles:';

    SELECT TABLE_SCHEMA, TABLE_NAME
    FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_TYPE = 'BASE TABLE'
    ORDER BY TABLE_SCHEMA, TABLE_NAME;

    THROW 51000, 'No se pudo agregar ShelfLocation porque no existe la tabla de herramientas en esta base. Revisa que estés conectado a la BD correcta o restaura/aplica migraciones primero.', 1;
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.columns c
    INNER JOIN sys.objects o ON c.object_id = o.object_id
    INNER JOIN sys.schemas sc ON o.schema_id = sc.schema_id
    WHERE sc.name = @schemaName
      AND o.name = @tableName
      AND c.name = 'ShelfLocation'
)
BEGIN
    DECLARE @sql NVARCHAR(MAX) =
        N'ALTER TABLE ' + QUOTENAME(@schemaName) + N'.' + QUOTENAME(@tableName) +
        N' ADD ShelfLocation NVARCHAR(120) NULL;';

    EXEC sp_executesql @sql;

    PRINT 'Columna ShelfLocation agregada correctamente.';
END
ELSE
BEGIN
    PRINT 'La columna ShelfLocation ya existe.';
END;

SELECT
    TABLE_SCHEMA,
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE COLUMN_NAME = 'ShelfLocation'
ORDER BY TABLE_SCHEMA, TABLE_NAME;
