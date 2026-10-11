namespace TaskFlow.Domain.Entities.Authorization;

/// <summary>Junction row granting a claim to a role; hard-deleted when removed.</summary>
public class RoleClaimEntity
{
    public Guid RoleId { get; set; }
    public required string ClaimId { get; set; }

    public RoleEntity? Role { get; set; }
    public ClaimEntity? Claim { get; set; }
}
