namespace TaskFlow.Application.Common.Interfaces;

/// <summary>A command sent by a signed-in user; the controller fills both properties before publishing it.</summary>
public interface IUserCommand
{
    UserCommandContext UserCommandContext { get; init; }

    Guid CorrelationId { get; init; }
}
