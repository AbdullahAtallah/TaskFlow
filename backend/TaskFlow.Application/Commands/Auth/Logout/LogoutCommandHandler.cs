using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Auth.Logout;

public sealed class LogoutCommandHandler(IRepository<UserEntity> userRepository, IUnitOfWork unitOfWork)
{
    public async Task Execute(LogoutCommand command)
    {
        var user = await userRepository.GetByIdAsync(command.UserId)
            ?? throw new TaskFlowNotFoundException(ErrorMessages.UserNotFound);

        user.SecurityStamp = Guid.NewGuid();

        await userRepository.UpdateAsync(user);
        await unitOfWork.SaveChangesAsync();
    }
}
