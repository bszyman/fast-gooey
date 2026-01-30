using FastGooey.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace FastGooey.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IClock clock) :
    IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Workspace> Workspaces { get; set; } = null!;
    public DbSet<GooeyInterface> GooeyInterfaces { get; set; } = null!;
    public DbSet<KeyValueStore> KeyValueStores { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            // InMemory provider does not support JsonDocument mapping.
            modelBuilder.Entity<GooeyInterface>()
                .Ignore(g => g.Config);
        }

        // Configure relationships
        modelBuilder.Entity<ApplicationUser>()
            .HasOne(u => u.Workspace)
            .WithMany(w => w.Users)
            .HasForeignKey(u => u.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<GooeyInterface>()
            .HasOne(g => g.Workspace)
            .WithMany(w => w.GooeyInterfaces)
            .HasForeignKey(g => g.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var now = clock.GetCurrentInstant();
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is Workspace workspace)
            {
                if (entry.State == EntityState.Added)
                    workspace.CreatedAt = now;
                workspace.UpdatedAt = now;
            }
            else if (entry.Entity is ApplicationUser user)
            {
                if (entry.State == EntityState.Added)
                    user.CreatedAt = now;
                user.UpdatedAt = now;
            }
            else if (entry.Entity is GooeyInterface gooeyInterface)
            {
                if (entry.State == EntityState.Added)
                    gooeyInterface.CreatedAt = now;
                gooeyInterface.UpdatedAt = now;
            }
        }
    }
}
