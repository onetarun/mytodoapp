using Microsoft.AspNetCore.Mvc;

using TodoApp.Api.DTOs.Tasks;
using TodoApp.Api.Services;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/v1/tasks")]
public sealed class TasksController(ITaskService taskService) : ControllerBase
{
    public const string StatusField = "status";

    public const string InvalidStatusMessage = "Status must be all, pending or completed.";

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<TaskListItemDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken, TaskStatusFilter status = TaskStatusFilter.All)
    {
        if (!Enum.IsDefined(status))
        {
            ModelState.AddModelError(StatusField, InvalidStatusMessage);
            return ValidationProblem(ModelState);
        }

        return Ok(await taskService.GetAllAsync(status, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<TaskDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        OkOrNotFound(await taskService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<TaskDetailDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        var task = await taskService.CreateAsync(request, cancellationToken);
        return Created($"/api/v1/tasks/{task.Id}", task);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<TaskDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken) =>
        OkOrNotFound(await taskService.UpdateAsync(id, request, cancellationToken));

    [HttpPatch("{id:int}/complete")]
    [ProducesResponseType<TaskDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetCompletionAsync(int id, SetCompletionRequest request, CancellationToken cancellationToken) =>
        OkOrNotFound(await taskService.SetCompletionAsync(id, request.IsCompleted, cancellationToken));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken) =>
        await taskService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    private IActionResult OkOrNotFound(TaskDetailDto? task) => task is null ? NotFound() : Ok(task);
}
