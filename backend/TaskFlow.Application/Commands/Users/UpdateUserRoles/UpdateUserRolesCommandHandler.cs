using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Commands.Roles.Common;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Users.UpdateUserRoles;

public sealed class UpdateUserRolesCommandHandler(
    IRepository<UserEntity> userRepository,
    IRepository<UserRoleEntity> userRoleRepository,
    IRepository<RoleEntity> roleRepository,
    ManagementAccessGuard managementAccessGuard,
    IValidator<UpdateUserRolesCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<UpdateUserRolesCommandHandler> logger)
{
    public async Task Handle(UpdateUserRolesCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var user = await userRepository.GetByIdAsync(command.UserId)
                ?? throw new TaskFlowNotFoundException(ErrorMessages.UserNotFound);

            await EnsureRolesExistAsync(command.RoleIds);

            if (user.Id == command.UserCommandContext.Id)
            {
                await managementAccessGuard.EnsureRolesGrantManagementAsync(command.RoleIds);
            }

            await ReplaceRolesAsync(user.Id, command.RoleIds);

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.User, ActionType.Modified, user.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.User, ActionType.Modified, exception, command.UserId);
        }
    }

    private async Task EnsureRolesExistAsync(List<Guid> roleIds)
    {
        var roles = await roleRepository.FindAsync(role => roleIds.Contains(role.Id));

        if (roles.Count != roleIds.Count)
        {
            throw new TaskFlowNotFoundException(ErrorMessages.RolesNotFound);
        }
    }

    private async Task ReplaceRolesAsync(Guid userId, List<Guid> roleIds)
    {
        var currentUserRoles = await userRoleRepository.FindAsync(userRole => userRole.UserId == userId);
        userRoleRepository.DeleteRange(currentUserRoles.Where(userRole => !roleIds.Contains(userRole.RoleId)));

        foreach (var roleId in roleIds.Except(currentUserRoles.Select(userRole => userRole.RoleId)))
        {
            await userRoleRepository.AddAsync(new UserRoleEntity { UserId = userId, RoleId = roleId });
        }

        await unitOfWork.SaveChangesAsync();
    }
}
