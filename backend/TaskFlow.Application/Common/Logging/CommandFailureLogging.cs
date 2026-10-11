using Microsoft.Extensions.Logging;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Notifying;

namespace TaskFlow.Application.Common.Logging;

public static class CommandFailureLogging
{
    /// <summary>
    /// Logs a failed command: rejected input and broken business rules as a warning, unexpected failures as an error.
    /// </summary>
    public static void LogCommandFailure(this ILogger logger, Exception exception, IUserCommand command)
    {
        var level = HandlerExceptionMessage.ForUser(exception) is null ? LogLevel.Error : LogLevel.Warning;

        logger.Log(
            level,
            exception,
            "{CommandName} for user {UserId} failed (correlation {CorrelationId})",
            command.GetType().Name,
            command.UserCommandContext.Id,
            command.CorrelationId);
    }
}
