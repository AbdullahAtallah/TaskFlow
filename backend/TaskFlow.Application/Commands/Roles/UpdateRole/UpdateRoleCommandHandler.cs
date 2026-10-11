using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Commands.Roles.Common;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Roles.UpdateRole;

public sealed class UpdateRoleCommandHandler(
    IRepository<RoleEntity> roleRepository,
    IRepository<UserRoleEntity> userRoleRepository,
    IRoleDataReader roleDataReader,
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

            role.EnsureUnchangedSince(command.RowVersion);

            var name = command.Name.Trim();
            await EnsureNameCanBeUsedAsync(role, name);
            EnsureRoleForNewUsersRemains(role, command);

            var assignedUserIds = (await userRoleRepository.FindAsync(userRole => userRole.RoleId == role.Id))
                .Select(userRole => userRole.UserId)
                .ToList();
            await EnsureSenderKeepsManagementAccessAsync(command, role, assignedUserIds);

            await UpdateRoleAsync(role, name, command);

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Role, ActionType.Modified, role.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Role, ActionType.Modified, exception, command.Id);
        }
    }

    private async Task EnsureNameCanBeUsedAsync(RoleEntity role, string name)
    {
        if (DefaultRoleBundles.IsDefaultRoleName(role.Name) && name != role.Name)
        {
            throw new TaskFlowException(ErrorMessages.DefaultRoleCannotBeRenamed);
        }

        if (await roleDataReader.IsNameInUseAsync(name, role.Id))
        {
            throw new TaskFlowException(ErrorMessages.RoleNameAlreadyInUse);
        }
    }

    private static void EnsureRoleForNewUsersRemains(RoleEntity role, UpdateRoleCommand command)
    {
        if (role.IsDefault && !command.IsDefault)
        {
            throw new TaskFlowException(ErrorMessages.RoleForNewUsersRequired);
        }
    }

    private async Task EnsureSenderKeepsManagementAccessAsync(UpdateRoleCommand command, RoleEntity role, List<Guid> assignedUserIds)
    {
        var senderId = command.UserCommandContext.Id;

        if (!assignedUserIds.Contains(senderId) || command.Packages.Contains(PackageTypes.Management))
        {
            return;
        }

        var senderOtherRoleIds = (await userRoleRepository.FindAsync(userRole => userRole.UserId == senderId && userRole.RoleId != role.Id))
            .Select(userRole => userRole.RoleId)
            .ToList();

        await managementAccessGuard.EnsureRolesGrantManagementAsync(senderOtherRoleIds);
    }

    private async Task UpdateRoleAsync(RoleEntity role, string name, UpdateRoleCommand command)
    {
        role.Name = name;
        role.Description = command.Description.TrimToNull();

        if (command.IsDefault && !role.IsDefault)
        {
            await rolePermissionsUpdater.MakeDefaultAsync(role);
        }

        if (await rolePermissionsUpdater.ReplaceAsync(role.Id, command.Packages, command.Claims))
        {
            // The grants are rows of their own; touching the role gives it a new row version, so a form that still
            // shows the old grants cannot overwrite these.
            role.DateTimeModified = DateTime.UtcNow;
        }

        await roleRepository.UpdateAsync(role);
        await unitOfWork.SaveChangesAsync();
    }
}
