namespace TaskFlow.Domain.Entities.Authorization;

/// <summary>A single permission, seeded from <c>ClaimTypes</c>.</summary>
public class ClaimEntity
{
    public required string Id { get; set; }
    public required string PackageId { get; set; }
    public string? FeatureId { get; set; }

    public PackageEntity? Package { get; set; }
    public FeatureEntity? Feature { get; set; }
}
