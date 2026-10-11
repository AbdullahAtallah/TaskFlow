using System.Net;
using AwesomeAssertions;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Domain.Enums;

namespace TaskFlow.IntegrationTests.Infrastructure;

public static class HttpResponseAssertions
{
    public static async Task ShouldHaveStatusAsync(this HttpResponseMessage response, HttpStatusCode expected)
    {
        if (response.StatusCode != expected)
        {
            var body = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(expected, $"the response body was: {body}");
        }
    }
}

public static class NotificationAssertions
{
    public static void ShouldBeError(this NotificationDto notification, string expectedMessage)
    {
        notification.NotificationType.Should().Be(NotificationType.Error);
        notification.Message.Should().Be(expectedMessage);
    }
}
