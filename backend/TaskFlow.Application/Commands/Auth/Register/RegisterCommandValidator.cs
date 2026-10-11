using FluentValidation;
using TaskFlow.Application.Commands.Users.Common;
using TaskFlow.Application.Common.Validation;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Commands.Auth.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.Email).UserEmailAddress();

        RuleFor(command => command.DisplayName).RequiredText("Display name", FieldLengths.DisplayName);

        RuleFor(command => command.Password).Password();
    }
}
