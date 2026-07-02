IF COL_LENGTH('dbo.ToolAssets', 'ShelfLocation') IS NULL
BEGIN
    ALTER TABLE dbo.ToolAssets
    ADD ShelfLocation NVARCHAR(120) NULL;
END
GO
