using System.Net;
using System.Security.Cryptography;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Domain.Constants;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Authentication;

/// <summary>
/// The production JwtBearer validation against hand-made tokens: signature and algorithm, issuer, audience, expiry
/// without clock skew, and the user and security stamp behind the token.
/// </summary>
public sealed class TokenValidationTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    private const string CurrentUserUrl = "/api/user/current";

    [Fact]
    public async Task Request_WithoutAToken_Returns401()
    {
        var response = await Factory.CreateClient().GetAsync(CurrentUserUrl);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ValidToken_ReachesProtectedEndpoints_WithTheUsersPermissions()
    {
        var user = await Data.UserAsync();

        await (await ClientWithToken(TestTokens.Create(Factory, await Data.AdminAsync())).GetAsync("/api/user"))
            .ShouldHaveStatusAsync(HttpStatusCode.OK);
        await (await ClientWithToken(TestTokens.Create(Factory, user)).GetAsync("/api/user"))
            .ShouldHaveStatusAsync(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ExpiredToken_Returns401_EvenSecondsAfterExpiry()
    {
        var token = TestTokens.Create(Factory, await Data.UserAsync(), expires: DateTime.UtcNow.AddSeconds(-30));

        await (await ClientWithToken(token).GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenSignedWithAnotherKey_Returns401()
    {
        var otherKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        var token = TestTokens.Create(Factory, await Data.UserAsync(), signingKey: otherKey);

        await (await ClientWithToken(token).GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnsignedToken_Returns401()
    {
        var token = TestTokens.Create(Factory, await Data.UserAsync(), signed: false);

        await (await ClientWithToken(token).GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenForAnotherAudience_OrFromAnotherIssuer_Returns401()
    {
        var user = await Data.UserAsync();

        await (await ClientWithToken(TestTokens.Create(Factory, user, audience: "AnotherApi")).GetAsync(CurrentUserUrl))
            .ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        await (await ClientWithToken(TestTokens.Create(Factory, user, issuer: "SomeoneElse")).GetAsync(CurrentUserUrl))
            .ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenWithoutAUser_Returns401()
    {
        var token = TestTokens.Create(Factory, user: null);

        await (await ClientWithToken(token).GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenOfADeletedUser_Returns401()
    {
        var user = await Data.UserAsync();
        var token = TestTokens.Create(Factory, user);
        await Data.ExecuteAsync(dbContext => dbContext.Users
            .Where(stored => stored.Id == user.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(stored => stored.DateTimeDeleted, DateTime.UtcNow)));

        await (await ClientWithToken(token).GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenOfAnEndedSession_Returns401()
    {
        var user = await Data.UserAsync();

        var token = TestTokens.Create(Factory, user, securityStamp: Guid.NewGuid());

        await (await ClientWithToken(token).GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenOfADeactivatedUser_Returns401AccountRevoked()
    {
        var user = await Data.UserAsync(isActive: false);

        var response = await ClientWithToken(TestTokens.Create(Factory, user)).GetAsync(CurrentUserUrl);

        await response.ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
        (await response.ReadErrorAsync()).Should().Be("account_revoked");
    }

    [Fact]
    public async Task ADeactivation_AppliesToTheNextRequest_WithoutANewToken()
    {
        var user = await Data.UserAsync(DefaultRoleBundles.UserRoleName);
        var client = ClientWithToken(TestTokens.Create(Factory, user));
        await (await client.GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.OK);

        await Data.ExecuteAsync(dbContext => dbContext.Users
            .Where(stored => stored.Id == user.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(stored => stored.IsActive, false)));

        await (await client.GetAsync(CurrentUserUrl)).ShouldHaveStatusAsync(HttpStatusCode.Unauthorized);
    }
}
