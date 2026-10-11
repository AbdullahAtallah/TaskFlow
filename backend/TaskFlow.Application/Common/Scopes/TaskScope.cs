using System.Linq.Expressions;
using TaskFlow.Application.Common.Interfaces;
using TaskFlow.Domain.Constants;
using TaskFlow.Domain.Entities.Projects;
using TaskFlow.Domain.Entities.Tasks;

namespace TaskFlow.Application.Common.Scopes;

/// <summary>Tasks: the tasks of the projects in your scope — every task with Tasks.View.All.</summary>
public static class TaskScope
{
    public static Expression<Func<TaskEntity, bool>> VisibleTo(IUserScope user)
    {
        if (SeesTasksOfEveryProject(user))
        {
            return _ => true;
        }

        var userId = user.Id;
        return task => task.Project!.Team!.Members.Any(member => member.UserId == userId);
    }

    /// <summary>The projects whose tasks the user sees, and in which they may create tasks.</summary>
    public static Expression<Func<ProjectEntity, bool>> ProjectsVisibleTo(IUserScope user)
    {
        if (SeesTasksOfEveryProject(user))
        {
            return _ => true;
        }

        var userId = user.Id;
        return project => project.Team!.Members.Any(member => member.UserId == userId);
    }

    private static bool SeesTasksOfEveryProject(IUserScope user)
    {
        return user.HasClaim(ClaimTypes.TasksViewAll) || user.HasClaim(ClaimTypes.ProjectsViewAll);
    }
}
