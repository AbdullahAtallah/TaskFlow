using FluentValidation;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Common.Notifying;

public static class HandlerExceptionMessage
{
    /// <summary>Only validation and business-rule messages reach the user; anything else stays in the log.</summary>
    public static string? ForUser(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException =>
                validationException.Errors.FirstOrDefault()?.ErrorMessage ?? validationException.Message,
            TaskFlowException => exception.Message,
            _ => null
        };
    }
}
