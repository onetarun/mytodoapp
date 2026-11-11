namespace TodoApp.Api.DTOs.Notifications;

/// <summary>
/// One task in the notification bell (FR-19, FR-20). <see cref="DueDate"/> is in UTC.
/// </summary>
public sealed record NotificationDto(int TaskId, string Title, DateTime DueDate, NotificationType Type);
