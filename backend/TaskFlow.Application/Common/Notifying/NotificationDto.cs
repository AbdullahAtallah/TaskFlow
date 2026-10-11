using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Common.Notifying;

/// <summary>The payload of the SignalR "ReceiveNotification" message.</summary>
public class NotificationDto
{
    public required NotificationType NotificationType { get; init; }
    public required ActionType ActionType { get; init; }
    public required EntityType EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public Guid CorrelationId { get; init; }
    public string? Message { get; init; }
}
