USE [NaviToolsAssetsDb];

IF NOT EXISTS (SELECT 1 FROM [Organization].[SettingCatalogItems] WHERE CatalogType = 'AssetStatus')
BEGIN
    INSERT INTO [Organization].[SettingCatalogItems]
    (Id, CatalogType, Code, Name, Description, IsActive, IsDeleted, CreatedAt, CreatedBy)
    VALUES
    (NEWID(), 'AssetStatus', 'AVAILABLE', 'Disponible', 'Activo disponible para uso o préstamo.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'AssetStatus', 'ASSIGNED', 'Asignado', 'Activo asignado a responsable.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'AssetStatus', 'LOANED', 'Prestado', 'Activo entregado en préstamo.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'AssetStatus', 'IN_MAINTENANCE', 'En mantenimiento', 'Activo en proceso de mantenimiento.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'AssetStatus', 'DAMAGED', 'Dañado', 'Activo con daño reportado.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'AssetStatus', 'DISPOSED', 'Dado de baja', 'Activo retirado o dado de baja.', 1, 0, GETUTCDATE(), 'seed');
END;

IF NOT EXISTS (SELECT 1 FROM [Organization].[SettingCatalogItems] WHERE CatalogType = 'DocumentType')
BEGIN
    INSERT INTO [Organization].[SettingCatalogItems]
    (Id, CatalogType, Code, Name, Description, IsActive, IsDeleted, CreatedAt, CreatedBy)
    VALUES
    (NEWID(), 'DocumentType', 'TECHNICAL_DOCUMENT', 'Ficha técnica', 'Documento técnico del activo.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'DocumentType', 'PHOTO_EVIDENCE', 'Registro fotográfico', 'Foto o evidencia visual.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'DocumentType', 'MAINTENANCE_SUPPORT', 'Evidencia de mantenimiento', 'Soporte o evidencia de mantenimiento.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'DocumentType', 'DELIVERY_ACT', 'Acta de entrega', 'Acta de entrega o asignación.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'DocumentType', 'RETURN_ACT', 'Acta de devolución', 'Acta de devolución del activo.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'DocumentType', 'OTHER', 'Otro', 'Otro tipo de documento.', 1, 0, GETUTCDATE(), 'seed');
END;

IF NOT EXISTS (SELECT 1 FROM [Organization].[SettingCatalogItems] WHERE CatalogType = 'MaintenanceType')
BEGIN
    INSERT INTO [Organization].[SettingCatalogItems]
    (Id, CatalogType, Code, Name, Description, IsActive, IsDeleted, CreatedAt, CreatedBy)
    VALUES
    (NEWID(), 'MaintenanceType', 'PREVENTIVE', 'Preventivo', 'Mantenimiento preventivo programado.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'MaintenanceType', 'CORRECTIVE', 'Correctivo', 'Mantenimiento correctivo por daño o falla.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'MaintenanceType', 'CALIBRATION', 'Calibración', 'Proceso de calibración técnica.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'MaintenanceType', 'INSPECTION', 'Inspección', 'Inspección técnica o visual.', 1, 0, GETUTCDATE(), 'seed'),
    (NEWID(), 'MaintenanceType', 'CERTIFICATION', 'Certificación', 'Certificación técnica o normativa.', 1, 0, GETUTCDATE(), 'seed');
END;
