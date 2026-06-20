using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Organization;

namespace Navi.ToolsAssets.Infrastructure.Persistence.Context;

public class NaviToolsAssetsDbContext : DbContext
{
    public NaviToolsAssetsDbContext(DbContextOptions<NaviToolsAssetsDbContext> options)
        : base(options)
    {
    }

    public DbSet<SystemParameter> SystemParameters => Set<SystemParameter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureOrganization(modelBuilder);
    }

    private static void ConfigureOrganization(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SystemParameter>(entity =>
        {
            entity.ToTable("SystemParameters", "Organization");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Value)
                .HasMaxLength(1000);

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.Property(x => x.CreatedBy)
                .HasMaxLength(150);

            entity.Property(x => x.UpdatedBy)
                .HasMaxLength(150);

            entity.HasIndex(x => x.Code)
                .IsUnique();

            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
