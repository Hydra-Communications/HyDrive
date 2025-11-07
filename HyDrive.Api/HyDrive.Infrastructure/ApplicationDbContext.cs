using Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HyDrive.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public DbSet<Bucket> Buckets { get; set; }
    public DbSet<BucketObject> BucketObjects { get; set; }
    public DbSet<User> Users { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BucketObject>(entity =>
        {
            entity.HasOne(o => o.Bucket)
                .WithMany(b => b.BucketObjects)
                .HasForeignKey(o => o.BucketId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(o => o.BucketId);
        });

        modelBuilder.Entity<Bucket>(entity =>
        {
            entity.HasOne(b => b.User)
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();
        });
    }
    
    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }
    
    public override EntityEntry Remove(object entity)
        {
        if (entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = true;
            Entry(baseEntity).State = EntityState.Modified;
            return Entry(baseEntity);
        }

        return base.Remove(entity);
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