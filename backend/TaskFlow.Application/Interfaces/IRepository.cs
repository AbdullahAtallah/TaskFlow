using System.Linq.Expressions;

namespace TaskFlow.Application.Interfaces;

/// <summary>
/// Write-side access for command handlers. Reads for the UI go through the DataReaders instead.
/// Junction entities (composite keys) are supported too, except for <see cref="GetByIdAsync"/>.
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T> AddAsync(T entity);

    Task<T> UpdateAsync(T entity);

    Task DeleteAsync(T entity);

    void DeleteRange(IEnumerable<T> entityList);

    Task<T?> GetByIdAsync(Guid id);

    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
}
