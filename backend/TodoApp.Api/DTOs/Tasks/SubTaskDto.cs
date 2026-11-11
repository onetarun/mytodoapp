namespace TodoApp.Api.DTOs.Tasks;

/// <summary>
/// A checklist item of a task (FR-11, FR-12). Listed in <see cref="TaskDetailDto.SubTasks"/> by <c>SortOrder</c>.
/// </summary>
public sealed record SubTaskDto(int Id, string Title, bool IsCompleted, int SortOrder);
