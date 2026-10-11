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
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Roles.CreateRole;

public sealed class CreateRoleCommandHandler(
    IRepository<RoleEntity> roleRepository,
    IRoleDataReader roleDataReader,
    RolePermissionsUpdater rolePermissionsUpdater,
    IValidator<CreateRoleCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<CreateRoleCommandHandler> logger)
{
    public async Task Handle(CreateRoleCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var name = command.Name.Trim();
            if (await roleDataReader.IsNameInUseAsync(name, excludeRoleId: null))
            {
                throw new TaskFlowException(ErrorMessages.RoleNameAlreadyInUse);
            }

            var role = new RoleEntity
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = command.Description.TrimToNull()
            };

            await roleRepository.AddAsync(role);

            if (command.IsDefault)
            {
                await rolePermissionsUpdater.MakeDefaultAsync(role);
            }

            await rolePermissionsUpdater.ReplaceAsync(role.Id, command.Packages, command.Claims);
            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Role, ActionType.Created, role.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Role, ActionType.Created, exception);
        }
    }
}
