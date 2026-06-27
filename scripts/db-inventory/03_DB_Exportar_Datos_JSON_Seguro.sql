SET NOCOUNT ON;

DECLARE @TopRowsPerTable int = 1000;
DECLARE @SchemaName sysname;
DECLARE @TableName sysname;
DECLARE @ObjectId int;
DECLARE @Sql nvarchar(max);
DECLARE @SelectList nvarchar(max);

PRINT '============================================================';
PRINT 'NAVI HERRAMIENTAS - EXPORTAR DATOS JSON SEGURO';
PRINT '============================================================';

DECLARE table_cursor CURSOR LOCAL FAST_FORWARD FOR
SELECT s.name, t.name, t.object_id
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;

OPEN table_cursor;
FETCH NEXT FROM table_cursor INTO @SchemaName, @TableName, @ObjectId;

WHILE @@FETCH_STATUS = 0
BEGIN
    SELECT @SelectList = STRING_AGG(
        CASE
            WHEN LOWER(c.name) LIKE '%password%'
              OR LOWER(c.name) LIKE '%token%'
              OR LOWER(c.name) LIKE '%secret%'
              OR LOWER(c.name) LIKE '%hash%'
              OR LOWER(c.name) LIKE '%salt%'
              OR LOWER(c.name) LIKE '%credential%'
              OR LOWER(c.name) LIKE '%authorization%'
              OR LOWER(c.name) LIKE '%email%'
              OR LOWER(c.name) LIKE '%phone%'
              OR LOWER(c.name) LIKE '%mobile%'
              OR LOWER(c.name) LIKE '%document%'
              OR LOWER(c.name) LIKE '%cedula%'
              OR LOWER(c.name) LIKE '%identification%'
            THEN 'CAST(''***MASKED***'' AS nvarchar(max)) AS ' + QUOTENAME(c.name)
            ELSE QUOTENAME(c.name)
        END,
        ', '
    ) WITHIN GROUP (ORDER BY c.column_id)
    FROM sys.columns c
    WHERE c.object_id = @ObjectId;

    IF @SelectList IS NOT NULL
    BEGIN
        SET @Sql =
            N'PRINT ''============================================================'';' + CHAR(13) +
            N'PRINT ''JSON TABLE: ' + REPLACE(QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName), '''', '''''') + N''';' + CHAR(13) +
            N'SELECT TOP (' + CONVERT(nvarchar(20), @TopRowsPerTable) + N') ' + @SelectList +
            N' FROM ' + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@TableName) +
            N' FOR JSON PATH, INCLUDE_NULL_VALUES;';

        EXEC sp_executesql @Sql;
    END

    FETCH NEXT FROM table_cursor INTO @SchemaName, @TableName, @ObjectId;
END

CLOSE table_cursor;
DEALLOCATE table_cursor;
