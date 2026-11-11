using System.Linq.Expressions;

using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Tasks;

namespace TodoApp.Api.Services;

/// <summary>
/// Shared SQL projection of a task to <see cref="TaskListItemDto"/>, used by the task list and the dashboard.
/// </summary>
public static class TaskProjections
{
    /// <summary>
    /// Projects a task in the query. <c>IsOverdue</c> is left <c>false</c>; set it with <see cref="WithOverdue"/>.
    /// </summary>
    public static readonly Expression<Func<TodoTask, TaskListItemDto>> ListItem = t => new TaskListItemDto(
        t.Id,
        t.Title,
        t.Priority,
        t.CategoryId,
        t.Category != null ? t.Category.Name : null,
        t.DueDate,
        t.IsCompleted,
        false,
        t.SubTasks.Count(s => !s.IsDeleted),
        t.SubTasks.Count(s => !s.IsDeleted && s.IsCompleted));

    /// <summary>
    /// Sets <c>IsOverdue</c> on projected items with one shared <paramref name="utcNow"/> (FR-21).
    /// </summary>
    public static IReadOnlyList<TaskListItemDto> WithOverdue(IEnumerable<TaskListItemDto> items, DateTime utcNow) =>
        items
            .Select(item => item with { IsOverdue = TaskRules.IsOverdue(item.IsCompleted, item.DueDate, utcNow) })
            .ToList();
}
