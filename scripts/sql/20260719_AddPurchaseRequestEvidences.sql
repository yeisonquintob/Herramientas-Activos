/*
NAVI PURCHASE PERSISTENT EVIDENCE V142
Metadatos en SQL Server. El binario se almacena en MinIO.
Script idempotente.
*/

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Purchases')
BEGIN
    EXEC('CREATE SCHEMA [Purchases]');
END;

IF OBJECT_ID('[Purchases].[PurchaseRequestEvidences]', 'U') IS NULL
BEGIN
    CREATE TABLE [Purchases].[PurchaseRequestEvidences](
        [Id] UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT [PK_PurchaseRequestEvidences] PRIMARY KEY,
        [WorkspaceId] UNIQUEIDENTIFIER NOT NULL,
        [PurchaseRequestId] UNIQUEIDENTIFIER NULL,
        [ReferenceId] UNIQUEIDENTIFIER NOT NULL,
        [EvidenceType] NVARCHAR(40) NOT NULL,
        [ReferenceName] NVARCHAR(300) NULL,
        [FileName] NVARCHAR(260) NOT NULL,
        [ContentType] NVARCHAR(100) NOT NULL,
        [ObjectKey] NVARCHAR(700) NOT NULL,
        [FileSize] BIGINT NOT NULL,
        [UploadedBy] NVARCHAR(150) NOT NULL,
        [UploadedAt] DATETIME2 NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [CreatedBy] NVARCHAR(150) NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] NVARCHAR(150) NULL,
        [IsDeleted] BIT NOT NULL
            CONSTRAINT [DF_PurchaseRequestEvidences_IsDeleted] DEFAULT(0)
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PurchaseRequestEvidences_WorkspaceId'
      AND object_id = OBJECT_ID('[Purchases].[PurchaseRequestEvidences]')
)
BEGIN
    CREATE INDEX [IX_PurchaseRequestEvidences_WorkspaceId]
        ON [Purchases].[PurchaseRequestEvidences]([WorkspaceId]);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PurchaseRequestEvidences_PurchaseRequestId'
      AND object_id = OBJECT_ID('[Purchases].[PurchaseRequestEvidences]')
)
BEGIN
    CREATE INDEX [IX_PurchaseRequestEvidences_PurchaseRequestId]
        ON [Purchases].[PurchaseRequestEvidences]([PurchaseRequestId]);
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_PurchaseRequestEvidences_Workspace_Type_Reference'
      AND object_id = OBJECT_ID('[Purchases].[PurchaseRequestEvidences]')
)
BEGIN
    CREATE INDEX [IX_PurchaseRequestEvidences_Workspace_Type_Reference]
        ON [Purchases].[PurchaseRequestEvidences](
            [WorkspaceId],
            [EvidenceType],
            [ReferenceId]
        );
END;

IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys
    WHERE name = 'FK_PurchaseRequestEvidences_PurchaseRequests_PurchaseRequestId'
)
BEGIN
    ALTER TABLE [Purchases].[PurchaseRequestEvidences]
        ADD CONSTRAINT [FK_PurchaseRequestEvidences_PurchaseRequests_PurchaseRequestId]
        FOREIGN KEY ([PurchaseRequestId])
        REFERENCES [Purchases].[PurchaseRequests]([Id]);
END;
