using Domain.Models;
using HyDrive.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HyDrive.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
    }
}