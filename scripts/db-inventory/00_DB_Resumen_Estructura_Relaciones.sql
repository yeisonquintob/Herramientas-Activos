SET NOCOUNT ON;

PRINT '============================================================';
PRINT 'NAVI HERRAMIENTAS - ESTRUCTURA, RELACIONES Y CONDICIONES';
PRINT '============================================================';
PRINT 'Base de datos: ' + DB_NAME();
PRINT 'Servidor: ' + @@SERVERNAME;
PRINT 'Fecha: ' + CONVERT(varchar(30), SYSDATETIME(), 120);
PRINT '============================================================';

PRINT '';
PRINT '1. TABLAS Y REGISTROS APROXIMADOS';

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

PRINT '';
PRINT '2. COLUMNAS POR TABLA';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    c.column_id AS ColumnOrder,
    c.name AS ColumnName,
    ty.name AS DataType,
    CASE
        WHEN ty.name IN ('nvarchar','nchar') AND c.max_length > 0 THEN c.max_length / 2
        ELSE c.max_length
    END AS MaxLength,
    c.precision AS PrecisionValue,
    c.scale AS ScaleValue,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity,
    dc.definition AS DefaultDefinition
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints dc ON dc.parent_object_id = t.object_id AND dc.parent_column_id = c.column_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name, c.column_id;

PRINT '';
PRINT '3. PRIMARY KEYS';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    kc.name AS PrimaryKeyName,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS PrimaryKeyColumns
FROM sys.key_constraints kc
INNER JOIN sys.tables t ON t.object_id = kc.parent_object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.index_columns ic ON ic.object_id = t.object_id AND ic.index_id = kc.unique_index_id
INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id
WHERE kc.type = 'PK'
GROUP BY s.name, t.name, kc.name
ORDER BY s.name, t.name;

PRINT '';
PRINT '4. FOREIGN KEYS / RELACIONES ENTRE TABLAS';

SELECT
    fk.name AS ForeignKeyName,
    schChild.name AS ChildSchema,
    tblChild.name AS ChildTable,
    STRING_AGG(colChild.name, ', ') WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS ChildColumns,
    schParent.name AS ParentSchema,
    tblParent.name AS ParentTable,
    STRING_AGG(colParent.name, ', ') WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS ParentColumns,
    fk.delete_referential_action_desc AS OnDeleteAction,
    fk.update_referential_action_desc AS OnUpdateAction,
    fk.is_disabled AS IsDisabled,
    fk.is_not_trusted AS IsNotTrusted
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
PRINT '5. INDICES';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique AS IsUnique,
    i.is_primary_key AS IsPrimaryKey,
    i.is_unique_constraint AS IsUniqueConstraint,
    STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY ic.key_ordinal) AS IndexColumns
FROM sys.indexes i
INNER JOIN sys.tables t ON t.object_id = i.object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id
WHERE t.is_ms_shipped = 0
  AND i.index_id > 0
  AND ic.is_included_column = 0
GROUP BY
    s.name,
    t.name,
    i.name,
    i.type_desc,
    i.is_unique,
    i.is_primary_key,
    i.is_unique_constraint
ORDER BY s.name, t.name, i.name;

PRINT '';
PRINT '6. CHECK CONSTRAINTS';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    cc.name AS ConstraintName,
    cc.definition AS Definition,
    cc.is_disabled AS IsDisabled,
    cc.is_not_trusted AS IsNotTrusted
FROM sys.check_constraints cc
INNER JOIN sys.tables t ON t.object_id = cc.parent_object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name, cc.name;

PRINT '';
PRINT '7. DEFAULT CONSTRAINTS';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    dc.name AS ConstraintName,
    dc.definition AS Definition
FROM sys.default_constraints dc
INNER JOIN sys.tables t ON t.object_id = dc.parent_object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = dc.parent_column_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name, c.column_id;
