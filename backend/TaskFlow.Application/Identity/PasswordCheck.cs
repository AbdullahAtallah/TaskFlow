namespace TaskFlow.Application.Identity;

public enum PasswordCheck
{
    Failed,
    Succeeded,

    /// <summary>The password is right, but the hash uses older settings; store a new hash.</summary>
    SucceededRehashNeeded
}
