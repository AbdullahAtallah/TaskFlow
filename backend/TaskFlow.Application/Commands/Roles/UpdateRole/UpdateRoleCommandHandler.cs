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

namespace TaskFlow.Application.Commands.Roles.UpdateRole;

public sealed class UpdateRoleCommandHandler(
    IRepository<RoleEntity> roleRepository,
    IRepository<UserRoleEntity> userRoleRepository,
    RolePermissionsUpdater rolePermissionsUpdater,
    ManagementAccessGuard managementAccessGuard,
    IValidator<UpdateRoleCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<UpdateRoleCommandHandler> logger)
{
    public async Task Handle(UpdateRoleCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var role = await roleRepository.GetByIdAsync(command.Id)
                ?? throw new TaskFlowNotFoundException(ErrorMessages.RoleNotFound);

            EnsureRoleForNewUsersCannotManage(role, command);
            await EnsureSenderKeepsManagementAccessAsync(command, role);

            if (await rolePermissionsUpdater.ReplaceAsync(role.Id, command.Packages, command.Claims))
            {
                role.DateTimeModified = DateTime.UtcNow;
                await roleRepository.UpdateAsync(role);
            }

            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Role, ActionType.Modified, role.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Role, ActionType.Modified, exception, command.Id);
        }
    }

    /// <summary>Everyone who registers gets the role for new users, so it must never open user and role management.</summary>
    private static void EnsureRoleForNewUsersCannotManage(RoleEntity role, UpdateRoleCommand command)
    {
        if (role.IsDefault && command.Packages.Contains(PackageTypes.Management))
        {
            throw new TaskFlowException(ErrorMessages.RoleForNewUsersCannotGrantManagement);
        }
    }

    private async Task EnsureSenderKeepsManagementAccessAsync(UpdateRoleCommand command, RoleEntity role)
    {
        var senderId = command.UserCommandContext.Id;

        if (command.Packages.Contains(PackageTypes.Management))
        {
            return;
        }

        var senderRoleIds = (await userRoleRepository.FindAsync(userRole => userRole.UserId == senderId))
            .Select(userRole => userRole.RoleId)
            .ToList();

        if (!senderRoleIds.Contains(role.Id))
        {
            return;
        }

        await managementAccessGuard.EnsureRolesGrantManagementAsync(senderRoleIds.Where(roleId => roleId != role.Id).ToList());
    }
}
