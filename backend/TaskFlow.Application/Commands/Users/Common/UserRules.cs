using FluentValidation;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Application.Identity;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Users.Common;

public static class UserRules
{
    public static IRuleBuilderOptions<T, string> UserEmailAddress<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("E-mail address is required.")
            .MaximumLength(FieldLengths.UserName)
            .WithMessage($"E-mail address cannot exceed {FieldLengths.UserName} characters.")
            .EmailAddress()
            .WithMessage("Enter a valid e-mail address.");
    }

    /// <summary>The <see cref="PasswordPolicy"/>, with a message per broken rule.</summary>
    public static IRuleBuilderOptions<T, string> Password<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("Password is required.")
            .Must(PasswordPolicy.HasAllowedLength)
            .WithMessage($"Password must be {FieldLengths.PasswordMinimum} to {FieldLengths.PasswordMaximum} characters long.")
            .Must(PasswordPolicy.HasLetterAndDigit)
            .WithMessage("Password must contain at least one letter and one digit.");
    }

    public static IRuleBuilderOptions<T, List<Guid>> RoleIds<T>(this IRuleBuilder<T, List<Guid>> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("Select at least one role.")
            .Must(roleIds => ValidationRules.BeDistinct(roleIds))
            .WithMessage("A role can only be assigned once.")
            .Must(roleIds => roleIds is null || roleIds.All(roleId => roleId != Guid.Empty))
            .WithMessage("Roles must be valid role ids.");
    }
}
