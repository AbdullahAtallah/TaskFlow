using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Domain.Enums;
using TaskFlow.IntegrationTests.Infrastructure;

namespace TaskFlow.IntegrationTests.Notifications;

/// <summary>The full round trip: a command accepted over HTTP comes back as SignalR notifications for that user only.</summary>
public sealed class NotificationHubTests(TaskFlowFixture fixture) : IntegrationTestBase(fixture)
{
    [Fact]
    public async Task CommandOutcome_ReachesTheSendersConnection_AndNobodyElse()
    {
        var bystander = await Data.UserAsync();
        var senderMessages = new ConcurrentQueue<NotificationDto>();
        var bystanderMessages = new ConcurrentQueue<NotificationDto>();
        await using var senderConnection = await ConnectAsync(AdminEmail, senderMessages);
        await using var bystanderConnection = await ConnectAsync(bystander.UserName, bystanderMessages);

        var done = await SendCommandExpectingDoneAsync(AdminEmail, HttpMethod.Post, "/api/team", new { name = "Facilities" });

        await WaitUntilAsync(() => senderMessages.Any(message => message.NotificationType == NotificationType.Done));
        senderMessages.Select(message => message.NotificationType).Should().Equal(NotificationType.Confirmation, NotificationType.Done);
        senderMessages.Should().OnlyContain(message => message.CorrelationId == done.CorrelationId);
        senderMessages.Last().EntityType.Should().Be(EntityType.Team);
        senderMessages.Last().ActionType.Should().Be(ActionType.Created);
        bystanderMessages.Should().BeEmpty();
    }

    [Fact]
    public async Task ErrorNotification_CarriesTheMessageForTheUser()
    {
        var messages = new ConcurrentQueue<NotificationDto>();
        await using var connection = await ConnectAsync(AdminEmail, messages);

        await SendCommandAsync(AdminEmail, HttpMethod.Delete, $"/api/team/{Guid.NewGuid()}");

        await WaitUntilAsync(() => messages.Any(message => message.NotificationType == NotificationType.Error));
        messages.Single(message => message.NotificationType == NotificationType.Error).Message.Should().Be(Domain.Constants.ErrorMessages.TeamNotFound);
    }

    [Fact]
    public async Task Hub_RefusesADeactivatedUser()
    {
        var deactivated = await Data.UserAsync(isActive: false);

        var connect = async () => await ConnectAsync(deactivated.UserName, new ConcurrentQueue<NotificationDto>());

        await connect.Should().ThrowAsync<HttpRequestException>();
    }

    private async Task<HubConnection> ConnectAsync(string email, ConcurrentQueue<NotificationDto> messages)
    {
        var accessToken = Factory.TokenFor(await UserByEmailAsync(email));
        var connection = new HubConnectionBuilder()
            .WithUrl(new Uri(Factory.Server.BaseAddress, "notifications"), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => Factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
            })
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .Build();

        connection.On<NotificationDto>("ReceiveNotification", messages.Enqueue);
        await connection.StartAsync();

        return connection;
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (!condition() && DateTime.UtcNow < deadline)
        {
            await Task.Delay(25);
        }

        condition().Should().BeTrue("the notification should arrive over SignalR");
    }
}
