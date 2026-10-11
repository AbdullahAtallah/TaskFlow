using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Roles.DeleteRole;

public sealed class DeleteRoleCommandHandler(
    IRepository<RoleEntity> roleRepository,
    IRepository<RolePackageEntity> rolePackageRepository,
    IRepository<RoleClaimEntity> roleClaimRepository,
    IRoleDataReader roleDataReader,
    IValidator<DeleteRoleCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<DeleteRoleCommandHandler> logger)
{
    public async Task Handle(DeleteRoleCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var role = await roleRepository.GetByIdAsync(command.Id)
                ?? throw new TaskFlowNotFoundException(ErrorMessages.RoleNotFound);

            await EnsureRoleCanBeDeletedAsync(role);

            await DeleteRoleAsync(role);

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Role, ActionType.Deleted, role.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Role, ActionType.Deleted, exception, command.Id);
        }
    }

    private async Task EnsureRoleCanBeDeletedAsync(RoleEntity role)
    {
        if (DefaultRoleBundles.IsDefaultRoleName(role.Name))
        {
            throw new TaskFlowException(ErrorMessages.DefaultRoleCannotBeDeleted);
        }

        if (role.IsDefault)
        {
            throw new TaskFlowException(ErrorMessages.RoleForNewUsersCannotBeDeleted);
        }

        if (await roleDataReader.HasUsersAsync(role.Id))
        {
            throw new TaskFlowException(ErrorMessages.RoleStillAssigned);
        }
    }

    private async Task DeleteRoleAsync(RoleEntity role)
    {
        rolePackageRepository.DeleteRange(await rolePackageRepository.FindAsync(rolePackage => rolePackage.RoleId == role.Id));
        roleClaimRepository.DeleteRange(await roleClaimRepository.FindAsync(roleClaim => roleClaim.RoleId == role.Id));

        await roleRepository.DeleteAsync(role);
        await unitOfWork.SaveChangesAsync();
    }
}
