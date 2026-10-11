using AwesomeAssertions;
using TaskFlow.Domain.Constants;

namespace TaskFlow.Application.UnitTests.Domain;

public sealed class DefaultRoleBundlesTests
{
    [Fact]
    public void Admin_HasEveryPackageAndClaim()
    {
        DefaultRoleBundles.Admin.PackageIds.Should().BeEquivalentTo(PackageTypes.All);
        DefaultRoleBundles.Admin.ClaimIds.Should().BeEquivalentTo(ClaimTypes.All.Select(claim => claim.Id));
    }

    [Fact]
    public void User_HasEveryPackageButManagement_AndNoTeamAdministrationOrViewAllClaims()
    {
        DefaultRoleBundles.User.PackageIds.Should().BeEquivalentTo(
            PackageTypes.General, PackageTypes.Teams, PackageTypes.Projects, PackageTypes.Tasks);
        DefaultRoleBundles.User.ClaimIds.Should().BeEquivalentTo(
            ClaimTypes.ProjectsCreate, ClaimTypes.ProjectsManage,
            ClaimTypes.TasksCreate, ClaimTypes.TasksManage, ClaimTypes.TasksAssign, ClaimTypes.TasksBoardView);
    }

    [Fact]
    public void ReadOnlyUser_HasOnlyViewClaims_AndNoManagement()
    {
        DefaultRoleBundles.ReadOnlyUser.PackageIds.Should().NotContain(PackageTypes.Management);
        DefaultRoleBundles.ReadOnlyUser.ClaimIds.Should().BeEquivalentTo(
            ClaimTypes.TeamsViewAll, ClaimTypes.ProjectsViewAll, ClaimTypes.TasksViewAll, ClaimTypes.TasksBoardView);
    }

    [Fact]
    public void ExactlyTheUserRole_IsTheRoleForNewUsers()
    {
        DefaultRoleBundles.All.Where(bundle => bundle.IsDefaultForNewUsers)
            .Should().ContainSingle()
            .Which.RoleName.Should().Be(DefaultRoleBundles.UserRoleName);
    }

    [Fact]
    public void EveryBundle_GrantsTheGeneralPackage()
    {
        DefaultRoleBundles.All.Should().OnlyContain(bundle => bundle.PackageIds.Contains(PackageTypes.General));
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("user", true)]
    [InlineData("READONLYUSER", true)]
    [InlineData("Contractors", false)]
    [InlineData(null, false)]
    public void IsDefaultRoleName_IgnoresCase(string? roleName, bool expected)
    {
        DefaultRoleBundles.IsDefaultRoleName(roleName).Should().Be(expected);
    }
}
