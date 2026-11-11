using TodoApp.Api.DTOs.SubTasks;
using TodoApp.Api.DTOs.Tasks;

namespace TodoApp.Api.Services;

/// <summary>
/// Checklist items of the current user's tasks (FR-11, FR-12). Every method returns <c>null</c>/<c>false</c>
/// when the parent task is not the user's or is deleted, or when the subtask does not belong to that task (AD-5).
/// </summary>
public interface ISubTaskService
{
    /// <summary>
    /// Adds a subtask at the end of the checklist (<c>SortOrder</c> = current max + 1).
    /// </summary>
    Task<SubTaskDto?> CreateAsync(int taskId, CreateSubTaskRequest request, CancellationToken cancellationToken = default);

    Task<SubTaskDto?> UpdateAsync(int taskId, int subTaskId, UpdateSubTaskRequest request, CancellationToken cancellationToken = default);

    Task<SubTaskDto?> SetCompletionAsync(int taskId, int subTaskId, bool isCompleted, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes a subtask (NFR-7).
    /// </summary>
    Task<bool> DeleteAsync(int taskId, int subTaskId, CancellationToken cancellationToken = default);
}
