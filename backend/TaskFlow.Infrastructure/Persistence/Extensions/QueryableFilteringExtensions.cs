using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Filtering;

namespace TaskFlow.Infrastructure.Persistence.Extensions;

internal static class QueryableFilteringExtensions
{
    /// <summary>
    /// Counts the filtered rows and fetches one sorted page, both in SQL. The tie breaker (the id) keeps the
    /// order stable, so paging over equal sort values never skips or repeats a row.
    /// </summary>
    public static async Task<FilteringResult<T>> ToFilteringResultAsync<T>(
        this IQueryable<T> query,
        FilterParams filterParams,
        SortMap<T> sortMap,
        OrderBy defaultOrder,
        Expression<Func<T, Guid>> tieBreaker)
    {
        var total = await query.CountAsync();

        if (total <= filterParams.Skip)
        {
            return new FilteringResult<T> { Data = [], Total = total };
        }

        var data = await sortMap.Apply(query, filterParams.OrderByFields, defaultOrder)
            .ThenBy(tieBreaker)
            .Skip(filterParams.Skip)
            .Take(filterParams.Take)
            .ToListAsync();

        return new FilteringResult<T> { Data = data, Total = total };
    }
}
