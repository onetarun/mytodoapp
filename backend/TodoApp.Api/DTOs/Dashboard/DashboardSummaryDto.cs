using TodoApp.Api.DTOs.Tasks;

namespace TodoApp.Api.DTOs.Dashboard;

/// <summary>
/// Dashboard counts (FR-17) and the upcoming list for the next 7 days (FR-18).
/// </summary>
public sealed record DashboardSummaryDto(
    int Total,
    int Completed,
    int Pending,
    int Overdue,
    int DueToday,
    IReadOnlyList<TaskListItemDto> Upcoming);
