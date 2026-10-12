using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Tasks;
using TaskFlow.Domain.Constants;
using TaskFlow.Infrastructure.Middleware.Attributes;

namespace TaskFlow.Api.Controllers.Tasks;

[Route("api/task")]
[ApiController]
[Authorization(PackageTypes.Tasks)]
public class TaskQueryController(ITaskDataReader taskDataReader) : ControllerBase
{
    /// <summary>Paged list of the visible tasks, filtered by project, status, priority, assignee and due date.</summary>
    [HttpGet]
    public async Task<ActionResult> Get([FromQuery] GetTasksQuery filter)
    {
        return Ok(await taskDataReader.GetGridAsync(filter));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult> Get(Guid id)
    {
        return Ok(await taskDataReader.GetByIdAsync(id));
    }

    /// <summary>Dashboard: the open tasks assigned to the current user, by default ordered by due date.</summary>
    [HttpGet("my-tasks")]
    public async Task<ActionResult> GetMyTasks([FromQuery] GetMyTasksQuery filter)
    {
        return Ok(await taskDataReader.GetMyTasksAsync(filter));
    }

    /// <summary>Dashboard: the number of visible tasks per status, plus the overdue ones.</summary>
    [HttpGet("status-counts")]
    public async Task<ActionResult> GetStatusCounts()
    {
        return Ok(await taskDataReader.GetStatusCountsAsync());
    }
}
