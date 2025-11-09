using Domain.Models;

namespace HyDrive.Application.Interfaces.Repositories;

public interface IBucketObjectRepository : IBaseRepository<BucketObject>
{
    Task<List<BucketObject>> GetAllByBucketId(Guid bucketId);
}