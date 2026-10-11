using FluentValidation;

namespace TaskFlow.Application.Common.Validation;

/// <summary>Rules that most commands share, with messages that can be shown to the user as they are.</summary>
public static class ValidationRules
{
    public static IRuleBuilderOptions<T, string> RequiredText<T>(this IRuleBuilder<T, string> ruleBuilder, string fieldName, int maxLength)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage($"{fieldName} is required.")
            .MaximumLength(maxLength)
            .WithMessage($"{fieldName} cannot exceed {maxLength} characters.");
    }

    public static IRuleBuilderOptions<T, string?> OptionalText<T>(this IRuleBuilder<T, string?> ruleBuilder, string fieldName, int maxLength)
    {
        return ruleBuilder
            .MaximumLength(maxLength)
            .WithMessage($"{fieldName} cannot exceed {maxLength} characters.");
    }

    public static IRuleBuilderOptions<T, Guid> RequiredId<T>(this IRuleBuilder<T, Guid> ruleBuilder, string fieldName)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage($"{fieldName} is required.");
    }

    /// <summary>The version of the record that the edit form showed, as it came with the record.</summary>
    public static IRuleBuilderOptions<T, byte[]> RequiredRowVersion<T>(this IRuleBuilder<T, byte[]> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("The version of the record is missing. Reload it and try again.");
    }

    public static bool BeDistinct<TItem>(IReadOnlyCollection<TItem>? values)
    {
        return values is null || values.Distinct().Count() == values.Count;
    }
}
