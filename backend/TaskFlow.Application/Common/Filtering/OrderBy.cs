using TaskFlow.Application.Common.Filtering.Enums;

namespace TaskFlow.Application.Common.Filtering;

public class OrderBy
{
    public string Field { get; set; } = string.Empty;
    public OrderDirection Dir { get; set; }
}
