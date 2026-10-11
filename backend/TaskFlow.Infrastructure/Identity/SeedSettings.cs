namespace TaskFlow.Infrastructure.Identity;

public sealed class SeedSettings
{
    public const string SectionName = "Seed";

    /// <summary>
    /// The e-mail address of the first administrator. The seeder creates this account with the Admin role, but never
    /// promotes an existing account that has a password of its own.
    /// </summary>
    public string? AdministratorEmail { get; set; }

    /// <summary>The display name for the first administrator when the seeder creates the user.</summary>
    public string? AdministratorDisplayName { get; set; }

    /// <summary>
    /// A secret (User Secrets or Seed__AdministratorPassword). The seeder sets it only while the administrator it created
    /// has no password yet, so a password set later is never overwritten.
    /// </summary>
    public string? AdministratorPassword { get; set; }
}
