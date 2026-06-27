namespace Navi.ToolsAssets.MobilePwa.Models;

public sealed class MobileToolDetailDto
{
    public Guid Id { get; set; }
    public string InternalCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? FixedAssetCode { get; set; }
    public string? FenixCode { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal Quantity { get; set; }

    public bool IsSpecialized { get; set; }
    public bool RequiresMaintenance { get; set; }
    public bool RequiresPreOperationalCheck { get; set; }
    public bool RequiresCertification { get; set; }
    public DateTime? CertificationExpirationDate { get; set; }

    public string? ZoneCode { get; set; }
    public string? ZoneName { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? LocationCode { get; set; }
    public string? LocationName { get; set; }
    public Guid? ResponsiblePersonId { get; set; }
    public string? ResponsiblePersonName { get; set; }

    public string? ToolTypeCode { get; set; }
    public string? ToolTypeName { get; set; }
    public string? ToolCategoryCode { get; set; }
    public string? ToolCategoryName { get; set; }

    public string OperationalStatus { get; set; } = string.Empty;
    public string PhysicalStatus { get; set; } = string.Empty;
    public string CustodyStatus { get; set; } = string.Empty;
    public string ReconciliationStatus { get; set; } = string.Empty;
    public string SyncStatus { get; set; } = string.Empty;
}

public sealed class MobileLifeCycleEventDto
{
    public Guid Id { get; set; }
    public Guid ToolAssetId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? PreviousValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime EventDate { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public sealed class MobileTechnicalLifeRecordDto
{
    public MobileLifeSummaryDto Summary { get; set; } = new();
    public MobileEquipmentInformationDto EquipmentInformation { get; set; } = new();
    public MobileTechnicalSpecificationsDto TechnicalSpecifications { get; set; } = new();
    public MobileWarrantyDto Warranty { get; set; } = new();
    public MobileUsefulLifeDto UsefulLife { get; set; } = new();
    public MobileMaintenancePlanDto MaintenancePlan { get; set; } = new();
    public List<MobileAccessoryDto> Accessories { get; set; } = new();
    public List<MobileSafePracticeDto> SafePractices { get; set; } = new();
    public MobileLifeDocumentsDto Documents { get; set; } = new();
    public List<MobileMaintenanceScheduleDto> MaintenanceSchedule { get; set; } = new();
    public List<MobileLifeCycleEventDto> LifeCycleEvents { get; set; } = new();
}

public sealed class MobileLifeSummaryDto
{
    public Guid Id { get; set; }
    public string? InternalCode { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? OperationalStatus { get; set; }
    public string? PhysicalStatus { get; set; }
    public string? CustodyStatus { get; set; }
    public string? ReconciliationStatus { get; set; }
    public string? SyncStatus { get; set; }
    public bool IsSpecialized { get; set; }
    public bool RequiresMaintenance { get; set; }
    public bool RequiresPreOperationalCheck { get; set; }
    public bool RequiresCertification { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class MobileEquipmentInformationDto
{
    public string? InternalCode { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Model { get; set; }
    public string? SerialNumber { get; set; }
    public string? FixedAssetCode { get; set; }
    public string? FenixCode { get; set; }
    public DateTime? AcquisitionDate { get; set; }
    public string? UnitOfMeasure { get; set; }
    public decimal Quantity { get; set; }

    public string? ZoneCode { get; set; }
    public string? ZoneName { get; set; }
    public string? BranchCode { get; set; }
    public string? BranchName { get; set; }
    public string? LocationCode { get; set; }
    public string? LocationName { get; set; }
    public string? ResponsibleName { get; set; }

    public string? ToolTypeCode { get; set; }
    public string? ToolTypeName { get; set; }
    public string? ToolCategoryCode { get; set; }
    public string? ToolCategoryName { get; set; }
}

public sealed class MobileTechnicalSpecificationsDto
{
    public string? Voltage { get; set; }
    public string? LoadCapacity { get; set; }
    public string? Provider { get; set; }
    public bool RequiresMaintenance { get; set; }
    public bool RequiresPreOperationalCheck { get; set; }
    public bool RequiresCertification { get; set; }
    public DateTime? CertificationExpirationDate { get; set; }
}

public sealed class MobileWarrantyDto
{
    public bool HasWarranty { get; set; }
    public string? WarrantyType { get; set; }
}

public sealed class MobileUsefulLifeDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? UsefulLifeMonths { get; set; }
    public int? UsefulLifeDays { get; set; }
    public int? ElapsedDays { get; set; }
    public int? RemainingDays { get; set; }
    public int? UsedPercentage { get; set; }
}

public sealed class MobileMaintenancePlanDto
{
    public DateTime? LastMaintenanceDate { get; set; }
    public DateTime? NextMaintenanceDate { get; set; }
    public int? MaintenancePeriodMonths { get; set; }
    public bool RequiresMaintenance { get; set; }
    public int TotalMaintenanceRecords { get; set; }
    public int CompletedMaintenanceRecords { get; set; }
    public int PendingMaintenanceRecords { get; set; }
}

public sealed class MobileAccessoryDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public decimal Quantity { get; set; }
    public bool RequiresMaintenance { get; set; }
    public bool IsMeasurementEquipment { get; set; }
    public string? Observation { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public sealed class MobileSafePracticeDto
{
    public Guid Id { get; set; }
    public string? PracticeName { get; set; }
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public sealed class MobileLifeDocumentsDto
{
    public List<MobileDocumentDto> PhotoRecords { get; set; } = new();
    public List<MobileDocumentDto> TechnicalDocuments { get; set; } = new();
    public List<MobileDocumentDto> MaintenanceEvidences { get; set; } = new();
    public List<MobileDocumentDto> AllDocuments { get; set; } = new();
}

public sealed class MobileDocumentDto
{
    public Guid Id { get; set; }
    public string? DocumentType { get; set; }
    public string? FileName { get; set; }
    public string? ObjectKey { get; set; }
    public string? ContentType { get; set; }
    public long SizeBytes { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime? UploadedAt { get; set; }
    public string? Description { get; set; }
    public string? DownloadUrl { get; set; }
}

public sealed class MobileMaintenanceScheduleDto
{
    public Guid Id { get; set; }
    public string? MaintenanceNumber { get; set; }
    public string? Type { get; set; }
    public string? Status { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public string? Provider { get; set; }
    public string? Technician { get; set; }
    public string? Description { get; set; }
    public string? MaintenanceActivities { get; set; }
    public string? ExecutionNotes { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? ResponsibleName { get; set; }
    public string? ResponsiblePosition { get; set; }
    public bool IsToolOperational { get; set; }
    public decimal? Cost { get; set; }
    public string? Result { get; set; }
    public Guid? EvidenceDocumentId { get; set; }
    public string? EvidenceFileName { get; set; }
    public string? EvidenceDownloadUrl { get; set; }
}
