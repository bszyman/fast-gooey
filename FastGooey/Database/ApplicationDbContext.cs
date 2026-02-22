using FastGooey.Models;
using FastGooey.Models.Media;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace FastGooey.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IClock clock) :
    IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Workspace> Workspaces { get; set; } = null!;
    public DbSet<GooeyInterface> GooeyInterfaces { get; set; } = null!;
    public DbSet<MediaSource> MediaSources { get; set; } = null!;
    public DbSet<KeyValueStore> KeyValueStores { get; set; } = null!;
    public DbSet<PasskeyCredential> PasskeyCredentials { get; set; } = null!;
    public DbSet<MagicLinkToken> MagicLinkTokens { get; set; } = null!;

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
        
        modelBuilder.Entity<Workspace>()
            .HasOne(w => w.OwnerUser)
            .WithMany(u => u.OwnedWorkspaces)
            .HasForeignKey(w => w.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PasskeyCredential>()
            .HasOne(p => p.User)
            .WithMany(u => u.PasskeyCredentials)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<PasskeyCredential>()
            .HasIndex(p => p.DescriptorId)
            .IsUnique();

        modelBuilder.Entity<MagicLinkToken>()
            .HasOne(t => t.User)
            .WithMany(u => u.MagicLinkTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MagicLinkToken>()
            .HasIndex(t => t.TokenHash)
            .IsUnique();

        modelBuilder.Entity<GooeyInterface>()
            .HasOne(g => g.Workspace)
            .WithMany(w => w.GooeyInterfaces)
            .HasForeignKey(g => g.WorkspaceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MediaSource>()
            .HasOne(s => s.Workspace)
            .WithMany(w => w.MediaSources)
            .HasForeignKey(s => s.WorkspaceId)
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
            else if (entry.Entity is MediaSource mediaSource)
            {
                if (entry.State == EntityState.Added)
                    mediaSource.CreatedAt = now;
                mediaSource.UpdatedAt = now;
            }
        }
    }
}
