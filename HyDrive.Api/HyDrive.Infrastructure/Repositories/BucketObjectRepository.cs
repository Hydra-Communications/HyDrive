using Domain.Models;
using HyDrive.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HyDrive.Infrastructure.Repositories;

public class BucketObjectRepository : IBucketObjectRepository
{
    private readonly ApplicationDbContext _context;
    
    public BucketObjectRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BucketObject?> GetByIdAsync(Guid bucketObjectId)
    {
        return await _context.BucketObjects.FindAsync(bucketObjectId);
    }

    public async Task<List<BucketObject>> GetAllByBucketIdAsync(Guid bucketId)
    {
        return await _context.BucketObjects.ToListAsync();
    }
    
    public async Task AddAsync(BucketObject bucketObject)
    {
        await _context.BucketObjects.AddAsync(bucketObject);
    }
    
    public async Task DeleteByIdAsync(Guid bucketObjectId)
    {
        var bucketObject = await _context.BucketObjects.FindAsync(bucketObjectId);
        if (bucketObject != null)
        {
            bucketObject.IsDeleted = true;
            await _context.SaveChangesAsync();
        }
        else
        {
            throw new Exception("BucketObject not found");
        }
    }
    
    public async Task SaveAsync() => await _context.SaveChangesAsync();
}