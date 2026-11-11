using TodoApp.Api.Data.Entities;

namespace TodoApp.Api.DTOs.Tasks;

/// <summary>
/// A task with all its fields and subtasks. Dates are in UTC.
/// </summary>
public sealed record TaskDetailDto(
    int Id,
    string Title,
    Priority Priority,
    int? CategoryId,
    string? CategoryName,
    DateTime? DueDate,
    bool IsCompleted,
    bool IsOverdue,
    int SubTaskTotal,
    int SubTaskCompleted,
    string? Description,
    DateTime? CompletedAt,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IReadOnlyList<SubTaskDto> SubTasks);
