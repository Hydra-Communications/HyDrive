using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace HyDrive.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public DbSet<Bucket> Buckets { get; set; }
    public DbSet<BucketObject> BucketObjects { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BucketObject>()
            .HasOne(e => e.Bucket)
            .WithMany(e => e.BucketObjects)
            .HasForeignKey(e => e.BucketId)
            .IsRequired();

        modelBuilder.Entity<BucketObject>()
            .HasIndex(b => b.BucketId);
        
        
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
        var modifiedEntries = ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in modifiedEntries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }
    }
}