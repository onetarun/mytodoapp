using System.Linq.Expressions;

using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Services;

/// <summary>
/// Shared time rules for tasks (FR-21), reused by the dashboard and notifications (Task 08).
/// All times are UTC.
/// </summary>
public static class TaskRules
{
    /// <summary>
    /// Overdue: not completed and the due date is before <paramref name="utcNow"/>.
    /// </summary>
    public static bool IsOverdue(bool isCompleted, DateTime? dueDate, DateTime utcNow) =>
        !isCompleted && dueDate.HasValue && dueDate.Value < utcNow;

    /// <summary>
    /// The same rule as <see cref="IsOverdue"/>, as an expression EF Core can translate to SQL.
    /// </summary>
    public static Expression<Func<TodoTask, bool>> IsOverdueExpression(DateTime utcNow) =>
        task => !task.IsCompleted && task.DueDate != null && task.DueDate < utcNow;
}
