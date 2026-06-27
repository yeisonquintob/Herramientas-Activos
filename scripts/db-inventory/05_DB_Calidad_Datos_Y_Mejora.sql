SET NOCOUNT ON;

PRINT '============================================================';
PRINT 'NAVI HERRAMIENTAS - CALIDAD DE DATOS Y MEJORAS';
PRINT '============================================================';

PRINT '';
PRINT '1. TABLAS SIN PRIMARY KEY';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    'Agregar PK si la tabla es transaccional o maestra.' AS Recommendation
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.key_constraints kc ON kc.parent_object_id = t.object_id AND kc.type = 'PK'
WHERE t.is_ms_shipped = 0
  AND kc.object_id IS NULL
ORDER BY s.name, t.name;

PRINT '';
PRINT '2. FOREIGN KEYS DESHABILITADAS O NO CONFIABLES';

SELECT
    fk.name AS ForeignKeyName,
    s.name AS SchemaName,
    t.name AS TableName,
    fk.is_disabled AS IsDisabled,
    fk.is_not_trusted AS IsNotTrusted,
    'Revisar integridad y ejecutar WITH CHECK CHECK CONSTRAINT si aplica.' AS Recommendation
FROM sys.foreign_keys fk
INNER JOIN sys.tables t ON t.object_id = fk.parent_object_id
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE fk.is_disabled = 1 OR fk.is_not_trusted = 1
ORDER BY s.name, t.name, fk.name;

PRINT '';
PRINT '3. TABLAS SIN COLUMNAS DE AUDITORIA';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    CASE WHEN NOT EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = t.object_id AND c.name = 'CreatedAt') THEN 1 ELSE 0 END AS MissingCreatedAt,
    CASE WHEN NOT EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = t.object_id AND c.name = 'UpdatedAt') THEN 1 ELSE 0 END AS MissingUpdatedAt,
    CASE WHEN NOT EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = t.object_id AND c.name = 'CreatedBy') THEN 1 ELSE 0 END AS MissingCreatedBy,
    CASE WHEN NOT EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = t.object_id AND c.name = 'UpdatedBy') THEN 1 ELSE 0 END AS MissingUpdatedBy,
    CASE WHEN NOT EXISTS (SELECT 1 FROM sys.columns c WHERE c.object_id = t.object_id AND c.name = 'IsDeleted') THEN 1 ELSE 0 END AS MissingSoftDelete
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE t.is_ms_shipped = 0
ORDER BY s.name, t.name;

PRINT '';
PRINT '4. TABLAS VACIAS';

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    SUM(CASE WHEN p.index_id IN (0,1) THEN p.rows ELSE 0 END) AS ApproxRows,
    'Validar si requiere seed, catálogo inicial o carga desde Excel/Fenix365.' AS Recommendation
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
LEFT JOIN sys.partitions p ON p.object_id = t.object_id
WHERE t.is_ms_shipped = 0
GROUP BY s.name, t.name
HAVING SUM(CASE WHEN p.index_id IN (0,1) THEN p.rows ELSE 0 END) = 0
ORDER BY s.name, t.name;

PRINT '';
PRINT '5. INDICES RECOMENDADOS PARA FOREIGN KEYS';

;WITH FKColumns AS
(
    SELECT
        fk.object_id AS ForeignKeyId,
        fk.name AS ForeignKeyName,
        fk.parent_object_id AS TableObjectId,
        STRING_AGG(CONVERT(varchar(20), fkc.parent_column_id), ',') WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS ColumnIds,
        STRING_AGG(c.name, ', ') WITHIN GROUP (ORDER BY fkc.constraint_column_id) AS ColumnNames
    FROM sys.foreign_keys fk
    INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    INNER JOIN sys.columns c ON c.object_id = fk.parent_object_id AND c.column_id = fkc.parent_column_id
    GROUP BY fk.object_id, fk.name, fk.parent_object_id
),
IndexColumns AS
(
    SELECT
        i.object_id AS TableObjectId,
        i.index_id AS IndexId,
        i.name AS IndexName,
        STRING_AGG(CONVERT(varchar(20), ic.column_id), ',') WITHIN GROUP (ORDER BY ic.key_ordinal) AS ColumnIds
    FROM sys.indexes i
    INNER JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    WHERE i.index_id > 0 AND ic.is_included_column = 0
    GROUP BY i.object_id, i.index_id, i.name
)
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    fk.ForeignKeyName,
    fk.ColumnNames,
    'CREATE INDEX IX_' + t.name + '_' + REPLACE(REPLACE(fk.ColumnNames, ', ', '_'), 'Id', 'Id') +
    ' ON ' + QUOTENAME(s.name) + '.' + QUOTENAME(t.name) + ' (' + fk.ColumnNames + ');' AS SuggestedIndex
FROM FKColumns fk
INNER JOIN sys.tables t ON t.object_id = fk.TableObjectId
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE NOT EXISTS
(
    SELECT 1
    FROM IndexColumns ix
    WHERE ix.TableObjectId = fk.TableObjectId
      AND (ix.ColumnIds = fk.ColumnIds OR ix.ColumnIds LIKE fk.ColumnIds + ',%')
)
ORDER BY s.name, t.name, fk.ForeignKeyName;
