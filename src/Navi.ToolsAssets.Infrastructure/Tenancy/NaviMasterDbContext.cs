using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Tenancy;

namespace Navi.ToolsAssets.Infrastructure.Tenancy;

public sealed class NaviMasterDbContext : DbContext
{
    public NaviMasterDbContext(DbContextOptions<NaviMasterDbContext> options)
        : base(options)
    {
    }

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<CompanyDatabase> CompanyDatabases => Set<CompanyDatabase>();
    public DbSet<UserCompanyAccess> UserCompanyAccesses => Set<UserCompanyAccess>();
    public DbSet<CompanySchemaVersion> CompanySchemaVersions => Set<CompanySchemaVersion>();
    public DbSet<CompanyBackup> CompanyBackups => Set<CompanyBackup>();
    public DbSet<CompanyImportJob> CompanyImportJobs => Set<CompanyImportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Master");

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.TaxIdentifier).HasMaxLength(80);
            entity.Property(x => x.PlanCode).HasMaxLength(80);
            entity.HasIndex(x => x.Code).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasIndex(x => new { x.IsActive, x.Status });
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<CompanyDatabase>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.DatabaseName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.ConnectionKey).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.SchemaVersion).HasMaxLength(80);
            entity.HasIndex(x => x.CompanyId).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasIndex(x => x.DatabaseName).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasIndex(x => x.ConnectionKey).IsUnique().HasFilter("[IsDeleted] = 0");
            entity.HasOne(x => x.Company)
                .WithMany(x => x.Databases)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted && x.Company != null && !x.Company.IsDeleted);
        });

        modelBuilder.Entity<UserCompanyAccess>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.RoleCode).HasMaxLength(80);
            entity.HasIndex(x => new { x.UserId, x.CompanyId })
                .IsUnique()
                .HasFilter("[IsDeleted] = 0");
            entity.HasIndex(x => new { x.UserId, x.IsActive });
            entity.HasOne(x => x.Company)
                .WithMany(x => x.UserAccesses)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted && x.Company != null && !x.Company.IsDeleted);
        });

        modelBuilder.Entity<CompanySchemaVersion>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Version).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.ErrorCode).HasMaxLength(120);
            entity.HasIndex(x => new { x.CompanyId, x.Version }).IsUnique();
            entity.HasOne(x => x.Company)
                .WithMany(x => x.SchemaVersions)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted && x.Company != null && !x.Company.IsDeleted);
        });

        modelBuilder.Entity<CompanyBackup>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.BackupNumber).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.StorageObjectKey).HasMaxLength(700).IsRequired();
            entity.Property(x => x.SchemaVersion).HasMaxLength(80);
            entity.Property(x => x.Sha256Checksum).HasMaxLength(64);
            entity.Property(x => x.ErrorCode).HasMaxLength(120);
            entity.HasIndex(x => x.BackupNumber).IsUnique();
            entity.HasIndex(x => new { x.CompanyId, x.RequestedAtUtc });
            entity.HasOne(x => x.Company)
                .WithMany(x => x.Backups)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted && x.Company != null && !x.Company.IsDeleted);
        });

        modelBuilder.Entity<CompanyImportJob>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasMaxLength(40).IsRequired();
            entity.Property(x => x.FileName).HasMaxLength(260).IsRequired();
            entity.Property(x => x.Sha256Checksum).HasMaxLength(64);
            entity.HasIndex(x => x.ImportJobId).IsUnique();
            entity.HasIndex(x => new { x.CompanyId, x.RequestedAtUtc });
            entity.HasOne(x => x.Company)
                .WithMany(x => x.ImportJobs)
                .HasForeignKey(x => x.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted && x.Company != null && !x.Company.IsDeleted);
        });
    }
}
