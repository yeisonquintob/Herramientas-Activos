using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Damages;
using Navi.ToolsAssets.Domain.Entities.Documents;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Loans;
using Navi.ToolsAssets.Domain.Entities.Maintenance;
using Navi.ToolsAssets.Domain.Entities.Organization;
using Navi.ToolsAssets.Domain.Entities.PhysicalCounts;
using Navi.ToolsAssets.Domain.Entities.Sync;

namespace Navi.ToolsAssets.Infrastructure.Persistence.Context;

public class NaviToolsAssetsDbContext : DbContext
{
    public NaviToolsAssetsDbContext(DbContextOptions<NaviToolsAssetsDbContext> options)
        : base(options)
    {
    }

    public DbSet<SystemParameter> SystemParameters => Set<SystemParameter>();
    public DbSet<Zone> Zones => Set<Zone>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ToolLocation> ToolLocations => Set<ToolLocation>();
    public DbSet<ResponsiblePerson> ResponsiblePeople => Set<ResponsiblePerson>();

    public DbSet<ToolType> ToolTypes => Set<ToolType>();
    public DbSet<ToolCategory> ToolCategories => Set<ToolCategory>();
    public DbSet<ToolAsset> ToolAssets => Set<ToolAsset>();

    public DbSet<ToolLifeCycleEvent> ToolLifeCycleEvents => Set<ToolLifeCycleEvent>();
    public DbSet<ToolDocument> ToolDocuments => Set<ToolDocument>();
    public DbSet<ToolLoan> ToolLoans => Set<ToolLoan>();
    public DbSet<ToolLoanItem> ToolLoanItems => Set<ToolLoanItem>();
    public DbSet<DamageReport> DamageReports => Set<DamageReport>();
    public DbSet<MaintenanceRecord> MaintenanceRecords => Set<MaintenanceRecord>();
    public DbSet<PhysicalCount> PhysicalCounts => Set<PhysicalCount>();
    public DbSet<PhysicalCountItem> PhysicalCountItems => Set<PhysicalCountItem>();
    public DbSet<FenixReconciliationRecord> FenixReconciliationRecords => Set<FenixReconciliationRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureOrganization(modelBuilder);
        ConfigureInventory(modelBuilder);
        ConfigureLifeCycle(modelBuilder);
        ConfigureDocuments(modelBuilder);
        ConfigureLoans(modelBuilder);
        ConfigureDamages(modelBuilder);
        ConfigureMaintenance(modelBuilder);
        ConfigurePhysicalCounts(modelBuilder);
        ConfigureSync(modelBuilder);
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemParameter>(entity =>
        {
            entity.ToTable("SystemParameters", "Organization");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Value).HasMaxLength(1000);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Zone>(entity =>
        {
            entity.ToTable("Zones", "Organization");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches", "Organization");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.City).HasMaxLength(150);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasOne(x => x.Zone)
                .WithMany(x => x.Branches)
                .HasForeignKey(x => x.ZoneId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ToolLocation>(entity =>
        {
            entity.ToTable("ToolLocations", "Organization");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => new { x.BranchId, x.Code }).IsUnique();
            entity.HasOne(x => x.Branch)
                .WithMany(x => x.Locations)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ResponsiblePerson>(entity =>
        {
            entity.ToTable("ResponsiblePeople", "Organization");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EmployeeCode).HasMaxLength(100);
            entity.Property(x => x.DocumentNumber).HasMaxLength(100);
            entity.Property(x => x.FullName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(250);
            entity.Property(x => x.Position).HasMaxLength(150);
            entity.Property(x => x.Area).HasMaxLength(150);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.EmployeeCode).HasFilter("[EmployeeCode] IS NOT NULL");
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    private static void ConfigureInventory(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ToolType>(entity =>
        {
            entity.ToTable("ToolTypes", "Inventory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ToolCategory>(entity =>
        {
            entity.ToTable("ToolCategories", "Inventory");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ToolAsset>(entity =>
        {
            entity.ToTable("ToolAssets", "Inventory");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.InternalCode).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1500);
            entity.Property(x => x.Brand).HasMaxLength(150);
            entity.Property(x => x.Model).HasMaxLength(150);
            entity.Property(x => x.SerialNumber).HasMaxLength(150);
            entity.Property(x => x.FixedAssetCode).HasMaxLength(100);
            entity.Property(x => x.FenixCode).HasMaxLength(100);
            entity.Property(x => x.UnitOfMeasure).HasMaxLength(20);
            entity.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(x => x.OperationalStatus).HasConversion<string>().HasMaxLength(80);
            entity.Property(x => x.PhysicalStatus).HasConversion<string>().HasMaxLength(80);
            entity.Property(x => x.CustodyStatus).HasConversion<string>().HasMaxLength(80);
            entity.Property(x => x.ReconciliationStatus).HasConversion<string>().HasMaxLength(100);
            entity.Property(x => x.SyncStatus).HasConversion<string>().HasMaxLength(80);
            entity.Property(x => x.FenixName).HasMaxLength(250);
            entity.Property(x => x.FenixStatus).HasMaxLength(100);
            entity.Property(x => x.FenixBranch).HasMaxLength(150);
            entity.Property(x => x.FenixResponsible).HasMaxLength(250);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);

            entity.HasIndex(x => x.InternalCode).IsUnique();
            entity.HasIndex(x => x.FenixCode).IsUnique().HasFilter("[FenixCode] IS NOT NULL");
            entity.HasIndex(x => x.FixedAssetCode).IsUnique().HasFilter("[FixedAssetCode] IS NOT NULL");
            entity.HasIndex(x => x.SerialNumber).HasFilter("[SerialNumber] IS NOT NULL");

            entity.HasOne(x => x.Zone)
                .WithMany()
                .HasForeignKey(x => x.ZoneId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Branch)
                .WithMany(x => x.Tools)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Location)
                .WithMany(x => x.Tools)
                .HasForeignKey(x => x.LocationId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ResponsiblePerson)
                .WithMany(x => x.ToolsAsResponsible)
                .HasForeignKey(x => x.ResponsiblePersonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToolType)
                .WithMany(x => x.Tools)
                .HasForeignKey(x => x.ToolTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ToolCategory)
                .WithMany(x => x.Tools)
                .HasForeignKey(x => x.ToolCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    private static void ConfigureLifeCycle(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ToolLifeCycleEvent>(entity =>
        {
            entity.ToTable("ToolLifeCycleEvents", "LifeCycle");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EventType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Title).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.PreviousValue).HasMaxLength(1000);
            entity.Property(x => x.NewValue).HasMaxLength(1000);
            entity.Property(x => x.RegisteredBy).HasMaxLength(150);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasOne(x => x.ToolAsset)
                .WithMany(x => x.LifeCycleEvents)
                .HasForeignKey(x => x.ToolAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    private static void ConfigureDocuments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ToolDocument>(entity =>
        {
            entity.ToTable("ToolDocuments", "Documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(100);
            entity.Property(x => x.FileName).HasMaxLength(300).IsRequired();
            entity.Property(x => x.ObjectKey).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(150);
            entity.Property(x => x.UploadedBy).HasMaxLength(150);
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasOne(x => x.ToolAsset)
                .WithMany(x => x.Documents)
                .HasForeignKey(x => x.ToolAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    private static void ConfigureLoans(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ToolLoan>(entity =>
        {
            entity.ToTable("ToolLoans", "Loans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.LoanNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(80);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.LoanNumber).IsUnique();
            entity.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.RequestedByPerson)
                .WithMany()
                .HasForeignKey(x => x.RequestedByPersonId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ToolLoanItem>(entity =>
        {
            entity.ToTable("ToolLoanItems", "Loans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Quantity).HasColumnType("decimal(18,2)");
            entity.Property(x => x.DeliveryCondition).HasMaxLength(1000);
            entity.Property(x => x.ReturnCondition).HasMaxLength(1000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasOne(x => x.ToolLoan)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ToolLoanId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ToolAsset)
                .WithMany(x => x.LoanItems)
                .HasForeignKey(x => x.ToolAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    private static void ConfigureDamages(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DamageReport>(entity =>
        {
            entity.ToTable("DamageReports", "Damages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ReportNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Severity).HasConversion<string>().HasMaxLength(80);
            entity.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.ReportedBy).HasMaxLength(150);
            entity.Property(x => x.ActionTaken).HasMaxLength(2000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.ReportNumber).IsUnique();
            entity.HasOne(x => x.ToolAsset)
                .WithMany(x => x.DamageReports)
                .HasForeignKey(x => x.ToolAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    private static void ConfigureMaintenance(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MaintenanceRecord>(entity =>
        {
            entity.ToTable("MaintenanceRecords", "Maintenance");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.MaintenanceNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Type).HasConversion<string>().HasMaxLength(80);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(80);
            entity.Property(x => x.Provider).HasMaxLength(250);
            entity.Property(x => x.Technician).HasMaxLength(250);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Result).HasMaxLength(2000);
            entity.Property(x => x.Cost).HasColumnType("decimal(18,2)");
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.MaintenanceNumber).IsUnique();
            entity.HasOne(x => x.ToolAsset)
                .WithMany(x => x.MaintenanceRecords)
                .HasForeignKey(x => x.ToolAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    private static void ConfigurePhysicalCounts(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PhysicalCount>(entity =>
        {
            entity.ToTable("PhysicalCounts", "PhysicalCounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CountNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(80);
            entity.Property(x => x.ResponsibleBy).HasMaxLength(150);
            entity.Property(x => x.Notes).HasMaxLength(2000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasIndex(x => x.CountNumber).IsUnique();
            entity.HasOne(x => x.Branch)
                .WithMany()
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PhysicalCountItem>(entity =>
        {
            entity.ToTable("PhysicalCountItems", "PhysicalCounts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ExpectedLocation).HasMaxLength(250);
            entity.Property(x => x.FoundLocation).HasMaxLength(250);
            entity.Property(x => x.Observation).HasMaxLength(1500);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasOne(x => x.PhysicalCount)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.PhysicalCountId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.ToolAsset)
                .WithMany(x => x.PhysicalCountItems)
                .HasForeignKey(x => x.ToolAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }

    private static void ConfigureSync(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FenixReconciliationRecord>(entity =>
        {
            entity.ToTable("FenixReconciliationRecords", "Sync");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.SourceSystem).HasMaxLength(100).IsRequired();
            entity.Property(x => x.FenixCode).HasMaxLength(100);
            entity.Property(x => x.FixedAssetCode).HasMaxLength(100);
            entity.Property(x => x.FenixStatus).HasMaxLength(100);
            entity.Property(x => x.FenixBranch).HasMaxLength(150);
            entity.Property(x => x.FenixResponsible).HasMaxLength(250);
            entity.Property(x => x.ResultStatus).HasConversion<string>().HasMaxLength(100);
            entity.Property(x => x.Differences).HasMaxLength(4000);
            entity.Property(x => x.CreatedBy).HasMaxLength(150);
            entity.Property(x => x.UpdatedBy).HasMaxLength(150);
            entity.HasOne(x => x.ToolAsset)
                .WithMany(x => x.ReconciliationRecords)
                .HasForeignKey(x => x.ToolAssetId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
