namespace TaskFlow.Domain.Entities.Authorization;

/// <summary>An optional part of a package, seeded from <c>FeatureTypes</c> and enabled by default.</summary>
public class FeatureEntity
{
    public required string Id { get; set; }
    public required string PackageId { get; set; }
    public bool IsEnabled { get; set; } = true;

    public PackageEntity? Package { get; set; }
}
