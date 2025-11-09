using Domain.Models;

namespace HyDrive.Application.Interfaces.Repositories;

public interface IBucketRepository : IBaseRepository<Bucket>
{
    Task<List<Bucket>> GetAllByUserIdAsync(Guid userId);
}