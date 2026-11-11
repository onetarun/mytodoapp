using TodoApp.Api.DTOs.Notifications;

namespace TodoApp.Api.Services;

public interface INotificationService
{
    /// <summary>
    /// The current user's overdue and due-within-24h tasks, overdue first, each group by due date (FR-19, FR-20).
    /// </summary>
    Task<NotificationsResponse> GetAsync(CancellationToken cancellationToken = default);
}
