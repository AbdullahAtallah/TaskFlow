using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Api.Shared;

/// <summary>
/// Maps the exceptions that reach a controller to their HTTP status codes. Anything else is left to the
/// exception handler middleware, which answers 500 with a problem details body.
/// </summary>
internal sealed class ApiExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        context.Result = context.Exception switch
        {
            AccountRevokedException => Unauthorized(AccountRevokedException.Reason),
            InvalidCredentialsException => Unauthorized(InvalidCredentialsException.Reason),
            TooManySignInAttemptsException => new ObjectResult(new { error = TooManySignInAttemptsException.Reason })
            {
                StatusCode = StatusCodes.Status429TooManyRequests
            },
            UnauthorizedAccessException => new UnauthorizedResult(),
            ValidationException validationException => new BadRequestObjectResult(validationException.ToProblemDetails()),
            TaskFlowNotFoundException notFoundException => Problem(StatusCodes.Status404NotFound, "Not Found", notFoundException),
            ConcurrencyConflictException conflictException => Problem(StatusCodes.Status409Conflict, "Conflict", conflictException),
            TaskFlowException businessRuleException => Problem(StatusCodes.Status400BadRequest, "Bad Request", businessRuleException),
            _ => null
        };

        context.ExceptionHandled = context.Result is not null;
    }

    private static ObjectResult Unauthorized(string reason)
    {
        return new ObjectResult(new { error = reason }) { StatusCode = StatusCodes.Status401Unauthorized };
    }

    private static ObjectResult Problem(int status, string title, Exception exception)
    {
        return new ObjectResult(new ProblemDetails { Status = status, Title = title, Detail = exception.Message }) { StatusCode = status };
    }
}
