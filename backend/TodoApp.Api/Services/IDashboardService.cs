using TodoApp.Api.DTOs.Dashboard;

namespace TodoApp.Api.Services;

public interface IDashboardService
{
    /// <summary>
    /// Counts and upcoming tasks of the current user, computed on request (ARCHITECTURE §6).
    /// "Due today" uses the calendar day of <paramref name="timeZone"/>.
    /// </summary>
    Task<DashboardSummaryDto> GetSummaryAsync(TimeZoneInfo timeZone, CancellationToken cancellationToken = default);
}
