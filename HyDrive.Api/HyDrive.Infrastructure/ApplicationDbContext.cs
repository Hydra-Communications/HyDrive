using Microsoft.EntityFrameworkCore;

namespace HyDrive.Infrastructure;

public class ApplicationDbContext : DbContext
{
    private ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}