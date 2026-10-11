using FluentValidation;
using FluentValidation.Results;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Identity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Users;

namespace TaskFlow.Application.Commands.Auth.Register;

/// <summary>
/// Answers directly instead of through a notification: whoever registers has no token yet, so no notification
/// connection either. Errors reach the response as exceptions (400 with the messages per field).
/// </summary>
public sealed class RegisterCommandHandler(
    IRepository<UserEntity> userRepository,
    IUserDataReader userDataReader,
    IRoleDataReader roleDataReader,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokenIssuer,
    IValidator<RegisterCommand> validator,
    IUnitOfWork unitOfWork)
{
    public async Task<AccessTokenDto> Execute(RegisterCommand command)
    {
        await validator.ValidateAndThrowAsync(command);

        var userName = command.Email.NormalizeUserName();
        if (await userDataReader.IsUserNameInUseAsync(userName))
        {
            throw new ValidationException([new ValidationFailure(nameof(RegisterCommand.Email), ErrorMessages.UserNameAlreadyInUse)]);
        }

        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            DisplayName = command.DisplayName.Trim(),
            PasswordHash = passwordHasher.Hash(command.Password)
        };
        user.UserRoles.Add(new UserRoleEntity { UserId = user.Id, RoleId = await roleDataReader.GetRoleIdForNewUsersAsync() });

        await userRepository.AddAsync(user);
        await unitOfWork.SaveChangesAsync();

        return accessTokenIssuer.Issue(user);
    }
}
