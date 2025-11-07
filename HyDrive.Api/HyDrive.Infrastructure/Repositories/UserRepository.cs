using Domain.Models;
using HyDrive.Application.Interfaces.Repositories;

namespace HyDrive.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }
}