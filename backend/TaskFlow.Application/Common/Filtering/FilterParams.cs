namespace TaskFlow.Application.Common.Filtering;

/// <summary>
/// Server-side search, sorting and paging of a grid, bound from the query string:
/// <c>?query=&amp;skip=&amp;take=&amp;orderByFields[0].field=&amp;orderByFields[0].dir=</c>.
/// </summary>
public class FilterParams
{
    public const int DefaultTake = 20;

    public string? Query { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; } = DefaultTake;
    public List<OrderBy> OrderByFields { get; set; } = [];
}
