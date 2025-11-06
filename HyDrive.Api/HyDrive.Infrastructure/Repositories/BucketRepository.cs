using Domain.Models;
using HyDrive.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HyDrive.Infrastructure.Repositories;

public class BucketRepository : IBucketRepository
{
    private readonly ApplicationDbContext _context;
    
    public BucketRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task AddAsync(Bucket bucket)
    {
        await _context.Buckets.AddAsync(bucket);
    }
    
    public async Task<Bucket?> GetByIdAsync(Guid bucketId)
    {
        return await _context.Buckets.FindAsync(bucketId);
    }

    public async Task<List<Bucket>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Buckets.Where(b => b.UserId == userId).ToListAsync();
    }
    
    public async Task<List<Bucket>> GetAllAsync()
    {
        return await _context.Buckets.ToListAsync();
    }
    
    public async Task UpdateByIdAsync(Bucket bucket)
    {
        _context.Buckets.Update(bucket);
        await Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(Guid bucketId)
    {
        await _context.Buckets
            .Where(b => b.Id == bucketId)
            .ExecuteDeleteAsync();
    }
    
    public async Task SaveAsync() => await _context.SaveChangesAsync();
}