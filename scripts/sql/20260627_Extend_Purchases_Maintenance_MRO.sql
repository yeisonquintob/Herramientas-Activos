SET NOCOUNT ON;

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Purchases')
BEGIN
    EXEC('CREATE SCHEMA [Purchases]');
END;

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Maintenance')
BEGIN
    EXEC('CREATE SCHEMA [Maintenance]');
END;

IF OBJECT_ID('[Purchases].[PurchaseRequests]', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Purchases.PurchaseRequests', 'PurchaseType') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [PurchaseType] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'RequestChannel') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [RequestChannel] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'InventoryClassification') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [InventoryClassification] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'GenericCode') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [GenericCode] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'ItemVariant') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [ItemVariant] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'VariantDetail') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [VariantDetail] NVARCHAR(500) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'CodeExists') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [CodeExists] BIT NOT NULL CONSTRAINT [DF_PurchaseRequests_CodeExists] DEFAULT(0);
    IF COL_LENGTH('Purchases.PurchaseRequests', 'RequiresCodeCreation') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [RequiresCodeCreation] BIT NOT NULL CONSTRAINT [DF_PurchaseRequests_RequiresCodeCreation] DEFAULT(0);
    IF COL_LENGTH('Purchases.PurchaseRequests', 'RequiresVariantCreation') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [RequiresVariantCreation] BIT NOT NULL CONSTRAINT [DF_PurchaseRequests_RequiresVariantCreation] DEFAULT(0);
    IF COL_LENGTH('Purchases.PurchaseRequests', 'PlanningRequestReference') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [PlanningRequestReference] NVARCHAR(200) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'TechnicalSpecifications') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [TechnicalSpecifications] NVARCHAR(3000) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'Capacity') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [Capacity] NVARCHAR(300) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'Dimensions') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [Dimensions] NVARCHAR(300) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'RequiredUse') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [RequiredUse] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'SerialReference') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [SerialReference] NVARCHAR(300) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'FailureDetail') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [FailureDetail] NVARCHAR(2000) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'MaintenanceTypeIfApplies') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [MaintenanceTypeIfApplies] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'HasPhotoSupport') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [HasPhotoSupport] BIT NOT NULL CONSTRAINT [DF_PurchaseRequests_HasPhotoSupport] DEFAULT(0);
    IF COL_LENGTH('Purchases.PurchaseRequests', 'PhotoSupportDescription') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [PhotoSupportDescription] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'DocumentSupportReference') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [DocumentSupportReference] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'CostCenter') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [CostCenter] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'AccountingConcept') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [AccountingConcept] NVARCHAR(300) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'AccountingAccount') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [AccountingAccount] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'RequiresAccountingValidation') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [RequiresAccountingValidation] BIT NOT NULL CONSTRAINT [DF_PurchaseRequests_RequiresAccountingValidation] DEFAULT(0);
    IF COL_LENGTH('Purchases.PurchaseRequests', 'AccountingValidationStatus') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [AccountingValidationStatus] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'AccountingValidationComment') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [AccountingValidationComment] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'FixedAssetReason') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [FixedAssetReason] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'WarehouseCode') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [WarehouseCode] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'LocationCode') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [LocationCode] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'DeliveryWarehouse') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [DeliveryWarehouse] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'AmountRange') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [AmountRange] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'IsLocalLowAmountPurchase') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [IsLocalLowAmountPurchase] BIT NOT NULL CONSTRAINT [DF_PurchaseRequests_IsLocalLowAmountPurchase] DEFAULT(0);
    IF COL_LENGTH('Purchases.PurchaseRequests', 'RequiresMroManagement') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [RequiresMroManagement] BIT NOT NULL CONSTRAINT [DF_PurchaseRequests_RequiresMroManagement] DEFAULT(0);
    IF COL_LENGTH('Purchases.PurchaseRequests', 'SelectedVendor') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [SelectedVendor] NVARCHAR(300) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'QuotationCount') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [QuotationCount] INT NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'QuotationReferences') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [QuotationReferences] NVARCHAR(2000) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'VendorSelectionCriteria') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [VendorSelectionCriteria] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'MroBuyer') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [MroBuyer] NVARCHAR(150) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'MroValidationStatus') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [MroValidationStatus] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'PurchaseOrderNumber') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [PurchaseOrderNumber] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'PurchaseOrderStatus') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [PurchaseOrderStatus] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'PurchaseOrderDate') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [PurchaseOrderDate] DATETIME2 NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'InvoiceReference') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [InvoiceReference] NVARCHAR(120) NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'ReceivedAt') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [ReceivedAt] DATETIME2 NULL;
    IF COL_LENGTH('Purchases.PurchaseRequests', 'ReceivedBy') IS NULL ALTER TABLE [Purchases].[PurchaseRequests] ADD [ReceivedBy] NVARCHAR(150) NULL;
