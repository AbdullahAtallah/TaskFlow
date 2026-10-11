using Microsoft.AspNetCore.Mvc;
using TaskFlow.Application.Interfaces.DataReaders;
using TaskFlow.Application.Queries.Tasks;
using TaskFlow.Domain.Constants;
using TaskFlow.Infrastructure.Middleware.Attributes;

namespace TaskFlow.Api.Controllers.Tasks;

[Route("api/task/board")]
[ApiController]
[Authorization(PackageTypes.Tasks, FeatureTypes.TasksBoard)]
public class TaskBoardController(ITaskDataReader taskDataReader) : ControllerBase
{
    /// <summary>The tasks of a project in To do / In progress / Done columns.</summary>
    [HttpGet]
    [Claim(ClaimTypes.TasksBoardView)]
    public async Task<ActionResult<TaskBoardDto>> Get([FromQuery] GetTaskBoardQuery filter)
    {
        return Ok(await taskDataReader.GetBoardAsync(filter));
    }
}
