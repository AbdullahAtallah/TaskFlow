namespace TaskFlow.Domain.Exceptions;

/// <summary>The signed-in user has been deactivated; the API answers 401 with this reason.</summary>
public sealed class AccountRevokedException() : TaskFlowException("The account has been revoked")
{
    public const string Reason = "account_revoked";
}
