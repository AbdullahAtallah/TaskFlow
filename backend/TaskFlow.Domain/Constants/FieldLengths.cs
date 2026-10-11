namespace TaskFlow.Domain.Constants;

/// <summary>Column sizes shared by the EF Core configuration and the validators.</summary>
public static class FieldLengths
{
    public const int UserName = 256;
    public const int DisplayName = 100;
    public const int PasswordHash = 256;

    /// <summary>Length limits of a password; the upper limit keeps hashing a hostile input cheap.</summary>
    public const int PasswordMinimum = 8;
    public const int PasswordMaximum = 128;

    public const int RoleName = 100;
    public const int RoleDescription = 500;
    public const int TeamName = 100;
    public const int ProjectName = 100;
    public const int TaskTitle = 200;
    public const int CatalogueId = 100;

    /// <summary>Upper bound the validators enforce for the NVARCHAR(MAX) description columns.</summary>
    public const int Description = 4000;
}
