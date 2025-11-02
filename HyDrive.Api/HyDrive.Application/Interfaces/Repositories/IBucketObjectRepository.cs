using Domain.Models;

namespace HyDrive.Application.Interfaces.Repositories;

public interface IBucketObjectRepository
{
    Task<BucketObject?> GetByIdAsync(Guid bucketObjectId);
    Task<List<BucketObject>> GetAllAsync(Guid bucketId);
    Task AddAsync(BucketObject bucketObject);
    Task DeleteByIdAsync(Guid bucketObjectId);
}