namespace TodoApp.Api.DTOs.Notifications;

public sealed record NotificationsResponse(int Count, IReadOnlyList<NotificationDto> Items);
