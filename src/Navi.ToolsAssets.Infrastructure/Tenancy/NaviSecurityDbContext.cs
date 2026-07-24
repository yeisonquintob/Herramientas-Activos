using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Security;

namespace Navi.ToolsAssets.Infrastructure.Tenancy;

/// <summary>
/// Contexto fijo para autenticar y validar sesiones antes de resolver el tenant.
/// No debe usarse para información operativa de una compañía.
/// </summary>
public sealed class NaviSecurityDbContext : DbContext
{
    public NaviSecurityDbContext(DbContextOptions<NaviSecurityDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<AppRole> AppRoles => Set<AppRole>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppRole>(entity =>
        {
            entity.ToTable("AppRoles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Code).HasMaxLength(80);
            entity.Property(x => x.Name).HasMaxLength(200);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUsers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserName).HasMaxLength(150);
            entity.Property(x => x.PasswordHash).HasMaxLength(500);
            entity.Ignore(x => x.Branch);
            entity.Ignore(x => x.ResponsiblePerson);
            entity.HasOne(x => x.AppRole)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.AppRoleId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<UserSession>(entity =>
        {
            entity.ToTable("UserSessions", "Security");
            entity.HasKey(x => x.Id);
            entity.HasOne(x => x.AppUser)
                .WithMany()
                .HasForeignKey(x => x.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
