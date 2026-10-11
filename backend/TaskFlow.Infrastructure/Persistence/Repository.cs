using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.Persistence;

internal sealed class Repository<T>(TaskFlowDbContext dbContext) : IRepository<T> where T : class
{
    public async Task<T> AddAsync(T entity)
    {
        await dbContext.Set<T>().AddAsync(entity);
        return entity;
    }

    /// <summary>
    /// A tracked entity needs nothing here: SaveChanges then writes only the columns that changed. Update() would write
    /// every column and so undo what another command saved in the meantime, even a soft delete.
    /// </summary>
    public Task<T> UpdateAsync(T entity)
    {
        if (dbContext.Entry(entity).State == EntityState.Detached)
        {
            dbContext.Set<T>().Update(entity);
        }

        return Task.FromResult(entity);
    }

    public Task DeleteAsync(T entity)
    {
        dbContext.Set<T>().Remove(entity);
        return Task.CompletedTask;
    }

    public void DeleteRange(IEnumerable<T> entityList)
    {
        dbContext.Set<T>().RemoveRange(entityList);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await dbContext.Set<T>().FindAsync(id);
    }

    public Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return dbContext.Set<T>().Where(predicate).ToListAsync();
    }
}
