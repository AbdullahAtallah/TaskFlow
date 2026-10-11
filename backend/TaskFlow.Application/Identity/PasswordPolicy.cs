using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.Identity;

/// <summary>A password has 8 to 128 characters with at least one letter and one digit.</summary>
public static class PasswordPolicy
{
    public static bool HasAllowedLength(string password)
    {
        return password.Length is >= FieldLengths.PasswordMinimum and <= FieldLengths.PasswordMaximum;
    }

    public static bool HasLetterAndDigit(string password)
    {
        return password.Any(char.IsLetter) && password.Any(char.IsDigit);
    }

    public static bool IsSatisfiedBy(string password)
    {
        return HasAllowedLength(password) && HasLetterAndDigit(password);
    }
}
