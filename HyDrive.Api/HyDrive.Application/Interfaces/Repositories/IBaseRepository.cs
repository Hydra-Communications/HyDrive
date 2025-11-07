namespace HyDrive.Application.Interfaces.Repositories;

public interface IBaseRepository<T> where T : class
{
    Task AddAsync(T entity);
    Task<T?> GetByIdAsync(Guid id);
    Task<List<T>> GetAllAsync();
    Task UpdateAsync(T entity);
    Task DeleteByIdAsync(Guid id);
    Task SaveAsync();
}