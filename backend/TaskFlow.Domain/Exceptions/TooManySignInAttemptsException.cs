using TaskFlow.Domain.Constants;

namespace TaskFlow.Domain.Exceptions;

/// <summary>Too many failed sign-ins for one e-mail address; the API answers 429 with this reason.</summary>
public sealed class TooManySignInAttemptsException() : TaskFlowException(ErrorMessages.TooManySignInAttempts)
{
    public const string Reason = "too_many_attempts";
}
