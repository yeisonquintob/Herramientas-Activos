IF DB_ID(N'NaviToolsAssetsDb') IS NULL
BEGIN
    CREATE DATABASE [NaviToolsAssetsDb];
END
GO

USE [NaviToolsAssetsDb];
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Security')
    EXEC('CREATE SCHEMA Security');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Organization')
    EXEC('CREATE SCHEMA Organization');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Inventory')
    EXEC('CREATE SCHEMA Inventory');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'LifeCycle')
    EXEC('CREATE SCHEMA LifeCycle');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Documents')
    EXEC('CREATE SCHEMA Documents');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Loans')
    EXEC('CREATE SCHEMA Loans');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Maintenance')
    EXEC('CREATE SCHEMA Maintenance');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Imports')
    EXEC('CREATE SCHEMA Imports');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Sync')
    EXEC('CREATE SCHEMA Sync');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Audit')
    EXEC('CREATE SCHEMA Audit');
GO
