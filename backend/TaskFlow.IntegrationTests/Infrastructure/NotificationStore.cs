using System.Collections.Concurrent;
using AwesomeAssertions;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Domain.Enums;

namespace TaskFlow.IntegrationTests.Infrastructure;

public sealed record CapturedNotification(string UserId, NotificationDto Notification);

/// <summary>Records every notification the API sends, so tests can wait for the outcome of a command.</summary>
public sealed class NotificationStore
{
    private static readonly TimeSpan HandlerTimeout = TimeSpan.FromSeconds(30);

    private readonly ConcurrentQueue<CapturedNotification> _notifications = new();

    public IReadOnlyCollection<CapturedNotification> All => _notifications.ToArray();

    public void Add(string userId, NotificationDto notification)
    {
        _notifications.Enqueue(new CapturedNotification(userId, notification));
    }

    public void Clear()
    {
        _notifications.Clear();
    }

    /// <summary>The correlation id of the last command the API confirmed to this user.</summary>
    public Guid LastConfirmedCorrelationId(Guid userId)
    {
        var confirmation = All.LastOrDefault(captured => captured.UserId == userId.ToString()
                                                         && captured.Notification.NotificationType == NotificationType.Confirmation);

        confirmation.Should().NotBeNull("the API confirms every accepted command");
        return confirmation!.Notification.CorrelationId;
    }

    /// <summary>Waits until the handler of the command has sent its Done or Error notification.</summary>
    public async Task<NotificationDto> WaitForOutcomeAsync(Guid correlationId)
    {
        var deadline = DateTime.UtcNow + HandlerTimeout;

        while (DateTime.UtcNow < deadline)
        {
            var outcome = All.FirstOrDefault(captured => captured.Notification.CorrelationId == correlationId
                                                         && captured.Notification.NotificationType != NotificationType.Confirmation);
            if (outcome is not null)
            {
                return outcome.Notification;
            }

            await Task.Delay(25);
        }

        throw new TimeoutException($"No Done or Error notification arrived for correlation {correlationId}.");
    }
}
