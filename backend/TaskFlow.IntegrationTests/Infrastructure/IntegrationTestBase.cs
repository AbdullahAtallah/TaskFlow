using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Domain.Entities.Users;
using TaskFlow.Domain.Enums;

namespace TaskFlow.IntegrationTests.Infrastructure;

/// <summary>Every test starts from a freshly seeded database and talks to the API over HTTP.</summary>
[Collection(TaskFlowCollection.Name)]
public abstract class IntegrationTestBase(TaskFlowFixture fixture) : IAsyncLifetime
{
    protected const string AdminEmail = TaskFlowApiFactory.AdministratorEmail;

    protected TaskFlowFixture Fixture { get; } = fixture;

    protected TaskFlowApiFactory Factory => Fixture.Factory;

    protected TestDataBuilder Data { get; } = new(fixture);

    /// <summary>Today as the API sees it, in the team's time zone.</summary>
    protected DateOnly Today => Fixture.Today;

    public virtual Task InitializeAsync()
    {
        return Fixture.ResetAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>A client signed in as the user with this e-mail address.</summary>
    protected async Task<HttpClient> ClientForAsync(string email)
    {
        return ClientWithToken(Factory.TokenFor(await UserByEmailAsync(email)));
    }

    protected HttpClient ClientWithToken(string accessToken)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    protected Task<UserEntity> UserByEmailAsync(string email)
    {
        var userName = email.ToLowerInvariant();
        return Data.QueryAsync(dbContext => dbContext.Users.AsNoTracking().SingleAsync(user => user.UserName == userName));
    }

    protected async Task<HttpResponseMessage> SendAsync(string email, HttpMethod method, string url, object? body = null)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = body is null ? null : JsonContent.Create(body, options: TestJson.Options)
        };

        return await (await ClientForAsync(email)).SendAsync(request);
    }

    protected async Task<T> GetAsync<T>(string email, string url)
    {
        var response = await SendAsync(email, HttpMethod.Get, url);
        await response.ShouldHaveStatusAsync(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<T>(TestJson.Options))!;
    }

    /// <summary>Sends a write request, expects 202 Accepted and waits for the handler's Done or Error notification.</summary>
    protected async Task<NotificationDto> SendCommandAsync(string email, HttpMethod method, string url, object? body = null)
    {
        var response = await SendAsync(email, method, url, body);
        await response.ShouldHaveStatusAsync(HttpStatusCode.Accepted);

        var sender = await UserByEmailAsync(email);
        var correlationId = Factory.Notifications.LastConfirmedCorrelationId(sender.Id);
        return await Factory.Notifications.WaitForOutcomeAsync(correlationId);
    }

    protected async Task<NotificationDto> SendCommandExpectingDoneAsync(string email, HttpMethod method, string url, object? body = null)
    {
        var notification = await SendCommandAsync(email, method, url, body);
        notification.NotificationType.Should().Be(NotificationType.Done, notification.Message);

        return notification;
    }
}
