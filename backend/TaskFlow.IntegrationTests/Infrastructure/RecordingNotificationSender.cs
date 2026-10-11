using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Interfaces;
using TaskFlow.Infrastructure.SignalR;

namespace TaskFlow.IntegrationTests.Infrastructure;

/// <summary>Records each notification and still sends it over SignalR.</summary>
internal sealed class RecordingNotificationSender(
    NotificationStore notificationStore,
    SendNotificationCommandHandler signalRSender) : ISendNotificationCommandHandler
{
    public Task Execute(string userId, NotificationDto notification)
    {
        notificationStore.Add(userId, notification);
        return signalRSender.Execute(userId, notification);
    }
}
