namespace TaskFlow.Infrastructure.Identity;

/// <summary>The limits that slow down password guessing (the RateLimiting section).</summary>
public sealed class RateLimitingSettings
{
    public const string SectionName = "RateLimiting";

    /// <summary>Register and login requests allowed per client IP address in each window.</summary>
    public int SignInPermitLimit { get; set; } = 10;

    public TimeSpan SignInWindow { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>Failed sign-ins allowed per e-mail address, from any number of clients, before that address is refused.</summary>
    public int FailedSignInsPerAddress { get; set; } = 5;

    /// <summary>How long the failed sign-ins of an address count, starting at the first one.</summary>
    public TimeSpan FailedSignInWindow { get; set; } = TimeSpan.FromMinutes(15);
}
