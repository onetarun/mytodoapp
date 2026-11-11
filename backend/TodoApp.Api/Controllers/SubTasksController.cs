using Microsoft.AspNetCore.Mvc;

using TodoApp.Api.DTOs.SubTasks;
using TodoApp.Api.DTOs.Tasks;
using TodoApp.Api.Services;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/v1/tasks/{taskId:int}/subtasks")]
public sealed class SubTasksController(ISubTaskService subTaskService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<SubTaskDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateAsync(int taskId, CreateSubTaskRequest request, CancellationToken cancellationToken)
    {
        var subTask = await subTaskService.CreateAsync(taskId, request, cancellationToken);
        return subTask is null
            ? NotFound()
            : Created($"/api/v1/tasks/{taskId}/subtasks/{subTask.Id}", subTask);
    }

    [HttpPut("{subId:int}")]
    [ProducesResponseType<SubTaskDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int taskId, int subId, UpdateSubTaskRequest request, CancellationToken cancellationToken) =>
        OkOrNotFound(await subTaskService.UpdateAsync(taskId, subId, request, cancellationToken));

    [HttpPatch("{subId:int}/complete")]
    [ProducesResponseType<SubTaskDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetCompletionAsync(int taskId, int subId, SetCompletionRequest request, CancellationToken cancellationToken) =>
        OkOrNotFound(await subTaskService.SetCompletionAsync(taskId, subId, request.IsCompleted, cancellationToken));

    [HttpDelete("{subId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int taskId, int subId, CancellationToken cancellationToken) =>
        await subTaskService.DeleteAsync(taskId, subId, cancellationToken) ? NoContent() : NotFound();

    private IActionResult OkOrNotFound(SubTaskDto? subTask) => subTask is null ? NotFound() : Ok(subTask);
}
