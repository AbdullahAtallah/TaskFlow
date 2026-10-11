using FluentValidation;
using TaskFlow.Application.Commands.Users.Common;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Users.SetUserPassword;

public sealed class SetUserPasswordCommandValidator : AbstractValidator<SetUserPasswordCommand>
{
    public SetUserPasswordCommandValidator()
    {
        RuleFor(command => command.UserId).RequiredId("User");

        RuleFor(command => command.Password).Password();
    }
}
