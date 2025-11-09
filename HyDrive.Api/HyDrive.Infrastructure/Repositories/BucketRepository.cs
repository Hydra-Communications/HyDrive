using Domain.Models;
using HyDrive.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HyDrive.Infrastructure.Repositories;

public class BucketRepository : BaseRepository<Bucket>, IBucketRepository
{
    public BucketRepository(ApplicationDbContext context) : base(context) {}
    
    public async Task<List<Bucket>> GetAllByUserIdAsync(Guid userId)
    {
        return await _context.Buckets.Where(b => b.UserId == userId).ToListAsync();
    }
    
    public async Task UpdateByIdAsync(Bucket bucket)
    {
        _context.Buckets.Update(bucket);
        await Task.CompletedTask;
    }
}