using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.Notifying;

public static class NotificationSenderExtensions
{
    public static Task SendDoneAsync(
        this ISendNotificationCommandHandler sender,
        IUserCommand command,
        EntityType entityType,
        ActionType actionType,
        Guid entityId)
    {
        return sender.Execute(command.UserCommandContext.Id.ToString(), new NotificationDto
        {
            NotificationType = NotificationType.Done,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            CorrelationId = command.CorrelationId
        });
    }

    public static Task SendErrorAsync(
        this ISendNotificationCommandHandler sender,
        IUserCommand command,
        EntityType entityType,
        ActionType actionType,
        Exception exception,
        Guid? entityId = null)
    {
        return sender.Execute(command.UserCommandContext.Id.ToString(), new NotificationDto
        {
            NotificationType = NotificationType.Error,
            ActionType = actionType,
            EntityType = entityType,
            EntityId = entityId,
            CorrelationId = command.CorrelationId,
            Message = HandlerExceptionMessage.ForUser(exception)
        });
    }
}
