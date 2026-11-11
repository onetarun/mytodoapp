using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using TodoApp.Api.Data;
using TodoApp.Api.Data.Entities;
using TodoApp.Api.DTOs.Notifications;
using TodoApp.Api.Services;
using TodoApp.Tests.Data;

namespace TodoApp.Tests.Services;

/// <summary>
/// Known data set, with "now" fixed at 2026-11-11 10:00 UTC:
/// 1 completed (due today 12:00) · 2 pending, no due date · 3 overdue (2 days ago) ·
/// 4 overdue today (08:00) · 5 due today later (20:00, due soon) · 6 due in 3 days ·
/// 7 due in 10 days · deleted (due today) · user B overdue.
/// </summary>
public sealed class DashboardAndNotificationServiceTests
{
    private const string UserA = "user-a";
    private const string UserB = "user-b";

    private static readonly DateTime Now = new(2026, 11, 11, 10, 0, 0, DateTimeKind.Utc);

    private readonly string _databaseName = Guid.NewGuid().ToString();

    [Fact]
    public async Task Summary_InUtc_CountsEveryRuleExactly()
    {
        var ids = await SeedKnownSetAsync();

        var summary = await GetSummaryAsync(UserA, TimeZoneInfo.Utc);

        summary.Total.Should().Be(7);
        summary.Completed.Should().Be(1);
        summary.Pending.Should().Be(6);
        summary.Overdue.Should().Be(2);
        summary.DueToday.Should().Be(2);
        summary.Upcoming.Select(t => t.Id).Should().Equal(ids["dueToday"], ids["in3Days"]);
        summary.Upcoming.Should().OnlyContain(t => !t.IsCompleted && !t.IsOverdue);
    }

