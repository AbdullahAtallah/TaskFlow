namespace TaskFlow.Domain.Enums;

public enum NotificationType
{
    /// <summary>The API accepted the command; the handler runs in the background.</summary>
    Confirmation,
    Done,
    Error
}
