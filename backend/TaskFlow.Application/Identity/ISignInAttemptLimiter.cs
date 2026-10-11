namespace TaskFlow.Application.Identity;

/// <summary>
/// Counts failed sign-ins per e-mail address, on top of the limit per client IP address, so guessing the password of
/// one account from many addresses still stops after a few attempts. Unknown addresses count too, so the limit reveals
/// nothing about which addresses have an account.
/// </summary>
public interface ISignInAttemptLimiter
{
    bool IsLimited(string userName);

    void RecordFailure(string userName);

    void Reset(string userName);
}
