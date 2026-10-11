using FluentValidation;
using TaskFlow.Application.Common.Validation;

namespace TaskFlow.Application.Commands.Users.DeleteUser;

public sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(command => command.Id).RequiredId("User");
    }
}
