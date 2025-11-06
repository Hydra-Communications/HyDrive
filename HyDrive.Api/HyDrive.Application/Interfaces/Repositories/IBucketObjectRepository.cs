using Domain.Models;

namespace HyDrive.Application.Interfaces.Repositories;

public interface IBucketObjectRepository
{
    Task<BucketObject?> GetByIdAsync(Guid bucketObjectId);
    Task<List<BucketObject>> GetAllByBucketIdAsync(Guid bucketId);
    Task AddAsync(BucketObject bucketObject);
    Task DeleteByIdAsync(Guid bucketObjectId);
    Task SaveAsync();
}