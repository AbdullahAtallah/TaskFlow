namespace TaskFlow.Infrastructure.Helper;

internal static class StringHelper
{
    /// <summary>A LIKE pattern that matches the text anywhere; wildcards typed by the user are matched literally.</summary>
    public static string ToContainsPattern(string text)
    {
        return $"%{EscapeLikePattern(text.Trim())}%";
    }

    private static string EscapeLikePattern(string pattern)
    {
        return pattern
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
    }
}
