using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using TaskFlow.Application.Identity;
using TaskFlow.Application.Queries.Users;
using TaskFlow.Domain.Constants;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Authentication;

/// <summary>Register, login and logout, and the passwords that administrators set.</summary>
public sealed class AuthTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    private const string Password = "Correct-horse-1";
    private const string CurrentUserUrl = "/api/user/current";

    [Fact]
    public async Task Register_CreatesAnAccountWithTheDefaultRole_AndSignsItIn()
    {
        var response = await RegisterAsync("  Nina.Newcomer@TaskFlow.test ", "  Nina Newcomer ", Password);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should().Be(CurrentUserUrl);
        var accessToken = await ReadAccessTokenAsync(response);
        accessToken.TokenType.Should().Be("Bearer");
        accessToken.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(8), TimeSpan.FromMinutes(1));
        new JsonWebToken(accessToken.AccessToken).TryGetPayloadValue<long>("nbf", out _)
            .Should().BeFalse("an instance whose clock runs a little behind must still accept a fresh token");

        var currentUser = await ReadCurrentUserAsync(accessToken.AccessToken);
        currentUser.UserName.Should().Be("nina.newcomer@taskflow.test");
        currentUser.DisplayName.Should().Be("Nina Newcomer");
        currentUser.Roles.Select(role => role.Name).Should().Equal(DefaultRoleBundles.UserRoleName);
        currentUser.Claims.Should().BeEquivalentTo(DefaultRoleBundles.User.ClaimIds);
    }

    [Fact]
    public async Task Register_StoresASaltedHash_NeverThePassword()
    {
        await RegisterAsync("hash.one@taskflow.test", "Hash One", Password);
        await RegisterAsync("hash.two@taskflow.test", "Hash Two", Password);

        var hashes = await Data.QueryAsync(dbContext => dbContext.Users
            .Where(user => user.UserName.StartsWith("hash."))
            .Select(user => user.PasswordHash!)
            .ToListAsync());

        hashes.Should().HaveCount(2).And.OnlyContain(hash => !hash.Contains(Password));
        hashes[0].Should().NotBe(hashes[1], "every password gets a salt of its own");
    }

    [Fact]
    public async Task Register_WithAnEmailAddressInUse_Returns400_WhateverTheCase()
    {
        await Data.UserInRolesAsync([DefaultRoleBundles.UserRoleName], email: "taken@taskflow.test");

        var response = await RegisterAsync("TAKEN@taskflow.test", "Someone", Password);

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        (await response.ReadValidationErrorsAsync())["Email"].Should().Equal(ErrorMessages.UserNameAlreadyInUse);
    }

    [Theory]
    [InlineData("", "Password is required.")]
    [InlineData("short1", "Password must be 8 to 128 characters long.")]
    [InlineData("only-letters", "Password must contain at least one letter and one digit.")]
    [InlineData("1234567890", "Password must contain at least one letter and one digit.")]
    public async Task Register_WithAPasswordThatBreaksThePolicy_Returns400WithTheReason(string password, string message)
    {
        var response = await RegisterAsync("policy@taskflow.test", "Policy", password);

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        (await response.ReadValidationErrorsAsync())["Password"].Should().Equal(message);
        (await Data.QueryAsync(dbContext => dbContext.Users.AnyAsync(user => user.UserName == "policy@taskflow.test"))).Should().BeFalse();
    }

    [Fact]
    public async Task Register_WithAnInvalidEmailAddressOrNoName_Returns400()
    {
        var response = await RegisterAsync("not-an-address", " ", Password);

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        var errors = await response.ReadValidationErrorsAsync();
        errors["Email"].Should().Equal("Enter a valid e-mail address.");
        errors["DisplayName"].Should().Equal("Display name is required.");
    }

    [Fact]
    public async Task Register_WhenTwoRolesAreMarkedAsDefault_GetsTheFirstByName()
    {
        var auditor = await Data.RoleAsync([PackageTypes.Tasks], [], name: "Auditor");
        await Data.ExecuteAsync(dbContext => dbContext.Roles
            .Where(role => role.Id == auditor.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(role => role.IsDefault, true)));

        var accessToken = await ReadAccessTokenAsync(await RegisterAsync("two.defaults@taskflow.test", "Two Defaults", Password));

        (await ReadCurrentUserAsync(accessToken.AccessToken)).Roles.Select(role => role.Name).Should().Equal("Auditor");
    }

    [Fact]
    public async Task Register_WhenNoRoleIsMarkedAsDefault_GetsTheBuiltInUserRole()
    {
        await Data.ExecuteAsync(dbContext => dbContext.Roles.ExecuteUpdateAsync(setters => setters.SetProperty(role => role.IsDefault, false)));

        var accessToken = await ReadAccessTokenAsync(await RegisterAsync("no.default@taskflow.test", "No Default", Password));

        (await ReadCurrentUserAsync(accessToken.AccessToken)).Roles.Select(role => role.Name).Should().Equal(DefaultRoleBundles.UserRoleName);
    }

    [Fact]
    public async Task Login_WithTheRightPassword_ReturnsAWorkingToken_WhateverTheCaseOfTheAddress()
    {
        var user = await Data.UserWithPasswordAsync(Password);

        var response = await LoginAsync(user.UserName.ToUpperInvariant(), Password);

        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);
        var currentUser = await ReadCurrentUserAsync((await ReadAccessTokenAsync(response)).AccessToken);
        currentUser.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Login_WithAWrongPassword_OrAnUnknownAddress_GetsTheSameAnswer()
    {
        var user = await Data.UserWithPasswordAsync(Password);

        var wrongPassword = await LoginAsync(user.UserName, "Wrong-password-1");
        var unknownAddress = await LoginAsync("nobody@taskflow.test", Password);

        await wrongPassword.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        await unknownAddress.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        (await wrongPassword.ReadErrorAsync()).Should().Be("invalid_credentials");
        (await unknownAddress.ReadErrorAsync()).Should().Be("invalid_credentials");
    }

    [Fact]
    public async Task Login_AfterFiveFailedAttempts_IsRefusedForThatAddress_EvenWithTheRightPassword()
    {
        var user = await Data.UserWithPasswordAsync(Password);
        var bystander = await Data.UserWithPasswordAsync(Password);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await (await LoginAsync(user.UserName, "Wrong-password-1")).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        }

        var refused = await LoginAsync(user.UserName, Password);

        await refused.ShouldHaveStatusAsync(HttpStatusCode.TooManyRequests);
        (await refused.ReadErrorAsync()).Should().Be("too_many_attempts");
        await (await LoginAsync(bystander.UserName, Password)).ShouldHaveStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ThatSucceeds_ClearsTheFailedAttemptsOfTheAddress()
    {
        var user = await Data.UserWithPasswordAsync(Password);

        for (var attempt = 0; attempt < 4; attempt++)
        {
            await LoginAsync(user.UserName, "Wrong-password-1");
        }

        await (await LoginAsync(user.UserName, Password)).ShouldHaveStatusAsync(HttpStatusCode.OK);
        await LoginAsync(user.UserName, "Wrong-password-1");
        await (await LoginAsync(user.UserName, Password)).ShouldHaveStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_OfAnAccountWithoutAPassword_Returns401InvalidCredentials()
    {
        var response = await LoginAsync(AdminEmail, Password);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        (await response.ReadErrorAsync()).Should().Be("invalid_credentials");
    }

    [Fact]
    public async Task Login_OfADeactivatedAccount_Returns401AccountRevoked()
    {
        var user = await Data.UserWithPasswordAsync(Password, isActive: false);

        var response = await LoginAsync(user.UserName, Password);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        (await response.ReadErrorAsync()).Should().Be("account_revoked");
    }

    [Fact]
    public async Task Login_WithEmptyFields_Returns400()
    {
        var response = await LoginAsync("", "");

        await response.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        (await response.ReadValidationErrorsAsync()).Keys.Should().BeEquivalentTo("Email", "Password");
    }

    [Fact]
    public async Task Logout_EndsEverySessionOfTheUser()
    {
        var user = await Data.UserWithPasswordAsync(Password);
        var firstToken = await TokenFromLoginAsync(user.UserName);
        var secondToken = await TokenFromLoginAsync(user.UserName);

        var logout = await ClientWithToken(firstToken).PostAsync("/api/auth/logout", null);

        await logout.ShouldHaveStatusAsync(HttpStatusCode.NoContent);
        await (await ClientWithToken(firstToken).GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        await (await ClientWithToken(secondToken).GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        (await ReadCurrentUserAsync(await TokenFromLoginAsync(user.UserName))).Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task SetPassword_ByAnAdministrator_EndsTheUsersSessions_AndOnlyTheNewPasswordWorks()
    {
        var user = await Data.UserWithPasswordAsync(Password);
        var oldToken = await TokenFromLoginAsync(user.UserName);

        var response = await SendAsync(AdminEmail, HttpMethod.Put, $"/api/user/{user.Id}/password", new { password = "Brand-new-2" });

        await response.ShouldHaveStatusAsync(HttpStatusCode.NoContent);
        await (await ClientWithToken(oldToken).GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        await (await LoginAsync(user.UserName, Password)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        await (await LoginAsync(user.UserName, "Brand-new-2")).ShouldHaveStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetPassword_ThatBreaksThePolicy_Returns400_AndOfAnUnknownUser_Returns404()
    {
        var user = await Data.UserWithPasswordAsync(Password);

        var weak = await SendAsync(AdminEmail, HttpMethod.Put, $"/api/user/{user.Id}/password", new { password = "weak" });
        var unknown = await SendAsync(AdminEmail, HttpMethod.Put, $"/api/user/{Guid.NewGuid()}/password", new { password = "Brand-new-2" });

        await weak.ShouldHaveStatusAsync(HttpStatusCode.BadRequest);
        await unknown.ShouldHaveStatusAsync(HttpStatusCode.NotFound);
        await (await LoginAsync(user.UserName, Password)).ShouldHaveStatusAsync(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UserCreatedByAnAdministrator_SignsInWithTheInitialPassword()
    {
        var readOnlyRole = await Data.RoleNamedAsync(DefaultRoleBundles.ReadOnlyUserRoleName);

        var response = await SendAsync(AdminEmail, HttpMethod.Post, "/api/user",
            new { userName = "Created.User@taskflow.test", displayName = "Created User", password = Password, roleIds = new[] { readOnlyRole.Id } });

        await response.ShouldHaveStatusAsync(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<UserDetailDto>(TestJson.Options))!;
        created.UserName.Should().Be("created.user@taskflow.test");
        created.Roles.Select(role => role.Name).Should().Equal(DefaultRoleBundles.ReadOnlyUserRoleName);
        response.Headers.Location!.ToString().Should().Be($"/api/user/{created.Id}");

        var currentUser = await ReadCurrentUserAsync(await TokenFromLoginAsync("created.user@taskflow.test"));
        currentUser.Id.Should().Be(created.Id);
        currentUser.Roles.Select(role => role.Name).Should().Equal(DefaultRoleBundles.ReadOnlyUserRoleName);
    }

    [Fact]
    public async Task Responses_NeverContainPasswordHashesOrSecurityStamps()
    {
        var user = await Data.UserWithPasswordAsync(Password);
        var stored = (await Data.FindIncludingDeletedAsync<TaskFlow.Domain.Entities.Users.UserEntity>(user.Id))!;
        var userRole = await Data.RoleNamedAsync(DefaultRoleBundles.UserRoleName);

        foreach (var url in new[] { "/api/user", $"/api/user/{user.Id}", $"/api/role/{userRole.Id}/user", CurrentUserUrl })
        {
            var body = await (await SendAsync(AdminEmail, HttpMethod.Get, url)).Content.ReadAsStringAsync();

            body.Should().NotContainEquivalentOf("passwordHash").And.NotContainEquivalentOf("securityStamp");
            body.Should().NotContain(stored.PasswordHash!).And.NotContain(stored.SecurityStamp.ToString());
        }
    }

    [Fact]
    public async Task RegisterAndLogin_AreRateLimitedPerClient()
    {
        await using var strictFactory = Factory.WithWebHostBuilder(builder => builder.UseSetting("RateLimiting:SignInPermitLimit", "2"));
        var client = strictFactory.CreateClient();

        async Task<HttpStatusCode> GuessAsync()
        {
            return (await client.PostAsJsonAsync("/api/auth/login", new { email = "guess@taskflow.test", password = "Guess-1234" })).StatusCode;
        }

        (await GuessAsync()).Should().Be(HttpStatusCode.Unauthorized);
        (await GuessAsync()).Should().Be(HttpStatusCode.Unauthorized);
        (await GuessAsync()).Should().Be(HttpStatusCode.TooManyRequests);
    }

    private Task<HttpResponseMessage> RegisterAsync(string email, string displayName, string password)
    {
        return Factory.CreateClient().PostAsJsonAsync("/api/auth/register", new { email, displayName, password }, TestJson.Options);
    }

    private Task<HttpResponseMessage> LoginAsync(string email, string password)
    {
        return Factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password }, TestJson.Options);
    }

    private async Task<string> TokenFromLoginAsync(string email)
    {
        var response = await LoginAsync(email, Password);
        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);

        return (await ReadAccessTokenAsync(response)).AccessToken;
    }

    private async Task<CurrentUserResponse> ReadCurrentUserAsync(string accessToken)
    {
        return (await ClientWithToken(accessToken).GetFromJsonAsync<CurrentUserResponse>(CurrentUserUrl, TestJson.Options))!;
    }

    private static async Task<AccessTokenDto> ReadAccessTokenAsync(HttpResponseMessage response)
    {
        return (await response.Content.ReadFromJsonAsync<AccessTokenDto>(TestJson.Options))!;
    }
}