    [Fact]
    public async Task Summary_DueToday_UsesTheUsersLocalDay()
    {
        await SeedKnownSetAsync();

        // Asia/Kolkata (UTC+5:30): local day = 2026-11-10 18:30Z .. 2026-11-11 18:30Z, so 20:00Z is tomorrow.
        var kolkata = await GetSummaryAsync(UserA, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata"));

        // America/Los_Angeles (UTC-8): local day = 2026-11-11 08:00Z .. 2026-11-12 08:00Z.
        var losAngeles = await GetSummaryAsync(UserA, TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"));

        kolkata.DueToday.Should().Be(1);
        losAngeles.DueToday.Should().Be(2);
        kolkata.Overdue.Should().Be(2);
    }

    [Fact]
    public async Task Summary_Upcoming_IsLimitedTo10_AndOrderedByDueDate()
    {
        await using (var context = CreateContext(UserA))
        {
            for (var i = 12; i >= 1; i--)
            {
                context.TodoTasks.Add(new TodoTask { UserId = UserA, Title = $"Upcoming {i}", DueDate = Now.AddHours(i) });
            }

            await context.SaveChangesAsync();
        }

        var summary = await GetSummaryAsync(UserA, TimeZoneInfo.Utc);

        summary.Upcoming.Should().HaveCount(TaskRules.UpcomingMaxItems);
        summary.Upcoming.Select(t => t.Title).Should().Equal(Enumerable.Range(1, 10).Select(i => $"Upcoming {i}"));
    }

    [Fact]
    public async Task Summary_ForUserWithoutTasks_IsAllZero()
    {
        await SeedKnownSetAsync();

        var summary = await GetSummaryAsync("user-c", TimeZoneInfo.Utc);

        summary.Should().BeEquivalentTo(new { Total = 0, Completed = 0, Pending = 0, Overdue = 0, DueToday = 0 });
        summary.Upcoming.Should().BeEmpty();
    }

    [Fact]
    public async Task Summary_OtherUsersTasks_NeverAffectTheCounts()
    {
        await SeedKnownSetAsync();

        var summaryB = await GetSummaryAsync(UserB, TimeZoneInfo.Utc);

        summaryB.Total.Should().Be(1);
        summaryB.Overdue.Should().Be(1);
        summaryB.Upcoming.Should().BeEmpty();
    }

    [Fact]
    public async Task Notifications_IncludeOnlyOverdueAndDueWithin24h_OverdueFirst()
    {
        var ids = await SeedKnownSetAsync();

        var result = await GetNotificationsAsync(UserA);

        result.Count.Should().Be(3);
        result.Items.Select(n => (n.TaskId, n.Type)).Should().Equal(
            (ids["overdue"], NotificationType.Overdue),
            (ids["overdueToday"], NotificationType.Overdue),
            (ids["dueToday"], NotificationType.DueSoon));
        result.Items.Should().NotContain(n => n.TaskId == ids["completed"] || n.TaskId == ids["deleted"]);
    }

    [Fact]
    public async Task Notifications_DueExactlyIn24h_IsDueSoon_AndJustAfter_IsExcluded()
    {
        await using (var context = CreateContext(UserA))
        {
            context.TodoTasks.Add(new TodoTask { UserId = UserA, Title = "Edge", DueDate = Now.AddHours(TaskRules.DueSoonHours) });
            context.TodoTasks.Add(new TodoTask { UserId = UserA, Title = "Later", DueDate = Now.AddHours(TaskRules.DueSoonHours).AddMinutes(1) });
            await context.SaveChangesAsync();
        }

        var result = await GetNotificationsAsync(UserA);

        result.Items.Should().ContainSingle().Which.Should().Match<NotificationDto>(n => n.Title == "Edge" && n.Type == NotificationType.DueSoon);
    }

    [Fact]
    public async Task Notifications_ForOtherUser_ShowOnlyTheirOwnTasks()
    {
        await SeedKnownSetAsync();

        var result = await GetNotificationsAsync(UserB);

        result.Items.Should().ContainSingle().Which.Title.Should().Be("B overdue");
    }

    [Fact]
    public void TaskRules_DueSoonAndNotificationExpression_Agree()
    {
        var cases = new DateTime?[] { null, Now.AddHours(-1), Now, Now.AddHours(24), Now.AddHours(25) };
        var needsNotification = TaskRules.NeedsNotificationExpression(Now).Compile();

        foreach (var due in cases)
        {
            var task = new TodoTask { DueDate = due };
            needsNotification(task).Should().Be(
                TaskRules.IsOverdue(false, due, Now) || TaskRules.IsDueSoon(false, due, Now),
                $"due {due:O}");
        }

        TaskRules.IsDueSoon(true, Now.AddHours(1), Now).Should().BeFalse();
    }

    private async Task<Dictionary<string, int>> SeedKnownSetAsync()
    {
        var today = Now.Date;
        var tasks = new Dictionary<string, TodoTask>
        {
            ["completed"] = new() { UserId = UserA, Title = "Completed", DueDate = today.AddHours(12), IsCompleted = true, CompletedAt = Now },
            ["noDue"] = new() { UserId = UserA, Title = "No due date" },
            ["overdue"] = new() { UserId = UserA, Title = "Overdue", DueDate = Now.AddDays(-2) },
            ["overdueToday"] = new() { UserId = UserA, Title = "Overdue today", DueDate = today.AddHours(8) },
            ["dueToday"] = new() { UserId = UserA, Title = "Due today", DueDate = today.AddHours(20) },
            ["in3Days"] = new() { UserId = UserA, Title = "In 3 days", DueDate = Now.AddDays(3) },
            ["in10Days"] = new() { UserId = UserA, Title = "In 10 days", DueDate = Now.AddDays(10) },
            ["deleted"] = new() { UserId = UserA, Title = "Deleted", DueDate = today.AddHours(15), IsDeleted = true },
            ["otherUser"] = new() { UserId = UserB, Title = "B overdue", DueDate = Now.AddDays(-1) },
        };

        await using var context = CreateContext(UserA);
        context.TodoTasks.AddRange(tasks.Values);
        await context.SaveChangesAsync();

        return tasks.ToDictionary(pair => pair.Key, pair => pair.Value.Id);
    }

    private async Task<Api.DTOs.Dashboard.DashboardSummaryDto> GetSummaryAsync(string userId, TimeZoneInfo timeZone)
    {
        await using var context = CreateContext(userId);
        return await new DashboardService(context, new FixedTimeProvider(Now)).GetSummaryAsync(timeZone);
    }

    private async Task<NotificationsResponse> GetNotificationsAsync(string userId)
    {
        await using var context = CreateContext(userId);
        return await new NotificationService(context, new FixedTimeProvider(Now)).GetAsync();
    }

    private AppDbContext CreateContext(string userId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new AppDbContext(options, new FakeCurrentUserService(userId));
    }
}
