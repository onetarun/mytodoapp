using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.DTOs.Tasks;

/// <summary>
/// A task row in the list (FR-10). Dates are in UTC.
/// </summary>
public sealed record TaskListItemDto(
    int Id,
    string Title,
    Priority Priority,
    int? CategoryId,
    string? CategoryName,
    DateTime? DueDate,
    bool IsCompleted,
    bool IsOverdue,
    int SubTaskTotal,
    int SubTaskCompleted);
