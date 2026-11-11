using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.DTOs.Notifications;

namespace TodoApp.Api.Services;

/// <summary>
/// Computed on request (no background job). The query filters limit it to the current user's non-deleted tasks.
/// </summary>
public sealed class NotificationService(AppDbContext dbContext, TimeProvider timeProvider) : INotificationService
{
    public async Task<NotificationsResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;

        // Ordering by due date puts every overdue task (due < now) before every due-soon task (due ≥ now).
        var tasks = await dbContext.TodoTasks
            .AsNoTracking()
            .Where(TaskRules.NeedsNotificationExpression(utcNow))
            .OrderBy(t => t.DueDate)
            .ThenBy(t => t.Id)
            .Select(t => new { t.Id, t.Title, DueDate = t.DueDate!.Value })
            .ToListAsync(cancellationToken);

        var items = tasks
            .Select(t => new NotificationDto(
                t.Id,
                t.Title,
                t.DueDate,
                TaskRules.IsOverdue(false, t.DueDate, utcNow) ? NotificationType.Overdue : NotificationType.DueSoon))
            .ToList();

        return new NotificationsResponse(items.Count, items);
    }
}
