using Domain.Models;

namespace HyDrive.Application.Interfaces.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    public Task<User?> GetByEmailAsync(string email);
}