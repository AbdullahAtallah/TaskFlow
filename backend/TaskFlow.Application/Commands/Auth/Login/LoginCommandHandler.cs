using FluentValidation;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Identity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Auth.Login;

/// <summary>
/// An unknown address and a wrong password get the same answer (401 invalid_credentials) and take the same time, so
/// the response does not reveal which addresses have an account. A deactivated account gets 401 account_revoked, and
/// an address with too many failed sign-ins 429 too_many_attempts.
/// </summary>
public sealed class LoginCommandHandler(
    IRepository<UserEntity> userRepository,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokenIssuer,
    ISignInAttemptLimiter signInAttemptLimiter,
    IValidator<LoginCommand> validator,
    IUnitOfWork unitOfWork)
{
    public async Task<AccessTokenDto> Execute(LoginCommand command)
    {
        await validator.ValidateAndThrowAsync(command);

        var userName = command.Email.NormalizeUserName();
        if (signInAttemptLimiter.IsLimited(userName))
        {
            throw new TooManySignInAttemptsException();
        }

        var user = (await userRepository.FindAsync(candidate => candidate.UserName == userName)).SingleOrDefault();

        var passwordCheck = passwordHasher.Verify(user?.PasswordHash, command.Password);
        if (user is null || passwordCheck == PasswordCheck.Failed)
        {
            signInAttemptLimiter.RecordFailure(userName);
            throw new InvalidCredentialsException();
        }

        signInAttemptLimiter.Reset(userName);

        if (!user.IsActive)
        {
            throw new AccountRevokedException();
        }

        if (passwordCheck == PasswordCheck.SucceededRehashNeeded)
        {
            user.PasswordHash = passwordHasher.Hash(command.Password);
            await unitOfWork.SaveChangesAsync();
        }

        return accessTokenIssuer.Issue(user);
    }
}