END;

IF OBJECT_ID('[Maintenance].[MaintenanceRequests]', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'RequestChannel') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [RequestChannel] NVARCHAR(120) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'MaintenanceClassification') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [MaintenanceClassification] NVARCHAR(120) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'ServiceType') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [ServiceType] NVARCHAR(120) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'RequiredAt') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [RequiredAt] DATETIME2 NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'SerialNumber') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [SerialNumber] NVARCHAR(200) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'Brand') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [Brand] NVARCHAR(200) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'Model') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [Model] NVARCHAR(200) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'EquipmentReference') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [EquipmentReference] NVARCHAR(300) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'ImageEvidenceDescription') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [ImageEvidenceDescription] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'EvidenceReference') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [EvidenceReference] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'ExcelReference') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [ExcelReference] NVARCHAR(300) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'FailureDate') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [FailureDate] DATETIME2 NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'NeedDescription') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [NeedDescription] NVARCHAR(2000) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'FailureDetail') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [FailureDetail] NVARCHAR(2000) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'MaintenanceLocation') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [MaintenanceLocation] NVARCHAR(300) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'EstimatedDowntimeHours') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [EstimatedDowntimeHours] DECIMAL(18,2) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'WarrantyApplies') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [WarrantyApplies] BIT NOT NULL CONSTRAINT [DF_MaintenanceRequests_WarrantyApplies] DEFAULT(0);
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'WarrantyProvider') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [WarrantyProvider] NVARCHAR(300) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'ServiceProvider') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [ServiceProvider] NVARCHAR(300) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'RequiresQuotation') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [RequiresQuotation] BIT NOT NULL CONSTRAINT [DF_MaintenanceRequests_RequiresQuotation] DEFAULT(0);
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'QuotationCount') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [QuotationCount] INT NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'SelectedVendor') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [SelectedVendor] NVARCHAR(300) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'QuotationReferences') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [QuotationReferences] NVARCHAR(2000) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'VendorSelectionReason') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [VendorSelectionReason] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'RequiresPurchaseOrder') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [RequiresPurchaseOrder] BIT NOT NULL CONSTRAINT [DF_MaintenanceRequests_RequiresPurchaseOrder] DEFAULT(0);
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'PurchaseOrderNumber') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [PurchaseOrderNumber] NVARCHAR(120) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'PurchaseOrderStatus') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [PurchaseOrderStatus] NVARCHAR(120) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'MroCodeOrAccount') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [MroCodeOrAccount] NVARCHAR(120) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'AccountingConcept') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [AccountingConcept] NVARCHAR(300) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'AccountingAccount') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [AccountingAccount] NVARCHAR(120) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'ProviderActivationCriteria') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [ProviderActivationCriteria] NVARCHAR(1000) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'RequiresAccountingValidation') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [RequiresAccountingValidation] BIT NOT NULL CONSTRAINT [DF_MaintenanceRequests_RequiresAccountingValidation] DEFAULT(0);
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'AccountingValidationStatus') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [AccountingValidationStatus] NVARCHAR(120) NULL;
    IF COL_LENGTH('Maintenance.MaintenanceRequests', 'AccountingValidationComment') IS NULL ALTER TABLE [Maintenance].[MaintenanceRequests] ADD [AccountingValidationComment] NVARCHAR(1000) NULL;
END;
