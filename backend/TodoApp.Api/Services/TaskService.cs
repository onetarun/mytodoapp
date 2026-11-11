using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Tasks;

namespace TodoApp.Api.Services;

/// <summary>
/// The ownership + soft-delete query filters on <see cref="AppDbContext"/> limit every query
/// to the current user's non-deleted tasks. Reads are projected to DTOs with <c>AsNoTracking</c> (NFR-1).
/// </summary>
public sealed class TaskService(AppDbContext dbContext, ICurrentUserService currentUserService) : ITaskService
{
    public async Task<IReadOnlyList<TaskListItemDto>> GetAllAsync(TaskStatusFilter status, CancellationToken cancellationToken = default)
    {
        var query = dbContext.TodoTasks.AsNoTracking();
        query = status switch
        {
            TaskStatusFilter.Pending => query.Where(t => !t.IsCompleted),
            TaskStatusFilter.Completed => query.Where(t => t.IsCompleted),
            _ => query,
        };

        var items = await query
            .OrderBy(t => t.IsCompleted)
            .ThenBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .ThenByDescending(t => t.CreatedAt)
            .Select(TaskProjections.ListItem)
            .ToListAsync(cancellationToken);

        return TaskProjections.WithOverdue(items, DateTime.UtcNow);
    }

    public async Task<TaskDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var detail = await dbContext.TodoTasks
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TaskDetailDto(
                t.Id,
                t.Title,
                t.Priority,
                t.CategoryId,
                t.Category != null ? t.Category.Name : null,
                t.DueDate,
                t.IsCompleted,
                false,
                t.SubTasks.Count(s => !s.IsDeleted),
                t.SubTasks.Count(s => !s.IsDeleted && s.IsCompleted),
                t.Description,
                t.CompletedAt,
                t.CreatedAt,
                t.UpdatedAt,
                t.SubTasks
                    .Where(s => !s.IsDeleted)
                    .OrderBy(s => s.SortOrder)
                    .Select(s => new SubTaskDto(s.Id, s.Title, s.IsCompleted, s.SortOrder))
                    .ToList()))
            .SingleOrDefaultAsync(cancellationToken);

        return detail is null
            ? null
            : detail with { IsOverdue = TaskRules.IsOverdue(detail.IsCompleted, detail.DueDate, DateTime.UtcNow) };
    }

    public async Task<TaskDetailDto> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var userId = currentUserService.UserId
            ?? throw new InvalidOperationException("Creating a task requires an authenticated user.");

        var task = new TodoTask { UserId = userId };
        ApplyEditableFields(task, request);
        dbContext.TodoTasks.Add(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(task.Id, cancellationToken)
            ?? throw new InvalidOperationException("The created task could not be read back.");
    }

    public async Task<TaskDetailDto?> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.TodoTasks.SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        ApplyEditableFields(task, request);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<TaskDetailDto?> SetCompletionAsync(int id, bool isCompleted, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.TodoTasks.SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return null;
        }

        if (isCompleted && !task.IsCompleted)
        {
            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;
        }
        else if (!isCompleted)
        {
            task.IsCompleted = false;
            task.CompletedAt = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var task = await dbContext.TodoTasks.SingleOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task is null)
        {
            return false;
        }

        // Converted to a soft delete by AppDbContext (NFR-7).
        dbContext.TodoTasks.Remove(task);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ApplyEditableFields(TodoTask task, ITaskWriteRequest request)
    {
        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.DueDate = request.DueDate;
        task.Priority = request.Priority;
        task.CategoryId = request.CategoryId;
    }
}
