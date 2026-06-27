SET NOCOUNT ON;

DECLARE @CRLF nchar(2) = CHAR(13) + CHAR(10);
DECLARE @DatabaseName sysname = DB_NAME();

PRINT '============================================================';
PRINT 'NAVI HERRAMIENTAS - GENERAR SCRIPT DDL';
PRINT '============================================================';

SELECT
    '-- DATABASE' + @CRLF +
    'IF DB_ID(N''' + REPLACE(@DatabaseName, '''', '''''') + ''') IS NULL' + @CRLF +
    'BEGIN' + @CRLF +
    '    CREATE DATABASE ' + QUOTENAME(@DatabaseName) + ';' + @CRLF +
    'END' + @CRLF +
    'GO' + @CRLF +
    'USE ' + QUOTENAME(@DatabaseName) + ';' + @CRLF +
    'GO' AS GeneratedDDL;

PRINT 'SCHEMAS';

SELECT
    'IF SCHEMA_ID(N''' + REPLACE(s.name, '''', '''''') + ''') IS NULL EXEC(N''CREATE SCHEMA ' + QUOTENAME(s.name) + ''');' + @CRLF + 'GO' AS GeneratedDDL
FROM sys.schemas s
WHERE s.schema_id < 16384
  AND s.name NOT IN ('dbo', 'guest', 'sys', 'INFORMATION_SCHEMA')
ORDER BY s.name;

PRINT 'TABLES';

SELECT
    '-- TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + @CRLF +
    'CREATE TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + @CRLF +
    '(' + @CRLF +
    ca.ColumnDefinitions + @CRLF +
    ');' + @CRLF +
    'GO' AS GeneratedDDL
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
CROSS APPLY
(
    SELECT STRING_AGG(
        '    ' + QUOTENAME(c.name) + ' ' +
        CASE
            WHEN ty.name IN ('varchar','char','varbinary','binary') THEN ty.name + '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CONVERT(varchar(10), c.max_length) END + ')'
            WHEN ty.name IN ('nvarchar','nchar') THEN ty.name + '(' + CASE WHEN c.max_length = -1 THEN 'MAX' ELSE CONVERT(varchar(10), c.max_length / 2) END + ')'
            WHEN ty.name IN ('decimal','numeric') THEN ty.name + '(' + CONVERT(varchar(10), c.precision) + ',' + CONVERT(varchar(10), c.scale) + ')'
            WHEN ty.name IN ('datetime2','datetimeoffset','time') THEN ty.name + '(' + CONVERT(varchar(10), c.scale) + ')'
            ELSE ty.name
        END +
        CASE WHEN c.is_identity = 1 THEN ' IDENTITY(1,1)' ELSE '' END +
        CASE WHEN c.is_nullable = 1 THEN ' NULL' ELSE ' NOT NULL' END +
        ISNULL(' DEFAULT ' + dc.definition, ''),
        ',' + @CRLF
    ) WITHIN GROUP (ORDER BY c.column_id) AS ColumnDefinitions
    FROM sys.columns c
    INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
    LEFT JOIN sys.default_constraints dc ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE c.object_id = t.object_id
) ca
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;

PRINT 'PRIMARY KEYS';

SELECT
    'ALTER TABLE ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) +
    ' ADD CONSTRAINT ' + QUOTENAME(kc.name) +
    ' PRIMARY KEY (' +
    STRING_AGG(QUOTENAME(c.name), ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) +
    ');' + @CRLF + 'GO' AS GeneratedDDL
FROM sys.key_constraints kc
INNER JOIN sys.tables t ON t.object_id = kc.parent_object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.index_columns ic ON ic.object_id = t.object_id AND ic.index_id = kc.unique_index_id
INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id
WHERE kc.type = 'PK'
GROUP BY s.name, t.name, kc.name
ORDER BY s.name, t.name;

PRINT 'FOREIGN KEYS';

SELECT
    'ALTER TABLE ' + QUOTENAME(sChild.name) + '.' + QUOTENAME(tChild.name) +
    ' WITH CHECK ADD CONSTRAINT ' + QUOTENAME(fk.name) +
    ' FOREIGN KEY (' +
    STRING_AGG(QUOTENAME(cChild.name), ', ') WITHIN GROUP (ORDER BY fkc.constraint_column_id) +
    ') REFERENCES ' + QUOTENAME(sParent.name) + '.' + QUOTENAME(tParent.name) +
    ' (' +
    STRING_AGG(QUOTENAME(cParent.name), ', ') WITHIN GROUP (ORDER BY fkc.constraint_column_id) +
    ');' + @CRLF +
    'GO' AS GeneratedDDL
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
INNER JOIN sys.tables tChild ON tChild.object_id = fk.parent_object_id
INNER JOIN sys.schemas sChild ON sChild.schema_id = tChild.schema_id
INNER JOIN sys.columns cChild ON cChild.object_id = tChild.object_id AND cChild.column_id = fkc.parent_column_id
INNER JOIN sys.tables tParent ON tParent.object_id = fk.referenced_object_id
INNER JOIN sys.schemas sParent ON sParent.schema_id = tParent.schema_id
INNER JOIN sys.columns cParent ON cParent.object_id = tParent.object_id AND cParent.column_id = fkc.referenced_column_id
GROUP BY fk.name, sChild.name, tChild.name, sParent.name, tParent.name
ORDER BY sChild.name, tChild.name, fk.name;
