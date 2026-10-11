namespace TaskFlow.Domain.Entities.Authorization;

/// <summary>Junction row granting a package to a role; hard-deleted when removed.</summary>
public class RolePackageEntity
{
    public Guid RoleId { get; set; }
    public required string PackageId { get; set; }

    public RoleEntity? Role { get; set; }
    public PackageEntity? Package { get; set; }
}
