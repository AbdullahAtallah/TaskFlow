using Microsoft.AspNetCore.SignalR;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Interfaces;

namespace TaskFlow.Infrastructure.SignalR;

internal sealed class SendNotificationCommandHandler(IHubContext<NotificationHub> notificationHubContext) : ISendNotificationCommandHandler
{
    public Task Execute(string userId, NotificationDto notification)
    {
        return notificationHubContext.Clients
            .User(userId)
            .SendAsync(NotificationHubPaths.ReceiveNotificationMethod, notification);
    }
}
