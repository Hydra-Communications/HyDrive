using Domain.Models;
using HyDrive.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HyDrive.Infrastructure.Repositories;

public class BucketObjectRepository : BaseRepository<BucketObject>, IBucketObjectRepository
{
    public BucketObjectRepository(ApplicationDbContext context) : base(context) {}

    public async Task<List<BucketObject>> GetAllByBucketId(Guid bucketId)
    {
        return await _context.BucketObjects.ToListAsync();
    }
}