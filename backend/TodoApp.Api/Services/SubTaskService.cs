using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.SubTasks;
using TodoApp.Api.DTOs.Tasks;

namespace TodoApp.Api.Services;

/// <summary>
/// Every method first checks the parent task through the filtered <c>TodoTasks</c> set
/// (current user, not deleted), so subtasks are only reachable through their own task (AD-5).
/// </summary>
public sealed class SubTaskService(AppDbContext dbContext) : ISubTaskService
{
    public async Task<SubTaskDto?> CreateAsync(int taskId, CreateSubTaskRequest request, CancellationToken cancellationToken = default)
    {
        if (!await TaskExistsAsync(taskId, cancellationToken))
        {
            return null;
        }

        var maxSortOrder = await dbContext.SubTasks
            .Where(s => s.TodoTaskId == taskId)
            .MaxAsync(s => (int?)s.SortOrder, cancellationToken) ?? 0;

        var subTask = new SubTask
        {
            TodoTaskId = taskId,
            Title = request.Title.Trim(),
            SortOrder = maxSortOrder + 1,
        };
        dbContext.SubTasks.Add(subTask);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(subTask);
    }

    public async Task<SubTaskDto?> UpdateAsync(int taskId, int subTaskId, UpdateSubTaskRequest request, CancellationToken cancellationToken = default)
    {
        var subTask = await FindAsync(taskId, subTaskId, cancellationToken);
        if (subTask is null)
        {
            return null;
        }

        subTask.Title = request.Title.Trim();
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(subTask);
    }

    public async Task<SubTaskDto?> SetCompletionAsync(int taskId, int subTaskId, bool isCompleted, CancellationToken cancellationToken = default)
    {
        var subTask = await FindAsync(taskId, subTaskId, cancellationToken);
        if (subTask is null)
        {
            return null;
        }

        subTask.IsCompleted = isCompleted;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDto(subTask);
    }

    public async Task<bool> DeleteAsync(int taskId, int subTaskId, CancellationToken cancellationToken = default)
    {
        var subTask = await FindAsync(taskId, subTaskId, cancellationToken);
        if (subTask is null)
        {
            return false;
        }

        // Converted to a soft delete by AppDbContext (NFR-7).
        dbContext.SubTasks.Remove(subTask);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private Task<bool> TaskExistsAsync(int taskId, CancellationToken cancellationToken) =>
        dbContext.TodoTasks.AnyAsync(t => t.Id == taskId, cancellationToken);

    private async Task<SubTask?> FindAsync(int taskId, int subTaskId, CancellationToken cancellationToken)
    {
        if (!await TaskExistsAsync(taskId, cancellationToken))
        {
            return null;
        }

        return await dbContext.SubTasks
            .SingleOrDefaultAsync(s => s.Id == subTaskId && s.TodoTaskId == taskId, cancellationToken);
    }

    private static SubTaskDto ToDto(SubTask subTask) =>
        new(subTask.Id, subTask.Title, subTask.IsCompleted, subTask.SortOrder);
}
