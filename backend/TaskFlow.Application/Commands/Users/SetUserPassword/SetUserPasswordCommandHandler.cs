using FluentValidation;
using TaskFlow.Application.Identity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Users.SetUserPassword;

public sealed class SetUserPasswordCommandHandler(
    IRepository<UserEntity> userRepository,
    IPasswordHasher passwordHasher,
    IValidator<SetUserPasswordCommand> validator,
    IUnitOfWork unitOfWork)
{
    public async Task Execute(SetUserPasswordCommand command)
    {
        await validator.ValidateAndThrowAsync(command);

        var user = await userRepository.GetByIdAsync(command.UserId)
            ?? throw new TaskFlowNotFoundException(ErrorMessages.UserNotFound);

        user.PasswordHash = passwordHasher.Hash(command.Password);
        user.SecurityStamp = Guid.NewGuid();

        await userRepository.UpdateAsync(user);
        await unitOfWork.SaveChangesAsync();
    }
}
