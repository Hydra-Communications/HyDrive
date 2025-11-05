using Domain.Models;

namespace HyDrive.Application.Interfaces.Repositories;

public interface IBucketRepository
{
    Task AddAsync(Bucket bucket);
    Task<Bucket?> GetByIdAsync(Guid bucketId);
    Task UpdateByIdAsync(Bucket bucket);
    Task<List<Bucket>> GetAllAsync();
    Task DeleteByIdAsync(Guid bucketId);
    Task SaveAsync();
}