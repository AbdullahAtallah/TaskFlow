using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Services;
using Wolverine;

namespace TaskFlow.Api.Shared;

/// <summary>
/// Base of the write controllers: a command is published on the Wolverine bus and handled in the background;
/// its result reaches the user as a Done or Error notification over SignalR.
/// </summary>
public abstract class CommandControllerBase(
    IMessageBus messageBus,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ICurrentUserRetriever currentUserRetriever) : ControllerBase
{
    /// <summary>The current user, captured for the command handler.</summary>
    protected UserCommandContext UserContext => currentUserRetriever.GetAuthenticatedCurrentUser().ToCommandContext();

    /// <summary>Confirms the command to the user, publishes it and answers 202 Accepted.</summary>
    protected async Task<ActionResult> PublishAsync<TCommand>(TCommand command, EntityType entityType) where TCommand : IUserCommand
    {
        await sendNotificationCommandHandler.Execute(command.UserCommandContext.Id.ToString(), new NotificationDto
        {
            NotificationType = NotificationType.Confirmation,
            ActionType = ActionType.BeingProcessed,
            EntityType = entityType,
            CorrelationId = command.CorrelationId
        });

        await messageBus.PublishAsync(command);

        return Accepted();
    }
}
