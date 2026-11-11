using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.DTOs.Dashboard;

namespace TodoApp.Api.Services;

/// <summary>
/// The ownership + soft-delete query filters limit every count to the current user's non-deleted tasks.
/// </summary>
public sealed class DashboardService(AppDbContext dbContext, TimeProvider timeProvider) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(TimeZoneInfo timeZone, CancellationToken cancellationToken = default)
    {
        var utcNow = timeProvider.GetUtcNow().UtcDateTime;
        var (dayStartUtc, dayEndUtc) = TaskRules.LocalDayRange(utcNow, timeZone);
        var tasks = dbContext.TodoTasks.AsNoTracking();

        var totals = await tasks
            .GroupBy(_ => 1)
            .Select(g => new { Total = g.Count(), Completed = g.Count(t => t.IsCompleted) })
            .SingleOrDefaultAsync(cancellationToken);
        var total = totals?.Total ?? 0;
        var completed = totals?.Completed ?? 0;

        var overdue = await tasks.CountAsync(TaskRules.IsOverdueExpression(utcNow), cancellationToken);
        var dueToday = await tasks.CountAsync(TaskRules.IsDueTodayExpression(dayStartUtc, dayEndUtc), cancellationToken);

        var upcoming = await tasks
            .Where(TaskRules.IsUpcomingExpression(utcNow))
            .OrderBy(t => t.DueDate)
            .Take(TaskRules.UpcomingMaxItems)
            .Select(TaskProjections.ListItem)
            .ToListAsync(cancellationToken);

        return new DashboardSummaryDto(
            total,
            completed,
            total - completed,
            overdue,
            dueToday,
            TaskProjections.WithOverdue(upcoming, utcNow));
    }
}
