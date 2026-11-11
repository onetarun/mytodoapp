using System.Linq.Expressions;

using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.Services;

/// <summary>
/// Shared time rules for tasks (FR-17 to FR-21), used by the task list, the dashboard and notifications.
/// All times are UTC; only "due today" depends on the user's time zone.
/// </summary>
public static class TaskRules
{
    /// <summary>
    /// A not-completed task is "due soon" when it is due within this many hours from now (FR-19).
    /// </summary>
    public const int DueSoonHours = 24;

    /// <summary>
    /// The dashboard's upcoming list covers this many days from now (FR-18).
    /// </summary>
    public const int UpcomingDays = 7;

    /// <summary>
    /// Maximum number of tasks in the dashboard's upcoming list.
    /// </summary>
    public const int UpcomingMaxItems = 10;

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

    /// <summary>
    /// Due soon: not completed and <c>utcNow ≤ DueDate ≤ utcNow + 24h</c>.
    /// </summary>
    public static bool IsDueSoon(bool isCompleted, DateTime? dueDate, DateTime utcNow) =>
        !isCompleted && dueDate.HasValue && dueDate.Value >= utcNow && dueDate.Value <= utcNow.AddHours(DueSoonHours);

    /// <summary>
    /// Tasks for the notification bell: overdue or due soon. Equal to
    /// <c>IsOverdue || IsDueSoon</c>, i.e. not completed and due no later than <c>utcNow + 24h</c>.
    /// </summary>
    public static Expression<Func<TodoTask, bool>> NeedsNotificationExpression(DateTime utcNow)
    {
        var dueSoonEnd = utcNow.AddHours(DueSoonHours);
        return task => !task.IsCompleted && task.DueDate != null && task.DueDate <= dueSoonEnd;
    }

    /// <summary>
    /// Due today: not completed and the due date is within the user's local calendar day,
    /// given as the UTC range [<paramref name="dayStartUtc"/>, <paramref name="dayEndUtc"/>).
    /// </summary>
    public static Expression<Func<TodoTask, bool>> IsDueTodayExpression(DateTime dayStartUtc, DateTime dayEndUtc) =>
        task => !task.IsCompleted && task.DueDate != null && task.DueDate >= dayStartUtc && task.DueDate < dayEndUtc;

    /// <summary>
    /// Upcoming: not completed and <c>utcNow ≤ DueDate ≤ utcNow + 7 days</c>.
    /// </summary>
    public static Expression<Func<TodoTask, bool>> IsUpcomingExpression(DateTime utcNow)
    {
        var upcomingEnd = utcNow.AddDays(UpcomingDays);
        return task => !task.IsCompleted && task.DueDate != null && task.DueDate >= utcNow && task.DueDate <= upcomingEnd;
    }

    /// <summary>
    /// The UTC start (inclusive) and end (exclusive) of the local calendar day that contains <paramref name="utcNow"/>.
    /// </summary>
    public static (DateTime StartUtc, DateTime EndUtc) LocalDayRange(DateTime utcNow, TimeZoneInfo timeZone)
    {
        var localToday = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone).Date;
        return (ToUtc(localToday, timeZone), ToUtc(localToday.AddDays(1), timeZone));
    }

    private static DateTime ToUtc(DateTime localMidnight, TimeZoneInfo timeZone)
    {
        var unspecified = DateTime.SpecifyKind(localMidnight, DateTimeKind.Unspecified);

        // A DST jump at midnight makes local midnight not exist; the day then starts one hour later.
        return timeZone.IsInvalidTime(unspecified)
            ? TimeZoneInfo.ConvertTimeToUtc(unspecified.AddHours(1), timeZone)
            : TimeZoneInfo.ConvertTimeToUtc(unspecified, timeZone);
    }
}
