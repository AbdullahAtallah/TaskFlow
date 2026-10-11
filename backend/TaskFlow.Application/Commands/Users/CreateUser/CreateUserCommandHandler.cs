using FluentValidation;
using FluentValidation.Results;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Identity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Authorization;
using TaskFlow.Domain.Entities.Users;

namespace TaskFlow.Application.Commands.Users.CreateUser;

/// <summary>Answers with the id of the new user; errors reach the response as exceptions (400 with the messages per field).</summary>
public sealed class CreateUserCommandHandler(
    IRepository<UserEntity> userRepository,
    IRepository<RoleEntity> roleRepository,
    IUserDataReader userDataReader,
    IPasswordHasher passwordHasher,
    IValidator<CreateUserCommand> validator,
    IUnitOfWork unitOfWork)
{
    public async Task<Guid> Execute(CreateUserCommand command)
    {
        await validator.ValidateAndThrowAsync(command);

        var userName = command.UserName.NormalizeUserName();
        if (await userDataReader.IsUserNameInUseAsync(userName))
        {
            throw new ValidationException([new ValidationFailure(nameof(CreateUserCommand.UserName), ErrorMessages.UserNameAlreadyInUse)]);
        }

        await EnsureRolesExistAsync(command.RoleIds);

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            DisplayName = command.DisplayName.Trim(),
            PasswordHash = passwordHasher.Hash(command.Password)
        };

        foreach (var roleId in command.RoleIds)
        {
            user.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = roleId });
        }

        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        return user.Id;
    }

    private async Task EnsureRolesExistAsync(List<Guid> roleIds)
    {
        var roles = await roleRepository.FindAsync(role => roleIds.Contains(role.Id));

        if (roles.Count != roleIds.Count)
        {
            throw new ValidationException([new ValidationFailure(nameof(CreateUserCommand.RoleIds), ErrorMessages.RolesNotFound)]);
        }
    }
}
