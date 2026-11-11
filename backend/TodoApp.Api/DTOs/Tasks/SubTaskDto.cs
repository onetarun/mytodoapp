namespace TodoApp.Api.DTOs.Tasks;

/// <summary>
/// A checklist item of a task. Defined here for <see cref="TaskDetailDto.SubTasks"/>; filled from Task 07.
/// </summary>
public sealed record SubTaskDto(int Id, string Title, bool IsCompleted, int SortOrder);
