namespace TaskFlow.Application.Common.Extensions;

public static class StringExtensions
{
    public static string? TrimToNull(this string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>User names are e-mail addresses, stored trimmed and in lower case.</summary>
    public static string NormalizeUserName(this string userName)
    {
        return userName.Trim().ToLowerInvariant();
    }
}
