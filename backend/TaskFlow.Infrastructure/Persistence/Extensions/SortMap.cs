using System.Linq.Expressions;
using TaskFlow.Application.Common.Filtering;
using TaskFlow.Application.Common.Filtering.Enums;

namespace TaskFlow.Infrastructure.Persistence.Extensions;

/// <summary>
/// The fields a grid can be sorted on, each mapped to a typed key, so sorting is translated to SQL and
/// a field name from the query string can never reach the database. The validators reject unknown fields first.
/// </summary>
internal sealed class SortMap<T>
{
    private readonly Dictionary<string, Func<IQueryable<T>, bool, IOrderedQueryable<T>>> _orderBy = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Func<IOrderedQueryable<T>, bool, IOrderedQueryable<T>>> _thenBy = new(StringComparer.OrdinalIgnoreCase);

    public SortMap<T> Add<TKey>(string field, Expression<Func<T, TKey>> key)
    {
        _orderBy[field] = (query, descending) => descending ? query.OrderByDescending(key) : query.OrderBy(key);
        _thenBy[field] = (query, descending) => descending ? query.ThenByDescending(key) : query.ThenBy(key);
        return this;
    }

    /// <summary>A nullable key whose empty values come last in both directions, like tasks without a due date.</summary>
    public SortMap<T> AddNullsLast<TKey>(string field, Expression<Func<T, TKey?>> key, Expression<Func<T, bool>> isNull)
        where TKey : struct
    {
        _orderBy[field] = (query, descending) => descending
            ? query.OrderBy(isNull).ThenByDescending(key)
            : query.OrderBy(isNull).ThenBy(key);
        _thenBy[field] = (query, descending) => descending
            ? query.ThenBy(isNull).ThenByDescending(key)
            : query.ThenBy(isNull).ThenBy(key);
        return this;
    }

    public IOrderedQueryable<T> Apply(IQueryable<T> query, IReadOnlyList<OrderBy> orderByFields, OrderBy defaultOrder)
    {
        IReadOnlyList<OrderBy> fields = orderByFields.Count > 0 ? orderByFields : [defaultOrder];

        var ordered = OrderBy(query, fields[0]);
        foreach (var orderBy in fields.Skip(1))
        {
            ordered = ThenBy(ordered, orderBy);
        }

        return ordered;
    }

    private IOrderedQueryable<T> OrderBy(IQueryable<T> query, OrderBy orderBy)
    {
        return Lookup(_orderBy, orderBy.Field)(query, orderBy.Dir == OrderDirection.Desc);
    }

    private IOrderedQueryable<T> ThenBy(IOrderedQueryable<T> query, OrderBy orderBy)
    {
        return Lookup(_thenBy, orderBy.Field)(query, orderBy.Dir == OrderDirection.Desc);
    }

    private static TSorter Lookup<TSorter>(Dictionary<string, TSorter> sorters, string field)
    {
        return sorters.TryGetValue(field, out var sorter)
            ? sorter
            : throw new InvalidOperationException($"'{field}' is not a sortable field of {typeof(T).Name}.");
    }
}
