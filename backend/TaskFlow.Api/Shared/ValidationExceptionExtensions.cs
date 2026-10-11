using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Api.Shared;

internal static class ValidationExceptionExtensions
{
    public static ValidationProblemDetails ToProblemDetails(this ValidationException validationException)
    {
        var errors = validationException.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred."
        };
    }
}
