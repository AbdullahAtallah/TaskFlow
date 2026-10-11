using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Common.Filtering;
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
    public async Task<ActionResult<FilteringResult<TaskRowDto>>> Get([FromQuery] GetTasksQuery filter)
    {
        return Ok(await taskDataReader.GetGridAsync(filter));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDetailDto>> Get(Guid id)
    {
        return Ok(await taskDataReader.GetByIdAsync(id));
    }

    /// <summary>Dashboard: the open tasks assigned to the current user, by default ordered by due date.</summary>
    [HttpGet("my-tasks")]
    public async Task<ActionResult<FilteringResult<TaskRowDto>>> GetMyTasks([FromQuery] GetMyTasksQuery filter)
    {
        return Ok(await taskDataReader.GetMyTasksAsync(filter));
    }

    /// <summary>Dashboard: the number of visible tasks per status, plus the overdue ones.</summary>
    [HttpGet("status-counts")]
    public async Task<ActionResult<TaskStatusCountsDto>> GetStatusCounts()
    {
        return Ok(await taskDataReader.GetStatusCountsAsync());
    }
}
