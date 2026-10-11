using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Identity;
using TaskFlow.Domain.Entities.Users;

namespace TaskFlow.Infrastructure.Identity;

/// <summary>
/// The password hasher of ASP.NET Core Identity: PBKDF2 with HMAC-SHA512, a random salt per password and many
/// iterations, so a stolen hash is slow to crack.
/// </summary>
internal sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<UserEntity> _passwordHasher = new();

    /// <summary>A hash of a random password: checking against it costs as much as checking a real one.</summary>
    private readonly Lazy<string> _unusedHash;

    public IdentityPasswordHasher()
    {
        _unusedHash = new Lazy<string>(() => Hash(Guid.NewGuid().ToString()));
    }

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(null!, password);
    }

    public PasswordCheck Verify(string? passwordHash, string password)
    {
        if (passwordHash is null)
        {
            _passwordHasher.VerifyHashedPassword(null!, _unusedHash.Value, password);
            return PasswordCheck.Failed;
        }

        return _passwordHasher.VerifyHashedPassword(null!, passwordHash, password) switch
        {
            PasswordVerificationResult.Success => PasswordCheck.Succeeded,
            PasswordVerificationResult.SuccessRehashNeeded => PasswordCheck.SucceededRehashNeeded,
            _ => PasswordCheck.Failed
        };
    }
}
