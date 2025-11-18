using System.Linq.Expressions;

namespace GoalboundFamily.Api.Repositories.Interfaces;

/// <summary>
/// Generic repository interface providing common CRUD operations
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : class
{
    // Read operations
    Task<T?> GetByIdAsync(int id);
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    // Create
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);

    // Update
    Task UpdateAsync(T entity);
    Task UpdateRangeAsync(IEnumerable<T> entities);

    // Delete
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(IEnumerable<T> entities);
    Task DeleteByIdAsync(int id);
    Task DeleteByIdAsync(Guid id);

    // Check existence
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    // Count
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);

    // Save changes (if not using Unit of Work)
    Task<int> SaveChangesAsync();
}
