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
    }
}