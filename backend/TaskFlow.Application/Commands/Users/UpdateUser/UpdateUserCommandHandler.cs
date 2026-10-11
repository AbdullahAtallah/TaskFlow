using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Users.UpdateUser;

public sealed class UpdateUserCommandHandler(
    IRepository<UserEntity> userRepository,
    IValidator<UpdateUserCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<UpdateUserCommandHandler> logger)
{
    public async Task Handle(UpdateUserCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var user = await userRepository.GetByIdAsync(command.Id)
                ?? throw new TaskFlowNotFoundException(ErrorMessages.UserNotFound);

            user.EnsureUnchangedSince(command.RowVersion);

            if (!command.IsActive && user.Id == command.UserCommandContext.Id)
            {
                throw new TaskFlowException(ErrorMessages.CannotDeactivateOwnAccount);
            }

            if (user.IsActive && !command.IsActive)
            {
                // Ends the user's sessions too, so reactivating them later does not bring an old token back to life.
                user.SecurityStamp = Guid.NewGuid();
            }

            user.DisplayName = command.DisplayName.Trim();
            user.IsActive = command.IsActive;

            await userRepository.UpdateAsync(user);
            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.User, ActionType.Modified, user.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.User, ActionType.Modified, exception, command.Id);
        }
    }
}
