using TaskFlow.Application.Common.Notifying;

namespace TaskFlow.Application.Interfaces;

public interface ISendNotificationCommandHandler
{
    /// <summary>Sends the notification to every SignalR connection of the user with this id.</summary>
    Task Execute(string userId, NotificationDto notification);
}
