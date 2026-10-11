using FluentValidation;

namespace TaskFlow.Application.Common.Filtering;

/// <summary>Shared rules for paging, search and sorting; each grid query passes the fields it can sort on.</summary>
public abstract class FilterParamsValidator<T> : AbstractValidator<T> where T : FilterParams
{
    protected FilterParamsValidator(IReadOnlyCollection<string> sortableFields)
    {
        RuleFor(query => query.Skip)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Skip cannot be negative.");

        RuleFor(query => query.Take)
            .InclusiveBetween(1, FilterLimits.MaxTake)
            .WithMessage($"Take must be between 1 and {FilterLimits.MaxTake}.");

        RuleFor(query => query.Query)
            .MaximumLength(FilterLimits.MaxQueryLength)
            .WithMessage($"The search text cannot exceed {FilterLimits.MaxQueryLength} characters.");

        RuleFor(query => query.OrderByFields)
            .Must(orderByFields => orderByFields is null || orderByFields.Count <= FilterLimits.MaxOrderByFields)
            .WithMessage($"Sort on at most {FilterLimits.MaxOrderByFields} fields.");

        RuleForEach(query => query.OrderByFields).ChildRules(orderBy =>
        {
            orderBy.RuleFor(o => o.Field)
                .Must(field => sortableFields.Contains(field, StringComparer.OrdinalIgnoreCase))
                .WithMessage(o => $"Sorting on '{o.Field}' is not supported. Supported fields: {string.Join(", ", sortableFields)}.");

            orderBy.RuleFor(o => o.Dir)
                .IsInEnum()
                .WithMessage("The sort direction must be Asc or Desc.");
        });
    }
}
