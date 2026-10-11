using FluentValidation;
using Microsoft.Extensions.Logging;
using TaskFlow.Application.Commands.Tasks.Common;
using TaskFlow.Application.Common.Extensions;
using TaskFlow.Application.Common.Logging;
using TaskFlow.Application.Common.Notifying;
using TaskFlow.Application.Common.Scopes;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Tasks;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

namespace TaskFlow.Application.Commands.Tasks.CreateTask;

public sealed class CreateTaskCommandHandler(
    IRepository<TaskEntity> taskRepository,
    IRepository<ProjectEntity> projectRepository,
    TaskAssigneeGuard taskAssigneeGuard,
    IValidator<CreateTaskCommand> validator,
    IUnitOfWork unitOfWork,
    ISendNotificationCommandHandler sendNotificationCommandHandler,
    ILogger<CreateTaskCommandHandler> logger)
{
    public async Task Handle(CreateTaskCommand command)
    {
        try
        {
            await validator.ValidateAndThrowAsync(command);

            var project = await projectRepository.FindInScopeAsync(command.ProjectId, TaskScope.ProjectsVisibleTo(command.UserCommandContext))
                ?? throw new TaskFlowNotFoundException(ErrorMessages.ProjectNotFound);

            if (command.AssigneeId is not null)
            {
                await taskAssigneeGuard.EnsureCanAssignAsync(command.UserCommandContext, project.TeamId, command.AssigneeId);
            }

            var task = new TaskEntity
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                Title = command.Title.Trim(),
                Description = command.Description.TrimToNull(),
                Status = command.Status,
                Priority = command.Priority,
                DueDate = command.DueDate,
                AssigneeId = command.AssigneeId,
                CreatedById = command.UserCommandContext.Id
            };

            await taskRepository.AddAsync(task);
            await unitOfWork.SaveChangesAsync();

            await sendNotificationCommandHandler.SendDoneAsync(command, EntityType.Task, ActionType.Created, task.Id);
        }
        catch (Exception exception)
        {
            logger.LogCommandFailure(exception, command);

            await sendNotificationCommandHandler.SendErrorAsync(command, EntityType.Task, ActionType.Created, exception);
        }
    }
}
