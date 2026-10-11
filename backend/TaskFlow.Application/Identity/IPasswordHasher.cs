namespace TaskFlow.Application.Identity;

/// <summary>Hashes passwords with a random salt per password and checks a password against a stored hash.</summary>
public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>
    /// Checks the password against the hash. Without a hash it does the same amount of work and fails, so the time a
    /// sign-in takes does not reveal whether the e-mail address has an account.
    /// </summary>
    PasswordCheck Verify(string? passwordHash, string password);
}
