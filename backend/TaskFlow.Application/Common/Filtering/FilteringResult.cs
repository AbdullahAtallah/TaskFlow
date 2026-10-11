namespace TaskFlow.Application.Common.Filtering;

/// <summary>One page of a grid plus the number of records that match the filters.</summary>
public class FilteringResult<T>
{
    public IReadOnlyList<T> Data { get; init; } = [];
    public int Total { get; init; }
}
