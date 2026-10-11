using System.Linq.Expressions;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Extensions;

namespace TaskFlow.Application.Common.Extensions;

public static class RepositoryExtensions
{
    /// <summary>Finds an entity by id, but only when it lies inside the given scope; otherwise returns null.</summary>
    public static async Task<T?> FindInScopeAsync<T>(
        this IRepository<T> repository,
        Guid id,
        Expression<Func<T, bool>> scope) where T : BaseEntity
    {
        var matches = await repository.FindAsync(ExpressionCombiner.AndAlso(scope, entity => entity.Id == id));

        return matches.SingleOrDefault();
    }
}
