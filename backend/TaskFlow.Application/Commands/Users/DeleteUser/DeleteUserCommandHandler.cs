using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Entities.Teams;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Users.DeleteUser;

public sealed class DeleteUserCommandHandler(
    IRepository<UserEntity> userRepository,
    IRepository<UserRoleEntity> userRoleRepository,
    IRepository<TeamMemberEntity> teamMemberRepository,
    IRepository<TaskEntity> taskRepository,
    IValidator<DeleteUserCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<DeleteUserCommandHandler> logger)
{
    public async Task Handle(DeleteUserCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            if (command.Id == command.UserCommandContext.Id)
            {
                throw new TaskFlowException(ErrorMessages.CannotDeleteOwnAccount);
            }

            var user = await userRepository.GetByIdAsync(command.Id)
                ?? throw new TaskFlowNotFoundException(ErrorMessages.UserNotFound);

            await DeleteUserAsync(user);

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.User, ActionType.Deleted, user.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.User, ActionType.Deleted, exception, command.Id);
        }
    }

    private async Task DeleteUserAsync(UserEntity user)
    {
        userRoleRepository.DeleteRange(await userRoleRepository.FindAsync(userRole => userRole.UserId == user.Id));
        teamMemberRepository.DeleteRange(await teamMemberRepository.FindAsync(member => member.UserId == user.Id));

        var assignedTasks = await taskRepository.FindAsync(task => task.AssigneeId == user.Id);
        foreach (var task in assignedTasks)
        {
            task.AssigneeId = null;
        }

        await userRepository.DeleteAsync(user);
        await unitOfWork.SaveChangesAsync();
    }
}
