using HyDrive.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HyDrive.Infrastructure.Repositories;

public class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await Task.CompletedTask;
    }

    public async Task DeleteByIdAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);
        if (entity != null)
            _dbSet.Remove(entity);
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}