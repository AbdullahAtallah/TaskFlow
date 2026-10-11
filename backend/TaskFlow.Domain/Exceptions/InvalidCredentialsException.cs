using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.Exceptions;

/// <summary>
/// Sign-in with an unknown e-mail address or a wrong password; both get the same answer, 401 with this reason, so the
/// response does not reveal which addresses have an account.
/// </summary>
public sealed class InvalidCredentialsException() : TaskFlowException(ErrorMessages.InvalidCredentials)
{
    public const string Reason = "invalid_credentials";
}
