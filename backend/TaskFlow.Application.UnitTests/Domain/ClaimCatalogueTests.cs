using AwesomeAssertions;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.UnitTests.Domain;

public sealed class ClaimCatalogueTests
{
    [Fact]
    public void Claims_HaveUniqueIds()
    {
        ClaimTypes.All.Select(claim => claim.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void Claims_BelongToAKnownPackage_AndStartWithItsName()
    {
        foreach (var (packageId, claimId, _) in ClaimTypes.All)
        {
            PackageTypes.All.Should().Contain(packageId);
            claimId.Should().StartWith($"{packageId}.");
        }
    }

    [Fact]
    public void FeatureClaims_BelongToAKnownFeatureOfTheirPackage_AndStartWithItsName()
    {
        foreach (var (packageId, claimId, featureId) in ClaimTypes.All.Where(claim => claim.FeatureId is not null))
        {
            FeatureTypes.All.Should().Contain((packageId, featureId!));
            claimId.Should().StartWith($"{featureId}.");
        }
    }

    [Fact]
    public void Features_BelongToAKnownPackage_AndStartWithItsName()
    {
        foreach (var (packageId, featureId) in FeatureTypes.All)
        {
            PackageTypes.All.Should().Contain(packageId);
            featureId.Should().StartWith($"{packageId}.");
        }
    }

    [Fact]
    public void GeneralAndManagement_ArePackagesWithoutClaims()
    {
        ClaimTypes.All.Should().NotContain(claim => claim.PackageId == PackageTypes.General || claim.PackageId == PackageTypes.Management);
    }

    [Fact]
    public void Catalogue_MatchesTheInternshipPlan()
    {
        ClaimTypes.All.Select(claim => claim.Id).Should().BeEquivalentTo(
            "Teams.View.All", "Teams.Manage", "Teams.Members.Manage",
            "Projects.View.All", "Projects.Create", "Projects.Manage",
            "Tasks.View.All", "Tasks.Create", "Tasks.Manage", "Tasks.Assign", "Tasks.Board.View");
        PackageTypes.All.Should().BeEquivalentTo("General", "Teams", "Projects", "Tasks", "Management");
        FeatureTypes.All.Should().Equal((PackageTypes.Tasks, "Tasks.Board"));
    }
}
