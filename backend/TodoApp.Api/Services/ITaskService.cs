using TodoApp.Api.DTOs.Tasks;

namespace TodoApp.Api.Services;

/// <summary>
/// Tasks of the current user (FR-6 to FR-10a). Every method returns <c>null</c>/<c>false</c>
/// when the task does not exist, is soft deleted, or belongs to another user (AD-5).
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Lists tasks filtered by status: pending first, then by due date (no due date last), then newest first.
    /// </summary>
    Task<IReadOnlyList<TaskListItemDto>> GetAllAsync(TaskStatusFilter status, CancellationToken cancellationToken = default);

    Task<TaskDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a task for the current user. The category, if any, was already checked by the request validator.
    /// </summary>
    Task<TaskDetailDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    Task<TaskDetailDto?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a task complete (sets <c>CompletedAt</c> to now) or incomplete (clears it) (FR-9).
    /// </summary>
    Task<TaskDetailDto?> SetCompletionAsync(int id, bool isCompleted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a task (FR-8, NFR-7).
    /// </summary>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
